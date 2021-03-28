using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimDeath : MonoBehaviour
{
    public AttackRadiusTrigger art;

    void Update()
    {
        art._deathAnim = art.GetComponent<Animator>();
        
            if (art.GetComponent<EnemyStats>().hp < 0)
        {
            art._deathAnim.SetTrigger("Active");
        }
    }
}
