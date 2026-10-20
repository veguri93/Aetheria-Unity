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

    private void OnDisable() => inventoryUI?.CancelItemHover(this);

    public void OnPointerClick(
        PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();
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
            InventoryItemContextMenu.Instance?.Show(
                item,
                transform as RectTransform);

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

        if (GameServerConnection.Instance == null)
            return;

        GameServerConnection.Instance
            .SendItemActionRequest(
                item.ObjectId);
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();

        if (item == null)
            return;

        if (rootCanvas == null)
            return;

        dragIconObject =
            new GameObject(
                "DragIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        dragIconObject.transform.SetParent(
            rootCanvas.transform,
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

    public void OnEndDrag(
        PointerEventData eventData)
    {
        InventoryUI.InterruptTooltips();
        if (dragIconObject == null)
            return;

        Destroy(
            dragIconObject);

        dragIconObject =
            null;
    }
}
