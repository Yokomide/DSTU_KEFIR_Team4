using UnityEngine;

public class Knight_Attack : Knight_FSM
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        knight.GetComponent<Knight_AI>().StartAttack();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 playerPos = opponent.transform.position - knight.transform.position;
        knight.transform.rotation = Quaternion.Lerp(knight.transform.rotation, Quaternion.LookRotation(playerPos), 5.0f * Time.deltaTime);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        knight.GetComponent<Knight_AI>().StopAttack();
    }


}
