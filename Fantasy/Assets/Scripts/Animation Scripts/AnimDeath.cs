using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimDeath : MonoBehaviour
{
    public AttackRadiusTrigger AttackRadiusT;
    private 

    void Update()
    {
            if (AttackRadiusT.GetComponent<EnemyStats>().enemyStats.hp < 0)
        {
            AttackRadiusT.DeathAnim.SetTrigger("Active");
        }
    }
}
