using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler
{
    [SerializeField]
    private RectTransform window;

    private Vector2 _pointerOffset;

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (window == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            window,
            eventData.position,
            eventData.pressEventCamera,
            out _pointerOffset);
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (window == null ||
            window.parent is not RectTransform parent)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        window.anchoredPosition =
            localPoint - _pointerOffset;
    }
}