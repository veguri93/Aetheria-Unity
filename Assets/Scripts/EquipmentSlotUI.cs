using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI :
    MonoBehaviour,
    IPointerClickHandler
{
    [SerializeField]
    private EquipmentSlot _slot;

    private Image _itemIcon;
    private int _objectId;

    public EquipmentSlot Slot => _slot;
    public int ObjectId => _objectId;
    public bool HasItem => _objectId != 0;

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

    public void SetItem(
        int objectId,
        Sprite icon)
    {
        EnsureItemIcon();

        _objectId = objectId;

        _itemIcon.sprite = icon;
        _itemIcon.enabled = icon != null;
    }

    public void Clear()
    {
        EnsureItemIcon();

        _objectId = 0;

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

        Debug.Log(
            $"Unequipping item: ObjectId={_objectId}, Slot={_slot}");

        GameServerConnection.Instance
            .SendUnequipItemRequest(_objectId);
    }
}