using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    Animator animAi;
    NPCBase_FSM NPC_base;
    public Transform direction;
    public GameObject player;

    public GameObject GetPlayer()
    {
        return player;
    }

    void Start()
    {
        animAi = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = this.GetComponent<MobMoving>().GetGoal();

        if (this.CompareTag("Enemy"))
        {
            animAi.SetFloat("distance", Vector3.Distance(transform.position, player.transform.position));
        }
         
    }
}
