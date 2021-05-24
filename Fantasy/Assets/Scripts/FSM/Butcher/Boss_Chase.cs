using UnityEngine;

public class Boss_Chase : Butcher_FSM
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var direction = opponent.transform.position;
        boss_move.speed = 2f;
        boss_move.SetDestination(direction);
    }
}
