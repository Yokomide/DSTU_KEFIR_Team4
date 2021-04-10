using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCBase_FSM : StateMachineBehaviour
{
    public GameObject NPC;
    public GameObject opponent;
    public Transform direction;
    public NavMeshAgent NPC_Move;

    public Transform Direction()
    {
        return direction;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NPC = animator.gameObject;
        NPC_Move = NPC.GetComponent<NavMeshAgent>();
        opponent = NPC.GetComponent<MobAI>().GetPlayer();
        direction = NPC.GetComponent<MobMoving>().GetGoal();
    }

}
