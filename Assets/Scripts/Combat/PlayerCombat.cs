using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _attackDelay = 1f;

    private PlayerMovement _movement;
    private Animator _animator;

    private Monster _target;

    private bool _isAutoAttacking;
    private bool _isAttackLocked;
    private Vector3 _lastAttackDestination;
    public bool IsAttackLocked => _isAttackLocked;

    private float _nextAttackTime;

    private bool _stopRequested;
    private Vector3 _queuedDestination;
    private bool _hasQueuedMovement;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!_isAutoAttacking || _target == null)
            return;

        if (_target.IsDead)
        {
            StopAttack();
            return;
        }

        if (_stopRequested)
            return;

        if (_movement.IsMoving)
            return;

        if (!CanAttack())
        {
            MoveIntoRange();
            return;
        }

        PerformAttack();
    }

    public void StartAttack(Monster monster)
    {
        if (monster == null || monster.IsDead)
            return;

        _target = monster;
        _isAutoAttacking = true;

        _lastAttackDestination =
            Vector3.positiveInfinity;

        MoveIntoRange();
    }

    public void StopAttack(Vector3 destination)
    {
        if (_stopRequested)
            return;

        _stopRequested = true;
        _queuedDestination = destination;
        _hasQueuedMovement = true;
    }
    public void StopAttack()
    {
        _isAutoAttacking = false;
        _target = null;
        _nextAttackTime = 0f;
        _isAttackLocked = false;
    }
    public void OnAttackFinished()
    {
        if (!_isAttackLocked)
            return;


        _isAttackLocked = false;

        if (_stopRequested)
        {
            StopAttack();

            _stopRequested = false;

            if (_hasQueuedMovement)
            {
                _movement.MoveTo(_queuedDestination);
                _hasQueuedMovement = false;
            }

            return;
        }

        if (_target == null)
        {
            StopAttack();
        }
    }

    private void MoveIntoRange()
    {
        Vector3 direction = (transform.position - _target.transform.position).normalized;
        Vector3 destination = _target.transform.position + direction * _attackRange;

        if (Vector3.Distance(_lastAttackDestination, destination) > 0.1f)
        {
            _lastAttackDestination = destination;
            _movement.MoveTo(destination);
        }
    }

    private bool CanAttack()
    {
        return Vector3.Distance(transform.position, _target.transform.position) <= _attackRange + 0.2f;
    }

    private void PerformAttack()
    {
        if (_isAttackLocked)
            return;

        _movement.LookAt(
            _target.transform.position);

        if (Time.time < _nextAttackTime)
            return;

        _isAttackLocked = true;

        GameServerConnection.Instance?.SendAttackRequest(
            _target.ObjectId);

        _animator.SetTrigger("Attack");

        _nextAttackTime =
            Time.time + _attackDelay;
    }
}