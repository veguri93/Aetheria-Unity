using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad] public static class AetheriaLiveEnergyCheck
{
 const string Out="C:/Users/Vesha/Desktop/codexx/HUD_Energy_Live/";
 const string Folder="Assets/Aetheria/UI/Energy/";
 static RectTransform panel;static Image[] fills;static Material[] materials;static int stage,frame;static double next;static float clockBefore;static string log="";
 static string[] names={"HP","MP","CP"};static float[] speeds={.42f,.28f,.35f};static float[] brightness={1.15f,1.1f,1.05f};static float[] opacity={.65f,.62f,.6f};
 static AetheriaLiveEnergyCheck(){EditorApplication.playModeStateChanged+=StateChanged;EditorApplication.delayCall+=Begin;}
 static void Note(string s){log+=s+"\n";File.WriteAllText(Out+"live-verification.txt",log);}
 static void Bind(){var scene=SceneManager.GetActiveScene();if(scene.path!="Assets/_Recovery/0.unity")throw new Exception("Expected actual gameplay scene, found "+scene.path);panel=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RectTransform>(true)).Single(p=>p.name=="PlayerInfoPanel"&&p.parent.name=="PlayerHUD");fills=names.Select(n=>panel.Find(n+"Bar/"+n+"Fill").GetComponent<Image>()).ToArray();materials=names.Select(n=>AssetDatabase.LoadAssetAtPath<Material>(Folder+n+"_Energy.mat")).ToArray();Note("REAL scene="+scene.path+"; PlayMode="+Application.isPlaying+"; HUD instance="+panel.gameObject.name+"; size="+panel.sizeDelta+"; position="+panel.anchoredPosition);}
 static void Begin(){if(!File.Exists(Out+"run.txt")||File.ReadAllText(Out+"run.txt")!="pending")return;File.WriteAllText(Out+"run.txt","started");try{Bind();if(!EditorApplication.isPlaying){var scene=SceneManager.GetActiveScene();var backup=AssetDatabase.GenerateUniqueAssetPath("Assets/_Recovery/HUD-before-live-energy.unity");EditorSceneManager.SaveScene(scene,backup,true);ConfigureReferences();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();SessionState.SetBool("AetheriaLiveEnergy.Run",true);EditorApplication.isPlaying=true;}else{ConfigureReferences();SessionState.SetBool("AetheriaLiveEnergy.Run",true);StartRun();}}catch(Exception e){Fail(e);}}
 static void ConfigureReferences(){
 var clock=panel.GetComponent<AetheriaHudEnergyClock>();Note("Clock before: present="+(clock!=null)+", active="+(clock&&clock.isActiveAndEnabled)+", animationEnabled="+(clock&&clock.animationEnabled)+", speed="+(clock?clock.globalSpeed:0));if(!clock)clock=panel.gameObject.AddComponent<AetheriaHudEnergyClock>();clock.enabled=true;clock.animationEnabled=true;clock.globalSpeed=1;
 var colors=new[]{new Color(1,.38f,.24f,1),new Color(.3f,.88f,1,1),new Color(1,.88f,.42f,1)};
 for(int i=0;i<3;i++){Note(names[i]+" assigned material before="+(fills[i].material?fills[i].material.name:"null")+"; shader="+fills[i].material.shader.name);fills[i].material=materials[i];materials[i].SetColor("_EnergyColor",colors[i]);EditorUtility.SetDirty(materials[i]);}
 }
 static void StateChanged(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("AetheriaLiveEnergy.Run",false))try{Bind();StartRun();}catch(Exception e){Fail(e);}}
 static void Settings(bool diagnostic){for(int i=0;i<3;i++){materials[i].SetFloat("_Speed",diagnostic?.8f:speeds[i]);materials[i].SetFloat("_Brightness",diagnostic?2f:brightness[i]);materials[i].SetFloat("_Opacity",diagnostic?1f:opacity[i]);EditorUtility.SetDirty(materials[i]);fills[i].SetMaterialDirty();}AssetDatabase.SaveAssets();}
 static void StartRun(){stage=0;frame=0;clockBefore=Shader.GetGlobalFloat("_AetheriaUIEnergyTime");Settings(true);Note("Diagnostic boost applied to real live HP/MP/CP, no Slider writes.");next=EditorApplication.timeSinceStartup+3;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
 static void Tick(){if(EditorApplication.timeSinceStartup<next)return;try{
 if(!EditorApplication.isPlaying)throw new Exception("Gameplay stopped before live verification finished");
 if(frame==0){var clock=panel.GetComponent<AetheriaHudEnergyClock>();float t=Shader.GetGlobalFloat("_AetheriaUIEnergyTime");Note((stage==0?"BOOSTED":"MODERATE")+" clock="+t+" previous="+clockBefore+"; active="+clock.isActiveAndEnabled+"; Time.unscaledTime="+Time.unscaledTime+"; Time.timeScale="+Time.timeScale);if(!clock.isActiveAndEnabled||t<=clockBefore)throw new Exception("Live shader clock is not advancing");clockBefore=t;for(int i=0;i<3;i++){if(fills[i].material!=materials[i]||fills[i].materialForRendering.shader.name!="Aetheria/UI/Resource Energy")throw new Exception(names[i]+" live renderer material mismatch");Note(names[i]+" renderer material="+fills[i].materialForRendering.name+"; active="+fills[i].isActiveAndEnabled+"; fill="+fills[i].fillAmount+"; clock count="+UnityEngine.Object.FindObjectsByType<AetheriaHudEnergyClock>().Length);}}
 Capture((stage==0?"Boosted":"Moderate")+"_"+frame.ToString("D2"),frame==0);frame++;next=EditorApplication.timeSinceStartup+.1;
 if(frame==40){if(stage==0){stage=1;frame=0;Settings(false);Note("Restored polished moderate presets with visibly stronger moving contrast.");next=EditorApplication.timeSinceStartup+1;}else{EditorApplication.update-=Tick;SessionState.SetBool("AetheriaLiveEnergy.Run",false);Settings(false);File.WriteAllText(Out+"run.txt","complete");Note("COMPLETE: real gameplay remains running, moderate material assets saved, no layout or Slider values changed.");}}
 }catch(Exception e){EditorApplication.update-=Tick;Settings(false);Fail(e);}}
 static void Capture(string name,bool full){var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;RenderTexture rt=null;for(var t=type;t!=null;t=t.BaseType)foreach(var f in t.GetFields(flags|BindingFlags.DeclaredOnly))if(typeof(RenderTexture).IsAssignableFrom(f.FieldType)){var r=f.GetValue(view) as RenderTexture;if(r&&r.width==1920&&r.height==1080)rt=r;}if(!rt)throw new Exception("Actual Game view not 1920x1080");var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();if(SystemInfo.graphicsUVStartsAtTop){var p=tex.GetPixels();var flip=new Color[p.Length];for(int y=0;y<tex.height;y++)Array.Copy(p,y*tex.width,flip,(tex.height-1-y)*tex.width,tex.width);tex.SetPixels(flip);tex.Apply();}if(full)File.WriteAllBytes(Out+name+"_RealGameView.png",tex.EncodeToPNG());var close=new Texture2D(430,160,TextureFormat.RGBA32,false);close.SetPixels(tex.GetPixels(0,920,430,160));close.Apply();File.WriteAllBytes(Out+name+".png",close.EncodeToPNG());UnityEngine.Object.DestroyImmediate(close);UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=old;}
 static void Fail(Exception e){File.WriteAllText(Out+"error.txt",e.ToString());Debug.LogException(e);}
}
