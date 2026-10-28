using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MissChatVerification
{
    const string Folder="C:/Users/Vesha/Desktop/codexx/MissChat/";
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic;
    static MissChatVerification(){EditorApplication.delayCall+=AutoRun;}
    static void AutoRun()
    {
        if(!File.Exists(Folder+"run.request"))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=AutoRun;return;}
        File.Delete(Folder+"run.request");Run();
    }
    static T Get<T>(object o,string n)=>(T)o.GetType().GetField(n,Fields).GetValue(o);
    static void Set(object o,string n,object v)=>o.GetType().GetField(n,Fields).SetValue(o,v);
    static void Check(bool ok,string label){if(!ok)throw new Exception(label);File.AppendAllText(Folder+"unity-tests.txt","PASS "+label+"\n");}
    [MenuItem("Tools/Aetheria/Verify chat-only MISS")]
    public static void Run()
    {
        File.WriteAllText(Folder+"unity-tests.txt","Unity "+Application.unityVersion+": actual SystemMessage handler and ChatManager with existing message prefab, temporary chat content.\n");
        if(EditorApplication.isPlaying){File.AppendAllText(Folder+"unity-tests.txt","BLOCKED: Leave Play Mode first.\n");return;}
        var previous=ChatManager.Instance;GameObject fixture=null;
        try
        {
            var source=UnityEngine.Object.FindAnyObjectByType<ChatManager>(FindObjectsInactive.Include);
            Check(source,"Existing chat manager present");var prefab=Get<TMP_Text>(source,"_combatMessagePrefab");Check(prefab,"Existing combat/system message prefab assigned");
            fixture=new GameObject("Temporary MISS chat verification",typeof(RectTransform));
            var chat=fixture.AddComponent<ChatManager>();Set(chat,"_combatContent",fixture.GetComponent<RectTransform>());Set(chat,"_combatMessagePrefab",prefab);Set(chat,"_systemColor",Get<Color>(source,"_systemColor"));
            typeof(ChatManager).GetProperty("Instance").SetValue(null,chat);
            var message=SystemMessageTable.Get(56);Check(message!=null&&message.Id==56&&message.Name=="YOUR_ATTACK_MISSED"&&message.Text=="Your attack missed.","Unity message 56 exact ID/key/text");
            int worldLabels=UnityEngine.Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include).Length;
            int count=0;
            foreach(var line in File.ReadAllLines(Folder+"wire-outcomes.tsv"))
            {
                var parts=line.Split('\t');if(parts[0]!="25")continue;
                int before=fixture.transform.childCount;
                SystemMessagePacketHandler.Handle(new ClientPacket(25,Encoding.UTF8.GetBytes(parts[1])));
                Check(fixture.transform.childCount==before+1,"One chat entry for wire MISS "+(++count));
                var text=fixture.transform.GetChild(before).GetComponent<TMP_Text>();
                Check(text.text=="Your attack missed."&&text.color==Get<Color>(source,"_systemColor"),"Normal system text and color");
            }
            Check(count>0,"Real server MISS packets exercised");
            Check(UnityEngine.Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include).Length==worldLabels,"No floating/world-space text created");
            SystemMessagePacketHandler.Handle(new ClientPacket(25,Encoding.UTF8.GetBytes("55|Test Sword")));
            Check(fixture.transform.GetChild(count).GetComponent<TMP_Text>().text=="Test Sword disappeared from your inventory.","Existing parameterized messages still render");
            int finalCount=fixture.transform.childCount;SystemMessagePacketHandler.Handle(new ClientPacket(25,Encoding.UTF8.GetBytes("invalid")));
            Check(fixture.transform.childCount==finalCount,"Malformed ID creates no chat entry");
            File.AppendAllText(Folder+"unity-tests.txt","COMPLETE\n");
        }
        catch(Exception e){File.AppendAllText(Folder+"unity-tests.txt","FAIL "+e+"\n");Debug.LogException(e);}
        finally{typeof(ChatManager).GetProperty("Instance").SetValue(null,previous);if(fixture)UnityEngine.Object.DestroyImmediate(fixture);}
    }
}
