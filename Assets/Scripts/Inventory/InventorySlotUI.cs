using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI :
    MonoBehaviour,
    IPointerClickHandler,
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
        InventorySlotUI sourceSlot =
            eventData.pointerDrag?
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
        item =
            itemInstance;

        itemIcon.sprite =
            icon;

        itemIcon.gameObject.SetActive(
            icon != null);

        quantityText.text =
            item.Quantity > 1
                ? item.Quantity.ToString()
                : string.Empty;
    }

    public void Clear()
    {
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

        inventoryUI?.ShowItemTooltip(
            item,
            transform as RectTransform);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (item == null)
            return;

        if (eventData.button ==
            PointerEventData.InputButton.Right)
        {
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
        ItemTooltipUI.Instance?.Hide();

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
        if (dragIconObject == null)
            return;

        Destroy(
            dragIconObject);

        dragIconObject =
            null;
    }
}