using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private int _maxHp = 100;

    private int _currentHp;
    private Animator _animator;

    private bool _isDead;

    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _currentHp =
            _maxHp;

        _animator =
            GetComponentInChildren<Animator>();
    }

    public void TakeDamage(
        int damage)
    {
        if (damage <= 0)
            return;

        if (_isDead)
            return;

        _currentHp -= damage;

        if (_currentHp < 0)
            _currentHp = 0;

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    public void SetServerHealth(
        int currentHp,
        int maxHp)
    {
        _currentHp =
            currentHp;

        _maxHp =
            maxHp;

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        PlayerCombat combat =
            GetComponent<PlayerCombat>();

        if (combat != null)
        {
            combat.StopAttack();
        }

        PlayerMovement movement =
            GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (_animator != null)
        {
            _animator.SetTrigger(
                "Death");
        }
    }

    public void Respawn()
    {
        _isDead = false;

        _currentHp =
            _maxHp;

        PlayerMovement movement =
            GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.enabled = true;
        }

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
            _animator.Play(
                "Idle",
                0,
                0f);
        }
    }
}