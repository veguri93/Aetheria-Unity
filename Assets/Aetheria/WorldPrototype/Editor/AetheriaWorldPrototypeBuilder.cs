using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;

[InitializeOnLoad]
public static class AetheriaWorldPrototypeBuilder
{
    static SceneSetup[] originalScenes;
    static AetheriaWorldPrototypeBuilder() { EditorApplication.update += CheckRequest; }
    static void CheckRequest()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        string marker=Out+"request.txt";
        if (!File.Exists(marker) || (File.ReadAllText(marker)!="pending" && File.ReadAllText(marker)!="refine")) return;
        bool refine=File.ReadAllText(marker)=="refine";
        File.WriteAllText(marker,"started");
        for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
            if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
            { File.WriteAllText(Out+"error.txt","Open scene has unsaved edits; please save before generation."); return; }
        originalScenes=EditorSceneManager.GetSceneManagerSetup();
        if(refine) Refine(); else Build();
    }
    static void Finish(int code)
    {
        if(Application.isBatchMode) { EditorApplication.Exit(code); return; }
        if(originalScenes!=null) EditorSceneManager.RestoreSceneManagerSetup(originalScenes);
    }
    const string Root="Assets/Aetheria/WorldPrototype";
    const string ScenePath=Root+"/Aetheria_World_Prototype.unity";
    const string Pack="Assets/EmaceArt/Slavic World Free/";
    const string Out="C:/Users/Vesha/Desktop/codexx/WorldPrototype/";
    const int Resolution=1025;
    static TerrainData data;
    static Terrain terrain;
    static Camera camera;
    static System.Random rng=new System.Random(73129);
    static List<Vector2[]> routes=new List<Vector2[]>();
    static List<Vector2> trees=new List<Vector2>();
    static Dictionary<Material,Material> materials=new Dictionary<Material,Material>();
    static Vector2 townA=new Vector2(270,280),townB=new Vector2(670,710),lake=new Vector2(710,440);
    static int treeCount,rockCount,bushCount;
    static string report;
    static float R(float lo,float hi)=>Mathf.Lerp(lo,hi,(float)rng.NextDouble());
    static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));
    static float N(float x,float z,float scale)=>Mathf.PerlinNoise(x/scale+23.7f,z/scale+81.4f);
    static float G(float x,float z,float cx,float cz,float rx,float rz)=>Mathf.Exp(-((x-cx)*(x-cx)/(rx*rx)+(z-cz)*(z-cz)/(rz*rz)));
    static float LakeRadius(float x,float z)
    {
        float a=Mathf.Atan2((z-lake.y)/70,(x-lake.x)/110);
        return Mathf.Sqrt(Mathf.Pow((x-lake.x)/110,2)+Mathf.Pow((z-lake.y)/70,2))/(1+.07f*Mathf.Sin(3*a)+.04f*Mathf.Cos(5*a));
    }
    static float BaseHeight(float x,float z)
    {
        float h=36+16*N(x,z,360)+7*N(x,z,135)+2*N(x,z,45);
        h+=115*G(x,z,130,870,155,170)+145*G(x,z,435,990,180,100)+110*G(x,z,940,850,95,155);
        h+=48*G(x,z,18,600,65,200)+18*G(x,z,500,620,160,200);
        float mountain=Smooth(80,160,h);
        h+=mountain*(13*N(x,z,55)+6*N(x,z,23));
        float lr=LakeRadius(x,z);
        h=Mathf.Lerp(30+2*N(x,z,80),h,Smooth(.65f,1.45f,lr));
        h=Mathf.Lerp(h,47,1-Smooth(90,135,Vector2.Distance(new Vector2(x,z),townA)));
        h=Mathf.Lerp(h,60,1-Smooth(82,125,Vector2.Distance(new Vector2(x,z),townB)));
        return h;
    }
    static float PathDistance(float x,float z,out float pathHeight)
    {
        var p=new Vector2(x,z);float best=10000;pathHeight=0;
        foreach(var route in routes)for(int i=1;i<route.Length;i++)
        {
            var a=route[i-1];var b=route[i];var d=b-a;
            float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
            float distance=(p-(a+d*t)).magnitude;
            if(distance<best){best=distance;pathHeight=Mathf.Lerp(BaseHeight(a.x,a.y),BaseHeight(b.x,b.y),t);}
        }
        return best;
    }
    static Vector2[] Curve(params Vector2[] points)
    {
        var result=new List<Vector2>();
        for(int i=0;i<points.Length-1;i++)for(int j=0;j<14;j++)
        {
            float t=j/14f;var a=points[Mathf.Max(0,i-1)];var b=points[i];var c=points[i+1];var d=points[Mathf.Min(points.Length-1,i+2)];
            result.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
        }
        result.Add(points.Last());return result.ToArray();
    }
    static void InitRoutes()
    {
        routes.Clear();
        routes.Add(Curve(new Vector2(465,20),new Vector2(430,115),new Vector2(345,205),townA));
        routes.Add(Curve(townA,new Vector2(320,385),new Vector2(425,460),new Vector2(495,540),new Vector2(585,620),townB));
        routes.Add(Curve(new Vector2(425,460),new Vector2(510,435),new Vector2(550,380),new Vector2(600,330),new Vector2(700,315),new Vector2(820,340),new Vector2(865,445),new Vector2(825,540),new Vector2(760,585),townB));
        routes.Add(Curve(townA,new Vector2(160,350),new Vector2(110,455),new Vector2(145,570),new Vector2(280,590),new Vector2(425,460)));
        routes.Add(Curve(townB,new Vector2(750,750),new Vector2(795,820),new Vector2(800,920)));
    }
    public static void Build()
    {
        try
        {
            if(File.Exists(ScenePath))throw new Exception("Prototype already exists; refusing to overwrite it.");
            Directory.CreateDirectory(Out);Directory.CreateDirectory(Root+"/Generated");Directory.CreateDirectory(Root+"/Prefabs");
            AssetDatabase.Refresh();InitRoutes();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            scene.name="Aetheria_World_Prototype";
            data=new TerrainData {name="Aetheria 1000m landscape",heightmapResolution=Resolution,size=new Vector3(1000,240,1000),alphamapResolution=512,baseMapResolution=1024};
            var heights=new float[Resolution,Resolution];
            for(int z=0;z<Resolution;z++)for(int x=0;x<Resolution;x++)
            {
                float wx=x*1000f/(Resolution-1),wz=z*1000f/(Resolution-1);
                float d=PathDistance(wx,wz,out float ph);
                heights[z,x]=Mathf.Lerp(BaseHeight(wx,wz),ph,1-Smooth(4,13,d))/240;
            }
            data.SetHeights(0,0,heights);AssetDatabase.CreateAsset(data,Root+"/Generated/Landscape.asset");
            terrain=Terrain.CreateTerrainGameObject(data).GetComponent<Terrain>();terrain.name="Landscape • 1 km × 1 km";
            terrain.drawInstanced=true;terrain.heightmapPixelError=6;terrain.basemapDistance=700;
            terrain.detailObjectDistance=65;terrain.detailObjectDensity=.55f;
            terrain.materialTemplate=new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
            AssetDatabase.CreateAsset(terrain.materialTemplate,Root+"/Generated/Terrain.mat");
            MakeLayers();MakeWater();
            var treePrefabs=MakeVariants("Prefabs/Nature/Tree",true,false);
            var rockPrefabs=MakeVariants("Prefabs/Environment/Rock",false,true);
            var bushPrefabs=MakeVariants("Prefabs/Nature/Bushes",false,false);
            Scatter(treePrefabs,rockPrefabs,bushPrefabs);MakeGrass();
            Mark("Village reserve A • 180 m diameter • elevation 47 m",townA,90);
            Mark("Village reserve B • 164 m diameter • elevation 60 m",townB,82);
            Mark("Character entry • south road",new Vector2(440,100),0);
            Lighting();
            BakeNavigation();
            report=Validate();
            File.WriteAllText(Root+"/REGION_NOTES.txt",report);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
            File.WriteAllText(Out+"validation.txt",report);
            AssetDatabase.Refresh();
            // Give the render pipeline time to initialize before requesting preview frames.
            EditorApplication.update+=CaptureWhenReady;frames=0;
        }
        catch(Exception ex){File.WriteAllText(Out+"error.txt",ex.ToString());Debug.LogException(ex);Finish(1);}
    }
    static void Refine()
    {
        try {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            terrain=UnityEngine.Object.FindAnyObjectByType<Terrain>();data=terrain.terrainData;
            camera=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).First(c=>c.name=="Region overview camera");
            foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{Root+"/Prefabs"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var prefab=PrefabUtility.LoadPrefabContents(path);
                bool tree=path.Contains("Tree"),rock=path.Contains("Rock");
                foreach(var group in prefab.GetComponentsInChildren<LODGroup>()) {
                    var lods=group.GetLODs();
                    for(int i=0;i<lods.Length;i++) lods[i].screenRelativeTransitionHeight=i==lods.Length-1?(tree?.004f:rock?.006f:.018f):Mathf.Lerp(.25f,.045f,i/(float)Mathf.Max(1,lods.Length-1));
                    group.SetLODs(lods);group.RecalculateBounds();
                }
                PrefabUtility.SaveAsPrefabAsset(prefab,path);PrefabUtility.UnloadPrefabContents(prefab);
            }
            var rockRoot=scene.GetRootGameObjects().First(o=>o.name.StartsWith("Rock outcrops"));
            foreach(Transform rock in rockRoot.transform) {
                var rs=rock.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;
                var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
                float low=10000;
                for(int z=-1;z<=1;z++)for(int x=-1;x<=1;x++)low=Mathf.Min(low,terrain.SampleHeight(new Vector3(b.center.x+x*b.extents.x*.75f,0,b.center.z+z*b.extents.z*.75f)));
                rock.position+=Vector3.up*(low-b.min.y-.4f);
            }
            var surface=UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            surface.BuildNavMesh();
            var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(Root+"/Generated/Navigation.asset");
            EditorUtility.CopySerialized(surface.navMeshData,saved);surface.RemoveData();surface.navMeshData=saved;surface.AddData();EditorUtility.SetDirty(saved);
            InitRoutes();treeCount=1382;rockCount=120;bushCount=240;
            report=Validate();File.WriteAllText(Out+"validation.txt",report);File.WriteAllText(Root+"/REGION_NOTES.txt",report);
            camera.transform.position=new Vector3(1060,610,-150);camera.transform.LookAt(new Vector3(470,40,510));
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
            frames=0;EditorApplication.update+=CaptureWhenReady;
        } catch(Exception ex){File.WriteAllText(Out+"error.txt",ex.ToString());Debug.LogException(ex);Finish(1);}
    }
    static TerrainLayer Layer(string name,string texture,Vector2 tile,Color? tint=null)
    {
        var source=AssetDatabase.LoadAssetAtPath<Texture2D>(Pack+"Texture/"+texture);
        if(!source)throw new Exception("Missing texture "+texture);
        if(tint.HasValue)
        {
            var rt=RenderTexture.GetTemporary(256,256);Graphics.Blit(source,rt);var old=RenderTexture.active;RenderTexture.active=rt;
            var tex=new Texture2D(256,256,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,256,256),0,0);tex.Apply();
            var pixels=tex.GetPixels();for(int i=0;i<pixels.Length;i++){float gray=pixels[i].grayscale;pixels[i]=tint.Value*(.7f+gray*.5f);}
            tex.SetPixels(pixels);tex.Apply();var path=Root+"/Generated/"+name+".png";File.WriteAllBytes(path,tex.EncodeToPNG());
            RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
            source=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        var layer=new TerrainLayer {name=name,diffuseTexture=source,tileSize=tile,smoothness=.05f};
        AssetDatabase.CreateAsset(layer,Root+"/Generated/"+name+".terrainlayer");return layer;
    }
    static void MakeLayers()
    {
        data.terrainLayers=new[]{Layer("Meadow","Flower_Grass.png",new Vector2(13,13)),Layer("Forest floor","Flower_Grass.png",new Vector2(17,17),new Color(.24f,.32f,.15f)),Layer("Dirt and shore","Pobbles_Sand_Dirt.png",new Vector2(9,9)),Layer("Weathered rock","Pobbles_Sand_Dirt.png",new Vector2(17,17),new Color(.46f,.48f,.44f))};
        var alpha=new float[512,512,4];
        for(int z=0;z<512;z++)for(int x=0;x<512;x++)
        {
            float wx=x*1000f/511,wz=z*1000f/511;
            float d=PathDistance(wx,wz,out _),slope=data.GetSteepness(x/511f,z/511f),h=data.GetInterpolatedHeight(x/511f,z/511f);
            float path=1-Smooth(3.2f,6.2f+N(wx,wz,11),d);
            float shore=(1-Smooth(1.12f,1.35f,LakeRadius(wx,wz)))*.95f;
            float rock=Mathf.Max(Smooth(25,42,slope),Smooth(110,165,h)*.9f);
            float forest=Forest(wx,wz)*.5f;
            float dirt=Mathf.Max(path,shore);
            alpha[z,x,2]=dirt;alpha[z,x,3]=rock*(1-dirt);alpha[z,x,1]=forest*(1-dirt)*(1-rock);alpha[z,x,0]=(1-forest)*(1-dirt)*(1-rock);
        }
        data.SetAlphamaps(0,0,alpha);
    }
    static float Forest(float x,float z)
    {
        return Mathf.Clamp01((G(x,z,205,480,190,180)+G(x,z,420,770,180,110)+G(x,z,835,610,130,180)+G(x,z,650,160,180,100))*1.25f+(.5f-N(x,z,95))*.9f-.12f);
    }
    static void MakeWater()
    {
        var go=new GameObject("Stillmere Lake • shallow edges, 2–5 m basin");
        var mesh=new Mesh {name="Stillmere irregular shoreline"};var vertices=new List<Vector3>{new Vector3(lake.x,35,lake.y)};var triangles=new List<int>();
        for(int i=0;i<128;i++)
        {
            float a=i*Mathf.PI*2/128;float radius=1+.07f*Mathf.Sin(3*a)+.04f*Mathf.Cos(5*a);
            vertices.Add(new Vector3(lake.x+110*radius*Mathf.Cos(a),35,lake.y+70*radius*Mathf.Sin(a)));
            triangles.Add(0);triangles.Add((i+1)%128+1);triangles.Add(i+1);
        }
        mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Root+"/Generated/LakeMesh.asset");go.AddComponent<MeshFilter>().sharedMesh=mesh;
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Stillmere water",color=new Color(.11f,.36f,.38f)};
        mat.SetFloat("_Smoothness",.88f);mat.SetFloat("_Metallic",.25f);AssetDatabase.CreateAsset(mat,Root+"/Generated/LakeWater.mat");go.AddComponent<MeshRenderer>().sharedMaterial=mat;
        // No invisible floor collider over the lake. Keep the submerged basin off the navigation mesh.
        var volume=go.AddComponent<NavMeshModifierVolume>();volume.area=1;volume.center=new Vector3(lake.x,30,lake.y);volume.size=new Vector3(220,20,140);
    }
    static List<GameObject> MakeVariants(string folder,bool tree,bool rock)
    {
        var list=new List<GameObject>();
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{Pack+folder}))
        {
            var original=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var go=UnityEngine.Object.Instantiate(original);go.name=original.name;
            foreach(var c in go.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(c);
            foreach(var c in go.GetComponentsInChildren<MonoBehaviour>(true))UnityEngine.Object.DestroyImmediate(c);
            foreach(var t in go.GetComponentsInChildren<Transform>(true))GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>Compatible(m)).ToArray();
                renderer.lightProbeUsage=LightProbeUsage.Off;
                if(!tree&&!rock)renderer.shadowCastingMode=ShadowCastingMode.Off;
            }
            var renderers=go.GetComponentsInChildren<Renderer>();if(renderers.Length==0){UnityEngine.Object.DestroyImmediate(go);continue;}
            var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            if(tree)
            {
                var c=go.AddComponent<CapsuleCollider>();c.radius=Mathf.Clamp(bounds.size.x*.035f,.18f,.55f);c.height=Mathf.Min(bounds.size.y,5);c.center=new Vector3(0,c.height/2,0);
            }
            if(rock)
            {
                // Use the lowest available mesh for collision rather than decorative high-detail geometry.
                var mf=go.GetComponentsInChildren<MeshFilter>().LastOrDefault();
                if(mf)mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
            }
            if(go.GetComponentsInChildren<LODGroup>().Length==0)
            {
                var lod=go.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(tree ? .012f : rock ? .018f : .035f,renderers)});lod.RecalculateBounds();
            }
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+go.name+".prefab");list.Add(prefab);UnityEngine.Object.DestroyImmediate(go);
        }
        return list;
    }
    static Material Compatible(Material m)
    {
        if(!m)return null;if(materials.TryGetValue(m,out var existing))return existing;
        var copy=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=m.name+"_Prototype",enableInstancing=true};
        Texture tex=m.HasProperty("_BaseMap")?m.GetTexture("_BaseMap"):m.mainTexture;
        copy.SetTexture("_BaseMap",tex);copy.color=m.HasProperty("_BaseColor")?m.GetColor("_BaseColor"):Color.white;copy.SetFloat("_Smoothness",.08f);
        copy.SetFloat("_Cull",0);
        if(m.HasProperty("_AlphaClip")&&m.GetFloat("_AlphaClip")>.5f){copy.SetFloat("_AlphaClip",1);copy.EnableKeyword("_ALPHATEST_ON");}
        AssetDatabase.CreateAsset(copy,Root+"/Generated/Material_"+materials.Count+".mat");materials.Add(m,copy);return copy;
    }
    static bool Reserve(float x,float z,float margin=0)=>Vector2.Distance(new Vector2(x,z),townA)<94+margin||Vector2.Distance(new Vector2(x,z),townB)<86+margin;
    static GameObject Place(List<GameObject> prefabs,Transform parent,float x,float z,float scale)
    {
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefabs[rng.Next(prefabs.Count)]);go.transform.SetParent(parent);
        go.transform.position=new Vector3(x,data.GetInterpolatedHeight(x/1000,z/1000)-.08f,z);go.transform.rotation=Quaternion.Euler(0,R(0,360),0);go.transform.localScale=Vector3.one*scale;return go;
    }
    static void Scatter(List<GameObject> treeP,List<GameObject> rockP,List<GameObject> bushP)
    {
        var forest=new GameObject("Woodlands • clustered native trees with LODs").transform;
        // A jittered candidate grid plus habitat masks makes groves and glades, not uniform random scatter.
        for(float z=28;z<975;z+=12)for(float x=28;x<975;x+=12)
        {
            float px=x+R(-5,5),pz=z+R(-5,5);float slope=data.GetSteepness(px/1000,pz/1000),height=data.GetInterpolatedHeight(px/1000,pz/1000);
            if(Reserve(px,pz)||LakeRadius(px,pz)<1.42f||PathDistance(px,pz,out _)<10||slope>29||height>135)continue;
            float density=Forest(px,pz);
            if(R(0,1)>density*.86f)continue;
            Place(treeP,forest,px,pz,R(.85f,1.6f));trees.Add(new Vector2(px,pz));treeCount++;
        }
        var rocks=new GameObject("Rock outcrops • ridge and shoreline formations").transform;
        for(int i=0;i<700;i++)
        {
            float x=R(15,985),z=R(15,985),h=data.GetInterpolatedHeight(x/1000,z/1000),s=data.GetSteepness(x/1000,z/1000);
            if(Reserve(x,z,10)||PathDistance(x,z,out _)<14||LakeRadius(x,z)<1.18f)continue;
            if(h<90&&!(LakeRadius(x,z)<1.5f&&R(0,1)<.5f))continue;
            var go=Place(rockP,rocks,x,z,R(1.5f,h>110?5:2.5f));go.transform.position-=Vector3.up*.8f;rockCount++;
        }
        var bushes=new GameObject("Understory • woodland edges and lakeside shrubs").transform;
        for(int i=0;i<2200;i++)
        {
            float x=R(20,980),z=R(20,980);
            if(Reserve(x,z)||PathDistance(x,z,out _)<8||LakeRadius(x,z)<1.3f||data.GetSteepness(x/1000,z/1000)>23)continue;
            if(R(0,1)>Forest(x,z)*.48f)continue;
            Place(bushP,bushes,x,z,R(.65f,1.25f));bushCount++;
        }
    }
    static void MakeGrass()
    {
        var variants=MakeVariants("Prefabs/Nature/Grass",false,false);
        if(variants.Count==0)return;
        data.SetDetailResolution(512,16);
        data.detailPrototypes=new[]{new DetailPrototype {prototype=variants[0],usePrototypeMesh=true,useInstancing=true,renderMode=DetailRenderMode.VertexLit,minWidth=.65f,maxWidth=1.15f,minHeight=.6f,maxHeight=1.1f,healthyColor=Color.white,dryColor=Color.white,noiseSpread=.3f}};
        var detail=new int[512,512];
        for(int z=0;z<512;z++)for(int x=0;x<512;x++)
        {
            float wx=x*1000f/511,wz=z*1000f/511;
            if(PathDistance(wx,wz,out _)>8&&LakeRadius(wx,wz)>1.35f&&data.GetSteepness(x/511f,z/511f)<22&&data.GetInterpolatedHeight(x/511f,z/511f)<95&&N(wx,wz,23)>.55f)
                detail[z,x]=1;
        }
        data.SetDetailLayer(0,0,0,detail);
    }
    static void Mark(string name,Vector2 p,float radius)
    {
        var go=new GameObject(name);go.transform.position=new Vector3(p.x,data.GetInterpolatedHeight(p.x/1000,p.y/1000),p.y);
        // Editor-only reserve guides: no visible structures or runtime colliders.
        go.tag="EditorOnly";
    }
    static void Lighting()
    {
        var sun=new GameObject("Afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.65f;sun.color=new Color(1,.91f,.76f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(37,-32,0);RenderSettings.sun=sun;
        var sky=new Material(Shader.Find("Skybox/Procedural"));sky.SetColor("_SkyTint",new Color(.49f,.61f,.75f));sky.SetFloat("_AtmosphereThickness",.85f);sky.SetFloat("_SunSize",.035f);AssetDatabase.CreateAsset(sky,Root+"/Generated/Sky.mat");RenderSettings.skybox=sky;
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.63f,.72f,.85f);RenderSettings.ambientEquatorColor=new Color(.44f,.51f,.44f);RenderSettings.ambientGroundColor=new Color(.24f,.28f,.21f);RenderSettings.ambientIntensity=1;
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=650;RenderSettings.fogEndDistance=1800;RenderSettings.fogColor=new Color(.65f,.75f,.8f);
        camera=new GameObject("Region overview camera").AddComponent<Camera>();camera.tag="MainCamera";camera.nearClipPlane=.3f;camera.farClipPlane=2200;camera.fieldOfView=52;camera.allowHDR=true;camera.backgroundColor=RenderSettings.fogColor;
        camera.transform.position=new Vector3(1060,610,-150);camera.transform.LookAt(new Vector3(470,40,510));
        var extra=camera.GetUniversalAdditionalCameraData();extra.renderPostProcessing=false;
    }
    static void BakeNavigation()
    {
        var go=new GameObject("Prototype navigation • terrain and simple scenery collision");var surface=go.AddComponent<NavMeshSurface>();
        surface.collectObjects=CollectObjects.All;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.overrideVoxelSize=true;surface.voxelSize=.5f;surface.overrideTileSize=true;surface.tileSize=128;
        surface.BuildNavMesh();if(surface.navMeshData)AssetDatabase.CreateAsset(surface.navMeshData,Root+"/Generated/Navigation.asset");
    }
    static string Validate()
    {
        float maxSlope=0;int samples=0;
        foreach(var route in routes)foreach(var p in route){maxSlope=Mathf.Max(maxSlope,data.GetSteepness(p.x/1000,p.y/1000));samples++;}
        if(maxSlope>32)throw new Exception("Main path slope exceeds intended walkability: "+maxSlope);
        float a=0,b=0;foreach(var p in new[]{townA,townB})for(int z=-50;z<=50;z+=10)for(int x=-50;x<=50;x+=10)
        {float slope=data.GetSteepness((p.x+x)/1000,(p.y+z)/1000);if(p==townA)a=Mathf.Max(a,slope);else b=Mathf.Max(b,slope);}
        var path=new NavMeshPath();bool nav=NavMesh.SamplePosition(new Vector3(townA.x,47,townA.y),out var ha,5,NavMesh.AllAreas)&&NavMesh.SamplePosition(new Vector3(townB.x,60,townB.y),out var hb,5,NavMesh.AllAreas)&&NavMesh.CalculatePath(ha.position,hb.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
        if(!nav)throw new Exception("Village reserves do not share a complete navigation path.");
        return "AETHERIA — STILLMERE BASIN\nExperimental world region; no gameplay systems included.\n\nTerrain: 1000 × 1000 metres; 1025 height samples; 512 splatmap; four layers.\nVillage A: (270,47,280), clear radius 90m. Village B: (670,60,710), clear radius 82m.\nBoth sites remain empty; reserve markers are EditorOnly.\nLake: about 220 × 140m, water elevation 35m. No swimming mechanics.\nTrees: "+treeCount+"; rocks: "+rockCount+"; shrubs: "+bushCount+".\nNative Slavic World Free assets reused through prototype-only prefab/material variants.\nTree LODs retained; instanced materials; near-field instanced terrain grass at 65m; simple trunk collision.\nTerrainCollider and baked prototype NavMesh included. Water basin excluded from navigation.\nMain route maximum sampled slope: "+maxSlope.ToString("F2")+" degrees ("+samples+" samples).\nTown-core maximum slope A/B: "+a.ToString("F2")+" / "+b.ToString("F2")+" degrees.\nPASS: complete navigable connection between both town sites.\n\nExisting gameplay scenes, GameServer, scripts and shared source assets are unchanged.\nMissing production features: animated/reflection-quality water, audio ambience, world streaming and gameplay spawn integration.\nUse Scene view fly navigation to explore; overview camera is for composition, not a player controller.\n";
    }
    static int frames;
    static void CaptureWhenReady()
    {
        if(++frames<50)return;EditorApplication.update-=CaptureWhenReady;
        try
        {
            Capture("Overview",new Vector3(1060,610,-150),new Vector3(470,40,510));
            Capture("Lakeside",new Vector3(530,76,300),new Vector3(730,46,485));
            Capture("VillageMeadow",new Vector3(220,55,185),new Vector3(300,53,420));
            File.WriteAllText(Out+"complete.txt",ScenePath);Finish(0);
        }
        catch(Exception e){File.WriteAllText(Out+"capture-error.txt",e.ToString());Finish(1);}
    }
    static void Capture(string name,Vector3 position,Vector3 target)
    {
        camera.transform.position=position;camera.transform.LookAt(target);
        var rt=new RenderTexture(1600,1000,24,RenderTextureFormat.ARGB32);rt.Create();
        var request=new UniversalRenderPipeline.SingleCameraRequest {destination=rt};RenderPipeline.SubmitRenderRequest(camera,request);RenderPipeline.SubmitRenderRequest(camera,request);
        var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(Out+name+".png",image.EncodeToPNG());
        RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
    }
}
