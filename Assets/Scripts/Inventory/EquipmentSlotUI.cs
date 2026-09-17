using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI :
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
    private EquipmentSlot _slot;

    private Image _itemIcon;

    private int _objectId;
    private int _itemId;

    private GameObject _dragIconObject;
    private RectTransform _dragIconRect;

    public EquipmentSlot Slot => _slot;

    public int ObjectId => _objectId;

    public int ItemId => _itemId;

    public bool HasItem => _objectId != 0;

    public ShortcutType ShortcutType =>
        ShortcutType.Item;

    public int ShortcutReferenceId =>
        _itemId;

    public Sprite ShortcutIcon =>
        _itemIcon != null
            ? _itemIcon.sprite
            : null;

    private void Awake()
    {
        EnsureItemIcon();
    }

    private void EnsureItemIcon()
    {
        if (_itemIcon != null)
            return;

        Transform existing =
            transform.Find("ItemIcon");

        if (existing != null)
        {
            _itemIcon =
                existing.GetComponent<Image>();

            if (_itemIcon != null)
                return;
        }

        GameObject iconObject =
            new GameObject(
                "ItemIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        iconObject.transform.SetParent(
            transform,
            false);

        RectTransform rect =
            iconObject.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _itemIcon =
            iconObject.GetComponent<Image>();

        _itemIcon.preserveAspect = true;
        _itemIcon.raycastTarget = false;
        _itemIcon.enabled = false;
    }

    // Temporary compatibility with the old EquipmentUI call.
    public void SetItem(
        int objectId,
        Sprite icon)
    {
        SetItem(
            objectId,
            0,
            icon);
    }

    public void SetItem(
        int objectId,
        int itemId,
        Sprite icon)
    {
        EnsureItemIcon();

        _objectId = objectId;
        _itemId = itemId;

        _itemIcon.sprite = icon;
        _itemIcon.enabled = icon != null;
    }

    public void Clear()
    {
        EnsureItemIcon();

        _objectId = 0;
        _itemId = 0;

        _itemIcon.sprite = null;
        _itemIcon.enabled = false;
    }

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (!HasItem)
            return;

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
            .SendUnequipItemRequest(
                _objectId);
    }
    public void OnPointerEnter(
    PointerEventData eventData)
    {
        if (!HasItem)
            return;

        if (_itemId == 0)
            return;

        EquipmentUI.Instance?.ShowItemTooltip(
            _itemId,
            transform as RectTransform);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }
    public void OnBeginDrag(
        PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();

        if (!HasItem)
            return;

        if (_itemIcon == null ||
            _itemIcon.sprite == null)
        {
            return;
        }

        Canvas canvas =
            GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        Canvas rootCanvas =
            canvas.rootCanvas;

        _dragIconObject =
            new GameObject(
                "EquipmentDragIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        _dragIconObject.transform.SetParent(
            rootCanvas.transform,
            false);

        _dragIconObject.transform.SetAsLastSibling();

        _dragIconRect =
            _dragIconObject
                .GetComponent<RectTransform>();

        _dragIconRect.sizeDelta =
            new Vector2(32f, 32f);

        Image dragImage =
            _dragIconObject
                .GetComponent<Image>();

        dragImage.sprite =
            _itemIcon.sprite;

        dragImage.preserveAspect =
            true;

        dragImage.raycastTarget =
            false;

        _dragIconRect.position =
            eventData.position;
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (_dragIconRect == null)
            return;

        _dragIconRect.position =
            eventData.position;
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (_dragIconObject != null)
        {
            Destroy(
                _dragIconObject);
        }

        _dragIconObject = null;
        _dragIconRect = null;
    }
    public void OnDrop(
    PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        InventorySlotUI inventorySlot =
            eventData.pointerDrag
                .GetComponent<InventorySlotUI>();

        if (inventorySlot == null)
            return;

        if (inventorySlot.Item == null)
            return;

        if (_slot == EquipmentSlot.None)
            return;

        if (GameServerConnection.Instance == null)
            return;

        GameServerConnection.Instance
            .SendItemActionRequest(
                inventorySlot.Item.ObjectId,
                _slot);
    }
}