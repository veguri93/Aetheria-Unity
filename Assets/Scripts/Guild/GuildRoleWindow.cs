using TMPro;
using UnityEngine;

public class GuildRoleWindow : MonoBehaviour
{
    public static GuildRoleWindow Instance { get; private set; }

    [SerializeField]
    private TMP_Text memberText;

    private int _characterId;

    private string _memberName;

    private void Awake()
    {
        Instance =
            this;

        Hide();
    }

    public void Show(
        int characterId,
        string memberName)
    {
        _characterId =
            characterId;

        _memberName =
            memberName;

        memberText.text =
            memberName;

        gameObject.SetActive(
            true);
    }

    public void SelectWarLord()
    {
        ConfirmRoleChange(
            1);
    }

    public void SelectDiplomat()
    {
        ConfirmRoleChange(
            2);
    }

    public void SelectViceCaptain()
    {
        ConfirmRoleChange(
            3);
    }

    public void SelectLeader()
    {
        ConfirmRoleChange(
            4);
    }

    private void ConfirmRoleChange(
        int role)
    {
        if (_characterId <= 0)
            return;

        ConfirmationWindow.Instance?.Show(
            () =>
            {
                GameServerConnection.Instance?
                    .SendGuildMemberRoleChangeRequest(
                        _characterId,
                        role);

                Hide();
            });
    }

    public void Hide()
    {
        _characterId =
            0;

        _memberName =
            string.Empty;

        gameObject.SetActive(
            false);
    }
}