using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] private GameObject _selectionCircle;
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

        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(
                false);
        }

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

        transform.position =
            Vector3.Lerp(
                transform.position,
                _serverTargetPosition,
                positionT);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                _serverTargetRotation,
                rotationT);

        float remainingDistance =
            Vector3.Distance(
                transform.position,
                _serverTargetPosition);

        if (remainingDistance <= 0.02f)
        {
            transform.position =
                _serverTargetPosition;

            transform.rotation =
                _serverTargetRotation;

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
        string name)
    {
        _name =
            name;
    }

    public void Select()
    {
        if (_isDead)
            return;

        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(
                true);
        }
    }

    public void Deselect()
    {
        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(
                false);
        }
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
        _currentHp =
            currentHp;

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

        if (_ai != null &&
            LocalPlayer.Instance != null)
        {
            _ai.SetAggro(
                LocalPlayer.Instance);
        }
    }

    private void Die()
    {
        _isDead =
            true;

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

        transform.position =
            position;

        transform.rotation =
            Quaternion.Euler(
                0f,
                rotation,
                0f);

        _serverTargetPosition =
            position;

        _serverTargetRotation =
            transform.rotation;

        _hasServerMovementTarget =
            false;

        _isDead =
            false;

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

        if (TargetManager.Instance?.CurrentTarget == this)
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