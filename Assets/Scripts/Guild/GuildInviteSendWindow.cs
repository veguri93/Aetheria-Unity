using TMPro;
using UnityEngine;

public class GuildInviteSendWindow : MonoBehaviour
{
    public static GuildInviteSendWindow Instance { get; private set; }

    [SerializeField]
    private GameObject windowRoot;

    [SerializeField]
    private TMP_Text targetText;

    [SerializeField]
    private TMP_InputField messageInput;

    private int _targetPlayerId;

    private const int MaxMessageLength = 120;

    private void Awake()
    {
        Instance = this;

        messageInput.characterLimit =
            MaxMessageLength;

        Hide();
    }

    public void Show(
        int targetPlayerId,
        string targetPlayerName)
    {
        _targetPlayerId =
            targetPlayerId;

        targetText.text =
            $"Invite {targetPlayerName} to your guild";

        messageInput.text =
            string.Empty;

        windowRoot.SetActive(
            true);

        messageInput.Select();
        messageInput.ActivateInputField();
    }

    public void Send()
    {
        string message =
            messageInput.text.Trim();

        GameServerConnection.Instance?
            .SendGuildInviteRequest(
                _targetPlayerId,
                message);

        Hide();
    }

    public void Cancel()
    {
        Hide();
    }

    public void Hide()
    {
        _targetPlayerId =
            0;

        messageInput.text =
            string.Empty;

        windowRoot.SetActive(
            false);
    }
}