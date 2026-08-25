using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _yaw;
    [SerializeField] private Transform _pitch;
    [SerializeField] private Camera _camera;

    [Header("Follow")]
    [SerializeField] private float _followSpeed = 10f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 0.1f;
    [SerializeField] private float _minPitch = -20f;
    [SerializeField] private float _maxPitch = 65f;

    [Header("Zoom")]

    [SerializeField] private float _minZoom = -2f;
    [SerializeField] private float _maxZoom = -12f;

    private PlayerInputActions _controls;
    private float _currentZoom;
    private float _yawRotation;
    private float _pitchRotation;

    private void Awake()
    {
        _controls = new PlayerInputActions();

        _yawRotation = _yaw.localEulerAngles.y;

        _pitchRotation = _pitch.localEulerAngles.x;

        _currentZoom = _camera.transform.localPosition.z;

        if (_pitchRotation > 180f)
            _pitchRotation -= 360f;
    }

    private void OnEnable()
    {
        if (_controls == null)
            _controls = new PlayerInputActions();

        _controls.Enable();
    }

    private void OnDisable()
    {
        if (_controls == null)
            return;

        _controls.Disable();
    }

    private void LateUpdate()
    {
        RotateCamera();
        FollowTarget();
        ZoomCamera();
    }

    private void FollowTarget()
    {
        if (_target == null)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            _target.position,
            _followSpeed * Time.deltaTime);
    }

    private void ZoomCamera()
    {
        float scroll = _controls.Player.Zoom.ReadValue<float>();

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        _currentZoom += scroll * 10f;
        _currentZoom = Mathf.Clamp(_currentZoom, _maxZoom, _minZoom);

        Vector3 position = _camera.transform.localPosition;
        position.z = Mathf.Lerp(position.z, _currentZoom, 10f * Time.deltaTime);
        _camera.transform.localPosition = position;
    }

    private void RotateCamera()
    {
        if (!_controls.Player.RightClick.IsPressed())
            return;

        Vector2 look = _controls.Player.Look.ReadValue<Vector2>();

        _yawRotation += look.x * _rotationSpeed;

        _pitchRotation -= look.y * _rotationSpeed;
        _pitchRotation = Mathf.Clamp(
            _pitchRotation,
            _minPitch,
            _maxPitch);

        _yaw.localRotation = Quaternion.Euler(0f, _yawRotation, 0f);
        _pitch.localRotation = Quaternion.Euler(_pitchRotation, 0f, 0f);
    }
}