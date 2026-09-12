using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropQuantityWindowUI : MonoBehaviour
{
    public static DropQuantityWindowUI Instance { get; private set; }

    [SerializeField]
    private TMP_InputField quantityInput;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button cancelButton;

    private ClientItemInstance selectedItem;

    private void Awake()
    {
        Instance = this;

        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }

    public void Show(ClientItemInstance item)
    {
        selectedItem = item;

        quantityInput.text = "1";

        Debug.Log(
            $"[DROP QUANTITY] Window opened. " +
            $"ObjectId={item.ObjectId}, " +
            $"ItemId={item.ItemId}, " +
            $"AvailableQuantity={item.Quantity}");

        gameObject.SetActive(true);

        quantityInput.Select();
        quantityInput.ActivateInputField();
    }

    public void Hide()
    {
        selectedItem = null;

        quantityInput.text = string.Empty;

        gameObject.SetActive(false);
    }

    private void OnConfirmClicked()
    {
        Debug.Log(
            $"[DROP QUANTITY] Confirm clicked. " +
            $"Input='{quantityInput.text}'");

        if (selectedItem == null)
        {
            Debug.LogWarning(
                "[DROP QUANTITY] No selected item.");

            return;
        }

        if (!int.TryParse(
                quantityInput.text,
                out int quantity))
        {
            Debug.LogWarning(
                $"[DROP QUANTITY] Invalid quantity: " +
                $"'{quantityInput.text}'");

            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning(
                $"[DROP QUANTITY] Quantity must be greater than 0. " +
                $"Received={quantity}");

            return;
        }

        if (quantity > selectedItem.Quantity)
        {
            Debug.LogWarning(
                $"[DROP QUANTITY] Tried to drop too many. " +
                $"Requested={quantity}, " +
                $"Available={selectedItem.Quantity}");

            return;
        }

        if (GameServerConnection.Instance == null)
        {
            Debug.LogWarning(
                "[DROP QUANTITY] GameServerConnection.Instance is null.");

            return;
        }

        Debug.Log(
            $"[DROP QUANTITY] Sending DropItemRequest. " +
            $"ObjectId={selectedItem.ObjectId}, " +
            $"Quantity={quantity}");

        GameServerConnection.Instance.SendDropItemRequest(
            selectedItem.ObjectId,
            quantity);

        Hide();
    }
}