using System;
using System.Text;
using UnityEngine;
public static class NpcHtmlPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        try
        {
            var parts = Encoding.UTF8.GetString(packet.Data).Split('|');
            NpcHtmlWindow.Trace("Received packet 43 fields=" + parts.Length);
            if (parts.Length != 9) { Debug.LogWarning("NPC dialogue requires the updated GameServer."); return; }
            var window = NpcHtmlWindow.Instance;
            if (!window) { Debug.LogError("[NPC dialogue] No NPC window in the loaded scene."); return; }
            window.Receive(parts, Encoding.UTF8.GetString(Convert.FromBase64String(parts[2])));
        }
        catch (FormatException) { Debug.LogWarning("[NPC dialogue] Malformed packet 43."); }
        // A presentation failure must not escape into the network receive loop and disconnect gameplay.
        catch (Exception ex) { Debug.LogError("[NPC dialogue] Rendering packet 43 failed: " + ex); }
    }
}
