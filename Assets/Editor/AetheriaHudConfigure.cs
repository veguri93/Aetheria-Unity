using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
[InitializeOnLoad] public static class AetheriaHudConfigure
{
 static AetheriaHudConfigure(){EditorApplication.delayCall+=AutoRun;}
 static void AutoRun(){string marker="C:/Users/Vesha/Desktop/codexx/HUD_Verification/run.txt";if(!Application.isBatchMode&&File.Exists(marker)&&File.ReadAllText(marker)=="pending"){if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogWarning("HUD configuration deferred while playing");return;}File.WriteAllText(marker,"started");Run();}}
 const string Art="Assets/Art/character hp bar/new hpcpmp/";
 const string Derived=Art+"Configured/";
 const string ScenePath="Assets/_Recovery/0.unity";
 const string Evidence="C:/Users/Vesha/Desktop/codexx/HUD_Verification/";
 const float Scale=460f/1792f;
 static string Log="";
 static void Note(string s){Log+=s+"\n";Debug.Log("HUD: "+s);}
 static void Require(bool b,string s){if(!b)throw new Exception(s);Note("PASS "+s);}
 static Texture2D Read(string path){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(path));return t;}
 // Pixel rectangles use artwork coordinates measured from the top-left.
 static Color Pixel(Texture2D t,float x,float y){return t.GetPixel(Mathf.Clamp(Mathf.RoundToInt(x),0,t.width-1),Mathf.Clamp(t.height-1-Mathf.RoundToInt(y),0,t.height-1));}
 static void Patch(Texture2D dst,Texture2D src,Rect from,Rect to,int border=0){
  for(int y=0;y<(int)to.height;y++)for(int x=0;x<(int)to.width;x++){
   float u=Map(x,to.width,from.width,border);float v=Map(y,to.height,from.height,border);
   dst.SetPixel((int)to.x+x,dst.height-1-((int)to.y+y),Pixel(src,from.x+u,from.y+v));
  }
 }
 static float Map(float n,float dst,float src,int b){if(b==0)return n*(src-1)/Mathf.Max(1,dst-1);if(n<b)return n;if(n>=dst-b)return src-(dst-n);return b+(n-b)*(src-2*b)/(dst-2*b);}
 static Sprite WriteSprite(Texture2D t,string name){
  string p=Derived+name+".png";File.WriteAllBytes(p,t.EncodeToPNG());AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
  var imp=(TextureImporter)AssetImporter.GetAtPath(p);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=4096;imp.npotScale=TextureImporterNPOTScale.None;imp.filterMode=FilterMode.Bilinear;imp.wrapMode=TextureWrapMode.Clamp;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(p);
 }
 static Sprite Crop(string name,int x,int y,int w,int h){var src=Read(Art+name+".png");var t=new Texture2D(w,h,TextureFormat.RGBA32,false);Patch(t,src,new Rect(x,y,w,h),new Rect(0,0,w,h));var sp=WriteSprite(t,name+"-trimmed");UnityEngine.Object.DestroyImmediate(src);UnityEngine.Object.DestroyImmediate(t);return sp;}
 static Sprite Frame(){
  var src=Read(Art+"frame.png");var dst=new Texture2D(1792,392,TextureFormat.RGBA32,false);
  // Reuse the original metal frame at native stroke widths, then its neutral header surface.
  Patch(dst,src,new Rect(58,137,1658,613),new Rect(0,0,1792,392),15);
  Patch(dst,src,new Rect(80,170,1600,100),new Rect(15,15,1762,362));
  Patch(dst,src,new Rect(75,300,1625,8),new Rect(15,103,1762,5));
  foreach(int y in new[]{132,216,301})Patch(dst,src,new Rect(380,342,1295,99),new Rect(150,y,1604,56),7);
  var sp=WriteSprite(dst,"frame-reference-layout");UnityEngine.Object.DestroyImmediate(src);UnityEngine.Object.DestroyImmediate(dst);return sp;
 }
 static RectTransform RT(Transform t){return (RectTransform)t;}
 static void Box(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.localScale=Vector3.one;r.localRotation=Quaternion.identity;r.anchoredPosition=new Vector2(x*Scale,-y*Scale);r.sizeDelta=new Vector2(w*Scale,h*Scale);}
 static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,.5f);r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;r.localRotation=Quaternion.identity;}
 static TMP_Text Text(Transform t,float size,TextAlignmentOptions alignment){var a=t.GetComponent<TMP_Text>();a.fontSize=size*Scale;a.enableAutoSizing=false;a.alignment=alignment;a.textWrappingMode=TextWrappingModes.NoWrap;a.overflowMode=TextOverflowModes.Ellipsis;a.margin=Vector4.zero;a.color=Color.white;a.raycastTarget=false;return a;}
 static GameObject Child(Transform p,string n,params Type[] types){var old=p.Find(n);if(old)return old.gameObject;var go=new GameObject(n,types);go.layer=p.gameObject.layer;go.transform.SetParent(p,false);return go;}
 static void Label(Transform panel,string n,string text,float x,float y,float w,float h,float size){var go=Child(panel,n,typeof(RectTransform),typeof(TextMeshProUGUI));Box(RT(go.transform),x,y,w,h);var t=Text(go.transform,size,TextAlignmentOptions.MidlineLeft);t.text=text;}
 static Slider Bar(Transform panel,string prefix,int y,Sprite sprite){
  var go=Child(panel,prefix+"Bar",typeof(RectTransform),typeof(Slider));Box(RT(go.transform),154,y,1596,48);
  var slider=go.GetComponent<Slider>();if(!slider)slider=go.AddComponent<Slider>();
  foreach(var im in go.GetComponentsInChildren<Image>(true))if(im.name!=prefix+"Fill")im.enabled=false;
  var f=Child(go.transform,prefix+"Fill",typeof(RectTransform),typeof(Image));Stretch(RT(f.transform));f.SetActive(true);f.transform.SetAsFirstSibling();
  var image=f.GetComponent<Image>();image.enabled=true;image.sprite=sprite;image.color=Color.white;image.type=Image.Type.Filled;image.fillMethod=Image.FillMethod.Horizontal;image.fillOrigin=0;image.preserveAspect=false;image.raycastTarget=false;image.fillAmount=slider.normalizedValue;
  slider.fillRect=RT(f.transform);slider.handleRect=null;slider.direction=Slider.Direction.LeftToRight;slider.minValue=0;slider.wholeNumbers=false;slider.interactable=false;slider.transition=Selectable.Transition.None;slider.targetGraphic=null;var nav=slider.navigation;nav.mode=Navigation.Mode.None;slider.navigation=nav;
  var tx=Child(go.transform,prefix+"Text",typeof(RectTransform),typeof(TextMeshProUGUI));Stretch(RT(tx.transform));tx.transform.SetAsLastSibling();var tt=Text(tx.transform,42,TextAlignmentOptions.Midline);if(prefix=="CP")tt.text="";
  // Small dark outline keeps current/max text readable on gold, red and blue.
  var material=new Material(tt.fontSharedMaterial);material.name=prefix+"Text_Readable";material.SetFloat(ShaderUtilities.ID_OutlineWidth,.16f);material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(0,0,0,.9f));string mp=Derived+prefix+"Text.mat";if(AssetDatabase.LoadAssetAtPath<Material>(mp))AssetDatabase.DeleteAsset(mp);AssetDatabase.CreateAsset(material,mp);tt.fontSharedMaterial=material;
  Label(panel,prefix+"Label",prefix,44,y-4,102,56,48);
  return slider;
 }
 public static void Run(){try{Configure();}catch(Exception ex){Directory.CreateDirectory(Evidence);File.WriteAllText(Evidence+"error.txt",ex.ToString());Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);}}
 static void Configure(){
  Directory.CreateDirectory(Evidence);Directory.CreateDirectory(Derived);
  var scene=SceneManager.GetActiveScene();if(Application.isBatchMode)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);Require(scene.path==ScenePath,"active HUD scene selected");var backup=AssetDatabase.GenerateUniqueAssetPath("Assets/_Recovery/HUD-before-configuration.unity");Require(EditorSceneManager.SaveScene(scene,backup,true),"active scene backed up including unsaved changes");Note("Active scene backup: "+backup);
  var panel=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RectTransform>(true)).Single(r=>r.name=="PlayerInfoPanel");
  var stats=panel.parent.GetComponent<PlayerStatsUI>();var progress=UnityEngine.Object.FindFirstObjectByType<PlayerProgressUI>(FindObjectsInactive.Include);
  Require(stats&&progress,"existing stats and progress controllers found");
  var so=new SerializedObject(stats);var hpRef=so.FindProperty("hpSlider").objectReferenceValue;var mpRef=so.FindProperty("mpSlider").objectReferenceValue;var hpText=so.FindProperty("hpText").objectReferenceValue;var mpText=so.FindProperty("mpText").objectReferenceValue;
  Note("Before panel size="+panel.sizeDelta+" position="+panel.anchoredPosition+"; HP="+hpRef+"; MP="+mpRef);
  var scaler=panel.GetComponentInParent<CanvasScaler>();Require(scaler!=null,"CanvasScaler present");Note("CanvasScaler="+scaler.uiScaleMode+" reference="+scaler.referenceResolution+" match="+scaler.matchWidthOrHeight);
  // Preserve existing global scaler so unrelated HUD panels retain their scale.
  Require(scaler.referenceResolution==new Vector2(1920,1080),"intended canvas resolution is 1920x1080");
  var frame=Frame();var cpSprite=Crop("cp-fill",38,300,2100,118);var hpSprite=Crop("hp-fill",34,287,2097,139);var mpSprite=Crop("mp-fill",28,303,2115,117);
  panel.anchorMin=panel.anchorMax=new Vector2(0,1);panel.pivot=new Vector2(0,1);panel.localScale=Vector3.one;panel.sizeDelta=new Vector2(460,392*Scale);
  var image=panel.GetComponent<Image>();image.sprite=frame;image.type=Image.Type.Simple;image.preserveAspect=true;image.color=Color.white;image.raycastTarget=true;
  var name=panel.Find("PlayerName");var level=panel.Find("PlayerLevel");Box(RT(name),44,18,1160,82);Text(name,64,TextAlignmentOptions.MidlineLeft);Box(RT(level),1220,18,524,82);Text(level,60,TextAlignmentOptions.MidlineRight);
  var originalCP=panel.Find("CpBar");if(originalCP){var duplicate=panel.Find("CPBar");if(duplicate)UnityEngine.Object.DestroyImmediate(duplicate.gameObject);originalCP.name="CPBar";var oldFill=originalCP.Find("CPfill");if(oldFill)oldFill.name="CPFill";Note("Reused original CpBar and CPfill from live scene; removed newly created duplicate.");}var cp=Bar(panel,"CP",136,cpSprite);var hp=Bar(panel,"HP",220,hpSprite);var mp=Bar(panel,"MP",305,mpSprite);
  // CP is a static visual only. No server values and no fabricated current/max text.
  cp.maxValue=1;cp.SetValueWithoutNotify(1);
  Require(hp==hpRef&&mp==mpRef,"original HP and MP Slider references preserved");Require(panel.Find("HPBar/HPText").GetComponent<TMP_Text>()==hpText&&panel.Find("MPBar/MPText").GetComponent<TMP_Text>()==mpText,"original HP and MP text references preserved");
  var ps=new SerializedObject(progress);Require(ps.FindProperty("nameText").objectReferenceValue==name.GetComponent<TMP_Text>()&&ps.FindProperty("levelText").objectReferenceValue==level.GetComponent<TMP_Text>(),"original name and level references preserved");
  var lt=level.GetComponent<TMP_Text>();if(!lt.text.StartsWith("Level "))lt.text="Level "+lt.text;
  Canvas.ForceUpdateCanvases();
  EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene),"configured scene saved");AssetDatabase.SaveAssets();
  Note("Reference measured visible frame: 1792x392; slots: x150, width1604, y132/216/301, height56. HUD width retained at 460.");
  File.WriteAllText(Evidence+"configuration.txt",Log);
  BeginTests(panel,hp,mp,cp,stats,progress);
 }
 static Scene originalScene;static GameObject[] originalRoots;static bool[] originalActive;static RectTransform testPanel;static Slider testHP,testMP,testCP;static PlayerStatsUI testStats;static PlayerProgressUI testProgress;static int testIndex;static double next;static Camera camera;static RenderTexture rt;static Scene previewScene;static string[] states={"100","50","0"};
 static void BeginTests(RectTransform panel,Slider hp,Slider mp,Slider cp,PlayerStatsUI stats,PlayerProgressUI progress){
  // Isolate the real saved HUD in a test scene; never start networking or mutate server state.
  originalScene=SceneManager.GetActiveScene();originalRoots=originalScene.GetRootGameObjects();originalActive=originalRoots.Select(r=>r.activeSelf).ToArray();previewScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);var canvasGo=new GameObject("HUD_TestCanvas",typeof(Canvas),typeof(CanvasScaler));SceneManager.MoveGameObjectToScene(canvasGo,previewScene);var canvas=canvasGo.GetComponent<Canvas>();canvasGo.layer=5;canvas.renderMode=RenderMode.WorldSpace;RT(canvasGo.transform).sizeDelta=new Vector2(1920,1080);canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;canvas.planeDistance=1;canvasGo.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
  var camgo=new GameObject("HUD_TestCamera",typeof(Camera));SceneManager.MoveGameObjectToScene(camgo,previewScene);camera=camgo.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.075f,.08f,.09f);camera.orthographic=true;camera.orthographicSize=540;camera.transform.position=new Vector3(0,0,-10);camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.cullingMask=-1;canvas.worldCamera=camera;
  var clone=UnityEngine.Object.Instantiate(panel.gameObject,canvasGo.transform,false);testPanel=RT(clone.transform);testPanel.anchoredPosition=new Vector2(15,-15);
  testHP=clone.transform.Find("HPBar").GetComponent<Slider>();testMP=clone.transform.Find("MPBar").GetComponent<Slider>();testCP=clone.transform.Find("CPBar").GetComponent<Slider>();
  var controllers=new GameObject("Test_ExistingControllers",typeof(PlayerStatsUI),typeof(PlayerProgressUI));SceneManager.MoveGameObjectToScene(controllers,previewScene);testStats=controllers.GetComponent<PlayerStatsUI>();testProgress=controllers.GetComponent<PlayerProgressUI>();var so=new SerializedObject(testStats);so.FindProperty("hpSlider").objectReferenceValue=testHP;so.FindProperty("mpSlider").objectReferenceValue=testMP;so.FindProperty("hpText").objectReferenceValue=clone.transform.Find("HPBar/HPText").GetComponent<TMP_Text>();so.FindProperty("mpText").objectReferenceValue=clone.transform.Find("MPBar/MPText").GetComponent<TMP_Text>();so.ApplyModifiedPropertiesWithoutUndo();so=new SerializedObject(testProgress);so.FindProperty("nameText").objectReferenceValue=clone.transform.Find("PlayerName").GetComponent<TMP_Text>();so.FindProperty("levelText").objectReferenceValue=clone.transform.Find("PlayerLevel").GetComponent<TMP_Text>();so.ApplyModifiedPropertiesWithoutUndo();
  // Render only the isolated HUD scene, avoiding other loaded game canvases.
  foreach(var root in originalRoots)root.SetActive(false);
  rt=new RenderTexture(1920,1080,24);rt.Create();camera.targetTexture=rt;testIndex=0;next=EditorApplication.timeSinceStartup+2;EditorApplication.update+=Tick;
 }
 static void InspectGameView(){try{var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);view.Repaint();string info="";for(var t=type;t!=null;t=t.BaseType){foreach(var p in t.GetProperties(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly))if(p.Name.ToLower().Contains("texture")||p.Name.ToLower().Contains("size"))info+="Property "+t.Name+" "+p.Name+" "+p.PropertyType+"\\n";foreach(var f in t.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly))if(f.Name.ToLower().Contains("texture"))info+="Field "+t.Name+" "+f.Name+" "+f.FieldType+"\\n";}File.WriteAllText(Evidence+"GameView-info.txt",info);}catch(Exception ex){File.WriteAllText(Evidence+"GameView-info.txt",ex.ToString());}}
 static void Restore(){if(originalRoots!=null){for(int i=0;i<originalRoots.Length;i++)if(originalRoots[i])originalRoots[i].SetActive(originalActive[i]);}if(previewScene.IsValid()&&previewScene.isLoaded)EditorSceneManager.CloseScene(previewScene,true);if(originalScene.IsValid()&&originalScene.isLoaded){SceneManager.SetActiveScene(originalScene);EditorSceneManager.SaveScene(originalScene);}}
 static void Tick(){if(EditorApplication.timeSinceStartup<next)return;try{
  if(testIndex<3){int v=testIndex==0?100:testIndex==1?50:0;testStats.SetHealth(v,100);testStats.SetMana(v,100);testCP.SetValueWithoutNotify(v/100f);testProgress.SetName("Aetheria");testProgress.SetProgress(24,0,0,100);Canvas.ForceUpdateCanvases();Note("Render: panelActive="+testPanel.gameObject.activeInHierarchy+" canvasRect="+RT(testPanel.GetComponentInParent<Canvas>().transform).rect+" panel="+testPanel.rect);foreach(var t in testPanel.GetComponentsInChildren<TMP_Text>())t.ForceMeshUpdate();
   foreach(var s in new[]{testHP,testMP,testCP}){Require(Mathf.Approximately(s.fillRect.GetComponent<Image>().fillAmount,v/100f),s.name+" fill at "+v+"%");Require(s.fillRect.offsetMin==Vector2.zero&&s.fillRect.offsetMax==Vector2.zero,s.name+" zero fill offsets");}
   Require(testPanel.Find("HPBar/HPText").GetComponent<TMP_Text>().text==v+" / 100","SetHealth updates current/max text");Require(testPanel.Find("MPBar/MPText").GetComponent<TMP_Text>().text==v+" / 100","SetMana updates current/max text");
   if(GraphicsSettings.currentRenderPipeline!=null){var request=new RenderPipeline.StandardRequest();request.destination=rt;RenderPipeline.SubmitRenderRequest(camera,request);}else camera.Render();
   var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Evidence+"HUD_"+states[testIndex]+".png",tex.EncodeToPNG());var close=new Texture2D(490,125,TextureFormat.RGBA32,false);close.ReadPixels(new Rect(0,955,490,125),0,0);close.Apply();File.WriteAllBytes(Evidence+"HUD_Close_"+states[testIndex]+".png",close.EncodeToPNG());UnityEngine.Object.DestroyImmediate(close);RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(tex);testIndex++;next=EditorApplication.timeSinceStartup+1;
  }else{File.WriteAllText(Evidence+"tests.txt",Log);EditorApplication.update-=Tick;Debug.Log("HUD_CONFIGURATION_AND_TESTS_COMPLETE");Restore();EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.delayCall+=InspectGameView;if(Application.isBatchMode)EditorApplication.Exit(0);}
 }catch(Exception ex){File.WriteAllText(Evidence+"error.txt",ex.ToString());Debug.LogException(ex);EditorApplication.update-=Tick;Restore();if(Application.isBatchMode)EditorApplication.Exit(1);}}
}






