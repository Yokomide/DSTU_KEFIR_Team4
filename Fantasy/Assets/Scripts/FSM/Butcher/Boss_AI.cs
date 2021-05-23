using UnityEngine;

public class Boss_AI : MonoBehaviour
{

    Animator animAi;
    Butcher_FSM boss_base;
    [SerializeField]
    private Transform direction;
    public GameObject player;
    private MainHeroHp hero;
    public GameObject GetPlayer()
    {
        return player;
    }

    void Attack()
    {
        hero = player.GetComponent<MainHeroHp>();
        if (hero.HeroHp < 0)
        {
            animAi.SetBool("dead", true);

        }
    }

    public void StopAttack()
    {
        CancelInvoke("Attack");
    }

    public void StartAttack()
    {
        if (gameObject.GetComponent<BossStats_>().isAlive)
        {
            InvokeRepeating("Attack", 0.2f, 0f);
        }
    }
    void Start()
    {
        animAi = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

        if (GetComponent<Boss_Move>() != null)
        {
            direction = GetComponent<Boss_Move>().GetGoal();

            if (CompareTag("Boss"))
            {
                animAi.SetFloat("distance", Vector3.Distance(transform.position, player.transform.position));
            }
        }
    }
}
