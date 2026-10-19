using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
[InitializeOnLoad] public static class AetheriaHudGameViewCheck {
 const string Out="C:/Users/Vesha/Desktop/codexx/HUD_Verification/";
 static EditorWindow view;static double time;static BindingFlags flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static AetheriaHudGameViewCheck(){EditorApplication.delayCall+=Begin;}
 static void Begin(){if(!File.Exists(Out+"capture.txt")||File.ReadAllText(Out+"capture.txt")!="pending")return;File.WriteAllText(Out+"capture.txt","started");try{
 var asm=typeof(EditorWindow).Assembly;var gt=asm.GetType("UnityEditor.GameView");view=EditorWindow.GetWindow(gt);view.Show();
 var sizesType=asm.GetType("UnityEditor.GameViewSizes");var st=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);var sizes=st.GetProperty("instance",BindingFlags.Static|BindingFlags.Public).GetValue(null);var groupEnum=asm.GetType("UnityEditor.GameViewSizeGroupType");var group=sizesType.GetMethod("GetGroup",flags).Invoke(sizes,new[]{Enum.Parse(groupEnum,"Standalone")});
 var sizeType=asm.GetType("UnityEditor.GameViewSize");var sizeEnum=asm.GetType("UnityEditor.GameViewSizeType");var get=group.GetType().GetMethod("GetGameViewSize",flags);int count=(int)group.GetType().GetMethod("GetTotalCount",flags).Invoke(group,null);int index=-1;
 for(int i=0;i<count;i++){var z=get.Invoke(group,new object[]{i});if((int)sizeType.GetProperty("width",flags).GetValue(z)==1920&&(int)sizeType.GetProperty("height",flags).GetValue(z)==1080){index=i;break;}}
 if(index<0){var ctor=sizeType.GetConstructor(flags,null,new[]{sizeEnum,typeof(int),typeof(int),typeof(string)},null);var z=ctor.Invoke(new object[]{Enum.Parse(sizeEnum,"FixedResolution"),1920,1080,"Aetheria HUD 1920x1080"});group.GetType().GetMethod("AddCustomSize",flags).Invoke(group,new[]{z});index=count;}
 gt.GetProperty("selectedSizeIndex",flags).SetValue(view,index);view.Repaint();EditorApplication.QueuePlayerLoopUpdate();time=EditorApplication.timeSinceStartup+3;EditorApplication.update+=Capture;
 }catch(Exception e){File.WriteAllText(Out+"GameView-error.txt",e.ToString());}}
 static void Capture(){if(EditorApplication.timeSinceStartup<time)return;EditorApplication.update-=Capture;try{
 RenderTexture rt=null;for(var t=view.GetType();t!=null;t=t.BaseType)foreach(var f in t.GetFields(flags|BindingFlags.DeclaredOnly)){if(typeof(RenderTexture).IsAssignableFrom(f.FieldType)){var value=f.GetValue(view) as RenderTexture;if(value&&value.width==1920&&value.height==1080)rt=value;}}
 if(!rt)throw new Exception("GameView render texture not ready at intended resolution");
 var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();if(SystemInfo.graphicsUVStartsAtTop){var pixels=tex.GetPixels();var flip=new Color[pixels.Length];for(int y=0;y<tex.height;y++)Array.Copy(pixels,y*tex.width,flip,(tex.height-1-y)*tex.width,tex.width);tex.SetPixels(flip);tex.Apply();}File.WriteAllBytes(Out+"GameView_1920x1080.png",tex.EncodeToPNG());var region=tex.GetPixels(0,tex.height-160,430,160);UnityEngine.Object.DestroyImmediate(tex);
 tex=new Texture2D(430,160,TextureFormat.RGBA32,false);tex.SetPixels(region);tex.Apply();File.WriteAllBytes(Out+"GameView_HUD_Close.png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=old;
 var panel=UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(r=>r.name=="PlayerInfoPanel");string log="Actual GameView captured: "+rt.width+" x "+rt.height+"\n";int images=0;
 foreach(var g in panel.GetComponentsInChildren<Graphic>(false).Where(g=>g.isActiveAndEnabled)) {var corners=new Vector3[4];g.rectTransform.GetWorldCorners(corners);foreach(var c in corners){var p=panel.InverseTransformPoint(c);if(p.x<-.1f||p.x>panel.rect.width+.1f||p.y>.1f||p.y< -panel.rect.height-.1f)throw new Exception(g.name+" extends outside frame");}images++;}
 log+="PASS all "+images+" active graphic RectTransforms inside frame\n";
 log+="CPBar count="+panel.GetComponentsInChildren<Slider>(true).Count(s=>s.name=="CPBar")+"\n";
 File.WriteAllText(Out+"GameView-verification.txt",log);File.WriteAllText(Out+"capture.txt","complete");Debug.Log(log);
 }catch(Exception e){File.WriteAllText(Out+"GameView-error.txt",e.ToString());}}
}


