using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
       
    }

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        PlayerCombat combat = animator.GetComponentInParent<PlayerCombat>();

        if (combat != null)
        {
            combat.OnAttackFinished();
        }
    }
}