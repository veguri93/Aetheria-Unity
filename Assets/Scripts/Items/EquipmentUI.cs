using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI Instance { get; private set; }

    [SerializeField]
    private ItemVisualDatabase _itemVisualDatabase;

    private readonly Dictionary<EquipmentSlot, EquipmentSlotUI>
        _slots = new();

    private void Awake()
    {
        Instance = this;

        EquipmentSlotUI[] slotUIs =
            GetComponentsInChildren<EquipmentSlotUI>(true);

        foreach (EquipmentSlotUI slotUI in slotUIs)
        {
            if (slotUI.Slot == EquipmentSlot.None)
                continue;

            _slots[slotUI.Slot] = slotUI;
        }

    }

    public bool TryGetSlot(
        EquipmentSlot slot,
        out EquipmentSlotUI slotUI)
    {
        return _slots.TryGetValue(
            slot,
            out slotUI);
    }

    public void EquipItem(
        int objectId,
        int itemId,
        EquipmentSlot slot)
    {
        if (!_slots.TryGetValue(
                slot,
                out EquipmentSlotUI slotUI))
        {
            Debug.LogWarning(
                $"Equipment slot not found: {slot}");

            return;
        }

        if (_itemVisualDatabase == null)
        {
            Debug.LogWarning(
                "ItemVisualDatabase is not assigned.");

            return;
        }

        if (!_itemVisualDatabase.TryGet(
                itemId,
                out ItemVisualDefinition definition))
        {
            Debug.LogWarning(
                $"Item visual not found for ItemId {itemId}");

            return;
        }

        slotUI.SetItem(
            objectId,
            definition.Icon);
    }

    public void UnequipItem(
        EquipmentSlot slot)
    {
        if (!_slots.TryGetValue(
                slot,
                out EquipmentSlotUI slotUI))
        {
            return;
        }

        slotUI.Clear();
    }
}