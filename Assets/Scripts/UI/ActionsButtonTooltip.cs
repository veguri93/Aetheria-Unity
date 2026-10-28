using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class ActionsButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [SerializeField] private ActionsWindow window;
    [SerializeField] private string actionName;
    public string ActionName => actionName;
    public void OnPointerEnter(PointerEventData data) => window?.ShowTooltip(this);
    public void OnPointerExit(PointerEventData data) => window?.HideTooltip(this);
    public void OnSelect(BaseEventData data) => window?.ShowTooltip(this);
    public void OnDeselect(BaseEventData data) => window?.HideTooltip(this);
    private void OnDisable() => window?.HideTooltip(this);
}
