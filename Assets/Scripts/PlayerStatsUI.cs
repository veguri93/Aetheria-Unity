using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField]
    private GameObject statsPanel;

    [SerializeField]
    private TMP_Text statsText;

    [Header("HP")]
    [SerializeField]
    private Slider hpSlider;

    [SerializeField]
    private TMP_Text hpText;

    [Header("MP")]
    [SerializeField]
    private Slider mpSlider;

    [SerializeField]
    private TMP_Text mpText;

    private void Start()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }
    }

    public void ToggleStatsPanel()
    {
        if (statsPanel == null)
            return;

        statsPanel.SetActive(
            !statsPanel.activeSelf);
    }

    public void SetStats(
        int maxHp,
        int physicalAttack,
        int physicalDefense,
        int magicAttack,
        int magicDefense,
        int attackSpeed,
        int castingSpeed)
    {
        if (statsText != null)
        {
            statsText.text =
                $"HP: {maxHp}\n" +
                $"P. Atk: {physicalAttack}\n" +
                $"P. Def: {physicalDefense}\n" +
                $"M. Atk: {magicAttack}\n" +
                $"M. Def: {magicDefense}\n" +
                $"Atk Spd: {attackSpeed}\n" +
                $"Cast Spd: {castingSpeed}";
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue =
                maxHp;
        }
    }

    public void SetHealth(
        int currentHp,
        int maxHp)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue =
                maxHp;

            hpSlider.value =
                currentHp;
        }

        if (hpText != null)
        {
            hpText.text =
                $"{currentHp} / {maxHp}";
        }
    }

    public void SetMana(
        int currentMp,
        int maxMp)
    {
        if (mpSlider != null)
        {
            mpSlider.maxValue =
                maxMp;

            mpSlider.value =
                currentMp;
        }

        if (mpText != null)
        {
            mpText.text =
                $"{currentMp} / {maxMp}";
        }
    }
}