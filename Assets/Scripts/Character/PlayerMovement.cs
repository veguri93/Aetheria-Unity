
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    private Animator _animator;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;

    private Vector3 _destination;
    private bool _isMoving;
    private bool _hasSpawned;

    public bool IsMoving => _isMoving;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -20f;

    private float _verticalVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _destination = transform.position;
        _hasSpawned = false;
    }

    private void Update()
    {
        if (!_hasSpawned)
            return;

        Move();
        ApplyGravity();
    }

    private void Move()
    {
        if (!_isMoving)
            return;

        Vector3 direction =
            _destination - transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance <= 0.1f)
        {
            Stop();
            return;
        }

        direction.Normalize();

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);

        _controller.Move(
            direction *
            _moveSpeed *
            Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded &&
            _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }

        _verticalVelocity +=
            _gravity * Time.deltaTime;

        _controller.Move(
            Vector3.up *
            _verticalVelocity *
            Time.deltaTime);
    }

    public void OnSpawnReceived()
    {
        _destination = transform.position;
        _isMoving = false;
        _verticalVelocity = 0f;

        if (_animator != null)
        {
            _animator.SetBool(
                "Moving",
                false);
        }

        _hasSpawned = true;
    }

    public void MoveTo(Vector3 position)
    {
        _destination = position;
        _isMoving = true;

        _animator.SetBool(
            "Moving",
            true);
    }

    public void Stop()
    {
        _destination = transform.position;
        _isMoving = false;

        _animator.SetBool(
            "Moving",
            false);
    }

    public void LookAt(Vector3 position)
    {
        Vector3 direction =
            position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }
}
