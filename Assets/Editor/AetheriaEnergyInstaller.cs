using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad] public static class AetheriaEnergyInstaller
{
 const string Out="C:/Users/Vesha/Desktop/codexx/HUD_Energy/";
 const string AssetsRoot="Assets/Aetheria/UI/Energy/";
 const string PreviewScene="Assets/Editor/AetheriaEnergyPreview.unity";
 static string report="";static int stage;static double next;static RectTransform panel;static Slider[] bars;static Material[] materials;static float pausedTime,pausedClock;static GameObject maskContainer;static bool editCapture;
 static string[] stages={"HP_Only_A","HP_Only_B","Full_A","Full_B","Half_A","Half_B","Thirty_A","Thirty_B","Empty_A","Empty_B","Paused_A","Paused_B","RectMask_A","RectMask_B","Stencil_A","Stencil_B"};
 static AetheriaEnergyInstaller(){EditorApplication.playModeStateChanged+=PlayChanged;EditorApplication.delayCall+=Install;}
 static void Check(bool value,string message){if(!value)throw new Exception(message);report+="PASS "+message+"\n";}
 static RectTransform FindPanel(){return UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(x=>(x.name=="PlayerInfoPanel"||x.name=="HUD_PlayModeCopy"));}
 static void Install(){if(!File.Exists(Out+"install.txt")||File.ReadAllText(Out+"install.txt")!="pending")return;if(EditorApplication.isPlayingOrWillChangePlaymode){File.WriteAllText(Out+"status.txt","Waiting for Unity to leave Play Mode before editing the scene.");return;}File.WriteAllText(Out+"install.txt","started");try{
 var scene=SceneManager.GetActiveScene();panel=FindPanel();Check(panel.sizeDelta==new Vector2(400,130),"existing 400x130 HUD found");SessionState.SetString("AetheriaEnergy.SourceScene",scene.path);var backup=AssetDatabase.GenerateUniqueAssetPath("Assets/_Recovery/HUD-before-energy.unity");Check(EditorSceneManager.SaveScene(scene,backup,true),"active scene backup saved: "+backup);
 var rects=panel.GetComponentsInChildren<RectTransform>(true).Select(r=>new {r,r.anchorMin,r.anchorMax,r.pivot,r.anchoredPosition,r.sizeDelta,r.localScale}).ToArray();var sprites=panel.GetComponentsInChildren<Image>(true).Select(i=>new {i,s=i.sprite}).ToArray();
 var shader=AssetDatabase.LoadAssetAtPath<Shader>(AssetsRoot+"ResourceEnergy.shader");Check(shader&&shader.isSupported,"UI energy shader supported by current renderer");Check(!ShaderUtil.ShaderHasError(shader),"shader compiled without errors");
 // Configure HP first; the same shader and clock are then reused for MP and CP.
 string[] names={"HP","MP","CP"};Color[] colors={new Color(1,.11f,.07f,1),new Color(.08f,.78f,1,1),new Color(1,.74f,.15f,1)};float[] speeds={.16f,.09f,.12f};float[] brightness={.9f,1f,.85f};float[] opacity={.34f,.32f,.26f};
 for(int i=0;i<3;i++){string path=AssetsRoot+names[i]+"_Energy.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}material.shader=shader;material.SetColor("_EnergyColor",colors[i]);material.SetFloat("_Speed",speeds[i]);material.SetFloat("_Brightness",brightness[i]);material.SetFloat("_Opacity",opacity[i]);material.SetFloat("_Style",i);material.SetFloat("_Phase",i*.73f);EditorUtility.SetDirty(material);var fill=panel.Find(names[i]+"Bar/"+names[i]+"Fill").GetComponent<Image>();Check(fill.type==Image.Type.Filled&&fill.fillMethod==Image.FillMethod.Horizontal,names[i]+" existing Slider geometry provides fill clipping");fill.material=material;}
 if(!panel.GetComponent<AetheriaHudEnergyClock>())panel.gameObject.AddComponent<AetheriaHudEnergyClock>();
 foreach(var r in rects)Check(r.r.anchorMin==r.anchorMin&&r.r.anchorMax==r.anchorMax&&r.r.pivot==r.pivot&&r.r.anchoredPosition==r.anchoredPosition&&r.r.sizeDelta==r.sizeDelta&&r.r.localScale==r.localScale,"layout preserved: "+r.r.name);
 foreach(var x in sprites)Check(x.i.sprite==x.s,"original sprite preserved: "+x.i.name);
 EditorSceneManager.MarkSceneDirty(scene);Check(EditorSceneManager.SaveScene(scene),"configured production scene saved");AssetDatabase.SaveAssets();File.WriteAllText(Out+"installation.txt",report);
 // A copy of this exact panel is tested in Play Mode without launching game networking.
 Directory.CreateDirectory("Assets/Editor/AetheriaEnergyTest");string prefab="Assets/Editor/AetheriaEnergyTest/HUD_PlayModeCopy.prefab";PrefabUtility.SaveAsPrefabAsset(panel.gameObject,prefab);
 var preview=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var cv=new GameObject("Energy_PlayModeTestCanvas",typeof(Canvas),typeof(CanvasScaler));cv.layer=5;var canvas=cv.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=cv.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;var testPanel=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefab),cv.transform);testPanel.name="PlayerInfoPanel";
 var camera=new GameObject("Energy_PlayModeTestCamera",typeof(Camera)).GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.075f,.08f,.09f);camera.cullingMask=0;EditorSceneManager.SaveScene(preview,PreviewScene);
 SessionState.SetBool("AetheriaEnergy.Testing",true);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.isPlaying=true;
 }catch(Exception ex){Fail(ex);}}
 static void PlayChanged(PlayModeStateChange state){if(!SessionState.GetBool("AetheriaEnergy.Testing",false))return;if(state==PlayModeStateChange.EnteredPlayMode){try{panel=FindPanel();bars=new[]{"HP","MP","CP"}.Select(p=>panel.Find(p+"Bar").GetComponent<Slider>()).ToArray();materials=bars.Select(s=>s.fillRect.GetComponent<Image>().material).ToArray();stage=0;report="Play Mode tests use an isolated copy of the configured production HUD. No networking or production Slider values are changed.\n";Prepare();EditorApplication.update+=Tick;}catch(Exception ex){Fail(ex);}}else if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("AetheriaEnergy.Testing",false);var path=SessionState.GetString("AetheriaEnergy.SourceScene","");if(!string.IsNullOrEmpty(path))EditorSceneManager.OpenScene(path,OpenSceneMode.Single);editCapture=true;next=EditorApplication.timeSinceStartup+3;EditorApplication.update+=FinalCapture;}}
 static void Prepare(){
  int percent=stage<4?100:stage<6?50:stage<8?30:stage<10?0:100;
  for(int i=0;i<3;i++){bars[i].minValue=0;bars[i].maxValue=100;bars[i].SetValueWithoutNotify(percent);bars[i].fillRect.GetComponent<Image>().material=stage<2&&i>0?null:materials[i];}
  if(stage==10){Time.timeScale=0;pausedTime=Time.time;pausedClock=Shader.GetGlobalFloat("_AetheriaUIEnergyTime");}
  if(stage==12){Time.timeScale=1;AddMask(false);}if(stage==14){UnityEngine.Object.DestroyImmediate(maskContainer.GetComponent<RectMask2D>());var image=maskContainer.AddComponent<Image>();image.color=Color.white;var mask=maskContainer.AddComponent<Mask>();mask.showMaskGraphic=false;Canvas.ForceUpdateCanvases();}
  Canvas.ForceUpdateCanvases();next=EditorApplication.timeSinceStartup+.9;
 }
 static void AddMask(bool stencil){var parent=panel.parent;maskContainer=new GameObject("TestOnly_Clip",typeof(RectTransform),typeof(RectMask2D));maskContainer.layer=5;maskContainer.transform.SetParent(parent,false);var r=(RectTransform)maskContainer.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=panel.anchoredPosition;r.sizeDelta=new Vector2(200,130);panel.SetParent(r,false);panel.anchoredPosition=Vector2.zero;}
 static void Tick(){if(EditorApplication.timeSinceStartup<next)return;try{
  foreach(var b in bars){var image=b.fillRect.GetComponent<Image>();Check(Mathf.Approximately(image.fillAmount,b.normalizedValue),stages[stage]+" Slider clips "+b.name);}
  if(stage==11){Check(Mathf.Approximately(Time.time,pausedTime),"scaled gameplay time remained paused");Check(Shader.GetGlobalFloat("_AetheriaUIEnergyTime")>pausedClock+.5f,"energy clock advanced on unscaled time while paused");}
  if(stage==14)Check(bars[0].fillRect.GetComponent<Image>().materialForRendering.GetInt("_Stencil")!=0,"stencil masking applied to energy material");
  Capture(stages[stage]);stage++;if(stage==stages.Length){File.WriteAllText(Out+"playmode-tests.txt",report);File.WriteAllText(Out+"install.txt","complete");EditorApplication.update-=Tick;Time.timeScale=1;EditorApplication.isPlaying=false;}else Prepare();
 }catch(Exception ex){EditorApplication.update-=Tick;Time.timeScale=1;Fail(ex);if(EditorApplication.isPlaying)EditorApplication.isPlaying=false;}}
 static void Capture(string name){
 var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);RenderTexture rt=null;var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 for(var t=type;t!=null;t=t.BaseType)foreach(var f in t.GetFields(flags|BindingFlags.DeclaredOnly))if(typeof(RenderTexture).IsAssignableFrom(f.FieldType)){var v=f.GetValue(view) as RenderTexture;if(v&&v.width==1920&&v.height==1080)rt=v;}
 if(!rt)throw new Exception("Expected 1920x1080 Game view render texture");var old=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();if(SystemInfo.graphicsUVStartsAtTop){var p=texture.GetPixels();var flip=new Color[p.Length];for(int y=0;y<texture.height;y++)Array.Copy(p,y*texture.width,flip,(texture.height-1-y)*texture.width,texture.width);texture.SetPixels(flip);texture.Apply();}var close=new Texture2D(430,160,TextureFormat.RGBA32,false);close.SetPixels(texture.GetPixels(0,920,430,160));close.Apply();File.WriteAllBytes(Out+name+".png",close.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(close);RenderTexture.active=old;
 }
 static void FinalCapture(){if(EditorApplication.timeSinceStartup<next)return;EditorApplication.update-=FinalCapture;try{Capture("Final_Production_GameView");File.WriteAllText(Out+"status.txt","Complete: production scene restored, layout unchanged, Play Mode tests finished.");}catch(Exception ex){Fail(ex);}}
 static void Fail(Exception ex){File.WriteAllText(Out+"error.txt",ex.ToString());Debug.LogException(ex);}
}

