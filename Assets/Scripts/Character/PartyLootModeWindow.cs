using TMPro;
using UnityEngine;

public class PartyLootModeWindow : MonoBehaviour
{
    public static PartyLootModeWindow Instance { get; private set; }

    [SerializeField]
    private TMP_Dropdown lootModeDropdown;

    private int _targetPlayerId;

    private string _targetPlayerName =
        string.Empty;

    private void Awake()
    {
        Instance =
            this;

        Hide();
    }

    public void Show(
        int targetPlayerId)
    {
        if (targetPlayerId <= 0)
            return;

        _targetPlayerId =
            targetPlayerId;

        _targetPlayerName =
            string.Empty;

        lootModeDropdown.value =
            0;

        gameObject.SetActive(
            true);
    }

    public void ShowByName(
        string playerName)
    {
        if (string.IsNullOrWhiteSpace(
                playerName))
        {
            return;
        }

        _targetPlayerId =
            0;

        _targetPlayerName =
            playerName.Trim();

        lootModeDropdown.value =
            0;

        gameObject.SetActive(
            true);
    }

    public void Confirm()
    {
        int lootMode =
            GetSelectedLootMode();

        if (_targetPlayerId > 0)
        {
            GameServerConnection.Instance?
                .SendPartyInviteRequest(
                    _targetPlayerId,
                    lootMode);

            Hide();

            return;
        }

        if (!string.IsNullOrWhiteSpace(
                _targetPlayerName))
        {
            GameServerConnection.Instance?
                .SendPartyInviteByNameRequest(
                    _targetPlayerName,
                    lootMode);

            Hide();
        }
    }

    public void Hide()
    {
        _targetPlayerId =
            0;

        _targetPlayerName =
            string.Empty;

        gameObject.SetActive(
            false);
    }

    private int GetSelectedLootMode()
    {
        return lootModeDropdown.value switch
        {
            // Dropdown:
            // 0 = By Turn
            // 1 = Finders Keepers
            // 2 = Random

            0 => 1,
            1 => 0,
            2 => 2,
            _ => 1
        };
    }
}