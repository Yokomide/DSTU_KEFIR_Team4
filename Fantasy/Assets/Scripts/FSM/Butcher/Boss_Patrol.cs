using UnityEngine;
using UnityEngine.AI;

public class Boss_Patrol : Butcher_FSM
{


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (boss_move.GetComponent<NavMeshAgent>() != null)
        {
            boss_move.speed = 3f;
            boss_move.SetDestination(direction.position);
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
