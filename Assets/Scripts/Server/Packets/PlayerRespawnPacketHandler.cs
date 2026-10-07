using System.Text;
using UnityEngine;

public static class PlayerRespawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 5)
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
            !int.TryParse(
                parts[3],
                out int currentHp) ||
            !int.TryParse(
                parts[4],
                out int maxHp))
        {
            return;
        }

        LocalPlayer localPlayer =
            Object.FindAnyObjectByType<LocalPlayer>();

        if (localPlayer == null)
            return;

        PlayerHealth health =
            localPlayer.GetComponent<PlayerHealth>();

        if (health == null)
            return;

        health.Respawn();

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetHealth(
            currentHp,
            maxHp);

        localPlayer.SetSpawnPosition(
            new Vector3(
                x,
                y,
                z),
            0f);

        RespawnWindow.Instance?.Hide();
    }
}