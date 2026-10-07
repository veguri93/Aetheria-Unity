using TMPro;
using UnityEngine;

public class GuildMemberRow : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text roleText;

    [SerializeField]
    private TMP_Text statusText;

    private int _characterId;

    private string _memberName;

    public void SetData(
        string memberName,
        string role,
        bool isOnline)
    {
        nameText.text =
            memberName;

        roleText.text =
            role;

        statusText.text =
            isOnline
                ? "Online"
                : "Offline";
    }
    public void SetIdentity(
    int characterId,
    string memberName)
    {
        _characterId =
            characterId;

        _memberName =
            memberName;
    }
    public void Select()
    {
        GuildWindow.Instance?.SelectMember(
            _characterId,
            _memberName);
    }
}