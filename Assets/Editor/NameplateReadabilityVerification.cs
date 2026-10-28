using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Runs in an isolated preview scene; does not save or alter the user's open scene.
[InitializeOnLoad]
public static class NameplateReadabilityVerification
{
    const string Folder = "C:/Users/Vesha/Desktop/codexx/NameplateReadability/";
    static NameplateReadabilityVerification() { EditorApplication.delayCall += AutoRun; }
    static void AutoRun()
    {
        if (!File.Exists(Folder + "run.request")) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += AutoRun; return; }
        File.Delete(Folder + "run.request");
        Run();
    }
    static void Set(object o, string n, object v) => o.GetType().GetField(n, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(o,v);
    static void Call(object o,string n) => o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,null);
    static void Check(bool ok,string name) { if (!ok) throw new Exception(name); File.AppendAllText(Folder+"verification.txt","PASS " + name + "\n"); }
    static void Singleton(Type t,object value) => t.GetProperty("Instance").SetValue(null,value);
    static GameObject Make(Scene scene,string name) { var go=new GameObject(name); SceneManager.MoveGameObjectToScene(go,scene); return go; }
    [MenuItem("Tools/Aetheria/Verify nameplate readability")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) { File.WriteAllText(Folder+"verification.txt","BLOCKED: Leave Play Mode before verification.\n"); return; }
        File.WriteAllText(Folder+"verification.txt","Unity " + Application.unityVersion + ": compiled editor preview-scene integration tests. No live GameServer or gameplay inputs.\n");
        var oldPlayer=LocalPlayer.Instance; var oldTarget=TargetManager.Instance;
        var scene=EditorSceneManager.NewPreviewScene();
        var plates=new List<WorldNameplate>();
        RenderTexture rt=null;
        Sprite crestFixture=null;
        try
        {
            var player=Make(scene,"Distance origin").AddComponent<LocalPlayer>(); Singleton(typeof(LocalPlayer),player);
            var targets=Make(scene,"Selection fixture").AddComponent<TargetManager>(); Singleton(typeof(TargetManager),targets);
            var camera=Make(scene,"Test camera").AddComponent<Camera>(); camera.scene=scene; camera.fieldOfView=60; camera.nearClipPlane=.01f; camera.farClipPlane=1000;
            rt=new RenderTexture(1920,1080,24); camera.targetTexture=rt; camera.backgroundColor=new Color(.25f,.4f,.15f); camera.clearFlags=CameraClearFlags.SolidColor;
            var manager=Make(scene,"Nameplate manager").AddComponent<WorldNameplateManager>(); Set(manager,"viewCamera",camera);
            var crestTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/Guild.png");
            crestFixture=Sprite.Create(crestTexture,new Rect(0,0,crestTexture.width,crestTexture.height),new Vector2(.5f,.5f),100);
            foreach(var path in new[]{"Assets/Prefabs/Player.prefab","Assets/Prefabs/RemotePlayer.prefab","Assets/Prefabs/monster.prefab","Assets/Prefabs/Ch34_nonPBR.prefab","Assets/Prefabs/Peasant Girl.prefab"})
            {
                var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                SceneManager.MoveGameObjectToScene(go,scene);
                var plate=go.GetComponent<WorldNameplate>(); Check(plate && plate.Label && plate.Style,"Bound presentation: "+path);
                plates.Add(plate); plate.SetInfo("Aetheria Test", "Existing title fixture");
                var presenter=go.GetComponentInChildren<PlayerNameplate>(true);
                if(presenter) presenter.SetPlayerInfo("Aetheria Test","Existing title fixture");
                Check(plate.Label.fontSharedMaterial.GetFloat("_OutlineWidth")>0 && plate.Label.fontSharedMaterial.GetColor("_OutlineColor").a>.5f,"Dark TMP outline: "+path);
                Check(plate.Label.fontSharedMaterial.IsKeywordEnabled("UNDERLAY_ON"),"Shared dark shadow: "+path);
                foreach(float distance in new[]{0f,29f,30f,32.5f,35f,37.5f,40f,41f,80f})
                {
                    go.transform.position=new Vector3(0,0,distance);
                    camera.transform.SetPositionAndRotation(plate.AnchorPosition-Vector3.forward*100,Quaternion.identity);
                    Call(manager,"LateUpdate");
                    float expected=WorldNameplateManager.DistanceAlpha(distance*distance,false);
                    Check(plate.Visible==(expected>0) && (expected==0 || Mathf.Abs(plate.Label.color.a-expected*plate.Style.color.a)<.001f),path+" distance "+distance+" alpha "+expected);
                }
                var unit=go.GetComponent<ITargetable>();
                typeof(TargetManager).GetProperty("CurrentTarget").SetValue(targets,unit);
                Call(manager,"LateUpdate");
                Check(plate.Visible && Mathf.Abs(plate.Label.color.a-plate.Style.color.a)<.001f,"Selected unit visible at 80m: "+path);
                typeof(TargetManager).GetProperty("CurrentTarget").SetValue(targets,null);
                Call(manager,"LateUpdate"); Check(!plate.Visible,"Deselect restores cutoff: "+path);
                go.transform.position=Vector3.zero;
                float prior=0;
                foreach(float zoom in new[]{.5f,2f,12f,100f,140f})
                {
                    camera.transform.position=plate.AnchorPosition-Vector3.forward*zoom;
                    Call(manager,"LateUpdate");
                    var t=plate.Label.transform;
                    float em=plate.Style.fontSize*.1f;
                    float pixels=Vector3.Distance(camera.WorldToScreenPoint(t.position),camera.WorldToScreenPoint(t.position+t.up*em*t.lossyScale.y));
                    Check(Mathf.Abs(pixels-24)<.05f,"24px name em at camera depth "+zoom+": "+path);
                    if(prior>0)Check(Mathf.Abs(pixels-prior)<.05f,"Stable projected size"); prior=pixels;
                    Check(Vector3.Distance(t.up,camera.transform.up)<.001f,"Camera-facing label");
                }
                camera.orthographic=true; camera.orthographicSize=30; Call(manager,"LateUpdate");
                Check(Mathf.Abs(plate.ReadabilityScale - 60f*(24f/1080f)/(.1f*plate.Style.fontSize))<.001f,"Orthographic sizing: "+path);camera.orthographic=false;
                Check(plate.Label.text=="Aetheria Test" && plate.TitleLabel.text=="Existing title fixture","Name and title preserved: "+path);
                var marquee=go.GetComponentInChildren<PlayerTitleMarquee>(true);
                if(marquee)
                {
                    var properties=new MaterialPropertyBlock(); marquee.FirstStrip.GetComponent<Renderer>().GetPropertyBlock(properties);
                    Check(marquee.IsShowing && Mathf.Abs(properties.GetVector("_MarqueeHalfSize").x-marquee.ViewportWidth*.5f*plate.ReadabilityScale)<.001f,"Marquee clip window scales with title");
                }
                var indicator=plate.TargetIndicator;
                if(presenter && indicator && indicator.CrestRenderer)
                {
                    indicator.SetGuildCrest(crestFixture);
                    indicator.SetState(TargetIndicatorState.Selected);
                    foreach(float zoom in new[]{2f,100f})
                    {
                        camera.transform.position=plate.AnchorPosition-Vector3.forward*zoom; Call(manager,"LateUpdate");
                        var crest=indicator.CrestRenderer;
                        Check(crest.enabled && crest.sprite==crestFixture,"Existing crest retained and visible");
                        float height=crestFixture.bounds.size.y*Mathf.Abs(crest.transform.lossyScale.y);
                        float width=crestFixture.bounds.size.x*Mathf.Abs(crest.transform.lossyScale.x);
                        Check(Mathf.Abs(width/height-crestFixture.rect.width/crestFixture.rect.height)<.001f,"Crest aspect ratio at zoom "+zoom);
                        Check(Mathf.Abs(height/plate.ReadabilityScale-.22f)<.001f,"Crest scales with name at zoom "+zoom);
                        Check(Mathf.Abs(Vector3.Dot(plate.Label.transform.position-plate.AnchorPosition,camera.transform.right)-indicator.NameOffset*plate.ReadabilityScale)<.001f,"Crest/name alignment at zoom "+zoom);
                        Check(indicator.LeftArrow.Renderer.enabled,"Selection arrow remains visible");
                    }
                    go.transform.position=new Vector3(0,0,35); camera.transform.position=plate.AnchorPosition-Vector3.forward*100;Call(manager,"LateUpdate");
                    Check(Mathf.Abs(indicator.CrestRenderer.color.a-.5f)<.001f,"Crest shares 35m fade");
                    go.transform.position=Vector3.zero;
                    indicator.SetGuildCrest(null); Check(!indicator.CrestRenderer.enabled,"Missing crest stays absent");
                }
                go.SetActive(false);
            }
            // Exceed the old 16-label cap using real NPC presentations.
            var source=plates[3];
            camera.transform.SetPositionAndRotation(new Vector3(0,3,-100),Quaternion.identity);
            for(int i=0;i<24;i++)
            {
                var go=Make(scene,"Crowd "+i); go.transform.position=new Vector3((i%8-3.5f)*3,0,i/8*4);
                var plate=WorldNameplate.AddPresentation(go,source); plates.Add(plate); plate.SetInfo("Unit "+i,"");
            }
            Call(manager,"LateUpdate");
            int visible=0; foreach(var plate in plates)if(plate.Visible)visible++;
            Check(visible==24,"24 simultaneous nearby nameplates, no arbitrary visibility cap");
            Singleton(typeof(LocalPlayer),null); Call(manager,"LateUpdate");
            foreach(var plate in plates)Check(!plate.Visible,"No camera-origin fallback when local player absent");
            Singleton(typeof(LocalPlayer),player); Call(manager,"LateUpdate");
            foreach(var plate in plates)if(plate.Visible)plate.Label.ForceMeshUpdate(true);
            camera.Render();
            var old=RenderTexture.active; RenderTexture.active=rt; var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0); image.Apply(); File.WriteAllBytes(Folder+"crowd-100m.png",image.EncodeToPNG());
            RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);
            foreach(var plate in plates)plate.gameObject.SetActive(false);
            var sample=plates[0];sample.gameObject.SetActive(true);sample.SetInfo("Aetheria Test","Existing title fixture");sample.SetTitleScrolling(true);
            sample.TargetIndicator.SetGuildCrest(crestFixture);sample.TargetIndicator.SetState(TargetIndicatorState.Selected);
            camera.transform.position=sample.AnchorPosition-Vector3.forward*100;Call(manager,"LateUpdate");
            foreach(var text in sample.GetComponentsInChildren<TMPro.TextMeshPro>(true))if(text.enabled)text.ForceMeshUpdate(true);
            int capture=0;
            foreach(var background in new[]{new Color(.2f,.35f,.08f),new Color(.95f,.97f,1f),new Color(.45f,.75f,1f)})
            {
                camera.backgroundColor=background; camera.Render(); RenderTexture.active=rt;
                var shot=new Texture2D(1920,1080,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,1920,1080),0,0);shot.Apply();
                File.WriteAllBytes(Folder+"readability-"+(capture++)+".png",shot.EncodeToPNG());UnityEngine.Object.DestroyImmediate(shot);RenderTexture.active=old;
            }
            Check(Mathf.Abs(WorldNameplateManager.DistanceAlpha(35*35,false)-.5f)<.001f,"35m midpoint alpha is 0.5");
            File.AppendAllText(Folder+"verification.txt","COMPLETE\n");
        }
        catch(Exception e) { File.AppendAllText(Folder+"verification.txt","FAIL "+e+"\n"); Debug.LogException(e); }
        finally
        {
            foreach(var plate in plates)if(plate)WorldNameplateManager.Unregister(plate);
            EditorSceneManager.ClosePreviewScene(scene);
            if(rt)UnityEngine.Object.DestroyImmediate(rt);
            if(crestFixture)UnityEngine.Object.DestroyImmediate(crestFixture);
            Singleton(typeof(LocalPlayer),oldPlayer);Singleton(typeof(TargetManager),oldTarget);
        }
    }
}
