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
}
#endif
