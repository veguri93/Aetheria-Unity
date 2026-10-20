using UnityEngine;

public class Monster : MonoBehaviour, ITargetable
{
    [SerializeField] private HealthBar _healthBar;
    [SerializeField] private int _maxHp = 100;

    [Header("Network Movement")]
    [SerializeField] private float _positionSmoothing = 12f;
    [SerializeField] private float _rotationSmoothing = 12f;

    private int _objectId;
    private int _currentHp;
    private Animator _animator;
    private bool _isDead;
    private MonsterAI _ai;

    private Vector3 _serverTargetPosition;
    private Quaternion _serverTargetRotation;
    private bool _hasServerMovementTarget;

    public Animator Animator => _animator;

    public bool HasServerHealth { get; private set; }
    public event System.Action<int, int, bool> ServerHealthChanged;

    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;
    public bool IsDead => _isDead;
    public int ObjectId => _objectId;

    private string _name;

    public string Name => _name;

    private void Awake()
    {
        _animator =
            GetComponent<Animator>();

        _ai =
            GetComponent<MonsterAI>();

        _currentHp =
            _maxHp;

        _serverTargetPosition =
            transform.position;

        _serverTargetRotation =
            transform.rotation;

        

        if (_healthBar != null)
        {
            _healthBar.SetHealth(
                _currentHp,
                _maxHp);
        }
    }

    private void Update()
    {
        UpdateServerMovement();
    }

    private void UpdateServerMovement()
    {
        if (!_hasServerMovementTarget)
            return;

        if (_isDead)
            return;

        float positionT =
            1f -
            Mathf.Exp(
                -_positionSmoothing *
                Time.deltaTime);

        float rotationT =
            1f -
            Mathf.Exp(
                -_rotationSmoothing *
                Time.deltaTime);

        
        transform.SetPositionAndRotation(
Vector3.Lerp(
                transform.position,
                _serverTargetPosition,
                positionT),
Quaternion.Slerp(
                transform.rotation,
                _serverTargetRotation,
                rotationT));
        float remainingDistance =
            Vector3.Distance(
                transform.position,
                _serverTargetPosition);

        if (remainingDistance <= 0.02f)
        {
            
            transform.SetPositionAndRotation(
_serverTargetPosition,
_serverTargetRotation);
            _hasServerMovementTarget =
                false;

            if (_ai != null)
            {
                _ai.SetServerRunning(
                    false);
            }
        }
    }

    public void SetServerMovement(
        Vector3 position,
        float rotation)
    {
        _serverTargetPosition =
            position;

        _serverTargetRotation =
            Quaternion.Euler(
                0f,
                rotation,
                0f);

        _hasServerMovementTarget =
            true;

        if (_ai != null)
        {
            _ai.SetServerRunning(
                true);
        }
    }

    public void SetObjectId(
        int objectId)
    {
        _objectId =
            objectId;
    }


    public void SetNpcName(
        string name,
        string title = "")
    {
        _name = name;

        if (TryGetComponent<WorldNameplate>(out var nameplate))
        {
            nameplate.SetInfo(
                name,
                title);
        }
    }


    public void Select()
    {
        if (_isDead) return;
        OverheadTargetIndicator.SetFor(this, TargetIndicatorState.Selected);
    }

    public void Deselect()
    {
        OverheadTargetIndicator.SetFor(this, TargetIndicatorState.None);
    }

    public void TakeDamage(
        int damage)
    {
        if (_isDead)
            return;

        if (damage <= 0)
            return;

        _currentHp -=
            damage;

        if (_currentHp < 0)
        {
            _currentHp =
                0;
        }

        if (_healthBar != null)
        {
            _healthBar.SetHealth(
                _currentHp,
                _maxHp);
        }

        if (_currentHp <= 0)
        {
            Die();
        }

        if (_ai != null &&
            LocalPlayer.Instance != null)
        {
            _ai.SetAggro(
                LocalPlayer.Instance);
        }
    }

    public void SetServerHealth(
        int currentHp,
        int maxHp)
    {
        bool firstSnapshot = !HasServerHealth;
        int previousHp = _currentHp;
        _maxHp = Mathf.Max(1, maxHp);
        _currentHp = Mathf.Clamp(currentHp, 0, _maxHp);
        HasServerHealth = true;
        ServerHealthChanged?.Invoke(_currentHp, _maxHp, firstSnapshot);

        if (_healthBar != null)
        {
            _healthBar.SetHealth(
                _currentHp,
                maxHp);
        }

        if (_currentHp <= 0)
        {
            Die();
            return;
        }

        // A spawn/selection snapshot must not provoke an otherwise idle monster.
        if (!firstSnapshot && _currentHp < previousHp && _ai != null &&
            LocalPlayer.Instance != null)
        {
            _ai.SetAggro(LocalPlayer.Instance);
        }
    }

    private void OnDisable()
    {
        if (TargetManager.Instance && ReferenceEquals(TargetManager.Instance.CurrentTarget, this))
            TargetManager.Instance.ClearTarget();
    }

    private void Die()
    {
        _isDead =
            true;
        if (TargetManager.Instance && ReferenceEquals(TargetManager.Instance.CurrentTarget, this))
            TargetManager.Instance.ClearTarget();
        else TargetManager.Instance?.ClearAttackTarget(this);

        _hasServerMovementTarget =
            false;

        Deselect();

        if (_animator != null)
        {
            _animator.SetTrigger(
                "Death");
        }
    }

    public void RespawnFromServer(
        Vector3 position,
        float rotation)
    {
        gameObject.SetActive(
            true);

        
        transform.SetPositionAndRotation(
position,
Quaternion.Euler(
                0f,
                rotation,
                0f));
        _serverTargetPosition =
            position;

        _serverTargetRotation =
            transform.rotation;

        _hasServerMovementTarget =
            false;

        _isDead =
            false;
        HasServerHealth = false;

        _currentHp =
            _maxHp;

        if (_healthBar != null)
        {
            _healthBar.SetHealth(
                _currentHp,
                _maxHp);
        }

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(
                0f);
        }

        if (_ai != null)
        {
            _ai.ResetAfterRespawn();
        }
    }

    public void DespawnFromServer()
    {
        _hasServerMovementTarget =
            false;

        if (TargetManager.Instance != null &&
            ReferenceEquals(
                TargetManager.Instance.CurrentTarget,
                this))
        {
            TargetManager.Instance.ClearTarget();
        }

        PlayerCombat combat =
            LocalPlayer.Instance?
                .GetComponent<PlayerCombat>();

        if (combat != null)
        {
            combat.StopAttack();
        }

        gameObject.SetActive(
            false);
    }
}