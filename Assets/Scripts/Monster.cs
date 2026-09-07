using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] private GameObject _selectionCircle;
    [SerializeField] private HealthBar _healthBar;
    [SerializeField] private int _maxHp = 100;

    private int _objectId;
    private int _currentHp;
    private Animator _animator;
    private bool _isDead;
    private MonsterAI _ai;

    public Animator Animator => _animator;

    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;
    public bool IsDead => _isDead;
    public int ObjectId => _objectId;
    private string _name;

    public string Name => _name;
    public void SetObjectId(int objectId)
    {
        _objectId = objectId;
    }
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _ai = GetComponent<MonsterAI>();
        _currentHp = _maxHp;

        if (_selectionCircle != null)
            _selectionCircle.SetActive(false);

        if (_healthBar != null)
        {
            _healthBar.SetHealth(
                _currentHp,
                _maxHp);
        }
    }
    public void SetNpcName(string name)
    {
        _name = name;
    }
    public void Select()
    {
        if (_isDead)
            return;

        if (_selectionCircle != null)
            _selectionCircle.SetActive(true);
    }

    public void Deselect()
    {
        if (_selectionCircle != null)
            _selectionCircle.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (_isDead)
            return;

        if (damage <= 0)
            return;

        _currentHp -= damage;

        if (_currentHp < 0)
            _currentHp = 0;

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
        _currentHp = currentHp;

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
        _isDead = true;

        Deselect();

        if (_animator != null)
        {
            _animator.SetTrigger("Death");
        }
    }
    public void RespawnFromServer(
        Vector3 position,
        float rotation)
    {
        gameObject.SetActive(true);

        transform.position = position;

        transform.rotation =
            Quaternion.Euler(
                0f,
                rotation,
                0f);

        _isDead = false;
        _currentHp = _maxHp;

        if (_healthBar != null)
        {
            _healthBar.SetHealth(
                _currentHp,
                _maxHp);
        }

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }

        if (_ai != null)
        {
            _ai.ResetAfterRespawn();
        }
    }

    public void DespawnFromServer()
    {
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

        gameObject.SetActive(false);
    }
}