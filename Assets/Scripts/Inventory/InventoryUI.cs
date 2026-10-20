using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [Header("Item tooltip")]
    [SerializeField, Min(0f)] private float tooltipHoverDelay = 1.5f;
    private static event System.Action TooltipInterrupted;
    private Coroutine tooltipRoutine;
    private InventorySlotUI tooltipSlot;
    private bool hoverNeedsMovement;
    private Vector2 interruptedPointerPosition;

    public static void InterruptTooltips()
    {
        TooltipInterrupted?.Invoke();
        ItemTooltipUI.Instance?.Hide();
    }

    private void Awake()
    {
        foreach (var scroll in GetComponentsInChildren<ScrollRect>(true))
            if (!scroll.TryGetComponent<InventoryTooltipScrollRelay>(out _))
                scroll.gameObject.AddComponent<InventoryTooltipScrollRelay>();
    }

    public void BeginItemHover(InventorySlotUI slot, PointerEventData eventData)
    {
        // Scrolling/menu dismissal may synthesize pointer entry under a stationary cursor.
        bool canRearm = !hoverNeedsMovement ||
            (eventData.position - interruptedPointerPosition).sqrMagnitude > 0.25f;
        InterruptTooltips();
        if (!canRearm || !isActiveAndEnabled || slot.Item == null || eventData.dragging ||
            eventData.pointerDrag != null || InventoryItemContextMenu.IsOpen)
            return;
        hoverNeedsMovement = false;
        tooltipSlot = slot;
        tooltipRoutine = StartCoroutine(WaitForItemHover(slot, slot.Item.ObjectId));
    }

    private IEnumerator WaitForItemHover(InventorySlotUI slot, int objectId)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, tooltipHoverDelay));
        tooltipRoutine = null;
        if (tooltipSlot != slot || !slot || !slot.isActiveAndEnabled ||
            slot.Item == null || slot.Item.ObjectId != objectId ||
            !ClientInventory.TryGetItem(objectId, out var item) ||
            item.Quantity <= 0 || !ShouldShowItem(item) || InventoryItemContextMenu.IsOpen)
            yield break;
        ShowItemTooltip(item, slot.transform as RectTransform);
    }

    public void CancelItemHover(InventorySlotUI slot)
    {
        if (tooltipSlot == slot)
        {
            CancelTooltip();
            ItemTooltipUI.Instance?.Hide();
        }
    }

    public void EndItemHover(InventorySlotUI slot, PointerEventData eventData)
    {
        CancelItemHover(slot);
        // A real exit permits re-entry even at the original coordinates. An exit
        // caused by a menu appearing under a stationary mouse does not rearm it.
        if ((eventData.position - interruptedPointerPosition).sqrMagnitude > 0.25f)
            hoverNeedsMovement = false;
    }

    private void CancelTooltip()
    {
        if (tooltipRoutine != null) StopCoroutine(tooltipRoutine);
        tooltipRoutine = null;
        tooltipSlot = null;
    }

    private void InterruptTooltip()
    {
        hoverNeedsMovement = true;
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null) interruptedPointerPosition = mouse.position.ReadValue();
        CancelTooltip();
    }

    public void OnPointerDown(PointerEventData eventData) => InterruptTooltips();
    public int? SelectedObjectId { get; private set; }

    public void SelectItem(ClientItemInstance item)
    {
        SelectedObjectId = item != null && ShouldShowItem(item) ? item.ObjectId : (int?)null;
        UpdateSelectionVisuals();
    }

    public void ClearSelection()
    {
        SelectedObjectId = null;
        UpdateSelectionVisuals();
    }

    private void UpdateSelectionVisuals()
    {
        foreach (InventorySlotUI slot in slots)
            slot.SetSelected(slot.Item != null && slot.Item.ObjectId == SelectedObjectId);
    }

    private void LateUpdate()
    {
        if (tooltipSlot != null && (tooltipSlot.Item == null ||
            !ClientInventory.TryGetItem(tooltipSlot.Item.ObjectId, out var hoverItem) ||
            hoverItem.Quantity <= 0 || !ShouldShowItem(hoverItem)))
            InterruptTooltips();
        // Also handles client mutations that do not immediately refresh this window.
        if (SelectedObjectId.HasValue &&
            (!ClientInventory.TryGetItem(SelectedObjectId.Value, out var selected) ||
             selected.Quantity <= 0 || !ShouldShowItem(selected)))
            ClearSelection();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InterruptTooltips();
        // Child slot/button/scrollbar handlers consume their own clicks.
        if (eventData.button == PointerEventData.InputButton.Left)
            ClearSelection();
    }
    private enum InventoryView
    {
        Items,
        QuestItems
    }

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

    private InventoryView currentView =
        InventoryView.Items;

    private void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot =
                Instantiate(
                    slotPrefab,
                    itemGrid);

            slot.SetSlotIndex(
                i);

            slot.Clear();

            slots.Add(
                slot);
        }

        Refresh();
    }

    private void OnEnable()
    {
        TooltipInterrupted += InterruptTooltip;
        if (slots.Count > 0)
        {
            Refresh();
        }
    }

    public void ShowItems()
    {
        currentView =
            InventoryView.Items;

        Refresh();
    }

    public void ShowQuestItems()
    {
        currentView =
            InventoryView.QuestItems;

        Refresh();
    }

    public void Refresh()
    {
        InterruptTooltips();
        SyncItemOrder();
        if (SelectedObjectId.HasValue &&
            (!ClientInventory.TryGetItem(SelectedObjectId.Value, out var selected) ||
             selected.Quantity <= 0 || !ShouldShowItem(selected)))
            SelectedObjectId = null;

        foreach (InventorySlotUI slot
            in slots)
        {
            slot.Clear();
        }

        int slotIndex =
            0;

        foreach (int objectId
            in itemOrder)
        {
            if (!ClientInventory.TryGetItem(
                    objectId,
                    out ClientItemInstance item))
            {
                continue;
            }

            if (!ShouldShowItem(
                    item))
            {
                continue;
            }

            if (slotIndex >=
                slots.Count)
            {
                break;
            }

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
        UpdateSelectionVisuals();
    }

    public void MoveItem(
        int objectId,
        int targetSlotIndex)
    {
        SyncItemOrder();

        List<int> visibleItems =
            new();

        foreach (int id
            in itemOrder)
        {
            if (!ClientInventory.TryGetItem(
                    id,
                    out ClientItemInstance item))
            {
                continue;
            }

            if (!ShouldShowItem(
                    item))
            {
                continue;
            }

            visibleItems.Add(
                id);
        }

        int oldIndex =
            visibleItems.IndexOf(
                objectId);

        if (oldIndex == -1)
            return;

        visibleItems.RemoveAt(
            oldIndex);

        targetSlotIndex =
            Mathf.Clamp(
                targetSlotIndex,
                0,
                visibleItems.Count);

        visibleItems.Insert(
            targetSlotIndex,
            objectId);

        HashSet<int> visibleSet =
            new(
                visibleItems);

        int visibleIndex =
            0;

        for (int i = 0;
            i < itemOrder.Count;
            i++)
        {
            if (!visibleSet.Contains(
                    itemOrder[i]))
            {
                continue;
            }

            itemOrder[i] =
                visibleItems[
                    visibleIndex];

            visibleIndex++;
        }

        Refresh();
    }

    private bool ShouldShowItem(
        ClientItemInstance item)
    {
        if (item.IsEquipped)
            return false;

        if (currentView ==
            InventoryView.Items)
        {
            return !item.IsQuestItem;
        }

        return item.IsQuestItem;
    }

    private void SyncItemOrder()
    {
        // Remove items that no longer exist.
        for (int i =
                itemOrder.Count - 1;
            i >= 0;
            i--)
        {
            if (!ClientInventory.TryGetItem(
                    itemOrder[i],
                    out _))
            {
                itemOrder.RemoveAt(
                    i);
            }
        }

        // Add newly received items at the end.
        foreach (ClientItemInstance item
            in ClientInventory.Items)
        {
            if (!itemOrder.Contains(
                    item.ObjectId))
            {
                itemOrder.Add(
                    item.ObjectId);
            }
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
        CancelTooltip();
        TooltipInterrupted -= InterruptTooltip;
        InventoryItemContextMenu.Instance?.Hide();
        ItemTooltipUI.Instance?.Hide();
    }
}
