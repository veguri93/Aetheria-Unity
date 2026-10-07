using System.Text;
using UnityEngine;

public static class EquipmentChangedPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 4)
            return;

        string action =
            parts[0];

        if (!int.TryParse(
                parts[1],
                out int objectId))
        {
            return;
        }

        if (!int.TryParse(
                parts[2],
                out int itemId))
        {
            return;
        }

        if (!System.Enum.TryParse(
                parts[3],
                out EquipmentSlot slot))
        {
            Debug.LogWarning(
                $"Unknown equipment slot: {parts[3]}");

            return;
        }

        if (ClientInventory.TryGetItem(
                objectId,
                out ClientItemInstance item))
        {
            if (action == "EQUIP")
            {
                item.SetEquippedSlot(
                    slot.ToString());
            }
            else if (action == "UNEQUIP")
            {
                item.SetEquippedSlot(
                    "None");
            }
        }

        if (action == "EQUIP")
        {
            EquipmentUI.Instance?.EquipItem(
                objectId,
                itemId,
                slot);
        }
        else if (action == "UNEQUIP")
        {
            EquipmentUI.Instance?.UnequipItem(
                slot);
        }

        InventoryUI inventoryUI =
            Object.FindAnyObjectByType<InventoryUI>();

        inventoryUI?.Refresh();
    }
}