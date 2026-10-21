using System;
using TMPro;
using UnityEngine;

public class ConfirmationWindow : MonoBehaviour
{
    private static ConfirmationWindow instance;
    public static ConfirmationWindow Instance
    {
        get
        {
            if (!instance) instance = FindAnyObjectByType<ConfirmationWindow>(FindObjectsInactive.Include);
            return instance;
        }
    }
    private bool initialized, opening;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text confirmLabel;
    [SerializeField] private TMP_Text cancelLabel;
    private string defaultMessage, defaultConfirm, defaultCancel;
    private Action _confirmAction, _cancelAction;

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
        defaultMessage = messageText ? messageText.text : string.Empty;
        defaultConfirm = confirmLabel ? confirmLabel.text : string.Empty;
        defaultCancel = cancelLabel ? cancelLabel.text : string.Empty;
    }
    public bool IsShowing(Action action) => isActiveAndEnabled && _confirmAction == action;
    public void Show(Action confirmAction) => Open(null, confirmAction, null);
    public void Show(string message, Action confirmAction, Action cancelAction) => Open(message, confirmAction, cancelAction);
    private void Open(string message, Action confirmAction, Action cancelAction)
    {
        Initialize();
        Hide();
        InventoryUI.InterruptTooltips();
        _confirmAction = confirmAction;
        _cancelAction = cancelAction;
        if (messageText) messageText.text = message ?? defaultMessage;
        if (confirmLabel) confirmLabel.text = message == null ? defaultConfirm : "Yes";
        if (cancelLabel) cancelLabel.text = message == null ? defaultCancel : "Cancel";
        opening = true;
        gameObject.SetActive(true);
        opening = false;
        GetComponent<FantasyPopupPresentation>()?.Present();
    }
    public void Confirm()
    {
        Action action = _confirmAction;
        _cancelAction = null;
        Hide(); // Consume before invoking: repeated clicks cannot resend.
        action?.Invoke();
    }
    public void Cancel() => Hide();
    public void Hide()
    {
        Action cancel = _cancelAction;
        _confirmAction = _cancelAction = null;
        gameObject.SetActive(false);
        cancel?.Invoke();
    }
    private void OnDisable()
    {
        Action cancel = _cancelAction;
        _confirmAction = _cancelAction = null;
        cancel?.Invoke();
    }
}
