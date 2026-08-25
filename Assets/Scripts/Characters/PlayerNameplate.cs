using TMPro;
using UnityEngine;

public class PlayerNameplate : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text titleText;

    public void SetPlayerInfo(
        string playerName,
        string title)
    {
        nameText.text = playerName;

        if (string.IsNullOrWhiteSpace(title))
        {
            titleText.gameObject.SetActive(false);
        }
        else
        {
            titleText.gameObject.SetActive(true);
            titleText.text = title;
        }
    }
}