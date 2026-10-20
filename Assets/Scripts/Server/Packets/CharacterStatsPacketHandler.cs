using System.Text;
using UnityEngine;

public static class CharacterStatsPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 7 && parts.Length != 9)
            return;

        if (!int.TryParse(
                parts[0],
                out int maxHp) ||
            !int.TryParse(
                parts[1],
                out int physicalAttack) ||
            !int.TryParse(
                parts[2],
                out int physicalDefense) ||
            !int.TryParse(
                parts[3],
                out int magicAttack) ||
            !int.TryParse(
                parts[4],
                out int magicDefense) ||
            !int.TryParse(
                parts[5],
                out int attackSpeed) ||
            !int.TryParse(
                parts[6],
                out int castingSpeed))
        {
            return;
        }

        int currentCp = 0, maxCp = 0;
        if (parts.Length == 9 && (!int.TryParse(parts[7], out currentCp) || !int.TryParse(parts[8], out maxCp))) return;

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        if (statsUI == null)
            return;

        statsUI.SetStats(
            maxHp,
            physicalAttack,
            physicalDefense,
            magicAttack,
            magicDefense,
            attackSpeed,
            castingSpeed);
        if (parts.Length == 9) statsUI.SetCombatPoints(currentCp, maxCp);
    }
}