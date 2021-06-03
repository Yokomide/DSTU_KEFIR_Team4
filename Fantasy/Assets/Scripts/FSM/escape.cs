using UnityEngine;

public class Escape : NPCBase_FSM
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if ((Enemy.GetComponent<EnemyStats>().enemyHp > 0))
        {
            if (Enemy.GetComponent<EnemyStats>().enemyHp < Enemy.GetComponent<EnemyStats>().enemyStats.hp && NPC.CompareTag("Citizen"))

            {
                NPC_Move.speed = 4f;
                Enemy.GetComponent<EnemyStats>().Escape();

            }

           else if (Enemy.GetComponent<EnemyStats>().enemyHp == Enemy.GetComponent<EnemyStats>().enemyStats.hp)
            {
                Enemy.GetComponent<Animator>().SetBool("escape", false);
            }
        }

    }

    public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
