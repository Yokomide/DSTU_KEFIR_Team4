using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobMoving : MonoBehaviour
{

    private AttackRadiusTrigger art;

    public AnimDeath AD;
    public Transform goal;


    void Start()
    {

        InvokeRepeating("GameObject_ChangePosition", 0.0f, 5.0f);
    }

    void GameObject_ChangePosition()
    {
        goal.position = new Vector3(Random.Range(-40.0F, 40.0F), 0, Random.Range(-40F, 40F));
    }

    void Update()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent.GetComponent<NavMeshAgent>() != null)
        {
           agent.SetDestination(goal.position);
        }
  



    }
}
