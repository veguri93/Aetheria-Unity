using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance { get; private set; }

    public event System.Action<ITargetable> TargetChanged;

    public ITargetable CurrentTarget { get; private set; }
    public ITargetable AttackTarget { get; private set; }

    // Presentation state only: combat/network authority remains in existing systems.
    public void SetAttackTarget(ITargetable target)
    {
        AttackTarget = target;
        RefreshIndicator();
    }

    public void ClearAttackTarget(ITargetable target)
    {
        if (!ReferenceEquals(AttackTarget, target)) return;
        AttackTarget = null;
        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        if (CurrentTarget is Component component && !component) return;
        if (CurrentTarget is Monster monster && monster.IsDead)
        {
            OverheadTargetIndicator.SetFor(CurrentTarget, TargetIndicatorState.None);
            return;
        }
        OverheadTargetIndicator.SetFor(CurrentTarget,
            ReferenceEquals(CurrentTarget, AttackTarget) ? TargetIndicatorState.Hostile : TargetIndicatorState.Selected);
    }

    private void LateUpdate()
    {
        if (AttackTarget is Component attacker && (!attacker || !attacker.gameObject.activeInHierarchy))
            AttackTarget = null;
        if (CurrentTarget is Component component && (!component || !component.gameObject.activeInHierarchy))
            ClearTarget();
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetTarget(
        ITargetable target)
    {

        if (target is Component candidate && (!candidate || !candidate.gameObject.activeInHierarchy)) return;
        if (target is Monster dead && dead.IsDead) return;

        if (ReferenceEquals(
                CurrentTarget,
                target))
        {


            return;
        }

        if (CurrentTarget != null && (!(CurrentTarget is Component oldComponent) || oldComponent))
        {


            CurrentTarget.Deselect();
        }

        CurrentTarget =
            target;
        if (CurrentTarget == null) TargetChanged?.Invoke(null);



        if (CurrentTarget != null)
        {


            CurrentTarget.Select();
            RefreshIndicator();


            TargetChanged?.Invoke(CurrentTarget);

            GameServerConnection.Instance?
                .SendTargetSelect(
                    CurrentTarget.ObjectId);
        }
    }

    public void ClearTarget() => ClearTarget(true);

    public void ClearTarget(bool stopCombat)
    {
        if (stopCombat) LocalPlayer.Instance?.GetComponent<PlayerCombat>()?.StopAttack();
        AttackTarget = null;


        if (CurrentTarget != null && (!(CurrentTarget is Component component) || component))
        {
            CurrentTarget.Deselect();
        }

        CurrentTarget =
            null;

        TargetChanged?.Invoke(null);
        GameServerConnection.Instance?
            .SendTargetSelect(
                0);
    }
}
