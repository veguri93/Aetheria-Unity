using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Quiet focus/hover light for chat controls, with no idle animation.</summary>
[RequireComponent(typeof(Selectable))]
public sealed class ChatControlFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private CanvasGroup illumination;
    [SerializeField] private ChatHistoryScroll history;
    private Coroutine transition;
    private bool hovered, focused, dragging;
    public void OnPointerEnter(PointerEventData e) { hovered = true; Refresh(); }
    public void OnPointerExit(PointerEventData e) { hovered = false; Refresh(); }
    public void OnSelect(BaseEventData e) { focused = true; Refresh(); }
    public void OnDeselect(BaseEventData e) { focused = false; Refresh(); }
    public void OnPointerDown(PointerEventData e) { if (history) history.InterruptFollow(); }
    public void OnPointerUp(PointerEventData e) { dragging = false; Refresh(); }
    public void OnBeginDrag(PointerEventData e) { dragging = true; if (history) history.InterruptFollow(); Refresh(); }
    public void OnEndDrag(PointerEventData e) { dragging = false; Refresh(); }
    private void Refresh()
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        if (illumination && isActiveAndEnabled) transition = StartCoroutine(Blend(focused || dragging ? .85f : hovered ? .5f : .12f));
    }
    private IEnumerator Blend(float alpha)
    {
        float from = illumination.alpha;
        for (float elapsed = 0; elapsed < .16f; elapsed += Time.unscaledDeltaTime)
        {
            illumination.alpha = Mathf.Lerp(from, alpha, elapsed / .16f);
            yield return null;
        }
        illumination.alpha = alpha; transition = null;
    }
    private void OnDisable()
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        hovered = focused = dragging = false;
        if (illumination) illumination.alpha = .12f;
    }
}
