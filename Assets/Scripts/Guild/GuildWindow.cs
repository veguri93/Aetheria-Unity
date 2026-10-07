using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class GuildWindow : MonoBehaviour
{
    public static GuildWindow Instance { get; private set; }

    [SerializeField]
    private TMP_Text guildNameText;

    [SerializeField]
    private Transform memberListContent;

    [SerializeField]
    private GameObject memberRowTemplate;

    [SerializeField]
    private GameObject promoteButton;

    [SerializeField]
    private GameObject dismissButton;

    [SerializeField]
    private GameObject inviteButton;

    [SerializeField]
    private TMP_Text leaderText;

    [SerializeField]
    private TMP_Text viceCaptainText;

    [SerializeField]
    private TMP_Text diplomatText;

    [SerializeField]
    private TMP_Text warLordText;

    private int _selectedMemberCharacterId;

    private string _selectedMemberName;

    private void Awake()
    {
        Instance =
            this;

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(
            true);

        GameServerConnection.Instance?
            .SendGuildWindowRequest();
    }
    public void SelectMember(
    int characterId,
    string memberName)
    {
        _selectedMemberCharacterId =
            characterId;

        _selectedMemberName =
            memberName;

    }
    public void OpenTitleWindow()
    {
        GuildTitleWindow.Instance?.Show();
    }
    public void ClearMemberRows()
    {
        foreach (Transform child
            in memberListContent)
        {
            if (child.gameObject ==
                memberRowTemplate)
            {
                continue;
            }

            Destroy(
                child.gameObject);
        }
    }
    public void ResetLeadership()
    {
        leaderText.text =
            "Leader -";

        viceCaptainText.text =
            "ViceCaptain -";

        diplomatText.text =
            "Diplomat -";

        warLordText.text =
            "WarLord -";
    }

    public void SetLeadershipMember(
        string memberName,
        int roleValue)
    {
        switch (roleValue)
        {
            case 4:
                leaderText.text =
                    $"Leader - {memberName}";
                break;

            case 3:
                viceCaptainText.text =
                    $"ViceCaptain - {memberName}";
                break;

            case 2:
                diplomatText.text =
                    $"Diplomat - {memberName}";
                break;

            case 1:
                warLordText.text =
                    $"WarLord - {memberName}";
                break;
        }
    }
    public void AddMemberRow(
        int characterId,
        string memberName,
        string role,
        bool isOnline)
    {
        GameObject rowObject =
            Instantiate(
                memberRowTemplate,
                memberListContent);

        rowObject.SetActive(
            true);

        GuildMemberRow row =
            rowObject.GetComponent<GuildMemberRow>();

        if (row == null)
        {
            Destroy(
                rowObject);

            return;
        }

        row.SetIdentity(
            characterId,
            memberName);

        row.SetData(
            memberName,
            role,
            isOnline);

        Button button =
            rowObject.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                row.Select);
        }
    }
    public void PromoteSelectedMember()
    {
        if (_selectedMemberCharacterId <= 0)
        {
            Debug.LogWarning(
                "Select a guild member first.");

            return;
        }

        GuildRoleWindow.Instance?.Show(
            _selectedMemberCharacterId,
            _selectedMemberName);
    }
    public void LeaveGuild()
    {
        ConfirmationWindow.Instance?.Show(
            ConfirmLeaveGuild);
    }
    public void DismissSelectedMember()
    {
        if (_selectedMemberCharacterId <= 0)
        {
            Debug.LogWarning(
                "Select a guild member first.");

            return;
        }

        ConfirmationWindow.Instance?.Show(
            ConfirmDismissSelectedMember);
    }

    private void ConfirmDismissSelectedMember()
    {
        GameServerConnection.Instance?
            .SendGuildDismissRequest(
                _selectedMemberCharacterId);
    }
    private void ConfirmLeaveGuild()
    {
        GameServerConnection.Instance?
            .SendGuildLeaveRequest();
    }
    public void SetGuildName(
    string guildName)
    {
        guildNameText.text =
            guildName;
    }
    public void Hide()
    {
        gameObject.SetActive(
            false);
    }
    public void InviteTargetedPlayer()
    {
        if (TargetManager.Instance == null)
            return;

        if (TargetManager.Instance.CurrentTarget
            is not RemotePlayer remotePlayer)
        {
            Debug.LogWarning(
                "Target a player before inviting.");

            return;
        }

        GuildInviteSendWindow.Instance?.Show(
            remotePlayer.ObjectId,
            remotePlayer.Name);
    }
    public void SetLeadershipActionsVisible(
        bool isLeader,
        bool isViceCaptain)
    {
        promoteButton.SetActive(
            isLeader);

        dismissButton.SetActive(
            isLeader ||
            isViceCaptain);

        inviteButton.SetActive(
            isLeader ||
            isViceCaptain);
    }
}