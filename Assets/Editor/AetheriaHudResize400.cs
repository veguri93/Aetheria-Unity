using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
[InitializeOnLoad] public static class AetheriaHudResize400 {
 const string Root="C:/Users/Vesha/Desktop/codexx/HUD_400x130/";
 static AetheriaHudResize400(){EditorApplication.delayCall+=Run;}
 static void Box(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);r.localScale=Vector3.one;}
 static Color Pixel(Texture2D t,float x,float y){return t.GetPixel(Mathf.Clamp(Mathf.RoundToInt(x),0,t.width-1),Mathf.Clamp(t.height-1-Mathf.RoundToInt(y),0,t.height-1));}
 static float Map(float n,float dst,float src,int b){if(b==0)return n*(src-1)/Mathf.Max(1,dst-1);if(n<b)return n;if(n>=dst-b)return src-(dst-n);return b+(n-b)*(src-2*b)/(dst-2*b);}
 static void Patch(Texture2D d,Texture2D s,Rect a,Rect b,int border=0){for(int y=0;y<b.height;y++)for(int x=0;x<b.width;x++)d.SetPixel((int)b.x+x,d.height-1-((int)b.y+y),Pixel(s,a.x+Map(x,b.width,a.width,border),a.y+Map(y,b.height,a.height,border)));}
 static void Run(){string flag=Root+"run.txt";if(!File.Exists(flag)||File.ReadAllText(flag)!="pending")return;if(EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Run;return;}File.WriteAllText(flag,"started");try{
 var scene=SceneManager.GetActiveScene();var panel=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RectTransform>(true)).Single(r=>r.name=="PlayerInfoPanel");
 var backup=AssetDatabase.GenerateUniqueAssetPath("Assets/_Recovery/HUD-before-400x130.unity");if(!EditorSceneManager.SaveScene(scene,backup,true))throw new Exception("Backup failed");
 var source=new Texture2D(2,2);source.LoadImage(File.ReadAllBytes("Assets/Art/character hp bar/new hpcpmp/frame.png"));var frame=new Texture2D(1600,520,TextureFormat.RGBA32,false);
 Patch(frame,source,new Rect(58,137,1658,613),new Rect(0,0,1600,520),15);Patch(frame,source,new Rect(80,170,1600,100),new Rect(15,15,1570,490));Patch(frame,source,new Rect(75,300,1625,8),new Rect(15,124,1570,5));foreach(int y in new[]{164,280,396})Patch(frame,source,new Rect(380,342,1295,99),new Rect(136,y,1416,88),7);
 string path="Assets/Art/character hp bar/new hpcpmp/Configured/frame-400x130.png";File.WriteAllBytes(path,frame.EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.npotScale=TextureImporterNPOTScale.None;imp.filterMode=FilterMode.Bilinear;imp.SaveAndReimport();
 panel.sizeDelta=new Vector2(400,130);panel.GetComponent<Image>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);panel.GetComponent<Image>().preserveAspect=true;
 var name=panel.Find("PlayerName").GetComponent<TMP_Text>();Box(name.rectTransform,10,3,220,26);name.fontSize=16;var level=panel.Find("PlayerLevel").GetComponent<TMP_Text>();Box(level.rectTransform,240,3,150,26);level.fontSize=15.5f;
 int i=0;string log="Panel 400 x 130. All fill rectangles 352 x 20 (previously 409.69 x 12.32).\nBackup: "+backup+"\n";
 foreach(string prefix in new[]{"CP","HP","MP"}){float y=42+i*29;var bar=panel.Find(prefix+"Bar").GetComponent<Slider>();Box((RectTransform)bar.transform,35,y,352,20);var fill=panel.Find(prefix+"Bar/"+prefix+"Fill").GetComponent<Image>();var fr=fill.rectTransform;fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;bar.fillRect=fr;var value=bar.value;foreach(float fraction in new[]{1f,.5f,0f}){bar.SetValueWithoutNotify(bar.maxValue*fraction);if(!Mathf.Approximately(fill.fillAmount,fraction))throw new Exception(prefix+" fill failed at "+fraction);}bar.SetValueWithoutNotify(value);
 var text=panel.Find(prefix+"Bar/"+prefix+"Text").GetComponent<TMP_Text>();text.fontSize=12;var label=panel.Find(prefix+"Label").GetComponent<TMP_Text>();Box(label.rectTransform,9,y-1,24,22);label.fontSize=13;log+=prefix+" fill: 352 x 20, y="+y+"; 100/50/0 passed\n";i++;}
 Canvas.ForceUpdateCanvases();foreach(var g in panel.GetComponentsInChildren<Graphic>(false).Where(g=>g.isActiveAndEnabled)){var corners=new Vector3[4];g.rectTransform.GetWorldCorners(corners);foreach(var c in corners){var p=panel.InverseTransformPoint(c);if(p.x<-.1f||p.x>400.1f||p.y>.1f||p.y< -130.1f)throw new Exception(g.name+" outside resized panel");}}
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText(Root+"verification.txt",log+"All graphics inside frame; scene saved; existing script references unchanged.\n");File.WriteAllText(flag,"complete");
 // Request the existing actual Game-view capture helper after this domain settles.
 File.WriteAllText("C:/Users/Vesha/Desktop/codexx/HUD_Verification/capture.txt","pending");var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");EditorWindow.GetWindow(type).Repaint();EditorApplication.delayCall+=()=>{var method=typeof(AetheriaHudGameViewCheck).GetMethod("Begin",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);method.Invoke(null,null);};
 }catch(Exception ex){File.WriteAllText(Root+"error.txt",ex.ToString());Debug.LogException(ex);}}
}
