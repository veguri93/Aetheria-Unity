using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    private GameServerConnection _connection;

    public bool IsTyping => _chatInput && _chatInput.isFocused;

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
    [SerializeField] private Color _normalCombatColor = Color.white;

    [Header("System Message Category Colors")]
    [SerializeField] private Color _systemInfoColor = new Color32(201, 205, 212, 255);
    [SerializeField] private Color _systemDamageColor = new Color32(240, 199, 94, 255);
    [SerializeField] private Color _systemMissColor = new Color32(221, 164, 92, 255);
    [SerializeField] private Color _systemExperienceColor = new Color32(141, 217, 155, 255);
    [SerializeField] private Color _systemRewardColor = new Color32(235, 203, 121, 255);
    [SerializeField] private Color _systemSocialColor = new Color32(120, 185, 233, 255);
    [SerializeField] private Color _systemSkillColor = new Color32(197, 160, 240, 255);
    [SerializeField] private Color _systemErrorColor = new Color32(241, 109, 109, 255);
    [SerializeField] private Color _systemHealingColor = new Color32(112, 218, 205, 255);

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
    private Color GetSystemMessageColor(int messageId)
    {
        return messageId switch
        {
            // Combat
            3 => _systemDamageColor,
            19 => _damageTakenColor,
            56 => _systemMissColor,

            // Experience, levels, class progression
            5 or 6 or 21 => _systemExperienceColor,

            // Items and rewards
            9 or 14 or 23 or 47 or 54 => _systemRewardColor,

            // Healing
            18 => _systemHealingColor,

            // Skills, buffs and quests
            7 or 8 or 11 or 20 or 22
                or 51 or 52 or 53 => _systemSkillColor,

            // Clan and party information
            24 or 28 or 29 or 30 or 31
                or 32 or 33 or 34 or 35 or 36
                or 37 or 38 or 39 or 41
                or 43 or 44 or 45 or 46
                or 49 => _systemSocialColor,

            // Errors and restrictions
            10 or 13 or 15 or 17 or 26
                or 27 or 40 or 42 or 48 => _systemErrorColor,

            // Warnings
            12 or 25 or 50 or 55 => _systemMissColor,

            // General information
            _ => _systemInfoColor
        };
    }

    public void AddMessage(
        ChatType type,
        ChatScope scope,
        string senderName,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (_messagePrefab == null || _chatContent == null)
            return;

        ChatHistoryScroll.BeforeAppend(_chatScroll);

        TMP_Text newMessage =
            Instantiate(
                _messagePrefab,
                _chatContent);

        newMessage.text =
    senderName + ": " + message;

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


    }

    public void AddCombatMessage(
        GameMessageType type,
        string message,
        int systemMessageId = 0)

    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (_combatMessagePrefab == null || _combatContent == null)
            return;

        ChatHistoryScroll.BeforeAppend(_combatScroll);

        TMP_Text newMessage =
            Instantiate(
                _combatMessagePrefab,
                _combatContent);

        newMessage.text = message;

        switch (type)
        {
            case GameMessageType.System:
                newMessage.color = systemMessageId > 0
                    ? GetSystemMessageColor(systemMessageId)
                    : _systemColor;
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

            case GameMessageType.Normal:
                newMessage.color = _normalCombatColor;
                break;
        }


    }



    private void OnMessageSubmitted(
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            return;
        }

        if (_connection == null)
        {
            _connection =
                FindAnyObjectByType<GameServerConnection>();

            if (_connection == null)
                return;
        }

        message =
            message.Trim();

        if (message.Equals(
                "/invite target",
                System.StringComparison.OrdinalIgnoreCase))
        {
            HandleInviteTargetCommand();

            ClearInput();

            return;
        }

        if (message.StartsWith(
                "/invite ",
                System.StringComparison.OrdinalIgnoreCase))
        {
            string playerName =
                message.Substring(
                    "/invite ".Length)
                .Trim();

            if (!string.IsNullOrWhiteSpace(
                    playerName))
            {
                HandleInviteByNameCommand(
                    playerName);
            }

            ClearInput();

            return;
        }

        ChatType chatType =
            (ChatType)_chatTypeDropdown.value;

        _connection.SendChatMessage(
            chatType,
            message);

        ClearInput();
    }

    private void HandleInviteByNameCommand(
    string playerName)
    {
        PartyWindow partyWindow =
            PartyWindow.Instance;

        if (partyWindow != null &&
            partyWindow.IsInParty)
        {
            _connection.SendPartyInviteByNameRequest(
                playerName,
                partyWindow.LootMode);

            return;
        }

        PartyLootModeWindow.Instance?
            .ShowByName(
                playerName);
    }

    private void HandleInviteTargetCommand()
    {
        if (TargetManager.Instance == null)
            return;

        if (TargetManager.Instance.CurrentTarget
            is not RemotePlayer remotePlayer)
        {
            Debug.LogWarning(
                "Target a player first.");

            return;
        }

        PartyWindow partyWindow =
            PartyWindow.Instance;

        if (partyWindow != null &&
            partyWindow.IsInParty)
        {
            _connection.SendPartyInviteRequest(
                remotePlayer.ObjectId,
                partyWindow.LootMode);

            return;
        }

        PartyLootModeWindow.Instance?
            .Show(
                remotePlayer.ObjectId);
    }

    private void ClearInput()
    {
        _chatInput.text =
            string.Empty;

        _chatInput.ActivateInputField();
    }
    public string ColorText(
    GameMessageType type,
    string text)
    {
        Color color = type switch
        {
            GameMessageType.System => _systemColor,
            GameMessageType.DamageDealt => _damageDealtColor,
            GameMessageType.DamageTaken => _damageTakenColor,
            GameMessageType.Critical => _criticalColor,
            GameMessageType.Kill => _killColor,
            GameMessageType.Reward => _rewardColor,
            GameMessageType.Normal => _normalCombatColor,
            _ => _normalCombatColor
        };

        string hex =
            ColorUtility.ToHtmlStringRGB(
                color);

        return $"<color=#{hex}>{text}</color>";
    }
}
