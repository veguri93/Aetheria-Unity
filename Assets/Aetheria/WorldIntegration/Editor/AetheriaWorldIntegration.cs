using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
[InitializeOnLoad]
public static class AetheriaWorldIntegration
{
 const string Out="C:/Users/Vesha/Desktop/codexx/WorldIntegration/";
 const string Main="Assets/_Recovery/0.unity";
 const string Root="Assets/Aetheria/WorldIntegration";
 const string Source="Assets/Aetheria/WorldPrototype/Aetheria_World_Prototype.unity";
 static Scene scene;static Terrain terrain;static GameObject environment;static Vector3 offset;
 static StringBuilder report;static int frame;
 static AetheriaWorldIntegration(){EditorApplication.update+=Tick;}
 static void Check(bool yes,string text){if(!yes)throw new Exception(text);report.AppendLine("PASS: "+text);}
 static IEnumerable<T> Components<T>() where T:Component =>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true));
 static float FlatWeight(float x,float z){float dx=Mathf.Max(-62-x,0,x-25),dz=Mathf.Max(-15-z,0,z-58);return 1-Mathf.SmoothStep(0,1,Mathf.Clamp01(Mathf.Max(dx,dz)/20));}
 static void Tick(){
  if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  if(!File.Exists(Out+"request.txt"))return;
  if(File.ReadAllText(Out+"request.txt")=="verify"){
   File.WriteAllText(Out+"request.txt","verified");
   try{VerifySaved();}catch(Exception e){File.WriteAllText(Out+"verify-error.txt",e.ToString());}
   return;
  }
  if(File.ReadAllText(Out+"request.txt")!="integrate")return;
  File.WriteAllText(Out+"request.txt","started");report=new StringBuilder();
  try{Integrate();}catch(Exception e){File.WriteAllText(Out+"error.txt",e.ToString());Debug.LogException(e);}
 }
 static void Integrate(){
  for(int i=0;i<SceneManager.sceneCount;i++)Check(!SceneManager.GetSceneAt(i).isDirty,"open scene has no unsaved edits");
  scene=SceneManager.GetActiveScene();if(scene.path!=Main)scene=EditorSceneManager.OpenScene(Main);
  Check(!scene.GetRootGameObjects().Any(g=>g.name=="Aetheria integrated landscape"),"integration not already present");
  Directory.CreateDirectory(Root+"/Backups");Directory.CreateDirectory(Root+"/Generated");AssetDatabase.Refresh();
  string backup=Root+"/Backups/Main_before_world_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity";
  Check(AssetDatabase.CopyAsset(Main,backup),"scene backup created: "+backup);
  File.WriteAllText(Out+"backup.txt",backup);
  Check(File.ReadAllBytes(Main).SequenceEqual(File.ReadAllBytes(backup)),"backup matches original scene byte-for-byte");
  var originals=Components<Component>().Where(c=>c).ToArray();
  var player=Components<LocalPlayer>().Single();var controller=player.GetComponent<CharacterController>();
  var oldGround=scene.GetRootGameObjects().Single(g=>g.name=="Ground");
  var snapshots=originals.Where(c=>c!=player.transform && c.gameObject!=oldGround).ToDictionary(c=>c, c=>EditorJsonUtility.ToJson(c));
  int missingBefore=Components<Transform>().Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
  var source=EditorSceneManager.OpenScene(Source,OpenSceneMode.Additive);
  var sourceTerrain=source.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Terrain>()).Single();
  offset=new Vector3(-465,-sourceTerrain.terrainData.GetInterpolatedHeight(.44f,.1f)-.045f,-75);
  SceneManager.SetActiveScene(scene);environment=new GameObject("Aetheria integrated landscape");
  foreach(var root in source.GetRootGameObjects()){
   if(root.GetComponent<Camera>()||root.GetComponent<Light>()||root.GetComponent<NavMeshSurface>())continue;
   var copy=UnityEngine.Object.Instantiate(root);copy.name=root.name;SceneManager.MoveGameObjectToScene(copy,scene);copy.transform.SetParent(environment.transform,true);
  }
  environment.transform.position=offset;
  EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(scene);
  terrain=environment.GetComponentInChildren<Terrain>();
  var data=UnityEngine.Object.Instantiate(terrain.terrainData);data.name="Playable world terrain";
  AssetDatabase.CreateAsset(data,Root+"/Generated/PlayableLandscape.asset");terrain.terrainData=data;terrain.GetComponent<TerrainCollider>().terrainData=data;
  int n=data.heightmapResolution;var heights=data.GetHeights(0,0,n,n);
  for(int z=0;z<n;z++)for(int x=0;x<n;x++){
   float wx=offset.x+x*data.size.x/(n-1),wz=offset.z+z*data.size.z/(n-1);
   heights[z,x]=Mathf.Lerp(heights[z,x],(-.045f-offset.y)/data.size.y,FlatWeight(wx,wz));
  }
  data.SetHeights(0,0,heights);
  foreach(Transform group in environment.transform){
   if(!(group.name.StartsWith("Woodlands")||group.name.StartsWith("Rock outcrops")||group.name.StartsWith("Understory")))continue;
   foreach(Transform prop in group){
    if(FlatWeight(prop.position.x,prop.position.z)>.001f){prop.gameObject.SetActive(false);continue;}
   }
  }
  // The two reserved village meadows are far outside the compatibility clearing.
  oldGround.SetActive(false);
  Vector3 p=player.transform.position;p.y=terrain.SampleHeight(p)+offset.y-controller.center.y*player.transform.lossyScale.y+controller.height*.5f*player.transform.lossyScale.y+controller.skinWidth+.02f;player.transform.position=p;
  Physics.SyncTransforms();
  var nav=environment.AddComponent<NavMeshSurface>();nav.collectObjects=CollectObjects.Children;nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.overrideVoxelSize=true;nav.voxelSize=.5f;nav.overrideTileSize=true;nav.tileSize=128;nav.BuildNavMesh();
  Check(nav.navMeshData!=null,"navigation rebuilt for translated terrain");AssetDatabase.CreateAsset(nav.navMeshData,Root+"/Generated/PlayableNavigation.asset");
  Check(snapshots.All(kv=>kv.Key&&EditorJsonUtility.ToJson(kv.Key)==kv.Value),"all existing gameplay component settings and references preserved");
  Check(originals.All(c=>c),"every original scene component retained");
  Check(Components<Terrain>().Count(t=>t.isActiveAndEnabled)==1,"one active terrain");
  Check(Components<Camera>().Count(c=>c.isActiveAndEnabled)==1,"one active camera; original camera retained");
  Check(Components<EventSystem>().Count(c=>c.isActiveAndEnabled)==1,"one active EventSystem");
  Check(Components<Light>().Count(c=>c.isActiveAndEnabled&&c.type==LightType.Directional)==1,"one active directional light; original lighting retained");
  Check(Components<Transform>().Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))==missingBefore,"no added missing scripts (baseline "+missingBefore+")");
  int broken=0;
  foreach(var c in Components<MonoBehaviour>().Where(c=>c)){
   var so=new SerializedObject(c);var it=so.GetIterator();while(it.Next(true))if(it.propertyType==SerializedPropertyType.ObjectReference&&it.objectReferenceValue==null&&it.objectReferenceEntityIdValue!=default)broken++;
  }
  Check(broken==0,"no broken serialized object references");
  report.AppendLine("Landscape translation: "+offset.ToString("F4")+". Size: "+data.size);
  report.AppendLine("Scene player positioned at "+p.ToString("F4")+"; login remains server-authoritative.");
  Check(terrain.GetComponent<TerrainCollider>().Raycast(new Ray(p+Vector3.up*10,Vector3.down),out var hit,20),"terrain collider detected below player");
  AuditSpawns();
  var a=offset+new Vector3(270,47,280);var b=offset+new Vector3(670,60,710);
  foreach(var site in new[]{a,b})Check(terrain.terrainData.GetSteepness((site.x-offset.x)/1000,(site.z-offset.z)/1000)<.1f,"village reserve remains flat at "+site);
  var path=new NavMeshPath();
  Check(NavMesh.SamplePosition(p,out var start,5,NavMesh.AllAreas)&&NavMesh.SamplePosition(a,out var end,5,NavMesh.AllAreas)&&NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"complete navigation from player clearing to village A");
  Check(NavMesh.SamplePosition(a,out var va,5,NavMesh.AllAreas)&&NavMesh.SamplePosition(b,out var vb,5,NavMesh.AllAreas)&&NavMesh.CalculatePath(va.position,vb.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"complete navigation between village reserves");
  WalkCheck(controller);
  EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();Check(EditorSceneManager.SaveScene(scene,Main),"playable scene saved");
  File.WriteAllText(Out+"validation.txt",report.ToString());
  frame=0;EditorApplication.update+=Capture;
 }
 static void VerifySaved(){
  report=new StringBuilder();
  Check(!SceneManager.GetActiveScene().isDirty,"scene clean before saved-scene verification");
  scene=EditorSceneManager.OpenScene(Main);environment=scene.GetRootGameObjects().Single(g=>g.name=="Aetheria integrated landscape");
  terrain=environment.GetComponentInChildren<Terrain>();offset=environment.transform.position;
  Check(AssetDatabase.GetAssetPath(terrain.terrainData)==Root+"/Generated/PlayableLandscape.asset","saved scene references its own terrain data");
  Check(terrain.GetComponent<TerrainCollider>().terrainData==terrain.terrainData,"saved terrain collision uses matching heightmap");
  Check(environment.GetComponent<NavMeshSurface>().navMeshData!=null,"baked navigation survives scene reload");
  Check(environment.GetComponentsInChildren<MeshFilter>().All(m=>m.sharedMesh!=null),"environment mesh references present");
  Check(environment.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader)),"environment materials and shaders present");
  foreach(var local in new[]{new Vector3(270,47,280),new Vector3(670,60,710)}){
   var center=local+offset;
   var occupants=environment.GetComponentsInChildren<Renderer>().Where(r=> !(r is ParticleSystemRenderer)&&Vector2.Distance(new Vector2(r.bounds.center.x,r.bounds.center.z),new Vector2(center.x,center.z))<75).ToArray();
   Check(occupants.Length==0,"village core remains empty: "+center);
  }
  var player=Components<LocalPlayer>().Single();var a=offset+new Vector3(270,47,280);var path=new NavMeshPath();
  Check(NavMesh.SamplePosition(player.transform.position,out var start,5,NavMesh.AllAreas)&&NavMesh.SamplePosition(a,out var end,5,NavMesh.AllAreas)&&NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"saved navigation connects player to village A");
  File.WriteAllText(Out+"saved-scene-checks.txt",report.ToString());
 }
 static void AuditSpawns(){
  report.AppendLine("SERVER SPAWN AUDIT (read-only; gap = server root Y minus terrain Y):");
  foreach(var file in Directory.GetFiles("C:/GameServer/GameServer/GameServer/Game/Data/Npcs/Spawns","*.xml"))foreach(var s in XDocument.Load(file).Root.Elements("spawn")){
   float x=float.Parse(s.Attribute("x").Value,CultureInfo.InvariantCulture),y=float.Parse(s.Attribute("y").Value,CultureInfo.InvariantCulture),z=float.Parse(s.Attribute("z").Value,CultureInfo.InvariantCulture);
   float h=terrain.SampleHeight(new Vector3(x,0,z))+offset.y;
   Check(x>=offset.x&&x<=offset.x+1000&&z>=offset.z&&z<=offset.z+1000,"spawn inside landscape: "+Path.GetFileName(file)+" npcId="+s.Attribute("npcId").Value);
   report.AppendLine("  "+s+" terrainY="+h.ToString("F4")+" rootGap="+(y-h).ToString("F4"));
  }
  report.AppendLine("Teleport (-51.70,0.26,-1.93): terrainY="+(terrain.SampleHeight(new Vector3(-51.70f,0,-1.93f))+offset.y));
  report.AppendLine("Default respawn (0,0,0): terrainY="+(terrain.SampleHeight(Vector3.zero)+offset.y));
  report.AppendLine("Existing duplicate monster XML entries were preserved. Saved database character positions were not queried; live login/teleport/respawn needs testing.");
 }
 static void WalkCheck(CharacterController original){
  var g=new GameObject("Temporary collision verification");var c=g.AddComponent<CharacterController>();
  c.height=original.height;c.radius=original.radius;c.center=original.center;c.skinWidth=original.skinWidth;c.stepOffset=original.stepOffset;c.slopeLimit=original.slopeLimit;
  g.transform.position=new Vector3(5,.1f,0);Physics.SyncTransforms();
  for(int i=0;i<200;i++)c.Move(new Vector3(.05f,-.03f,.05f));
  var pos=g.transform.position;bool grounded=c.isGrounded;UnityEngine.Object.DestroyImmediate(g);
  Check(grounded&&Vector2.Distance(new Vector2(pos.x,pos.z),new Vector2(15,10))<.2f,"matching CharacterController walked 14m on the terrain without falling through");
 }
 static void Capture(){
  if(++frame<40)return;EditorApplication.update-=Capture;
  try{
   var go=new GameObject("Temporary inspection camera");var c=go.AddComponent<Camera>();c.farClipPlane=2200;c.fieldOfView=60;c.transform.position=new Vector3(-5,7,-12);c.transform.LookAt(new Vector3(-25,1,35));
   c.GetUniversalAdditionalCameraData().renderPostProcessing=false;
   var rt=new RenderTexture(1600,1000,24);rt.Create();var request=new UniversalRenderPipeline.SingleCameraRequest{destination=rt};RenderPipeline.SubmitRenderRequest(c,request);RenderPipeline.SubmitRenderRequest(c,request);
   var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Out+"IntegratedSpawnArea.png",tex.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(go);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
   // Temporary inspection objects are not kept in the playable scene.
   EditorSceneManager.SaveScene(scene,Main);File.WriteAllText(Out+"complete.txt",Main);
  }catch(Exception e){File.WriteAllText(Out+"capture-error.txt",e.ToString());}
 }
}

