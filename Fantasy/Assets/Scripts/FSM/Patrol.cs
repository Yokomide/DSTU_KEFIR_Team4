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

        if ((Enemy.GetComponent<EnemyStats>().enemyHp > 0) && NPC.CompareTag("Enemy"))
        {
            if (Enemy.GetComponent<EnemyStats>().enemyHp < Enemy.GetComponent<EnemyStats>().enemyStats.hp)

            {
                // standardHp = Enemy.GetComponent<EnemyStats>().enemyHp;
                NPC_Move.speed = 5f;
                Enemy.GetComponent<EnemyStats>().Escape();

            }

            if (NPC_Move.GetComponent<NavMeshAgent>() != null && (Enemy.GetComponent<EnemyStats>().enemyHp == Enemy.GetComponent<EnemyStats>().enemyStats.hp))
            {
                NPC_Move.speed = 1f;
                NPC_Move.SetDestination(direction.position);

            }

        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
