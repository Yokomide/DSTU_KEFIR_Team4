using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float _attackCoolDown = -2f;
    public MainHeroHp player;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MainHeroHp>();
    }

    private void Update()
    {
        if (gameObject.GetComponent<EnemyStats>().isAlive)
        {
            _attackCoolDown += Time.deltaTime;
            if (_attackCoolDown > 1f && Vector3.Distance(gameObject.GetComponent<EnemyStats>().GetComponent<Transform>().position, player.GetComponent<Transform>().position) < 4f)
            {
                _attackCoolDown = 0;
                Attack();
            }
        }
    }

    private void Attack()
    {
        player.HeroHp -= Random.Range(10, 15);
        if (player.GetComponent<HealingSkill>().lerping)
        {
            player.GetComponent<HealingSkill>().endHealingAmount-= Random.Range(10, 15);
        }
    }
}
