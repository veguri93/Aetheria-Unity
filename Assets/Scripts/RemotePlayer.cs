using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public int PlayerId { get; private set; }

    public string PlayerName { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    private Animator _animator;

    private float _lastMovementTime;

    [SerializeField]
    private float _moveSmoothSpeed = 12f;

    [SerializeField]
    private float _rotationSmoothSpeed = 12f;

    [SerializeField]
    private float _movementTimeout = 0.2f;

    public void Initialize(
        int playerId,
        string playerName,
        string title)
    {
        PlayerId = playerId;

        PlayerName = playerName;
        Title = title;

        _targetPosition =
            transform.position;

        _targetRotation =
            transform.rotation;

        _animator =
            GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogWarning(
                $"RemotePlayer {playerId} has no Animator.");
        }

        SetMoving(false);
    }

    private void Update()
    {
        transform.position =
            Vector3.Lerp(
                transform.position,
                _targetPosition,
                _moveSmoothSpeed * Time.deltaTime);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                _targetRotation,
                _rotationSmoothSpeed * Time.deltaTime);

        bool isMoving =
            Time.time - _lastMovementTime <
            _movementTimeout;

        SetMoving(isMoving);
    }

    public void SetTargetPosition(
        Vector3 position,
        float rotationY)
    {
        _targetPosition =
            position;

        _targetRotation =
            Quaternion.Euler(
                0f,
                rotationY,
                0f);

        _lastMovementTime =
            Time.time;
    }

    private void SetMoving(bool moving)
    {
        if (_animator == null)
            return;

        _animator.SetBool(
            "Moving",
            moving);
    }
}