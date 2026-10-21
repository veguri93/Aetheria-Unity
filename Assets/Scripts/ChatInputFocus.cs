using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Keeps chat ready without stealing another text field or dialog's keyboard focus.</summary>
public sealed class ChatInputFocus : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Dropdown channel;
    private InputAction enter;
    private Keyboard keyboard;
    private Coroutine resume;
    private readonly StringBuilder bufferedText = new StringBuilder();
    private bool channelChanged;

    private void OnEnable()
    {
        if (enter == null)
        {
            enter = new InputAction("Focus chat", InputActionType.Button);
            enter.AddBinding("<Keyboard>/enter");
            enter.AddBinding("<Keyboard>/numpadEnter");
            enter.performed += OnEnter;
        }
        enter.Enable();
        InputSystem.onDeviceChange += OnDeviceChange;
        HookKeyboard();
        if (input) { input.onDeselect.AddListener(OnFocusLost); input.onSubmit.AddListener(OnSubmitted); }
        if (channel) channel.onValueChanged.AddListener(OnChannelChanged);
        RequestFocus();
    }
    private void OnDisable()
    {
        enter?.Disable();
        InputSystem.onDeviceChange -= OnDeviceChange;
        if (keyboard != null) keyboard.onTextInput -= OnText;
        keyboard = null;
        if (input) { input.onDeselect.RemoveListener(OnFocusLost); input.onSubmit.RemoveListener(OnSubmitted); }
        if (channel) channel.onValueChanged.RemoveListener(OnChannelChanged);
        if (resume != null) StopCoroutine(resume);
        resume = null; bufferedText.Clear(); channelChanged = false;
    }
    private void OnDestroy() => enter?.Dispose();
    private void LateUpdate()
    {
        // EventSystem has no selection-changed event. This cheap idle guard restores
        // chat after a different dialog closes, without searches or per-frame allocations.
        if (input && !input.isFocused && resume == null && CanFocus(false)) RequestFocus();
    }
    private void OnDeviceChange(InputDevice device,InputDeviceChange change) { if (device is Keyboard) HookKeyboard(); }
    private void HookKeyboard()
    {
        if (keyboard == Keyboard.current) return;
        if (keyboard != null) keyboard.onTextInput -= OnText;
        keyboard = Keyboard.current;
        if (keyboard != null) keyboard.onTextInput += OnText;
    }
    private void OnEnter(InputAction.CallbackContext context) { if (input && !input.isFocused) RequestFocus(); }
    private void OnFocusLost(string text) => RequestFocus();
    private void OnSubmitted(string text) => RequestFocus();
    private void OnChannelChanged(int value) { channelChanged = true; RequestFocus(); }
    private void OnText(char character)
    {
        if (!input || input.isFocused || char.IsControl(character) || !CanFocus(true)) return;
        // The first character arrives before deferred input activation. Retain it exactly once.
        bufferedText.Append(character);
        RequestFocus();
    }
    private void RequestFocus()
    {
        if (resume == null && isActiveAndEnabled) resume = StartCoroutine(Resume());
    }
    private IEnumerator Resume()
    {
        yield return null;
        if (channelChanged) yield return new WaitForSecondsRealtime(.18f);
        bool typing = bufferedText.Length > 0;
        if (CanFocus(typing || channelChanged))
        {
            if (typing) input.text += bufferedText.ToString();
            input.Select(); input.ActivateInputField(); input.MoveTextEnd(false);
        }
        bufferedText.Clear(); channelChanged = false; resume = null;
    }
    private bool CanFocus(bool fromTyping)
    {
        if (!input || !input.isActiveAndEnabled || !EventSystem.current) return false;
        var selected = EventSystem.current.currentSelectedGameObject;
        if (!selected || !selected.activeInHierarchy || selected == input.gameObject) return true;
        if (selected.GetComponentInParent<TMP_InputField>() || selected.GetComponentInParent<FantasyPopupPresentation>()) return false;
        var dropdown = selected.GetComponentInParent<TMP_Dropdown>();
        if (dropdown && (dropdown != channel || dropdown.IsExpanded || !channelChanged)) return false;
        return fromTyping;
    }
}
