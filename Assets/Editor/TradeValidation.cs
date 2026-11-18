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
        Set("pending",false); typeof(TradeWindowUI).GetMethod("ExpireLocalInvitation",flags).Invoke(view,null);
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
        void Invoke(string method) => typeof(TradeWindowUI).GetMethod(method,flags).Invoke(view,null);
        Invoke("BeginPendingRequest");
        Check((bool)Get("pending") && (float)Get("pendingExpiresAt") > Time.unscaledTime,"initial request has deadline without trade ID");
        Invoke("CheckPendingTimeout"); Check((bool)Get("pending"),"request does not time out before deadline");
        Set("pendingExpiresAt",Time.unscaledTime-1f); Invoke("CheckPendingTimeout");
        Check(!(bool)Get("pending") && (Guid)Get("tradeId")==Guid.Empty,"initial response timeout clears pending");
        Check(!Visible("panel") && Visible("invitation") && ((UnityEngine.UI.Button)Get("invitationClose")).gameObject.activeSelf,"timeout is dismissible without opening trade");
        Check(((TMPro.TMP_Text)Get("invitationText")).text.Contains("may have completed"),"timeout does not claim rollback");
        id=Guid.NewGuid(); Packet(94,id+"|999|Partner|1"); Invoke("BeginPendingRequest");
        Set("pendingExpiresAt",Time.unscaledTime-1f); Invoke("CheckPendingTimeout");
        Check(!(bool)Get("pending") && !(bool)Get("active") && (Guid)Get("tradeId")==Guid.Empty,"active request timeout clears unusable trade controls");
        id=Guid.NewGuid(); Packet(94,id+"|999|Partner|1"); Invoke("BeginPendingRequest"); Packet(97,id+"|0||");
        Set("pendingExpiresAt",Time.unscaledTime-1f); Invoke("CheckPendingTimeout");
        Check((bool)Get("active") && (Guid)Get("tradeId")==id,"acknowledged request does not time out");
        Packet(101,id.ToString());
        id=Guid.NewGuid(); Packet(91,id+"|999|Partner|1"); Invoke("BeginPendingRequest"); Invoke("ExpireLocalInvitation");
        Check(!(bool)Get("pending") && (Guid)Get("tradeId")==Guid.Empty && ((TMPro.TMP_Text)Get("invitationText")).text.Contains("Reconnect"),"expiry with an outstanding accept requires reconnect");
        Directory.CreateDirectory("Builds/TradeClient");
        File.WriteAllText("Builds/TradeClient/ui-regression.txt",$"PASS: {passed} Unity UI regression checks.");
        Debug.Log($"PASS: {passed} Unity UI regression checks.");
        // Leave the recipient presentation visible for visual inspection. Stop Play Mode to discard it.
        Packet(91,Guid.NewGuid()+"|999|Preview partner|1");
    }

    [MenuItem("Tools/Aetheia/Trade/Run Timeout Regression (Play Mode)")]
    public static async void TimeoutRegression()
    {
        if (!EditorApplication.isPlaying || (GameServerConnection.Instance && GameServerConnection.Instance.IsConnected)) return;
        try
        {
            Regression();
            var connection=GameServerConnection.Instance;
            if (!connection) throw new Exception("Missing connection component");
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var field=typeof(GameServerConnection).GetField("_client",flags);
            var gate=(System.Threading.SemaphoreSlim)typeof(GameServerConnection).GetField("tradeSendLock",flags).GetValue(connection);
            var listener=new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback,0);
            listener.Start();
            using var client=new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(System.Net.IPAddress.Loopback,((System.Net.IPEndPoint)listener.LocalEndpoint).Port);
            using var peer=await listener.AcceptTcpClientAsync(); listener.Stop();
            field.SetValue(connection,client);
            await gate.WaitAsync();
            try
            {
                var task=(System.Threading.Tasks.Task)typeof(GameServerConnection).GetMethod("SendTradeFramesAsync",flags)
                    .Invoke(connection,new object[]{new byte[][]{new byte[]{2,0,90,0}}});
                if(await System.Threading.Tasks.Task.WhenAny(task,System.Threading.Tasks.Task.Delay(8000))!=task)
                    throw new Exception("Trade send timeout failed");
                await task;
                if(connection.IsConnected || field.GetValue(connection)!=null || gate.CurrentCount!=0)
                    throw new Exception("Timed-out send did not disconnect safely or released unacquired lock");
            }
            finally {gate.Release();}
            File.WriteAllText("Builds/TradeClient/network-regression.txt","PASS: stalled trade send exits, disconnects, and preserves send-lock ownership.");
            Debug.Log("PASS: trade timeout network regression.");
        }
        catch(Exception error) {Debug.LogException(error);}
    }

}
#endif
