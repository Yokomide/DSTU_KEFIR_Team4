using UnityEngine;

public class Boss_Attack : Butcher_FSM
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        boss.GetComponent<Boss_AI>().StartAttack();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 playerPos = opponent.transform.position - boss.transform.position;
        boss.transform.rotation = Quaternion.Lerp(boss.transform.rotation, Quaternion.LookRotation(playerPos), 5.0f * Time.deltaTime);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        boss.GetComponent<Boss_AI>().StopAttack();
    }


}
