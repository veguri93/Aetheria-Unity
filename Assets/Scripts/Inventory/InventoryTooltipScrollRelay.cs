using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shares ScrollRect's GameObject without taking over its scroll/drag processing.
[RequireComponent(typeof(ScrollRect)), DisallowMultipleComponent]
public sealed class InventoryTooltipScrollRelay : MonoBehaviour, IScrollHandler, IBeginDragHandler
{
    private ScrollRect scroll;
    private void Awake() => scroll = GetComponent<ScrollRect>();
    private void OnEnable() => scroll.onValueChanged.AddListener(OnScrolled);
    private void OnDisable() => scroll.onValueChanged.RemoveListener(OnScrolled);
    private void OnScrolled(Vector2 _) => InventoryUI.InterruptTooltips();
    public void OnScroll(PointerEventData eventData) => InventoryUI.InterruptTooltips();
    public void OnBeginDrag(PointerEventData eventData) => InventoryUI.InterruptTooltips();
}
