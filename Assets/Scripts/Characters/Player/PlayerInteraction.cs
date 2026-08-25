using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    private PlayerMovement _movement;
    private PlayerCombat _combat;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
    }

    public void OnGroundClicked(Vector3 position)
    {
        if (_combat.IsAttackLocked)
        {
            _combat.StopAttack(position);
            return;
        }

        _combat.StopAttack();
        _movement.MoveTo(position);
    }

    public void OnMonsterClicked(Monster monster)
    {
        if (TargetManager.Instance.CurrentTarget != monster)
        {
            TargetManager.Instance.SetTarget(monster);
            return;
        }

        _combat.StartAttack(monster);
    }
}