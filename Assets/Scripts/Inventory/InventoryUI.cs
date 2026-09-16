using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private InventorySlotUI slotPrefab;

    [SerializeField]
    private Transform itemGrid;

    [SerializeField]
    private ItemVisualDatabase itemVisualDatabase;

    [SerializeField]
    private int slotCount = 60;

    private readonly List<InventorySlotUI> slots =
        new();

    // Client-only item ordering.
    // This disappears when the client restarts/relogs.
    private readonly List<int> itemOrder =
        new();

    private void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot =
                Instantiate(
                    slotPrefab,
                    itemGrid);

            slot.SetSlotIndex(i);
            slot.Clear();
            slots.Add(slot);
        }

        Refresh();
    }

    private void OnEnable()
    {
        if (slots.Count > 0)
            Refresh();
    }

    public void Refresh()
    {
        SyncItemOrder();

        foreach (InventorySlotUI slot in slots)
            slot.Clear();

        int slotIndex = 0;

        foreach (int objectId in itemOrder)
        {
            if (!ClientInventory.TryGetItem(
                    objectId,
                    out ClientItemInstance item))
            {
                continue;
            }

            // Equipped items remain owned,
            // but are hidden from the bag grid.
            if (item.IsEquipped)
                continue;

            if (slotIndex >= slots.Count)
                break;

            if (!itemVisualDatabase.TryGet(
                    item.ItemId,
                    out ItemVisualDefinition definition))
            {
                Debug.LogWarning(
                    $"No visual definition for item ID {item.ItemId}.");

                continue;
            }

            slots[slotIndex].SetItem(
                item,
                definition.Icon);

            slotIndex++;
        }
    }
    public void MoveItem(
    int objectId,
    int targetSlotIndex)
    {
        SyncItemOrder();

        List<int> visibleItems = new();

        foreach (int id in itemOrder)
        {
            if (!ClientInventory.TryGetItem(
                    id,
                    out ClientItemInstance item))
            {
                continue;
            }

            if (item.IsEquipped)
                continue;

            visibleItems.Add(id);
        }

        int oldIndex =
            visibleItems.IndexOf(objectId);

        if (oldIndex == -1)
            return;

        visibleItems.RemoveAt(oldIndex);

        targetSlotIndex =
            Mathf.Clamp(
                targetSlotIndex,
                0,
                visibleItems.Count);

        visibleItems.Insert(
            targetSlotIndex,
            objectId);

        HashSet<int> visibleSet =
            new(visibleItems);

        int visibleIndex = 0;

        for (int i = 0; i < itemOrder.Count; i++)
        {
            if (!visibleSet.Contains(itemOrder[i]))
                continue;

            itemOrder[i] =
                visibleItems[visibleIndex];

            visibleIndex++;
        }

        Refresh();
    }
    private void SyncItemOrder()
    {
        // Remove items that no longer exist.
        for (int i = itemOrder.Count - 1; i >= 0; i--)
        {
            if (!ClientInventory.TryGetItem(
                    itemOrder[i],
                    out _))
            {
                itemOrder.RemoveAt(i);
            }
        }

        // Add newly received items at the end.
        foreach (ClientItemInstance item in ClientInventory.Items)
        {
            if (!itemOrder.Contains(item.ObjectId))
                itemOrder.Add(item.ObjectId);
        }
    }
    public void ShowItemTooltip(
    ClientItemInstance item,
    RectTransform itemRect)
    {
        if (!itemVisualDatabase.TryGet(
                item.ItemId,
                out ItemVisualDefinition definition))
        {
            return;
        }

        ItemTooltipUI.Instance?.Show(
            definition,
            itemRect);
    }

    private void OnDisable()
    {
        InventoryItemContextMenu.Instance?.Hide();
        ItemTooltipUI.Instance?.Hide();
    }

}