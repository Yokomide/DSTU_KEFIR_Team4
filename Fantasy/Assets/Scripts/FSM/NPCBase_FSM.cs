using UnityEngine;
using UnityEngine.AI;

public class NPCBase_FSM : StateMachineBehaviour
{
    public GameObject NPC;
    public GameObject opponent;
    public Transform direction;
    public NavMeshAgent NPC_Move;
    public float standardHp;
    public float damageMax;
    public EnemyStats Enemy;
    public Transform Direction()
    {
        return direction;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NPC = animator.gameObject;
        Enemy = NPC.GetComponent<EnemyStats>();
        NPC_Move = NPC.GetComponent<NavMeshAgent>();
        opponent = NPC.GetComponent<MobAI>().GetPlayer();
        direction = NPC.GetComponent<MobMoving>().GetGoal();
        standardHp = NPC.GetComponent<EnemyStats>().enemyHp;

    }
    

}

