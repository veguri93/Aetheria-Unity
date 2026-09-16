using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillEntryUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IShortcutDragSource
{
    [SerializeField]
    private Image icon;

    private int skillId;

    private SkillClientData skill;

    private GameObject dragIconObject;
    private Canvas rootCanvas;

    public int SkillId =>
        skillId;

    public bool HasSkill =>
        skillId > 0;

    public SkillClientData Skill =>
        skill;

    public ShortcutType ShortcutType =>
        ShortcutType.Skill;

    public int ShortcutReferenceId =>
        skillId;

    public Sprite ShortcutIcon =>
        icon != null
            ? icon.sprite
            : null;

    private void Awake()
    {
        rootCanvas =
            GetComponentInParent<Canvas>();
    }

    public void SetSkill(
        SkillClientData skillData)
    {
        skill =
            skillData;

        skillId =
            skillData.SkillId;

        if (icon != null)
        {
            icon.gameObject.SetActive(true);

            icon.sprite =
                skillData.Icon;
        }
    }

    public void ClearSkill()
    {
        skill =
            null;

        skillId =
            0;

        if (icon != null)
        {
            icon.sprite =
                null;

            icon.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (!HasSkill)
            return;

        if (skill == null)
            return;

        ItemTooltipUI.Instance?.ShowSkill(
            skill,
            transform as RectTransform);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();

        if (!HasSkill)
            return;

        if (skill == null)
            return;

        if (rootCanvas == null)
            return;

        dragIconObject =
            new GameObject(
                "SkillDragIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        dragIconObject.transform.SetParent(
            rootCanvas.transform,
            false);

        dragIconObject.transform.SetAsLastSibling();

        Image dragImage =
            dragIconObject.GetComponent<Image>();

        dragImage.sprite =
            icon.sprite;

        dragImage.preserveAspect =
            true;

        dragImage.raycastTarget =
            false;

        RectTransform dragRect =
            dragIconObject.GetComponent<RectTransform>();

        dragRect.sizeDelta =
            new Vector2(
                32f,
                32f);

        dragRect.position =
            eventData.position;
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (dragIconObject == null)
            return;

        dragIconObject.transform.position =
            eventData.position;
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (dragIconObject == null)
            return;

        Destroy(
            dragIconObject);

        dragIconObject =
            null;
    }
}