using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI :
    MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IScrollHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IShortcutDragSource
{
    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private TMP_Text quantityText;

    private ClientItemInstance item;

    [SerializeField] private InventorySelectionGlow selectionGlow;

    public void SetSelected(bool selected)
    {
        if (selectionGlow != null)
            selectionGlow.gameObject.SetActive(selected && item != null);
    }

    private int draggedObjectId, draggedItemId;
    private bool draggingItem, dropHandled;
    private int suppressClickFrame = -1;
    private GameObject dragIconObject;
    private Canvas rootCanvas;
    private InventoryUI inventoryUI;

    public ClientItemInstance Item =>
        item;

    public int SlotIndex { get; private set; }

    public ShortcutType ShortcutType =>
        ShortcutType.Item;

    public int ShortcutReferenceId =>
        item != null
            ? item.ItemId
            : 0;

    public Sprite ShortcutIcon =>
        itemIcon != null
            ? itemIcon.sprite
            : null;

    private void Awake()
    {
        rootCanvas =
            GetComponentInParent<Canvas>();

        inventoryUI =
            GetComponentInParent<InventoryUI>();

        Clear();
    }

    public void SetSlotIndex(
        int index)
    {
        SlotIndex =
            index;
    }

    public void OnDrop(
        PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();
        if (eventData.pointerDrag == null)
            return;

        eventData.pointerDrag.GetComponent<InventorySlotUI>()?.MarkDropHandled();

        // Equipped item dropped into Inventory.
        EquipmentSlotUI equipmentSlot =
            eventData.pointerDrag
                .GetComponent<EquipmentSlotUI>();

        if (equipmentSlot != null &&
            equipmentSlot.HasItem)
        {
            if (GameServerConnection.Instance == null)
                return;

            GameServerConnection.Instance
                .SendUnequipItemRequest(
                    equipmentSlot.ObjectId);

            return;
        }

        // Normal Inventory item reordering.
        InventorySlotUI sourceSlot =
            eventData.pointerDrag
                .GetComponent<InventorySlotUI>();

        if (sourceSlot == null)
            return;

        if (sourceSlot.Item == null)
            return;

        inventoryUI?.MoveItem(
            sourceSlot.Item.ObjectId,
            SlotIndex);
    }

    public void SetItem(
        ClientItemInstance itemInstance,
        Sprite icon)
    {
        CancelDrag();
        inventoryUI?.CancelItemHover(this);
        item =
            itemInstance;

        itemIcon.sprite =
            icon;

        itemIcon.gameObject.SetActive(
            icon != null);

        quantityText.text =
            item.Quantity > 999
                ? "999+"
                : item.Quantity > 1
                    ? item.Quantity.ToString()
                    : string.Empty;
    }

    public void Clear()
    {
        CancelDrag();
        inventoryUI?.CancelItemHover(this);
        SetSelected(false);
        item =
            null;

        itemIcon.sprite =
            null;

        itemIcon.gameObject.SetActive(
            false);

        quantityText.text =
            string.Empty;
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (item == null)
            return;

        inventoryUI?.BeginItemHover(this, eventData);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        inventoryUI?.EndItemHover(this, eventData);
    }

    public void OnPointerDown(PointerEventData eventData) => InventoryUI.InterruptTooltips();

    public void OnScroll(PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();
        // Forward the wheel event now received by this slot to preserve scrolling.
        GetComponentInParent<ScrollRect>()?.OnScroll(eventData);
    }

    private void OnDisable() { CancelDrag(); inventoryUI?.CancelItemHover(this); }

    public void OnPointerClick(
        PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();
        if (eventData.dragging || draggingItem || Time.frameCount == suppressClickFrame || InventoryUI.ItemInteractionDialogOpen) return;
        if (item == null)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                inventoryUI?.ClearSelection();
            return;
        }

        if (eventData.button ==
            PointerEventData.InputButton.Right)
        {
            inventoryUI?.SelectItem(item);
            InventoryUI.UseItem(item.ObjectId);

            return;
        }

        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        inventoryUI?.SelectItem(item);

        if (eventData.clickCount < 2)
            return;

        InventoryUI.UseItem(item.ObjectId);
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();

        if (eventData.button != PointerEventData.InputButton.Left || InventoryUI.ItemInteractionDialogOpen ||
            item == null || !isActiveAndEnabled || !ClientInventory.TryGetItem(item.ObjectId, out var current) || current.IsEquipped)
            return;

        if (rootCanvas == null)
            return;

        CancelDrag();
        draggedObjectId = item.ObjectId;
        draggedItemId = item.ItemId;
        draggingItem = true;
        dropHandled = false;
        InventoryUI.BeginItemDrag(this);

        dragIconObject =
            new GameObject(
                "DragIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        dragIconObject.transform.SetParent(
            rootCanvas.rootCanvas.transform,
            false);

        dragIconObject.transform.SetAsLastSibling();

        Image dragImage =
            dragIconObject.GetComponent<Image>();

        dragImage.sprite =
            itemIcon.sprite;

        dragImage.preserveAspect =
            true;

        dragImage.raycastTarget =
            false;

        RectTransform dragRect =
            dragIconObject.GetComponent<RectTransform>();

        dragRect.sizeDelta =
            itemIcon.rectTransform.rect.size;

        dragRect.position =
            eventData.position;
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (dragIconObject == null)
            return;

        dragIconObject.transform.position =
            eventData.position;
    }

    public void MarkDropHandled() => dropHandled = true;
    public void CancelDrag()
    {
        if (draggingItem) suppressClickFrame = Time.frameCount;
        draggingItem = false;
        draggedObjectId = draggedItemId = 0;
        InventoryUI.EndItemDrag(this);
        if (dragIconObject) Destroy(dragIconObject);
        dragIconObject = null;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();
        bool worldDrop = draggingItem && !dropHandled && isActiveAndEnabled &&
            eventData.button == PointerEventData.InputButton.Left &&
            UnityEngine.InputSystem.Keyboard.current?.escapeKey.isPressed != true &&
            ClientInventory.TryGetItem(draggedObjectId, out var current) && current.ItemId == draggedItemId;
        int id = draggedObjectId;
        CancelDrag();
        if (worldDrop) inventoryUI?.TryDropOutsideInventory(id, eventData);
    }
}
