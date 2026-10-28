using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    // Last received authoritative values for secondary text presentations; no client stat calculation.
    public event System.Action Changed;
    public bool HasHealth { get; private set; }
    public bool HasMana { get; private set; }
    public bool HasCombatPoints { get; private set; }
    public bool HasStats { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public int CurrentMp { get; private set; }
    public int MaxMp { get; private set; }
    public int PhysicalAttack { get; private set; }
    public int PhysicalDefense { get; private set; }
    public int MagicAttack { get; private set; }
    public int MagicDefense { get; private set; }
    public int AttackSpeed { get; private set; }
    public int CastingSpeed { get; private set; }
    public double? Accuracy { get; private set; }
    public double? Evasion { get; private set; }
    public void SetAccuracyEvasion(double? accuracy, double? evasion)
    { Accuracy = accuracy; Evasion = evasion; Changed?.Invoke(); }
    public int? Might { get; private set; }
    public int? Grace { get; private set; }
    public int? Aether { get; private set; }
    public int? Resolve { get; private set; }
    public void SetEffectiveAttributes(int? might, int? grace, int? aether, int? resolve)
    { Might=might; Grace=grace; Aether=aether; Resolve=resolve; Changed?.Invoke(); }

    [Header("Character Stats")]
    [SerializeField]
    private GameObject statsPanel;

    [SerializeField]
    private TMP_Text statsText;

    [Header("CP (server authoritative)")]
    [SerializeField] private Slider cpSlider;
    [SerializeField] private TMP_Text cpText;
    public int CurrentCp { get; private set; }
    public int MaxCp { get; private set; }

    private void Awake()
    {
        // No demo/fabricated CP before the first server snapshot.
        if (cpSlider) { cpSlider.maxValue = 1; cpSlider.SetValueWithoutNotify(0); }
        if (cpText) cpText.text = "— / —";
    }

    public void SetCombatPoints(int current, int maximum)
    {
        HasCombatPoints = true;
        MaxCp = Mathf.Max(0, maximum); CurrentCp = Mathf.Clamp(current, 0, MaxCp);
        if (cpSlider) { cpSlider.maxValue = Mathf.Max(1, MaxCp); cpSlider.SetValueWithoutNotify(CurrentCp); }
        if (cpText) cpText.text = $"{CurrentCp} / {MaxCp}";
        Changed?.Invoke();
    }

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
        HasStats=true; MaxHp=maxHp; PhysicalAttack=physicalAttack; PhysicalDefense=physicalDefense;
        MagicAttack=magicAttack; MagicDefense=magicDefense; AttackSpeed=attackSpeed; CastingSpeed=castingSpeed;
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
        Changed?.Invoke();
    }

    public void SetHealth(
        int currentHp,
        int maxHp)
    {
        HasHealth=true; CurrentHp=currentHp; MaxHp=maxHp;
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
        Changed?.Invoke();
    }

    public void SetMana(
        int currentMp,
        int maxMp)
    {
        HasMana=true; CurrentMp=currentMp; MaxMp=maxMp;
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
        Changed?.Invoke();
    }
}