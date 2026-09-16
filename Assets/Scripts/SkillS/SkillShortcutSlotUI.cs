using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillShortcutSlotUI :
    MonoBehaviour,
    IPointerClickHandler,
    IDropHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IShortcutDragSource
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TMP_Text keyText;

    private int slotNumber;

    private ShortcutType shortcutType =
        ShortcutType.None;

    private int referenceId;

    private PlayerInteraction playerInteraction;
    private Canvas rootCanvas;

    private GameObject dragIconObject;

    private bool dragDropHandled;

    public int SlotNumber =>
        slotNumber;

    public ShortcutType ShortcutType =>
        shortcutType;

    public int ReferenceId =>
        referenceId;

    public bool HasShortcut =>
        shortcutType != ShortcutType.None &&
        referenceId > 0;

    public int SkillId =>
        shortcutType == ShortcutType.Skill
            ? referenceId
            : 0;

    public int ShortcutReferenceId =>
        referenceId;

    public Sprite ShortcutIcon =>
        icon != null
            ? icon.sprite
            : null;

    private void Awake()
    {
        playerInteraction =
            FindAnyObjectByType<PlayerInteraction>();

        rootCanvas =
            GetComponentInParent<Canvas>();
    }

    public void Initialize(
        int number)
    {
        slotNumber =
            number;

        if (keyText != null)
        {
            keyText.text =
                $"F{number}";
        }

        ClearShortcut();
    }

    public void SetSkill(
        SkillClientData skill)
    {
        SetShortcut(
            ShortcutType.Skill,
            skill.SkillId,
            skill.Icon);
    }

    public void SetItem(
        int itemId,
        Sprite itemSprite)
    {
        SetShortcut(
            ShortcutType.Item,
            itemId,
            itemSprite);
    }

    public void SetShortcut(
        ShortcutType type,
        int id,
        Sprite sprite)
    {
        if (type == ShortcutType.None ||
            id <= 0)
        {
            ClearShortcut();
            return;
        }

        shortcutType =
            type;

        referenceId =
            id;

        SetIcon(
            sprite);
    }

    public void ClearShortcut()
    {
        shortcutType =
            ShortcutType.None;

        referenceId =
            0;

        if (icon != null)
        {
            icon.sprite =
                null;

            icon.gameObject.SetActive(
                false);
        }
    }

    public void OnDrop(
        PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        IShortcutDragSource dragSource =
            FindShortcutDragSource(
                eventData.pointerDrag);

        if (dragSource == null)
            return;

        if (dragSource.ShortcutReferenceId <= 0)
            return;

        if (dragSource is SkillShortcutSlotUI sourceSlot)
        {
            HandleShortcutSlotDrop(
                sourceSlot);

            return;
        }

        SetShortcut(
            dragSource.ShortcutType,
            dragSource.ShortcutReferenceId,
            dragSource.ShortcutIcon);

        Debug.Log(
            $"[SHORTCUT] Assigned " +
            $"{dragSource.ShortcutType} " +
            $"{dragSource.ShortcutReferenceId} " +
            $"to F{slotNumber}.");
    }

    private void HandleShortcutSlotDrop(
        SkillShortcutSlotUI sourceSlot)
    {
        if (sourceSlot == this)
        {
            sourceSlot.dragDropHandled =
                true;

            return;
        }

        ShortcutType oldType =
            shortcutType;

        int oldReferenceId =
            referenceId;

        Sprite oldIcon =
            ShortcutIcon;

        ShortcutType sourceType =
            sourceSlot.ShortcutType;

        int sourceReferenceId =
            sourceSlot.ReferenceId;

        Sprite sourceIcon =
            sourceSlot.ShortcutIcon;

        SetShortcut(
            sourceType,
            sourceReferenceId,
            sourceIcon);

        if (oldType == ShortcutType.None ||
            oldReferenceId <= 0)
        {
            sourceSlot.ClearShortcut();
        }
        else
        {
            sourceSlot.SetShortcut(
                oldType,
                oldReferenceId,
                oldIcon);
        }

        sourceSlot.dragDropHandled =
            true;

        Debug.Log(
            $"[SHORTCUT] Moved shortcut " +
            $"from F{sourceSlot.SlotNumber} " +
            $"to F{slotNumber}.");
    }

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        if (!HasShortcut)
            return;

        if (playerInteraction == null)
        {
            playerInteraction =
                FindAnyObjectByType<PlayerInteraction>();
        }

        playerInteraction?.ActivateShortcut(
            slotNumber);
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (!HasShortcut)
            return;

        if (rootCanvas == null)
            return;

        dragDropHandled =
            false;

        dragIconObject =
            new GameObject(
                "ShortcutDragIcon",
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
            ShortcutIcon;

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
        if (dragIconObject != null)
        {
            Destroy(
                dragIconObject);

            dragIconObject =
                null;
        }

        if (!HasShortcut)
            return;

        if (dragDropHandled)
            return;

        Debug.Log(
            $"[SHORTCUT] Removed shortcut from F{slotNumber}.");

        ClearShortcut();
    }

    private static IShortcutDragSource FindShortcutDragSource(
        GameObject sourceObject)
    {
        MonoBehaviour[] behaviours =
            sourceObject.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IShortcutDragSource dragSource)
                return dragSource;
        }

        return null;
    }

    private void SetIcon(
        Sprite sprite)
    {
        if (icon == null)
            return;

        icon.sprite =
            sprite;

        icon.gameObject.SetActive(
            sprite != null);
    }
}