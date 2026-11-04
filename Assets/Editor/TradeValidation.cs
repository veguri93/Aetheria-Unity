#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class TradeValidation
{
    [MenuItem("Tools/Aetheia/Trade/Build Client")]
    public static void BuildClient()
    {
        string output = Path.GetFullPath("Builds/TradeClient/Aetheia.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = output, target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development });
        string result = $"Trade client build: {report.summary.result}; warnings={report.summary.totalWarnings}; errors={report.summary.totalErrors}; output={output}";
        File.WriteAllText("Builds/TradeClient/build-result.txt",result);
        if (report.summary.result != BuildResult.Succeeded) Debug.LogError(result); else Debug.Log(result);
    }

    [MenuItem("Tools/Aetheia/Trade/Preview UI (Play Mode)")]
    public static void Preview()
    {
        if (!EditorApplication.isPlaying || (GameServerConnection.Instance && GameServerConnection.Instance.IsConnected))
        { Debug.LogWarning("Trade preview requires Play Mode with no server connection."); return; }
        ClientInventory.Clear();
        ClientInventory.Add(new ClientItemInstance(12345,2003,50,"None",stackable:true,droppable:true));
        string id=Guid.NewGuid().ToString();
        TradeWindowUI.Handle(new ClientPacket(94,Encoding.UTF8.GetBytes(id+"|999|Preview partner|1")));
        TradeWindowUI.Handle(new ClientPacket(97,Encoding.UTF8.GetBytes(id+"|1|12345,5,2003,0|54321,2,1002,3")));
        TradeWindowUI.Handle(new ClientPacket(99,Encoding.UTF8.GetBytes(id+"|1|0|1")));
        TradeWindowUI.Instance.enabled = false; // Keep the disconnected preview visible until Play Mode stops.
        Debug.Log("TRADE_UI_PREVIEW: own and remote item rows, quantity, enchant, revision and confirmation populated. Stop Play Mode to discard preview data.");
    }
    [MenuItem("Tools/Aetheia/Trade/Run UI Regression (Play Mode)")]
    public static void Regression()
    {
        if (!EditorApplication.isPlaying || (GameServerConnection.Instance && GameServerConnection.Instance.IsConnected))
        { Debug.LogWarning("Trade regression requires disconnected Play Mode."); return; }
        TradeWindowUI.EnsureCreated();
        var view = TradeWindowUI.Instance;
        view.enabled = false;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object Get(string name) => typeof(TradeWindowUI).GetField(name,flags).GetValue(view);
        void Set(string name, object value) => typeof(TradeWindowUI).GetField(name,flags).SetValue(view,value);
        void Packet(ushort type,string data) => TradeWindowUI.Handle(new ClientPacket(type,Encoding.UTF8.GetBytes(data)));
        bool Visible(string name) => ((RectTransform)Get(name)).gameObject.activeSelf;
        int passed = 0;
        void Check(bool ok,string message) { if(!ok) throw new Exception("Trade UI regression: "+message); passed++; }
        view.Disconnected();
        Guid id=Guid.NewGuid();
        Packet(91,id+"|999|Partner|0");
        Check(!Visible("panel") && Visible("invitation"),"sender sees only small waiting popup");
        Packet(101,id.ToString());
        id=Guid.NewGuid(); Packet(91,id+"|999|Partner|1");
        Check(!Visible("panel") && Visible("invitation"),"recipient sees invitation only");
        Check(((TMPro.TMP_Text)Get("invitationText")).text=="Partner wants to trade with you.","invitation name text");
        Check(((UnityEngine.UI.Button)Get("accept")).gameObject.activeSelf && ((UnityEngine.UI.Button)Get("reject")).gameObject.activeSelf,"accept reject controls present");
        Check(((UnityEngine.UI.Slider)Get("invitationTimer")).fillRect.sizeDelta == Vector2.zero,"countdown fill stays inside popup");
        float deadline=(float)Get("invitationExpiresAt"); Packet(91,id+"|999|Partner|1");
        Check((float)Get("invitationExpiresAt")==deadline,"duplicate invitation does not restart countdown");
        Set("pending",true); typeof(TradeWindowUI).GetMethod("ExpireLocalInvitation",flags).Invoke(view,null);
        Check((Guid)Get("tradeId")==Guid.Empty && !(bool)Get("pending") && !Visible("invitation"),"local expiry clears ID and pending state");
        Packet(91,id+"|999|Partner|1"); Check(!Visible("invitation"),"late duplicate invite ignored");
        Packet(94,id+"|999|Partner|0"); Check(Visible("panel") && !Visible("invitation"),"server acceptance winning expiry race opens trade");
        Packet(97,id+"|1|123,5,1002,3|"); Packet(99,id+"|1|1|0");
        for(int i=0;i<4;i++) { Set("pending",true); Packet(97,id+"|1|123,5,1002,3|"); Check(!(bool)Get("pending") && (bool)Get("theirConfirmed"),"unchanged offer releases pending and preserves confirmation"); }
        Packet(97,id+"|2|123,6,1002,3|"); Check(!(bool)Get("theirConfirmed"),"changed revision resets confirmation");
        Set("pending",true); Packet(103,id+"|Invalid quantity"); Check(!(bool)Get("pending") && ((UnityEngine.UI.Button)Get("add")).interactable && ((UnityEngine.UI.Button)Get("cancel")).interactable,"rejected offer restores controls");
        Packet(101,id.ToString()); Packet(91,id+"|999|Partner|1"); Packet(94,id+"|999|Partner|0");
        Check((Guid)Get("tradeId")==Guid.Empty,"terminal trade ignores stale invitation and start");
        id=Guid.NewGuid(); Packet(91,id+"|999|Partner|1"); Check(Visible("invitation"),"new invitation allowed");
        Packet(101,id.ToString()); Check(!Visible("panel") && !Visible("invitation"),"rejection or expiry never opens trade window");
        Check(SystemMessageTable.Get(57).Text.Replace("$1","Partner")=="You have sent a trade request to Partner.","request message substitution");
        Check(SystemMessageTable.Get(58).Text.Replace("$1","Partner")=="Partner rejected your trade request.","rejection message substitution");
        Check(SystemMessageTable.Get(59).Text=="Trade successful.","success message");
        Check(SystemMessageTable.Get(60).Text.Replace("$1","Partner")=="Partner is busy trading with another player.","busy message substitution");
        Directory.CreateDirectory("Builds/TradeClient");
        File.WriteAllText("Builds/TradeClient/ui-regression.txt",$"PASS: {passed} Unity UI regression checks.");
        Debug.Log($"PASS: {passed} Unity UI regression checks.");
        // Leave the recipient presentation visible for visual inspection. Stop Play Mode to discard it.
        Packet(91,Guid.NewGuid()+"|999|Preview partner|1");
    }

}
#endif
