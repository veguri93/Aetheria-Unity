using System.Text;
using UnityEngine;

public static class PlayerSpawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split(
                '|',
                7,
                System.StringSplitOptions.None);

        if (parts.Length != 7)
            return;

        if (!int.TryParse(
                parts[0],
                out int playerId))
        {
            return;
        }

        string playerName =
            parts[1];

        string title =
            parts[2];

        if (!float.TryParse(
                parts[3],
                out float x) ||
            !float.TryParse(
                parts[4],
                out float y) ||
            !float.TryParse(
                parts[5],
                out float z) ||
            !float.TryParse(
                parts[6],
                out float rotationY))
        {
            return;
        }

        Vector3 position =
            new Vector3(
                x,
                y,
                z);

        LocalPlayer localPlayer =
            Object.FindAnyObjectByType<LocalPlayer>();

        if (localPlayer == null)
            return;

        localPlayer.SetPlayerId(
            playerId);

        localPlayer.SetSpawnPosition(
            position,
            rotationY);

        localPlayer.SetPlayerInfo(
            playerName,
            title);

        PlayerProgressUI.Instance?.SetName(
            playerName);
    }
}