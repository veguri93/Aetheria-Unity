
using System.Globalization;
using System.Text;
using UnityEngine;

public static class CharacterStatsPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        string[] parts =
            Encoding.UTF8.GetString(packet.Data).Split('|');

        // Legacy: 7, 9, 16
        // Range: 17
        // Range + Attack Interval: 18
        // Appended Accuracy + Evasion: 20
        if (parts.Length != 7 &&
            parts.Length != 9 &&
            parts.Length != 16 &&
            parts.Length != 17 &&
            parts.Length != 18 &&
            parts.Length != 20)
            return;

        int[] values = new int[7];

        for (int i = 0; i < 7; i++)
        {
            if (!int.TryParse(parts[i], out values[i]))
                return;
        }

        int currentCp = 0;
        int maxCp = 0;
        int currentHp = -1;
        int currentMp = -1;
        int maxMp = 0;

        if (parts.Length >= 9)
        {
            if (!int.TryParse(parts[7], out currentCp) ||
                !int.TryParse(parts[8], out maxCp))
                return;
        }

        int? might = null;
        int? grace = null;
        int? aether = null;
        int? resolve = null;

        if (parts.Length >= 16)
        {
            int?[] attributes = new int?[4];

            for (int i = 0; i < 4; i++)
            {
                if (int.TryParse(parts[9 + i], out int attribute))
                    attributes[i] = attribute;
                else if (parts[9 + i] != "—")
                    return;
            }

            might = attributes[0];
            grace = attributes[1];
            aether = attributes[2];
            resolve = attributes[3];

            if (!int.TryParse(parts[13], out currentHp) ||
                !int.TryParse(parts[14], out currentMp) ||
                !int.TryParse(parts[15], out maxMp))
                return;
        }

        float attackRange = 0f;
        int attackIntervalMs = 0;

        if (parts.Length >= 17)
        {
            if (!float.TryParse(
                    parts[16],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out attackRange))
                return;

            if (float.IsNaN(attackRange) ||
                float.IsInfinity(attackRange) ||
                attackRange <= 0f)
                return;
        }

        if (parts.Length >= 18)
        {
            if (!int.TryParse(
                    parts[17],
                    out attackIntervalMs))
                return;

            if (attackIntervalMs <= 0 ||
                attackIntervalMs == int.MaxValue)
                return;
        }

        // Validate the complete extension before changing any client state.
        // Older snapshots explicitly clear these values instead of showing stale stats.
        double? accuracy = null;
        double? evasion = null;
        if (parts.Length == 20)
        {
            if (!TryReadRating(parts[18], out double parsedAccuracy) ||
                !TryReadRating(parts[19], out double parsedEvasion))
                return;
            accuracy = parsedAccuracy;
            evasion = parsedEvasion;
        }

        // Apply combat values independently of the UI.
        PlayerCombat combat =
            LocalPlayer.Instance?
                .GetComponent<PlayerCombat>();

        if (combat != null)
        {
            if (parts.Length >= 17)
                combat.SetAttackRange(attackRange);

            if (parts.Length >= 18)
                combat.SetAttackInterval(
                    attackIntervalMs / 1000f);
        }

        PlayerStatsUI ui =
            Object.FindAnyObjectByType<PlayerStatsUI>(FindObjectsInactive.Include);

        if (!ui)
            return;

        ui.SetStats(
            values[0],
            values[1],
            values[2],
            values[3],
            values[4],
            values[5],
            values[6]);

        ui.SetAccuracyEvasion(accuracy, evasion);

        ui.SetEffectiveAttributes(
            might,
            grace,
            aether,
            resolve);

        if (parts.Length >= 9)
            ui.SetCombatPoints(currentCp, maxCp);

        if (parts.Length >= 16)
        {
            if (currentHp >= 0)
                ui.SetHealth(currentHp, values[0]);

            if (currentMp >= 0)
                ui.SetMana(currentMp, maxMp);
        }
    }
    private static bool TryReadRating(string text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
            !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }

}
