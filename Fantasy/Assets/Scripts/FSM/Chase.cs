using UnityEngine;

public class Chase : NPCBase_FSM
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var direction = opponent.transform.position;
        NPC_Move.speed = 5f;
        NPC_Move.SetDestination(direction);
    }

}
