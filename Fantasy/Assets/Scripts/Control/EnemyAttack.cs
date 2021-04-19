using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float _attackCoolDown = 1.5f;
    public MainHeroHp player;

    private EnemyStats _enemyStats;

    private Transform _enemyTransform;
    private Transform _playerTransform;
    private void Awake()
    {
        _enemyStats = gameObject.GetComponent<EnemyStats>();
        _enemyTransform = _enemyStats.GetComponent<Transform>();
        _playerTransform = player.GetComponent<Transform>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MainHeroHp>();
    }

    private void Update()
    {
        if (_enemyStats.isAlive)
        {
            _attackCoolDown += Time.deltaTime;
            if (_attackCoolDown > 1.5f && Vector3.Distance(_enemyTransform.position, _playerTransform.position) < 4f)
            {
                _attackCoolDown = 0;
                Attack();
            }
        }
    }

    private void Attack()
    {
        player.heroStats.HeroHp -= Random.Range(10, 15);
    }
}
