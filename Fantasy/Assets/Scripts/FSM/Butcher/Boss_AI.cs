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
        if (Vector3.Distance(gameObject.GetComponent<BossStats_>().GetComponent<Transform>().position, player.GetComponent<Transform>().position) < 4f)
        {
            EventManager.OnGetAttack(); // dealing damage through the EventManager
            // in case of problems in future line bellow is saved here
            //hero.heroStats.HeroHp -= Random.Range(10, 15);
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

        if (GetComponent<Boss_Move>() != null)
        {
            direction = GetComponent<Boss_Move>().GetGoal();

            if (CompareTag("Enemy"))
            {
                animAi.SetFloat("distance", Vector3.Distance(transform.position, player.transform.position));
            }
        }
    }
}
