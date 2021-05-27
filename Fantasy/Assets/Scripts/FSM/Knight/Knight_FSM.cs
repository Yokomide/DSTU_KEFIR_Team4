using UnityEngine;
using UnityEngine.AI;

public class Knight_FSM : StateMachineBehaviour
{
    public GameObject knight;
    public GameObject opponent;
    public Transform direction;
    public NavMeshAgent knight_move;
    public float damageMin;
    public float damageMax;
    public Transform Direction()
    {
        return direction;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        knight = animator.gameObject;
        knight_move = knight.GetComponent<NavMeshAgent>();
        opponent = knight.GetComponent<Knight_AI>().GetPlayer();
        direction = knight.GetComponent<Knight_Move>().GetGoal();
    }

}
