using System.Text;
using UnityEngine;

public static class InventorySnapshotPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        ClientInventory.Clear();

        if (string.IsNullOrWhiteSpace(
                data))
        {
            Debug.Log(
                "Inventory loaded: 0 items.");

            return;
        }

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
                    out int objectId) ||
                !int.TryParse(
                    parts[1],
                    out int itemId) ||
                !int.TryParse(
                    parts[2],
                    out int quantity))
            {
                continue;
            }

            string equippedSlot =
                parts[3];

            ClientItemInstance item =
                new(
                    objectId,
                    itemId,
                    quantity,
                    equippedSlot);

            ClientInventory.Add(
                item);
        }

        InventoryUI inventoryUI =
            Object.FindAnyObjectByType<InventoryUI>();

        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }
    }
}