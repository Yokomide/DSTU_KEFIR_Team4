using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    Animator animAi;
    NPCBase_FSM NPC_base;
    public Transform direction;
    public GameObject player;
    public MainHeroHp hero;
    public GameObject GetPlayer()
    {
        return player;
    }

    void Attack()
    {
       

        hero = player.GetComponent<MainHeroHp>();
        if (hero.heroStats.HeroHp < 0)
        {
            animAi.SetBool("Dead", true);
            animAi.Play("Patrol");
        }
        if (Vector3.Distance(gameObject.GetComponent<EnemyStats>().GetComponent<Transform>().position, player.GetComponent<Transform>().position) < 4f)
        {
            hero.heroStats.HeroHp -= Random.Range(10, 15);
        }
    }

    public void StopAttack()
    {
        CancelInvoke("Attack");
    }

    public void StartAttack()
    {
        InvokeRepeating("Attack", 0.2f, 0f);
    }
    void Start()
    {   
        animAi = GetComponent<Animator>();
  
    }

    // Update is called once per frame
    void Update()
    {

        if (GetComponent<MobMoving>() != null)
        {
            direction = GetComponent<MobMoving>().GetGoal();

            if (CompareTag("Enemy"))
            {
                animAi.SetFloat("distance", Vector3.Distance(transform.position, player.transform.position));
            }
        }
    }
}
