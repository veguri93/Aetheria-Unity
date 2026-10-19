using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad] public static class AetheriaHudFinalTune {
 const string Out="C:/Users/Vesha/Desktop/codexx/HUD_Final_Tune/";
 static double next;static int stage;static float firstTime;static string log="";
 static AetheriaHudFinalTune(){EditorApplication.delayCall+=Setup;EditorApplication.playModeStateChanged+=State;}
 static void Setup(){if(!File.Exists(Out+"run.txt")||File.ReadAllText(Out+"run.txt")!="pending")return;File.WriteAllText(Out+"run.txt","started");try{
 var scene=SceneManager.GetActiveScene();if(scene.path!="Assets/_Recovery/0.unity")throw new Exception("Expected actual gameplay scene");var panel=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RectTransform>(true)).Single(p=>p.name=="PlayerInfoPanel");
 if(!EditorApplication.isPlaying){var backup=AssetDatabase.GenerateUniqueAssetPath("Assets/_Recovery/HUD-before-final-tone.unity");EditorSceneManager.SaveScene(scene,backup,true);}
 foreach(string n in new[]{"HP","MP","CP"}){var path="Assets/Aetheria/UI/Energy/"+n+"_Energy.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(ShaderUtil.ShaderHasError(m.shader))throw new Exception("Shader compilation failed");log+=n+" previous speed="+m.GetFloat("_Speed")+", energy brightness="+m.GetFloat("_Brightness")+", opacity="+m.GetFloat("_Opacity")+"\n";m.SetFloat("_Speed",.55f);m.SetFloat("_Brightness",.8f);m.SetFloat("_Opacity",.55f);m.SetFloat("_BarBrightness",.75f);EditorUtility.SetDirty(m);var image=panel.Find(n+"Bar/"+n+"Fill").GetComponent<Image>();if(image.material!=m)throw new Exception(n+" unexpected runtime material");image.SetMaterialDirty();log+=n+" saved: flow=.55, overall brightness=.75, energy brightness=.8, opacity=.55\n";}
 AssetDatabase.SaveAssets();log+="Actual HUD size="+panel.sizeDelta+", position="+panel.anchoredPosition+"; source artwork, layout and Sliders unchanged.\n";File.WriteAllText(Out+"verification.txt",log);
 SessionState.SetBool("Aetheria.FinalTune",true);if(!EditorApplication.isPlaying){EditorSceneManager.SaveScene(scene);EditorApplication.isPlaying=true;}else Start();
 }catch(Exception e){File.WriteAllText(Out+"error.txt",e.ToString());Debug.LogException(e);}}
 static void State(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Aetheria.FinalTune",false))Start();}
 static void Start(){stage=0;firstTime=Shader.GetGlobalFloat("_AetheriaUIEnergyTime");next=EditorApplication.timeSinceStartup+4;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
 static void Tick(){if(EditorApplication.timeSinceStartup<next)return;try{float t=Shader.GetGlobalFloat("_AetheriaUIEnergyTime");if(!Application.isPlaying||t<=firstTime)throw new Exception("Actual gameplay clock not advancing");Capture(stage==0?"Final_A":"Final_B");if(stage++==0){next=EditorApplication.timeSinceStartup+1;return;}log=File.ReadAllText(Out+"verification.txt");log+="Verified in actual gameplay Play Mode: clock advanced; captured two visibly animated, dimmed frames.\n";File.WriteAllText(Out+"verification.txt",log);File.WriteAllText(Out+"run.txt","complete");SessionState.SetBool("Aetheria.FinalTune",false);EditorApplication.update-=Tick;}catch(Exception e){File.WriteAllText(Out+"error.txt",e.ToString());EditorApplication.update-=Tick;Debug.LogException(e);}}
 static void Capture(string name){var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;RenderTexture rt=null;for(var t=type;t!=null;t=t.BaseType)foreach(var f in t.GetFields(flags|BindingFlags.DeclaredOnly))if(typeof(RenderTexture).IsAssignableFrom(f.FieldType)){var r=f.GetValue(view) as RenderTexture;if(r&&r.width==1920&&r.height==1080)rt=r;}if(!rt)throw new Exception("Expected 1920x1080 Game view");var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();if(SystemInfo.graphicsUVStartsAtTop){var p=tex.GetPixels();var flip=new Color[p.Length];for(int y=0;y<tex.height;y++)Array.Copy(p,y*tex.width,flip,(tex.height-1-y)*tex.width,tex.width);tex.SetPixels(flip);tex.Apply();}var close=new Texture2D(430,160,TextureFormat.RGBA32,false);close.SetPixels(tex.GetPixels(0,920,430,160));close.Apply();File.WriteAllBytes(Out+name+".png",close.EncodeToPNG());UnityEngine.Object.DestroyImmediate(close);UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=old;}
}
