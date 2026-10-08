using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyInviteReceiveWindow : MonoBehaviour
{
    public static PartyInviteReceiveWindow Instance { get; private set; }

    [SerializeField]
    private TMP_Text inviteInfoText;

    [SerializeField]
    private TMP_Text lootModeText;

    [SerializeField]
    private Slider inviteTimerSlider;

    private const float InviteDuration =
        10f;

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
        int lootMode)
    {
        inviteInfoText.text =
            $"{inviterName} invites you to join a party";

        lootModeText.text =
            $"Loot: {GetLootModeName(lootMode)}";

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
        Debug.Log(
            "[PARTY] Accept clicked.");

        GameServerConnection.Instance?
            .SendPartyInviteAcceptRequest();

        Hide();
    }

    public void Reject()
    {
        Debug.Log(
            "[PARTY] Reject clicked.");

        GameServerConnection.Instance?
            .SendPartyInviteRejectRequest();

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

    private static string GetLootModeName(
        int lootMode)
    {
        return lootMode switch
        {
            0 => "Finders Keepers",
            1 => "By Turn",
            2 => "Random",
            _ => "Unknown"
        };
    }
}