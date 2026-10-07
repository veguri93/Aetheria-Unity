using UnityEngine;
using UnityEngine.EventSystems;

public class SelfTargetUI : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        if (LocalPlayer.Instance == null ||
            TargetManager.Instance == null)
        {
            return;
        }

        TargetManager.Instance.SetTarget(
            LocalPlayer.Instance);

    }
}