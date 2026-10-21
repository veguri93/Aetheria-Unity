using TMPro;
using UnityEngine;

public class PlayerProgressUI : MonoBehaviour
{
    public static PlayerProgressUI Instance { get; private set; }

    public string PlayerName { get; private set; } = string.Empty;
    public int? Level { get; private set; }
    public event System.Action Changed;

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
        PlayerName = playerName ?? string.Empty;
        nameText.text =
            playerName;
        Changed?.Invoke();
    }

    public void SetProgress(
        int level,
        long experience,
        long currentLevelExperience,
        long nextLevelExperience)
    {
        Level = level;
        levelText.text =
            $"Level {level}";
        Changed?.Invoke();
    }
}
