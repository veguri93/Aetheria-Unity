using UnityEngine;

public class PartyWindow : MonoBehaviour
{
    public static PartyWindow Instance { get; private set; }

    [SerializeField]
    private Transform memberListContent;

    [SerializeField]
    private GameObject memberRowTemplate;

    private void Awake()
    {
        Instance =
            this;

        memberRowTemplate.SetActive(
            false);

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(
            true);
    }

    public void Hide()
    {
        gameObject.SetActive(
            false);
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
    public void UpdateMemberVitals(
    int playerId,
    int currentHp,
    int maxHp,
    int currentMp,
    int maxMp)
    {
        foreach (Transform child
            in memberListContent)
        {
            if (child.gameObject ==
                memberRowTemplate)
            {
                continue;
            }

            PartyMemberRow row =
                child.GetComponent<PartyMemberRow>();

            if (row == null)
                continue;

            if (row.PlayerId !=
                playerId)
            {
                continue;
            }

            row.SetVitals(
                currentHp,
                maxHp,
                currentMp,
                maxMp);

            return;
        }
    }
    public void AddMemberRow(
        int playerId,
        string playerName)
    {
        GameObject rowObject =
            Instantiate(
                memberRowTemplate,
                memberListContent);

        rowObject.SetActive(
            true);

        PartyMemberRow row =
            rowObject.GetComponent<PartyMemberRow>();

        if (row == null)
        {
            Destroy(
                rowObject);

            return;
        }

        row.SetData(
            playerId,
            playerName);
    }
}