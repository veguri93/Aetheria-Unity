using UnityEngine;

public class MonsterAttackStateBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        MonsterAI ai =
            animator.GetComponent<MonsterAI>();

        if (ai != null)
        {
            ai.OnAttackFinished();
        }
    }
}