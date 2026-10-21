using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Coalesces message layout work and follows the tail only when the reader was there.</summary>
[RequireComponent(typeof(ScrollRect))]
public sealed class ChatHistoryScroll : MonoBehaviour, IScrollHandler, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect scroll;
    private Coroutine pending;
    private bool followPending, dragging;

    public static void BeforeAppend(ScrollRect target)
    {
        if (!target) return;
        var history = target.GetComponent<ChatHistoryScroll>();
        if (!history) history = target.gameObject.AddComponent<ChatHistoryScroll>();
        history.Schedule();
    }
    private void Schedule()
    {
        if (!isActiveAndEnabled || pending != null) return;
        if (!scroll) scroll = GetComponent<ScrollRect>();
        followPending = !dragging && (scroll.content.rect.height <= scroll.viewport.rect.height + 1 || scroll.verticalNormalizedPosition <= .015f);
        pending = StartCoroutine(ApplyLayout());
    }
    private IEnumerator ApplyLayout()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (scroll.content) LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        if (followPending && !dragging)
        {
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 0;
        }
        pending = null;
    }
    public void InterruptFollow() => followPending = false;
    public void OnScroll(PointerEventData e) => InterruptFollow();
    public void OnBeginDrag(PointerEventData e) { dragging = true; InterruptFollow(); }
    public void OnEndDrag(PointerEventData e) => dragging = false;
    private void OnDisable()
    {
        if (pending != null) StopCoroutine(pending);
        pending = null;
        dragging = followPending = false;
    }
}
