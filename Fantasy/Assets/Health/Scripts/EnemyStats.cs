using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : MonoBehaviour
{
    private float _maxHp = 100;
    public float hp;
    private AttackRadiusTrigger _beingAttacked;

    private void Start()
    {
        hp = 100;
    }


}
