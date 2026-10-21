using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    // Inventory owns one pending drop by instance ID, never by a cached item reference.
    [Header("World drops")]
    [SerializeField, Range(.5f, 3f)] private float maximumDropDistance = 3f;
    private Vector3? dropPosition;
    private int dropObjectId, dropItemId, dropAvailable, dropGeneration;
    private System.Action dropConfirmation;
    private static InventorySlotUI activeDragSource;
    private static int suppressWorldPointerThrough = -1;
    private readonly List<RaycastResult> dropRaycasts = new List<RaycastResult>(16);
    public static bool ItemInteractionDialogOpen =>
        (DropQuantityWindowUI.Instance && DropQuantityWindowUI.Instance.isActiveAndEnabled) ||
        (ConfirmationWindow.Instance && ConfirmationWindow.Instance.isActiveAndEnabled);
    public static bool ItemDragConsumesWorldPointer => activeDragSource || Time.frameCount <= suppressWorldPointerThrough;

    public static void BeginItemDrag(InventorySlotUI source)
    {
        if (activeDragSource && activeDragSource != source) activeDragSource.CancelDrag();
        activeDragSource = source;
    }
    public static void EndItemDrag(InventorySlotUI source)
    {
        if (activeDragSource != source) return;
        activeDragSource = null;
        suppressWorldPointerThrough = Time.frameCount + 1;
    }
    public static void UseItem(int objectId)
    {
        InterruptTooltips();
        if (ItemInteractionDialogOpen || !ClientInventory.TryGetItem(objectId, out var current) || current.Quantity <= 0)
            return;
        GameServerConnection.Instance?.SendItemActionRequest(current.ObjectId);
    }
    public void TryDropOutsideInventory(int objectId, PointerEventData pointer)
    {
        if (!isActiveAndEnabled || pointer.button != PointerEventData.InputButton.Left || ItemInteractionDialogOpen)
            return;
        if (pointer.position.x < 0 || pointer.position.y < 0 || pointer.position.x >= Screen.width || pointer.position.y >= Screen.height)
            return;
        if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, pointer.position, pointer.pressEventCamera))
            return;
        if (!EventSystem.current) return;
        dropRaycasts.Clear();
        EventSystem.current.RaycastAll(pointer, dropRaycasts);
        foreach (var hit in dropRaycasts)
            if (hit.module is GraphicRaycaster) return; // Every UI destination/window takes priority.
        var camera = Camera.main;
        if (!camera || !Physics.Raycast(camera.ScreenPointToRay(pointer.position), out var worldHit,
                Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore) || !IsDropGround(worldHit)) return;
        if (!TryGetDropPosition(worldHit.point, out var position)) return;
        BeginWorldDrop(objectId, position);
    }
    private static bool IsDropGround(RaycastHit hit) => hit.normal.y >= .5f &&
        hit.collider.GetComponentInParent<ITargetable>() == null &&
        hit.collider.GetComponentInParent<WorldItemDrop>() == null;
    private bool TryGetDropPosition(Vector3 pointedGround, out Vector3 position)
    {
        position = default;
        if (!LocalPlayer.Instance) return false;
        Vector3 origin = LocalPlayer.Instance.Position;
        Vector3 offset = pointedGround - origin;
        offset.y = 0;
        float radius = Mathf.Clamp(maximumDropDistance, .5f, 3f);
        if (offset.sqrMagnitude <= radius * radius)
        {
            position = pointedGround;
            return Mathf.Abs(position.y - origin.y) <= 2f;
        }
        Vector3 nearby = origin + offset.normalized * radius;
        // Project the clamped point onto nearby ground, never onto a character or item.
        if (!Physics.Raycast(nearby + Vector3.up * 2f, Vector3.down, out var ground,
                4f, ~0, QueryTriggerInteraction.Ignore) || !IsDropGround(ground)) return false;
        position = ground.point;
        return Mathf.Abs(position.y - origin.y) <= 2f;
    }
    public void BeginWorldDrop(int objectId, Vector3? position = null)
    {
        InterruptTooltips();
        if (!isActiveAndEnabled || ItemInteractionDialogOpen) return;
        CancelPendingDrop();
        if (!ClientInventory.TryGetItem(objectId, out var current) || !CanDrop(current)) return;
        dropPosition = position;
        dropObjectId = current.ObjectId;
        dropItemId = current.ItemId;
        dropAvailable = current.Quantity;
        int generation = ++dropGeneration;
        if (current.Stackable)
        {
            if (!DropQuantityWindowUI.Instance) { CancelPendingDrop(); return; }
            DropQuantityWindowUI.Instance.Show(current,
                quantity => ConfirmWorldDrop(generation, quantity),
                () => { if (generation == dropGeneration) CancelPendingDrop(); });
        }
        else ConfirmWorldDrop(generation, 1);
    }
    private static bool CanDrop(ClientItemInstance item) => item.HasDropRules && item.Droppable &&
        !item.IsQuestItem && !item.IsEquipped && item.Quantity > 0;
    private bool TryGetPendingDrop(out ClientItemInstance current) =>
        ClientInventory.TryGetItem(dropObjectId, out current) && CanDrop(current) &&
        current.ItemId == dropItemId && current.Quantity == dropAvailable;
    private void ConfirmWorldDrop(int generation, int quantity)
    {
        if (generation != dropGeneration) return;
        if (!TryGetPendingDrop(out var current) || quantity < 1 || quantity > current.Quantity ||
            !ConfirmationWindow.Instance)
        { CancelPendingDrop(); return; }
        string name = itemVisualDatabase.TryGet(current.ItemId, out var definition) ? definition.ItemName : "this item";
        string message = current.Stackable ? $"Are you sure you want to drop {quantity} {name}?" : $"Are you sure you want to drop {name}?";
        dropConfirmation = () =>
        {
            if (generation != dropGeneration) return;
            if (!TryGetPendingDrop(out var latest) || quantity < 1 || quantity > latest.Quantity)
            { CancelPendingDrop(); return; }
            int id = latest.ObjectId;
            Vector3? position = dropPosition;
            if (position.HasValue)
            {
                if (!LocalPlayer.Instance) { CancelPendingDrop(); return; }
                Vector3 offset = position.Value - LocalPlayer.Instance.Position;
                if (new Vector2(offset.x, offset.z).sqrMagnitude > 9.01f || Mathf.Abs(offset.y) > 2f)
                { CancelPendingDrop(); return; }
            }
            CancelPendingDrop(); // Consume ownership before sending. Only server synchronization removes items.
            GameServerConnection.Instance?.SendDropItemRequest(id, quantity, position);
        };
        ConfirmationWindow.Instance.Show(message, dropConfirmation,
            () => { if (generation == dropGeneration) CancelPendingDrop(); });
    }
    private void ValidatePendingDrop()
    {
        if (dropObjectId != 0 && !TryGetPendingDrop(out _)) CancelPendingDrop();
    }
    private void CancelPendingDrop()
    {
        int id = dropObjectId;
        var confirmation = dropConfirmation;
        dropPosition = null;
        dropObjectId = dropItemId = dropAvailable = 0;
        dropConfirmation = null;
        ++dropGeneration;
        if (confirmation != null && ConfirmationWindow.Instance && ConfirmationWindow.Instance.IsShowing(confirmation))
            ConfirmationWindow.Instance.Hide();
        if (id != 0 && DropQuantityWindowUI.Instance && DropQuantityWindowUI.Instance.IsShowingFor(id))
            DropQuantityWindowUI.Instance.Hide();
    }

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
            eventData.pointerDrag != null || ItemInteractionDialogOpen)
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
            item.Quantity <= 0 || !ShouldShowItem(item) || ItemInteractionDialogOpen)
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
        if (activeDragSource && UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            activeDragSource.CancelDrag();
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
        ClientInventory.Changed += ValidatePendingDrop;
        if (slots.Count > 0)
        {
            Refresh();
        }
    }

    public void ShowItems()
    {
        CancelPendingDrop();
        currentView =
            InventoryView.Items;

        Refresh();
    }

    public void ShowQuestItems()
    {
        CancelPendingDrop();
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
        ClientInventory.Changed -= ValidatePendingDrop;
        CancelPendingDrop();
        if (activeDragSource && activeDragSource.transform.IsChildOf(transform)) activeDragSource.CancelDrag();
        CancelTooltip();
        TooltipInterrupted -= InterruptTooltip;
        InventoryItemContextMenu.Instance?.Hide();
        ItemTooltipUI.Instance?.Hide();
    }
}


