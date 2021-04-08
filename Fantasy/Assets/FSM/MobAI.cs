using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    Animator animAi;
    NPCBase_FSM NPC_base;
    public Transform direction;
    public GameObject player;
    public float maxHP;
    public float HP;
    public GameObject GetPlayer()
    {
        return player;
    }

    void Start()
    {
        animAi = GetComponent<Animator>();
        maxHP = this.GetComponent<EnemyStats>().maxHp;
        HP = this.GetComponent<EnemyStats>().hp;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.CompareTag("Citizen") && maxHP > HP)
        {
            maxHP = HP;
            animAi.SetBool("escape", true);
            
        }
        if (this.GetComponent<MobMoving>() != null)
        {
            direction = this.GetComponent<MobMoving>().GetGoal();

            if (this.CompareTag("Enemy"))
            {
                animAi.SetFloat("distance", Vector3.Distance(transform.position, player.transform.position));
            }
        }
    }
}
