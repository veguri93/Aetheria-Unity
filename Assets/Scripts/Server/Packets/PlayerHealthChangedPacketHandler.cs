using System.Text;
using UnityEngine;

public static class PlayerHealthChangedPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(
                parts[0],
                out int currentHp) ||
            !int.TryParse(
                parts[1],
                out int maxHp))
        {
            return;
        }

        PlayerHealth health =
            Object.FindAnyObjectByType<PlayerHealth>();

        if (health == null)
            return;

        health.SetServerHealth(
            currentHp,
            maxHp);

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetHealth(
            currentHp,
            maxHp);
    }
}