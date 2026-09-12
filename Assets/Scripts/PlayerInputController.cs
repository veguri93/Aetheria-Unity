using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerInputController : MonoBehaviour
{
    private Player _player;
    private Camera _camera;
    private PlayerInputActions _controls;

    private bool _pointerOverUI;

    public PlayerInputActions Controls => _controls;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _camera = Camera.main;

        _controls = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (_controls == null)
            _controls = new PlayerInputActions();

        _controls.Enable();
        _controls.Player.LeftClick.performed += OnLeftClick;
    }

    private void OnDisable()
    {
        if (_controls == null)
            return;

        _controls.Player.LeftClick.performed -= OnLeftClick;
        _controls.Disable();
    }

    private void Update()
    {
        if (EventSystem.current != null)
        {
            _pointerOverUI =
                EventSystem.current.IsPointerOverGameObject();
        }
        else
        {
            _pointerOverUI = false;
        }
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        // UI gets the click instead of the game world.
        if (_pointerOverUI)
            return;

        Vector2 mousePosition =
            _controls.Player.MousePosition.ReadValue<Vector2>();

        Ray ray =
            _camera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit))
        {
            return;
        }

        Monster monster =
            hit.collider.GetComponentInParent<Monster>();

        if (monster != null)
        {
            _player.Interaction.OnMonsterClicked(
                monster);

            return;
        }

        WorldItemDrop worldItem =
            hit.collider.GetComponentInParent<WorldItemDrop>();

        if (worldItem != null)
        {
            _player.Interaction.OnWorldItemClicked(
                worldItem);

            return;
        }

        _player.Interaction.OnGroundClicked(
            hit.point);
    }
}