using UnityEngine;
using UnityEngine.AI;

public class Patrol : NPCBase_FSM
{


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Enemy.GetComponent<EnemyStats>().enemyHp > 0)
        {
            if (NPC_Move.GetComponent<NavMeshAgent>() != null)
            {
                NPC_Move.speed = 1f;
                NPC_Move.SetDestination(direction.position);
            }
        }
        else
        {
            NPC_Move.speed = 0f;
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
