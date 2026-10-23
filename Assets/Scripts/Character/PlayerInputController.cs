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
        if (_pointerOverUI ||
            InventoryUI.ItemDragConsumesWorldPointer ||
            InventoryUI.ItemInteractionDialogOpen)
        {
            return;
        }

        Vector2 mousePosition =
            _controls.Player.MousePosition.ReadValue<Vector2>();

        Ray ray =
            _camera.ScreenPointToRay(mousePosition);

        RaycastHit[] hits =
            Physics.RaycastAll(ray);

        if (hits == null || hits.Length == 0)
            return;

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance));

        Vector3? groundPoint = null;

        foreach (RaycastHit hit in hits)
        {
            // Ignore our own character completely,
            // so clicks can pass through to ground or other targets.
            if (hit.collider.GetComponentInParent<LocalPlayer>() != null)
                continue;

            Monster monster =
                hit.collider.GetComponentInParent<Monster>();

            if (monster != null)
            {
                _player.Interaction.OnMonsterClicked(
                    monster);

                return;
            }

            RemotePlayer remotePlayer =
                hit.collider.GetComponentInParent<RemotePlayer>();

            if (remotePlayer != null)
            {
                _player.Interaction.OnRemotePlayerClicked(
                    remotePlayer);

                return;
            }

            Npc npc =
                hit.collider.GetComponentInParent<Npc>();

            if (npc != null)
            {
                _player.Interaction.OnNpcClicked(
                    npc);

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

            // Save first valid non-self hit as ground fallback.
            if (groundPoint == null)
            {
                groundPoint = hit.point;
            }
        }

        if (groundPoint.HasValue)
        {
            _player.Interaction.OnGroundClicked(
                groundPoint.Value);
        }
    }
}