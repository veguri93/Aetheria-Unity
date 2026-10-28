using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class AccuracyEvasionVerification
{
    const string Folder="C:/Users/Vesha/Desktop/codexx/AccuracyEvasion/";
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic;
    static AccuracyEvasionVerification(){EditorApplication.delayCall+=AutoRun;}
    static void AutoRun()
    {
        if(!File.Exists(Folder+"run.request"))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=AutoRun;return;}
        File.Delete(Folder+"run.request");Run();
    }
    static T Field<T>(object obj,string field)=>(T)obj.GetType().GetField(field,Fields).GetValue(obj);
    static void Call(object obj,string name)=>obj.GetType().GetMethod(name,Fields).Invoke(obj,null);
    static void Check(bool ok,string text){if(!ok)throw new Exception(text);File.AppendAllText(Folder+"unity-tests.txt","PASS "+text+"\n");}
    static void Packet(string text)=>CharacterStatsPacketHandler.Handle(new ClientPacket(0,Encoding.UTF8.GetBytes(text)));
    [MenuItem("Tools/Aetheria/Verify Accuracy and Evasion")]
    public static void Run()
    {
        File.WriteAllText(Folder+"unity-tests.txt","Unity "+Application.unityVersion+" editor packet/UI integration checks; scene changes restored without saving.\n");
        if(EditorApplication.isPlaying){File.AppendAllText(Folder+"unity-tests.txt","BLOCKED: Leave Play Mode first.\n");return;}
        var stats=UnityEngine.Object.FindAnyObjectByType<PlayerStatsUI>(FindObjectsInactive.Include);
        var info=UnityEngine.Object.FindAnyObjectByType<CharacterInfoUI>(FindObjectsInactive.Include);
        if(!stats||!info){File.AppendAllText(Folder+"unity-tests.txt","BLOCKED: Open gameplay scene.\n");return;}
        var scalarFields=typeof(PlayerStatsUI).GetFields(Fields).Where(f=>f.Name.EndsWith("k__BackingField")).ToDictionary(f=>f,f=>f.GetValue(stats));
        var texts=stats.transform.root.GetComponentsInChildren<TMP_Text>(true).ToDictionary(t=>t,t=>t.text);
        var sliders=stats.transform.root.GetComponentsInChildren<Slider>(true).ToDictionary(s=>s,s=>(s.value,s.maxValue));
        var culture=CultureInfo.CurrentCulture;var oldPlayer=LocalPlayer.Instance;
        var go=new GameObject("Temporary packet combat-value fixture");var local=go.AddComponent<LocalPlayer>();var combat=go.AddComponent<PlayerCombat>();
        typeof(LocalPlayer).GetProperty("Instance").SetValue(null,local);
        bool subscribed=false;
        try
        {
            Call(info,"OnEnable");subscribed=true;
            var accuracy=Field<TMP_Text>(info,"accuracyValue");var evasion=Field<TMP_Text>(info,"evasionValue");
            Check(accuracy&&evasion,"Existing window has both value bindings");
            Check(info.GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text=="Accuracy")&&info.GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text=="Evasion"),"Existing Accuracy/Evasion row labels retained");
            var panel=(RectTransform)info.transform;var size=panel.sizeDelta;var position=panel.anchoredPosition;var font=accuracy.font;var fontSize=accuracy.fontSize;
            var snapshots=File.ReadAllLines(Folder+"server-packets.tsv");
            foreach(var name in new[]{"en-US","fr-FR"})
            {
                CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(name);
                foreach(var line in snapshots)
                {
                    var item=line.Split('\t');var values=item[1].Split('|');Packet(item[1]);
                    double expectedAccuracy=double.Parse(values[18],CultureInfo.InvariantCulture),expectedEvasion=double.Parse(values[19],CultureInfo.InvariantCulture);
                    Check(stats.Accuracy==expectedAccuracy&&stats.Evasion==expectedEvasion,item[0]+" parses server ratings under "+name);
                    Check(accuracy.text==expectedAccuracy.ToString("F1",CultureInfo.InvariantCulture)&&evasion.text==expectedEvasion.ToString("F1",CultureInfo.InvariantCulture),item[0]+" existing rows refresh with one decimal under "+name);
                    Check(Math.Abs(Field<float>(combat,"_attackRange")-float.Parse(values[16],CultureInfo.InvariantCulture))<.001f&&Math.Abs(Field<float>(combat,"_attackDelay")-int.Parse(values[17])/1000f)<.001f,"20-field snapshot retains existing attack range and interval");
                }
            }
            var full=snapshots[0].Split('\t')[1];var fields=full.Split('|');
            foreach(int length in new[]{7,9,16,17,18})
            {
                Packet(full);Packet(string.Join("|",fields.Take(length)));
                Check(stats.HasStats&&stats.MaxHp==int.Parse(fields[0]),"Legacy "+length+" fields accepted");
                Check(!stats.Accuracy.HasValue&&!stats.Evasion.HasValue&&accuracy.text=="—"&&evasion.text=="—","Legacy "+length+" clears stale ratings to unknown");
            }
            foreach(string invalid in new[]{"NaN","Infinity","-Infinity","-1","abc","72,5","1e9999",""})
            {
                foreach(int index in new[]{18,19})
                {
                    Packet(full);var broken=(string[])fields.Clone();broken[index]=invalid;broken[0]="9999";broken[17]="1234";Packet(string.Join("|",broken));
                    Check(stats.MaxHp==int.Parse(fields[0])&&stats.Accuracy==double.Parse(fields[18],CultureInfo.InvariantCulture)&&Math.Abs(Field<float>(combat,"_attackDelay")-int.Parse(fields[17])/1000f)<.001f,"Reject invalid rating "+index+" '"+invalid+"' before UI/combat mutation");
                }
            }
            Packet(full);Packet(string.Join("|",fields.Take(19)));Check(stats.Accuracy==double.Parse(fields[18],CultureInfo.InvariantCulture),"Incomplete 19-field extension rejected");
            var missingAttributes=(string[])fields.Clone();for(int i=9;i<13;i++)missingAttributes[i]="—";Packet(string.Join("|",missingAttributes));Check(stats.Might==null&&stats.Accuracy.HasValue,"Unknown legacy attributes coexist with authoritative ratings");
            var zero=(string[])fields.Clone();zero[18]=zero[19]="0";Packet(string.Join("|",zero));Check(accuracy.text=="0.0"&&evasion.text=="0.0","Zero ratings display one decimal");
            Call(info,"OnDisable");subscribed=false;Packet(snapshots[1].Split('\t')[1]);Call(info,"OnEnable");subscribed=true;
            Check(accuracy.text==stats.Accuracy.Value.ToString("F1",CultureInfo.InvariantCulture),"Reopened window displays latest snapshot received while unsubscribed");
            Check(panel.sizeDelta==size&&panel.anchoredPosition==position&&accuracy.font==font&&accuracy.fontSize==fontSize,"Existing layout, position and typography unchanged");
            Check(Field<TMP_Text>(info,"criticalRateValue").text=="—","Unrelated unavailable stats remain unknown");
            File.AppendAllText(Folder+"unity-tests.txt","COMPLETE\n");
        }
        catch(Exception e){File.AppendAllText(Folder+"unity-tests.txt","FAIL "+e+"\n");Debug.LogException(e);}
        finally
        {
            if(subscribed)Call(info,"OnDisable");
            foreach(var pair in scalarFields)pair.Key.SetValue(stats,pair.Value);
            foreach(var pair in sliders){pair.Key.maxValue=pair.Value.maxValue;pair.Key.SetValueWithoutNotify(pair.Value.value);}
            foreach(var pair in texts)pair.Key.text=pair.Value;
            typeof(LocalPlayer).GetProperty("Instance").SetValue(null,oldPlayer);UnityEngine.Object.DestroyImmediate(go);CultureInfo.CurrentCulture=culture;
        }
    }
}
