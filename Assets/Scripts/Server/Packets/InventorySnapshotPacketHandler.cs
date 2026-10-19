
using System.Globalization;
using System.Text;
using UnityEngine;

public static class InventorySnapshotPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        ClientInventory.Clear();

        if (!string.IsNullOrWhiteSpace(data))
        {
            string[] entries =
                data.Split(
                    ';',
                    System.StringSplitOptions
                        .RemoveEmptyEntries);

            foreach (string entry in entries)
            {
                string[] parts = entry.Split(',');

                if (parts.Length < 4)
                    continue;

                if (!int.TryParse(
                        parts[0], out int objectId) ||
                    !int.TryParse(
                        parts[1], out int itemId) ||
                    !int.TryParse(
                        parts[2], out int quantity))
                {
                    continue;
                }

                string equippedSlot = parts[3];

                bool isQuestItem = false;

                if (parts.Length >= 5)
                {
                    bool.TryParse(
                        parts[4],
                        out isQuestItem);
                }

                string tier =
                    parts.Length >= 6
                        ? parts[5]
                        : "None";

                string displayType =
                    parts.Length >= 7
                        ? parts[6]
                        : string.Empty;

                ClientItemInstance item = new(
                    objectId,
                    itemId,
                    quantity,
                    equippedSlot,
                    isQuestItem,
                    tier,
                    displayType,
                    ReadStat(parts, 7),
                    ReadStat(parts, 8),
                    ReadStat(parts, 9),
                    ReadStat(parts, 10));

                ClientInventory.Add(item);
            }
        }

        InventoryUI inventoryUI =
            Object.FindAnyObjectByType<InventoryUI>();

        inventoryUI?.Refresh();
    }

    private static double? ReadStat(
        string[] parts,
        int index)
    {
        if (index >= parts.Length)
            return null;

        if (double.TryParse(
                parts[index],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value))
        {
            return value;
        }

        return null;
    }
}
