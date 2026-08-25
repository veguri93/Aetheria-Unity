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

    public bool IsMoving => _isMoving;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _destination = transform.position;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (!_isMoving)
            return;

        Vector3 direction = _destination - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        if (distance <= 0.1f)
        {
            _isMoving = false;
            _animator.SetBool("Moving", false);
            return;
        }

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime);

        _controller.Move(direction * _moveSpeed * Time.deltaTime);
    }

    public void MoveTo(Vector3 position)
    {
       

        _destination = position;
        _isMoving = true;
        _animator.SetBool("Moving", true);
    }

    public void LookAt(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}