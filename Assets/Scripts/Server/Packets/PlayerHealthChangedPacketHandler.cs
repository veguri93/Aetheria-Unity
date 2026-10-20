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

        if (parts.Length != 2 && parts.Length != 4)
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

        int currentCp = 0, maxCp = 0;
        if (parts.Length == 4 && (!int.TryParse(parts[2], out currentCp) || !int.TryParse(parts[3], out maxCp))) return;

        PlayerHealth health =
            Object.FindAnyObjectByType<PlayerHealth>();

        health?.SetServerHealth(
            currentHp,
            maxHp);

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetHealth(currentHp, maxHp);
        if (parts.Length == 4) statsUI?.SetCombatPoints(currentCp, maxCp);
    }
}