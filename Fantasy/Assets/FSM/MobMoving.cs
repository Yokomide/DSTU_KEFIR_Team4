using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobMoving : MonoBehaviour
{

    public Transform goal;

    public Transform GetGoal() {

        return goal;
    }


    void Start()
    {   
        InvokeRepeating("GameObject_ChangePosition", 0.0f, 5.0f);
    }

    void GameObject_ChangePosition()
    {
        goal.position = new Vector3(Random.Range(-40.0F, 40.0F), 0, Random.Range(-40F, 40F));
    }
}
