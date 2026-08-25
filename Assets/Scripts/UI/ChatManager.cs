using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    private GameServerConnection _connection;

    [Header("Chat UI")]
    [SerializeField] private ScrollRect _chatScroll;
    [SerializeField] private RectTransform _chatContent;
    [SerializeField] private TMP_Text _messagePrefab;
    [SerializeField] private TMP_InputField _chatInput;
    [SerializeField] private TMP_Dropdown _chatTypeDropdown;

    [Header("Combat / System UI")]
    [SerializeField] private ScrollRect _combatScroll;
    [SerializeField] private RectTransform _combatContent;
    [SerializeField] private TMP_Text _combatMessagePrefab;

    [Header("Message Colors")]
    [SerializeField] private Color _systemColor = new Color(1f, 0.85f, 0.1f);       // Yellow
    [SerializeField] private Color _damageDealtColor = new Color(0.1f, 1f, 0.2f);  // Bright green
    [SerializeField] private Color _damageTakenColor = new Color(1f, 0.1f, 0.1f);  // Bright red
    [SerializeField] private Color _criticalColor = new Color(0.2f, 0.6f, 1f);     // Bright blue
    [SerializeField] private Color _killColor = new Color(1f, 0.85f, 0.1f);         // Yellow
    [SerializeField] private Color _rewardColor = new Color(1f, 0.85f, 0.1f);       // Yellow
    [SerializeField] private Color _generalChatColor = new Color(0.7f, 1f, 0.8f);
    [SerializeField] private Color _clanChatColor = new Color(0.8f, 0.6f, 1f);
    [SerializeField] private Color _globalChatColor = new Color(1f, 0.5f, 0.05f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    private void OnEnable()
    {
        if (_chatInput != null)
        {
            _chatInput.onSubmit.AddListener(OnMessageSubmitted);
        }
    }

    private void OnDisable()
    {
        if (_chatInput != null)
        {
            _chatInput.onSubmit.RemoveListener(OnMessageSubmitted);
        }
    }

    public void AddMessage(
        ChatType type,
        ChatScope scope,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (_messagePrefab == null || _chatContent == null)
            return;

        TMP_Text newMessage =
            Instantiate(
                _messagePrefab,
                _chatContent);

        newMessage.text = message;

        if (scope == ChatScope.Global)
        {
            newMessage.color = _globalChatColor;
        }
        else
        {
            switch (type)
            {
                case ChatType.General:
                    newMessage.color = _generalChatColor;
                    break;

                case ChatType.Clan:
                    newMessage.color = _clanChatColor;
                    break;
            }
        }

        StartCoroutine(ScrollToBottom());
    }

    public void AddCombatMessage(
        GameMessageType type,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (_combatMessagePrefab == null || _combatContent == null)
            return;

        TMP_Text newMessage =
            Instantiate(
                _combatMessagePrefab,
                _combatContent);

        newMessage.text = message;

        switch (type)
        {
            case GameMessageType.System:
                newMessage.color = _systemColor;
                break;

            case GameMessageType.DamageDealt:
                newMessage.color = _damageDealtColor;
                break;

            case GameMessageType.DamageTaken:
                newMessage.color = _damageTakenColor;
                break;

            case GameMessageType.Critical:
                newMessage.color = _criticalColor;
                break;

            case GameMessageType.Kill:
                newMessage.color = _killColor;
                break;

            case GameMessageType.Reward:
                newMessage.color = _rewardColor;
                break;
        }

        StartCoroutine(ScrollCombatToBottom());
    }



    private IEnumerator ScrollToBottom()
    {
        // Wait until Unity has finished calculating the new layout.
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (_chatContent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                _chatContent);

        if (_chatScroll != null)
        {
            _chatScroll.verticalNormalizedPosition = 0f;
        }
    }

    private IEnumerator ScrollCombatToBottom()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (_combatContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                _combatContent);
        }

        if (_combatScroll != null)
        {
            _combatScroll.verticalNormalizedPosition = 0f;
        }
    }

    private void OnMessageSubmitted(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (_connection == null)
        {
            _connection =
                FindAnyObjectByType<GameServerConnection>();

            if (_connection == null)
                return;
        }

        ChatType chatType =
            (ChatType)_chatTypeDropdown.value;

        _connection.SendChatMessage(
            chatType,
            message.Trim());

        _chatInput.text = string.Empty;
        _chatInput.ActivateInputField();
    }
}