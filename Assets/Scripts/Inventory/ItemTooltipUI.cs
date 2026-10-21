
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance
    {
        get;
        private set;
    }

    [SerializeField]
    private GameObject tooltipRoot;

    [SerializeField]
    private TMP_Text itemNameText;

    [SerializeField]
    private TMP_Text itemTypeText;

    [SerializeField]
    private TMP_Text itemTierText;

    [SerializeField]
    private TMP_Text statsText;

    [SerializeField]
    private TMP_Text descriptionText;


    [SerializeField]
    private RectTransform separatorTop;

    [SerializeField]
    private RectTransform separatorBottom;


    private void Awake()
    {
        // The scene has both a Systems host and a component on the shared root.
        // A late duplicate must not hide the first tooltip while it is opening.
        if (Instance != null && Instance != this) { enabled = false; return; }
        Instance = this;
        foreach (var graphic in tooltipRoot.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;
        var group = tooltipRoot.GetComponent<CanvasGroup>();
        if (group == null) group = tooltipRoot.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        tooltipRoot.SetActive(false);
    }

    public void Show(
        ItemVisualDefinition definition,
        RectTransform itemRect)
    {
        if (InventoryUI.ItemInteractionDialogOpen || InventoryItemContextMenu.IsOpen) return;
        itemNameText.text =
            definition.ItemName;

        if (ClientInventory.TryGetItemByItemId(
                definition.ItemId,
                out ClientItemInstance item))
        {
            itemTypeText.text =
                string.IsNullOrEmpty(item.DisplayType)
                    ? definition.Type
                    : item.DisplayType;



            itemTierText.text =
                item.Tier == "None"
                    ? string.Empty
                    : item.Tier;



            bool hasStats =
                item.PhysicalAttack.HasValue ||
                item.MagicAttack.HasValue ||
                item.CastingSpeed.HasValue ||
                item.AttackSpeed.HasValue;

            statsText.gameObject.SetActive(
                hasStats);

            statsText.text =
                "P.Atk: " + FormatStat(
                    item.PhysicalAttack) + "\n" +
                "M.Atk: " + FormatStat(
                    item.MagicAttack) + "\n" +
                "Casting Speed: " + FormatStat(
                    item.CastingSpeed) + "\n" +
                "Attack Speed: " + FormatStat(
                    item.AttackSpeed);
        }
        else
        {
            itemTypeText.text =
                definition.Type;

            itemTierText.text =
                string.Empty;

            statsText.text =
                string.Empty;

            statsText.gameObject.SetActive(false);
        }

        descriptionText.text =
            definition.Description;

        UpdateLayout();

        ShowAt(itemRect);
    }


    private void UpdateLayout()
    {
        RectTransform tooltipRect =
            tooltipRoot.GetComponent<RectTransform>();

        RectTransform statsRect =
            statsText.rectTransform;

        RectTransform descriptionRect =
            descriptionText.rectTransform;

        float contentWidth =
            tooltipRect.rect.width - 20f;

        float currentY = 54f;

        bool hasStats =
            statsText.gameObject.activeSelf;

        bool hasDescription =
            !string.IsNullOrWhiteSpace(
                descriptionText.text);

        separatorTop.gameObject.SetActive(
            hasStats || hasDescription);

        separatorBottom.gameObject.SetActive(
            hasStats && hasDescription);

        if (hasStats)
        {
            float statsHeight =
                Mathf.Ceil(
                    statsText.GetPreferredValues(
                        statsText.text,
                        contentWidth,
                        Mathf.Infinity).y) + 2f;

            statsRect.anchoredPosition =
                new Vector2(0f, -currentY);

            statsRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                statsHeight);

            currentY += statsHeight + 12f;
        }

        if (hasStats && hasDescription)
        {
            separatorBottom.anchoredPosition =
                new Vector2(0f, -currentY);

            currentY += 10f;
        }

        if (hasDescription)
        {
            float descriptionHeight =
                Mathf.Ceil(
                    descriptionText.GetPreferredValues(
                        descriptionText.text,
                        contentWidth,
                        Mathf.Infinity).y) + 2f;

            descriptionRect.anchoredPosition =
                new Vector2(0f, -currentY);

            descriptionRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                descriptionHeight);

            currentY += descriptionHeight;
        }

        tooltipRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            currentY + 14f);
    }


    private static string FormatStat(
        double? value)
    {
        return value.HasValue
            ? value.Value.ToString(
                "0.##",
                CultureInfo.InvariantCulture)
            : "—";
    }

    public void ShowSkill(
        SkillClientData skill,
        RectTransform skillRect)
    {
        // Skills remain immediate and cannot race a pending inventory hover.
        InventoryUI.InterruptTooltips();
        if (InventoryItemContextMenu.IsOpen) return;
        itemNameText.text =
            skill.DisplayName;

        itemTypeText.text =
            "Skill";

        itemTierText.text =
            string.Empty;

        statsText.text =
            string.Empty;

        statsText.gameObject.SetActive(false);

        descriptionText.text =
            skill.Description;

        UpdateLayout();

        ShowAt(skillRect);
    }

    private void ShowAt(
        RectTransform sourceRect)
    {
        tooltipRoot.transform.SetAsLastSibling();

        tooltipRoot.SetActive(true);

        RectTransform tooltipRect =
            tooltipRoot.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];

        sourceRect.GetWorldCorners(corners);

        Vector3 bottomCenter =
            (corners[0] + corners[3]) / 2f;

        tooltipRect.position =
            bottomCenter +
            new Vector3(0, -5f, 0);
    }

    public void Hide()
    {
        tooltipRoot.SetActive(false);
    }
}


