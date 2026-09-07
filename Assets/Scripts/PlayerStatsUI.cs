using TMPro;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField]
    private GameObject statsPanel;

    [SerializeField]
    private TMP_Text statsText;

    private void Start()
    {
        statsPanel.SetActive(false);
    }

    public void ToggleStatsPanel()
    {
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
        statsText.text =
            $"HP: {maxHp}\n" +
            $"P. Atk: {physicalAttack}\n" +
            $"P. Def: {physicalDefense}\n" +
            $"M. Atk: {magicAttack}\n" +
            $"M. Def: {magicDefense}\n" +
            $"Atk Spd: {attackSpeed}\n" +
            $"Cast Spd: {castingSpeed}";
    }
}