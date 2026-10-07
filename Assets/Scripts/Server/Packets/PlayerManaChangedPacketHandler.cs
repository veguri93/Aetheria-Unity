using System.Text;
using UnityEngine;

public static class PlayerManaChangedPacketHandler
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
                out int currentMp) ||
            !int.TryParse(
                parts[1],
                out int maxMp))
        {
            return;
        }

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetMana(
            currentMp,
            maxMp);
    }
}