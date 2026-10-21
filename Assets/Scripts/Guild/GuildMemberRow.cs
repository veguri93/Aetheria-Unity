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

    [SerializeField] private InventorySelectionGlow selectionGlow;
    [SerializeField] private Color onlineColor = new Color(.62f, .73f, .59f, 1);
    [SerializeField] private Color offlineColor = new Color(.52f, .54f, .58f, 1);
    public int CharacterId => _characterId;
    public void SetSelected(bool selected)
    {
        if (selectionGlow) selectionGlow.gameObject.SetActive(selected);
    }
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
        statusText.color = isOnline ? onlineColor : offlineColor;
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