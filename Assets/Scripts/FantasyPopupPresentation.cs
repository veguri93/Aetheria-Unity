using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Presentation and stacking only; invitation ownership and timers stay in their controllers.</summary>
[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public sealed class FantasyPopupPresentation : MonoBehaviour
{
    [SerializeField] private bool confirmationPriority;
    [SerializeField] private ScrollRect messageScroll;
    private static readonly List<FantasyPopupPresentation> Open = new List<FantasyPopupPresentation>(5);
    private Canvas popupCanvas;

    private void OnEnable() => Present();
    public void Present()
    {
        if (!isActiveAndEnabled) return;
        if (!popupCanvas) popupCanvas = GetComponent<Canvas>();
        Open.Remove(this);
        Open.Add(this);
        if (messageScroll)
        {
            // Text can change while this dialog is already open. Resolve its new height
            // before resetting the scroll so old inertia/offset cannot hide the new inviter.
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageScroll.content);
            messageScroll.StopMovement();
            messageScroll.verticalNormalizedPosition = 1;
            var position = messageScroll.content.anchoredPosition;
            messageScroll.content.anchoredPosition = new Vector2(position.x, 0);
        }
        Restack();
    }
    private void OnDisable()
    {
        Open.Remove(this);
        Restack();
    }
    private static void Restack()
    {
        int ordinary = 0, confirmation = 0;
        for (int i = Open.Count - 1; i >= 0; --i)
            if (!Open[i]) Open.RemoveAt(i);
        foreach (var popup in Open)
        {
            if (!popup.popupCanvas) popup.popupCanvas = popup.GetComponent<Canvas>();
            popup.popupCanvas.overrideSorting = true;
            popup.popupCanvas.sortingOrder = popup.confirmationPriority ? 400 + confirmation++ : 300 + ordinary++;
        }
    }
}
