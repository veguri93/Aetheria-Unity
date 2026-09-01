using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI :
    MonoBehaviour,
    IPointerClickHandler
{
    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private TMP_Text quantityText;

    private ClientItemInstance item;

    public ClientItemInstance Item =>
        item;

    private void Awake()
    {
        Clear();
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
        item = null;

        itemIcon.sprite = null;

        itemIcon.gameObject.SetActive(
            false);

        quantityText.text =
            string.Empty;
    }

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (item == null)
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
            $"Using inventory item: " +
            $"ObjectId={item.ObjectId}, " +
            $"ItemId={item.ItemId}");

        GameServerConnection.Instance
            .SendItemActionRequest(
                item.ObjectId);
    }
}