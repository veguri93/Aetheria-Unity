using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _attackDelay = 1f;
    private PlayerMovement _movement;
    private Animator _animator;
    private Monster _target;
    private bool _isAutoAttacking, _isAttackLocked, _stopRequested, _hasQueuedMovement;
    private Vector3 _queuedDestination, _lastAttackDestination;
    private float _nextAttackTime;
    private bool _attackRequestPending;
    private long _requestSequence, _activeRequestId;
    private int _pendingTargetObjectId = -1;
    public bool IsAttackLocked => _isAttackLocked;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!_isAutoAttacking) return;
        if (_target == null || _target.IsDead || !_target.gameObject.activeInHierarchy)
        {
            FinishStopAndMove();
            return;
        }
        if (_stopRequested) return;
        if (!CanAttack())
        {
            CancelServerAttack();
            MoveIntoRange();
            return;
        }
        // This movement belongs to the active combat approach, not a ground-click command.
        if (_movement.IsMoving) _movement.Stop();
        PerformAttack();
    }

    public void OnAttackConnectionLost()
    {
        ClearRequest();
        StopAttack();
    }

    public void SetAttackRange(float range)
    {
        if (float.IsNaN(range) || float.IsInfinity(range) || range <= 0) return;
        _attackRange = range;
        RefreshApproach();
    }

    public void SetAttackInterval(float seconds)
    {
        if (!float.IsNaN(seconds) && !float.IsInfinity(seconds) && seconds > 0)
            _attackDelay = seconds;
    }

    public void StartAttack(Monster monster)
    {
        if (monster == null || monster.IsDead) return;
        if (_target != monster) CancelServerAttack();
        _target = monster;
        TargetManager.Instance?.SetAttackTarget(monster);
        _isAutoAttacking = true;
        _stopRequested = _hasQueuedMovement = false;
        RefreshApproach();
    }

    private void RefreshApproach()
    {
        _lastAttackDestination = Vector3.positiveInfinity;
        if (!_isAutoAttacking || _stopRequested || _target == null || _target.IsDead) return;
        if (CanAttack()) _movement.Stop();
        else { CancelServerAttack(); MoveIntoRange(); }
    }

    public void StopAttack(Vector3 destination)
    {
        CancelServerAttack();
        TargetManager.Instance?.ClearAttackTarget(_target);
        _stopRequested = true;
        _queuedDestination = destination;
        _hasQueuedMovement = true;
        if (!_isAttackLocked) FinishStopAndMove();
    }

    public void StopAttack()
    {
        CancelServerAttack();
        TargetManager.Instance?.ClearAttackTarget(_target);
        _isAutoAttacking = false;
        _target = null;
        _isAttackLocked = _stopRequested = _hasQueuedMovement = false;
    }

    public void OnTargetUnavailable(Monster monster)
    {
        if (_target != monster) return;
        FinishStopAndMove();
    }

    private void FinishStopAndMove()
    {
        bool move = _hasQueuedMovement;
        Vector3 destination = _queuedDestination;
        StopAttack();
        _movement.Stop();
        if (move) _movement.MoveTo(destination);
    }

    public void OnAttackFinished()
    {
        if (!_isAttackLocked) return;
        _isAttackLocked = false;
        if (_stopRequested) FinishStopAndMove();
    }

    private void MoveIntoRange()
    {
        Vector3 direction = transform.position - _target.transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude < 0.0001f) direction = -Vector3.forward;
        Vector3 destination = _target.transform.position + direction.normalized * _attackRange;
        if (Vector3.Distance(_lastAttackDestination, destination) > 0.1f)
        {
            _lastAttackDestination = destination;
            _movement.MoveTo(destination);
        }
    }

    private bool CanAttack()
    {
        Vector3 difference = transform.position - _target.transform.position;
        difference.y = 0;
        return difference.sqrMagnitude <= (_attackRange + 0.2f) * (_attackRange + 0.2f);
    }

    private void PerformAttack()
    {
        if (_attackRequestPending || Time.unscaledTime < _nextAttackTime ||
            GameServerConnection.Instance == null) return;
        _movement.LookAt(_target.transform.position);
        _pendingTargetObjectId = _target.ObjectId;
        _activeRequestId = ++_requestSequence;
        _attackRequestPending = true;
        GameServerConnection.Instance.SendAttackRequest(_pendingTargetObjectId, _activeRequestId);
    }

    private void ClearRequest()
    {
        _attackRequestPending = false;
        _pendingTargetObjectId = -1;
        _activeRequestId = 0;
    }

    private void CancelServerAttack()
    {
        if (!_attackRequestPending) return;
        long requestId = _activeRequestId;
        ClearRequest(); // Clear before sending: a send failure may call back synchronously.
        GameServerConnection.Instance?.SendAttackCancel(requestId);
    }

    public void OnAttackResult(int objectId, string result, int milliseconds, long requestId)
    {
        if (!_attackRequestPending || requestId != _activeRequestId ||
            objectId != _pendingTargetObjectId) return;
        if (result != "ACCEPTED")
        {
            ClearRequest();
            _nextAttackTime = Time.unscaledTime + Mathf.Max(0.25f, milliseconds / 1000f);
            return;
        }
        SetAttackInterval(milliseconds / 1000f);
        if (!_isAutoAttacking || _stopRequested || _target == null || _target.IsDead) return;
        _movement.LookAt(_target.transform.position);
        _isAttackLocked = _animator != null;
        if (_animator != null) _animator.Play("Base Layer.Attacking", 0, 0f);
    }
}
