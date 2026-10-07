using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuildInviteReceiveWindow : MonoBehaviour
{
    public static GuildInviteReceiveWindow Instance { get; private set; }

    [SerializeField]
    private TMP_Text inviteInfoText;

    [SerializeField]
    private TMP_Text messageText;

    [SerializeField]
    private Slider inviteTimerSlider;

    private const float InviteDuration =
        30f;

    private float _remainingTime;

    private bool _isOpen;

    private void Awake()
    {
        Instance =
            this;

        Hide();
    }

    private void Update()
    {
        if (!_isOpen)
            return;

        _remainingTime -=
            Time.deltaTime;

        inviteTimerSlider.value =
            _remainingTime /
            InviteDuration;

        if (_remainingTime <= 0f)
        {
            Hide();
        }
    }

    public void Show(
        string inviterName,
        string guildName,
        string message)
    {
        inviteInfoText.text =
            $"{inviterName} invites you to join {guildName}";

        messageText.text =
            message;

        _remainingTime =
            InviteDuration;

        inviteTimerSlider.value =
            1f;

        _isOpen =
            true;

        gameObject.SetActive(
            true);
    }

    public void Accept()
    {
        GameServerConnection.Instance?
            .SendGuildInviteAcceptRequest();

        Hide();
    }

    public void Reject()
    {
        GameServerConnection.Instance?
            .SendGuildInviteRejectRequest();

        Hide();
    }

    public void Hide()
    {
        _isOpen =
            false;

        _remainingTime =
            0f;

        gameObject.SetActive(
            false);
    }
}