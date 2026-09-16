using TMPro;
using UnityEngine;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance { get; private set; }

    [SerializeField]
    private GameObject tooltipRoot;

    [SerializeField]
    private TMP_Text itemNameText;

    [SerializeField]
    private TMP_Text itemTypeText;

    [SerializeField]
    private TMP_Text descriptionText;

    private void Awake()
    {
        Instance = this;

        tooltipRoot.SetActive(false);
    }

    public void Show(
        ItemVisualDefinition definition,
        RectTransform itemRect)
    {
        itemNameText.text =
            definition.ItemName;

        itemTypeText.text =
            $"Type: {definition.Type}";

        descriptionText.text =
            definition.Description;

        ShowAt(
            itemRect);
    }

    public void ShowSkill(
        SkillClientData skill,
        RectTransform skillRect)
    {
        itemNameText.text =
            skill.DisplayName;

        itemTypeText.text =
            "Skill";

        descriptionText.text =
            skill.Description;

        ShowAt(
            skillRect);
    }

    private void ShowAt(
        RectTransform sourceRect)
    {
        tooltipRoot.SetActive(true);

        RectTransform tooltipRect =
            tooltipRoot.GetComponent<RectTransform>();

        Vector3[] corners =
            new Vector3[4];

        sourceRect.GetWorldCorners(
            corners);

        Vector3 bottomCenter =
            (corners[0] + corners[3]) / 2f;

        tooltipRect.position =
            bottomCenter +
            new Vector3(
                0,
                -5f,
                0);
    }

    public void Hide()
    {
        tooltipRoot.SetActive(false);
    }
}