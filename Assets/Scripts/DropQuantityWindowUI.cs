using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropQuantityWindowUI : MonoBehaviour
{
    private static DropQuantityWindowUI instance;
    public static DropQuantityWindowUI Instance
    {
        get
        {
            if (!instance) instance = FindAnyObjectByType<DropQuantityWindowUI>(FindObjectsInactive.Include);
            return instance;
        }
    }
    private bool initialized, opening;
    [SerializeField] private TMP_InputField quantityInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    private int objectId, itemId;
    private Action<int> confirmed;
    private Action cancelled;
    public bool IsShowingFor(int id) => isActiveAndEnabled && objectId == id;
    private void Awake()
    {
        Initialize();
        // An inactive scene dialog can be opened before its first Awake.
        if (!opening) Hide();
    }
    private void Initialize()
    {
        if (initialized) return;
        initialized = true;
        instance = this;
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(Hide);
    }
    // Compatibility for existing callers: all drops now pass through inventory ownership.
    public void Show(ClientItemInstance item) => FindAnyObjectByType<InventoryUI>()?.BeginWorldDrop(item.ObjectId);
    public void Show(ClientItemInstance item, Action<int> onConfirm, Action onCancel)
    {
        Initialize();
        Hide();
        InventoryUI.InterruptTooltips();
        objectId = item.ObjectId;
        itemId = item.ItemId;
        confirmed = onConfirm;
        cancelled = onCancel;
        quantityInput.text = "1";
        opening = true;
        gameObject.SetActive(true);
        opening = false;
        GetComponent<FantasyPopupPresentation>()?.Present();
        quantityInput.Select();
        quantityInput.ActivateInputField();
    }
    public void Hide()
    {
        Action cancel = cancelled;
        cancelled = null;
        confirmed = null;
        objectId = itemId = 0;
        quantityInput.text = string.Empty;
        gameObject.SetActive(false);
        cancel?.Invoke();
    }
    private void OnConfirmClicked()
    {
        if (!ClientInventory.TryGetItem(objectId, out var current) || current.ItemId != itemId ||
            current.IsEquipped || current.IsQuestItem || !current.Droppable)
        { Hide(); return; }
        if (!int.TryParse(quantityInput.text, out int quantity) || quantity < 1 || quantity > current.Quantity)
        {
            Debug.LogWarning("Enter a drop quantity between 1 and " + current.Quantity + ".");
            return;
        }
        Action<int> action = confirmed;
        cancelled = null;
        Hide();
        action?.Invoke(quantity);
    }
    private void OnDisable()
    {
        Action cancel = cancelled;
        cancelled = null;
        confirmed = null;
        objectId = itemId = 0;
        cancel?.Invoke();
    }
    private void OnDestroy()
    {
        if (confirmButton) confirmButton.onClick.RemoveListener(OnConfirmClicked);
        if (cancelButton) cancelButton.onClick.RemoveListener(Hide);
    }
}
