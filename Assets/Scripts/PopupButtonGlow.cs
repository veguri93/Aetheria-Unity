using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Short, unscaled border transitions; idle buttons have no running animation.</summary>
[RequireComponent(typeof(Button))]
public sealed class PopupButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private CanvasGroup borderLight;
    [SerializeField, Min(.01f)] private float transitionSeconds = .14f;
    private Button button;
    private Coroutine transition;
    private bool hovered, selected;
    private void OnEnable()
    {
        button = GetComponent<Button>();
        hovered = selected = false;
        if (borderLight) borderLight.alpha = .28f;
    }
    public void OnPointerEnter(PointerEventData e) { hovered = true; Animate(.9f); }
    public void OnPointerExit(PointerEventData e) { hovered = false; Animate(selected ? .75f : .28f); }
    public void OnPointerDown(PointerEventData e) => Animate(.45f);
    public void OnPointerUp(PointerEventData e) => Animate(hovered || selected ? .9f : .28f);
    public void OnSelect(BaseEventData e) { selected = true; Animate(.75f); }
    public void OnDeselect(BaseEventData e) { selected = false; Animate(hovered ? .9f : .28f); }
    private void Animate(float target)
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        if (borderLight && isActiveAndEnabled) transition = StartCoroutine(Blend(target));
    }
    private IEnumerator Blend(float target)
    {
        float from = borderLight.alpha;
        for (float elapsed = 0; elapsed < transitionSeconds; elapsed += Time.unscaledDeltaTime)
        {
            borderLight.alpha = Mathf.Lerp(from, button.IsInteractable() ? target : .16f,
                Mathf.SmoothStep(0, 1, elapsed / transitionSeconds));
            yield return null;
        }
        borderLight.alpha = button.IsInteractable() ? target : .16f;
        transition = null;
    }
    private void OnDisable()
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        hovered = selected = false;
        if (borderLight) borderLight.alpha = .28f;
    }
}
