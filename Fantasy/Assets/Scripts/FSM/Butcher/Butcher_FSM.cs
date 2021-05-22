using UnityEngine;
using UnityEngine.AI;

public class Butcher_FSM : StateMachineBehaviour
{
    public GameObject boss;
    public GameObject opponent;
    public Transform direction;
    public NavMeshAgent boss_move;
    public float damageMin;
    public float damageMax;
    public Transform Direction()
    {
        return direction;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        boss = animator.gameObject;
        boss_move = boss.GetComponent<NavMeshAgent>();
        opponent = boss.GetComponent<Boss_AI>().GetPlayer();
        direction = boss.GetComponent<Boss_Move>().GetGoal();
    }

}
