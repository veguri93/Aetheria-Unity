using System.Text;
using UnityEngine;

public static class PlayerTeleportPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 4)
            return;

        if (!float.TryParse(
                parts[0],
                out float x) ||
            !float.TryParse(
                parts[1],
                out float y) ||
            !float.TryParse(
                parts[2],
                out float z) ||
            !float.TryParse(
                parts[3],
                out float rotation))
        {
            return;
        }

        LocalPlayer localPlayer =
            Object.FindAnyObjectByType<LocalPlayer>();

        if (localPlayer == null)
            return;

        localPlayer.SetSpawnPosition(
            new Vector3(
                x,
                y,
                z),
            rotation);

        TargetManager.Instance?
            .ClearTarget();

        NpcHtmlWindow.Instance?
            .Hide();
    }
}