using UnityEngine;

public class escape : NPCBase_FSM
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NPC_Move.speed = 20f;
    }
}
