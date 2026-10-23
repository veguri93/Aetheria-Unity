using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// UI-only adapter: reserve a HUD-originating mouse press before gameplay actions
// process it. The UI input module has separate actions and continues normally.
// This also covers move+press arriving together before the world's cached UI check.
[DisallowMultipleComponent]
public sealed class PlayerHUDPointerGuard : MonoBehaviour
{
    [SerializeField] private PlayerInputController playerInput;
    [SerializeField] private RectTransform menuButtons;
    private InputAction reservedAction;
    private Mouse reservedMouse;
    private bool released;
    private void OnEnable(){InputSystem.onEvent+=BeforeMouseEvent;}
    private void BeforeMouseEvent(InputEventPtr evt,InputDevice device)
    {
        if(!(device is Mouse mouse)||(!evt.IsA<StateEvent>()&&!evt.IsA<DeltaStateEvent>()))return;
        if(!mouse.leftButton.ReadValueFromEvent(evt,out float down))return;
        if(reservedAction!=null){if(mouse==reservedMouse&&down<.5f)released=true;return;}
        if(!playerInput&&LocalPlayer.Instance)playerInput=LocalPlayer.Instance.GetComponent<PlayerInputController>();
        if(down<.5f||!playerInput||!playerInput.isActiveAndEnabled||playerInput.Controls==null)return;
        Vector2 point=mouse.position.ReadValueFromEvent(evt,out var position)?position:mouse.position.ReadValue();
        var canvas=GetComponentInParent<Canvas>().rootCanvas;
        var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        if(!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform,point,camera)&&
            !(menuButtons&&menuButtons.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(menuButtons,point,camera)))return;
        var action=playerInput.Controls.Player.LeftClick;
        if(!action.enabled)return;
        reservedAction=action;reservedMouse=mouse;released=false;action.Disable();
    }
    private void LateUpdate(){if(reservedAction!=null&&(released||reservedMouse==null||!reservedMouse.added||!reservedMouse.leftButton.isPressed))Release();}
    private void Release(){var action=reservedAction;reservedAction=null;reservedMouse=null;released=false;if(action!=null&&playerInput&&playerInput.isActiveAndEnabled)action.Enable();}
    private void OnDisable(){InputSystem.onEvent-=BeforeMouseEvent;Release();}
    private void OnApplicationFocus(bool focus){if(!focus)Release();}
}
