using TMPro;
using UnityEngine;

public class PlayerProgressUI : MonoBehaviour
{
    public static PlayerProgressUI Instance { get; private set; }

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text levelText;

    private void Awake()
    {
        Instance = this;
    }

    public void SetName(string playerName)
    {
        nameText.text =
            playerName;
    }

    public void SetProgress(
        int level,
        long experience,
        long currentLevelExperience,
        long nextLevelExperience)
    {
        levelText.text =
            level.ToString();
    }
}