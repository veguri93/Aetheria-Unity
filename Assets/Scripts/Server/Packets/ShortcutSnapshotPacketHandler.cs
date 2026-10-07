using System.Text;
using UnityEngine;

public static class ShortcutSnapshotPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        SkillShortcutBarUI shortcutBar =
            Object.FindAnyObjectByType<SkillShortcutBarUI>();

        if (shortcutBar == null)
        {
            Debug.LogWarning(
                "[SHORTCUT] Shortcut bar was not found.");

            return;
        }

        shortcutBar.ClearAllShortcuts();

        if (string.IsNullOrWhiteSpace(data))
            return;

        string[] entries =
            data.Split(
                ';',
                System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string entry in entries)
        {
            string[] parts =
                entry.Split(',');

            if (parts.Length != 4)
                continue;

            if (!int.TryParse(
                    parts[0],
                    out int page) ||
                !int.TryParse(
                    parts[1],
                    out int slot) ||
                !int.TryParse(
                    parts[2],
                    out int typeValue) ||
                !int.TryParse(
                    parts[3],
                    out int referenceId))
            {
                continue;
            }

            if (page != 0)
                continue;

            if (slot < 1 ||
                slot > 12)
            {
                continue;
            }

            ShortcutType shortcutType =
                (ShortcutType)typeValue;

            if (shortcutType != ShortcutType.Skill &&
                shortcutType != ShortcutType.Item)
            {
                continue;
            }

            if (referenceId <= 0)
                continue;

            shortcutBar.LoadShortcut(
                slot,
                shortcutType,
                referenceId);
        }
    }
}