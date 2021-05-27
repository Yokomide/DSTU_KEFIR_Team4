using UnityEngine;

public class Knight_Chase : Knight_FSM
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var direction = opponent.transform.position;
        knight_move.speed = 2f;
        knight_move.SetDestination(direction);
    }
}
