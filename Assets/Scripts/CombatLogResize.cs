using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Top-edge vertical resize; conversation and input remain anchored to the bottom.</summary>
public sealed class CombatLogResize : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform chatRoot;
    [SerializeField] private RectTransform logPanel;
    [SerializeField] private ScrollRect logScroll;
    [SerializeField] private Image grip;
    [SerializeField] private Graphic[] gripDetails;
    [SerializeField, Min(80)] private float minimumHeight = 110;
    [SerializeField, Min(110)] private float maximumHeight = 360;
    private Vector2 dragStart;
    private float startingHeight, fixedHeight, scrollInset, viewportInset, scrollbarInset;
    private bool dragging, followBottom;

    private void Awake()
    {
        fixedHeight = chatRoot.rect.height - logPanel.rect.height;
        scrollInset = logPanel.rect.height - ((RectTransform)logScroll.transform).rect.height;
        viewportInset = ((RectTransform)logScroll.transform).rect.height - logScroll.viewport.rect.height;
        scrollbarInset = ((RectTransform)logScroll.transform).rect.height - ((RectTransform)logScroll.verticalScrollbar.transform).rect.height;
    }
    public void OnBeginDrag(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || !LocalPoint(e, out dragStart)) return;
        dragging = true; startingHeight = logPanel.rect.height;
        followBottom = logScroll.content.rect.height <= logScroll.viewport.rect.height + 1 || logScroll.verticalNormalizedPosition <= .015f;
        logScroll.GetComponent<ChatHistoryScroll>()?.OnBeginDrag(e);
    }
    public void OnDrag(PointerEventData e)
    {
        if (!dragging || !LocalPoint(e, out var point)) return;
        float available = chatRoot.parent is RectTransform parent ? parent.rect.height - chatRoot.anchoredPosition.y - fixedHeight - 16 : maximumHeight;
        float height = Mathf.Clamp(startingHeight + point.y - dragStart.y, minimumHeight, Mathf.Max(minimumHeight, Mathf.Min(maximumHeight, available)));
        chatRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fixedHeight + height);
        logPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        var scrollRect = (RectTransform)logScroll.transform;
        scrollRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height - scrollInset);
        logScroll.viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollRect.rect.height - viewportInset);
        ((RectTransform)logScroll.verticalScrollbar.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollRect.rect.height - scrollbarInset);
        if (followBottom) { Canvas.ForceUpdateCanvases(); logScroll.verticalNormalizedPosition = 0; }
    }
    public void OnEndDrag(PointerEventData e)
    {
        if (!dragging) return;
        dragging = false; logScroll.GetComponent<ChatHistoryScroll>()?.OnEndDrag(e);
        if (followBottom) { Canvas.ForceUpdateCanvases(); logScroll.verticalNormalizedPosition = 0; }
    }
    private bool LocalPoint(PointerEventData e, out Vector2 point) => RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)chatRoot.parent,e.position,e.pressEventCamera,out point);
    public void OnPointerEnter(PointerEventData e) { TintGrip(new Color(1.55f,1.35f,1.05f,1f)); }
    public void OnPointerExit(PointerEventData e) { TintGrip(Color.white); }
    private void TintGrip(Color color)
    {
        if (grip && grip.enabled) grip.CrossFadeColor(color,.12f,true,true);
        if (gripDetails != null) foreach (var detail in gripDetails) if (detail) detail.CrossFadeColor(color,.12f,true,true);
    }
    private void OnDisable()
    {
        if (dragging && logScroll) logScroll.GetComponent<ChatHistoryScroll>()?.OnEndDrag(null);
        dragging = false;
    }
}


