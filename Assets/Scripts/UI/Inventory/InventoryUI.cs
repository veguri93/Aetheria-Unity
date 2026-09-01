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
    private int slotCount = 20;

    private readonly List<InventorySlotUI> slots =
        new();

    private void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot =
                Instantiate(
                    slotPrefab,
                    itemGrid);

            slot.Clear();

            slots.Add(
                slot);
        }

        Refresh();
    }
    private void OnEnable()
    {
        if (slots.Count > 0)
        {
            Refresh();
        }
    }
    public void Refresh()
    {

        Debug.Log(
    $"InventoryUI Refresh: {ClientInventory.Items.Count} items");
        foreach (InventorySlotUI slot in slots)
        {
            slot.Clear();
        }

        int slotIndex = 0;

        foreach (ClientItemInstance item
            in ClientInventory.Items)
        {
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
}