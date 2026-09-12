using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryItemContextMenu : MonoBehaviour
{
    public static InventoryItemContextMenu Instance { get; private set; }

    [SerializeField]
    private Button useButton;

    [SerializeField]
    private Button dropButton;

    private ClientItemInstance selectedItem;
    private int openedFrame;

    private void Awake()
    {
        Instance = this;

        dropButton.onClick.AddListener(OnDropClicked);

        gameObject.SetActive(false);
    }

    public void Show(
        ClientItemInstance item,
        RectTransform itemRect)
    {
        selectedItem = item;
        openedFrame = Time.frameCount;

        Debug.Log(
            $"[DROP UI] Context menu opened. " +
            $"ObjectId={item.ObjectId}, " +
            $"ItemId={item.ItemId}, " +
            $"Quantity={item.Quantity}");

        gameObject.SetActive(true);

        RectTransform menuRect =
            GetComponent<RectTransform>();

        Vector3[] corners =
            new Vector3[4];

        itemRect.GetWorldCorners(corners);

        Vector3 bottomCenter =
            (corners[0] + corners[3]) / 2f;

        menuRect.position =
            bottomCenter + new Vector3(0, -5f, 0);
    }

    public void Hide()
    {
        selectedItem = null;

        gameObject.SetActive(false);
    }

    private void OnDropClicked()
    {
        if (selectedItem == null)
        {
            Debug.LogWarning(
                "[DROP UI] Drop clicked, but no item is selected.");

            return;
        }

        ClientItemInstance itemToDrop =
            selectedItem;

        Debug.Log(
            $"[DROP UI] Drop clicked. " +
            $"ObjectId={itemToDrop.ObjectId}, " +
            $"ItemId={itemToDrop.ItemId}, " +
            $"Quantity={itemToDrop.Quantity}");

        Hide();

        if (itemToDrop.Quantity > 1)
        {
            Debug.Log(
                $"[DROP UI] Stack has {itemToDrop.Quantity} items. " +
                $"Opening quantity window.");

            DropQuantityWindowUI.Instance?.Show(
                itemToDrop);

            return;
        }

        Debug.Log(
            $"[DROP UI] Single item. " +
            $"Sending drop request for quantity 1.");

        if (GameServerConnection.Instance == null)
        {
            Debug.LogWarning(
                "[DROP UI] GameServerConnection.Instance is null.");

            return;
        }

        GameServerConnection.Instance
            .SendDropItemRequest(
                itemToDrop.ObjectId,
                1);
    }

    private void Update()
    {
        if (Time.frameCount == openedFrame)
            return;

        if (Mouse.current == null)
            return;

        bool leftClicked =
            Mouse.current.leftButton.wasPressedThisFrame;

        bool rightClicked =
            Mouse.current.rightButton.wasPressedThisFrame;

        if (!leftClicked && !rightClicked)
            return;

        RectTransform menuRect =
            transform as RectTransform;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        if (RectTransformUtility.RectangleContainsScreenPoint(
                menuRect,
                mousePosition))
        {
            return;
        }

        Hide();
    }
}