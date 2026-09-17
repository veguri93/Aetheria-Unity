using UnityEngine;

[RequireComponent(typeof(Monster))]
public class MonsterAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float _patrolRadius = 5f;
    [SerializeField] private float _minWaitTime = 1f;
    [SerializeField] private float _maxWaitTime = 3f;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 6f;

    private Monster _monster;
    private Animator _animator;

    private Vector3 _spawnPosition;
    private Vector3 _targetPosition;

    private bool _isWalking;
    private float _waitTimer;
    private bool _isAttacking;

    [SerializeField]
    private float _attackRange = 1.5f;

    private LocalPlayer _aggroTarget;

    private void Awake()
    {
        _monster =
            GetComponent<Monster>();

        _animator =
            GetComponent<Animator>();

        _spawnPosition =
            transform.position;

        _waitTimer =
            Random.Range(
                _minWaitTime,
                _maxWaitTime);

        SetWalking(false);
    }

    private void Update()
    {
        if (_monster.IsDead)
            return;

        if (_aggroTarget != null)
        {
            PlayerHealth health =
                _aggroTarget.GetComponent<PlayerHealth>();

            if (health != null &&
                health.IsDead)
            {
                StopAggro();
                return;
            }
        }

        if (_isAttacking)
            return;

        if (_aggroTarget != null)
        {
            HandleAggro();
            return;
        }

        if (_isWalking)
        {
            WalkToTarget();
            return;
        }

        Wait();
    }

    private void SetRunning(
        bool running)
    {
        if (_animator == null)
            return;

        _animator.SetBool(
            "Run",
            running);
    }

    private void StopAggro()
    {
        _aggroTarget = null;

        _isAttacking = false;

        SetWalking(false);
        SetRunning(false);

        _waitTimer =
            Random.Range(
                _minWaitTime,
                _maxWaitTime);
    }

    private void HandleAggro()
    {
        float distance =
            Vector3.Distance(
                transform.position,
                _aggroTarget.transform.position);

        if (distance > _attackRange)
        {
            ChaseTarget();
            return;
        }

        StopMovement();

        Attack();
    }

    private void ChaseTarget()
    {
        // Server controls movement and the network
        // movement code controls the Run animation.
    }

    private void StopMovement()
    {
        _isWalking = false;

        SetWalking(false);
        SetRunning(false);
    }
    public void SetServerRunning(
    bool running)
    {
        if (running)
        {
            SetWalking(false);
        }

        SetRunning(
            running);
    }
    private void Attack()
    {
        if (_isAttacking)
            return;

        _isAttacking = true;

        SetWalking(false);
        SetRunning(false);

        if (_aggroTarget != null)
        {
            Vector3 direction =
                _aggroTarget.transform.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        direction);
            }
        }

        if (_animator != null)
        {
            _animator.SetTrigger(
                "Attack");
        }
    }

    public void OnAttackFinished()
    {
        _isAttacking = false;
    }

    private void Wait()
    {
        _waitTimer -=
            Time.deltaTime;

        if (_waitTimer > 0f)
            return;

        PickNewDestination();
    }

    private void PickNewDestination()
    {
        Vector2 random =
            Random.insideUnitCircle *
            _patrolRadius;

        _targetPosition =
            _spawnPosition +
            new Vector3(
                random.x,
                0f,
                random.y);

        _isWalking = true;

        SetWalking(true);
    }

    private void WalkToTarget()
    {
        Vector3 direction =
            _targetPosition -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance <= 0.15f)
        {
            StopWalking();
            return;
        }

        direction.Normalize();

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(
                    direction),
                _rotationSpeed *
                Time.deltaTime);

        float movement =
            Mathf.Min(
                _moveSpeed *
                Time.deltaTime,
                distance);

        transform.position +=
            direction *
            movement;
    }

    private void StopWalking()
    {
        _isWalking = false;

        SetWalking(false);

        _waitTimer =
            Random.Range(
                _minWaitTime,
                _maxWaitTime);
    }

    public void SetAggro(
        LocalPlayer player)
    {
        if (_monster.IsDead)
            return;

        if (player == null)
            return;

        _aggroTarget =
            player;
    }

    private void SetWalking(
        bool walking)
    {
        if (_animator == null)
            return;

        _animator.SetBool(
            "Walk",
            walking);
    }

    public void ResetAfterRespawn()
    {
        _aggroTarget = null;
        _isAttacking = false;
        _isWalking = false;

        SetWalking(false);
        SetRunning(false);

        _waitTimer =
            Random.Range(
                _minWaitTime,
                _maxWaitTime);
    }
}