using System.Collections;
using UnityEngine;

public class HeroAttack : MonoBehaviour
{
    public GameObject hero;
    public AudioClip sound;
    public AudioClip miss;
    public Animator animator;
    private MainHeroHp heroStats;
    private Player _playerMove;
    private float _tempSpeed;
    public GameObject effect;
    private GameObject tempEffect;
    private GameObject Corp;
    AudioSource audio;
    public float _attackCoolDown = -2f;
    public float coolDownTimer = 1.5f;

    private bool _missattack;
    private void Start()
    {
        _missattack = true;
        _attackCoolDown = coolDownTimer;
        _tempSpeed = hero.GetComponent<Player>().speed;
    }

    private void Awake()
    {
        animator = hero.GetComponent<Animator>();
        hero.AddComponent<AudioSource>();
        audio = hero.GetComponent<AudioSource>();
        hero.GetComponent<AudioSource>().clip = sound;
        heroStats = hero.GetComponent<MainHeroHp>();
        Corp = GameObject.Find("Corpses");
    }
    private void Update()
    {
        _attackCoolDown += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer && hero.GetComponent<MainHeroHp>().HeroHp > 0)
        {
            hero.GetComponent<AnimMoveset>().AttackAnimation();
            _attackCoolDown = 0f;
            hero.GetComponent<Player>().speed = 0.2f;
            audio.PlayOneShot(miss);
            StartCoroutine(StopOnAttack(_tempSpeed));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")))
            {
                if (!other.GetComponent<EnemyStats>().isBeenAttacked && other.GetComponent<EnemyStats>().isAlive)
                {
                    tempEffect = Instantiate(effect, other.transform.position, Quaternion.identity);
                    other.GetComponent<EnemyStats>().AttackM();
                    audio.PlayOneShot(sound);
                    other.GetComponent<EnemyStats>().enemyHp -= (Random.Range(20, 40) + heroStats.damage);
                    if (other.GetComponent<EnemyStats>().enemyHp <= 0)
                    {
                        
                        other.GetComponent<EnemyStats>().AttackM();
                        heroStats.ExpNum += Random.Range(25, 75);
                        other.GetComponent<EnemyStats>().isAlive = false;
                    }
                }

            }
            if (other.CompareTag("Boss"))
            {
                if ((!other.GetComponent<BossStats_>().isBeenAttacked && other.GetComponent<BossStats_>().isAlive) || (!other.GetComponent<KnightStats_>().isBeenAttacked && other.GetComponent<KnightStats_>().isAlive))
                {
                    tempEffect = Instantiate(effect, other.transform.position, Quaternion.identity);
                    other.GetComponent<BossStats_>().AttackM(hero.GetComponent<MainHeroHp>().damage);
                    other.GetComponent<KnightStats_>().AttackM(hero.GetComponent<MainHeroHp>().damage);
                    audio.PlayOneShot(sound);
                    other.GetComponent<BossStats_>().bossHp -= (Random.Range(20, 40) + heroStats.damage);
                    other.GetComponent<KnightStats_>().knightHp -= (Random.Range(20, 40) + heroStats.damage);
                    if (other.GetComponent<BossStats_>().bossHp <= 0)
                    {
                        other.GetComponent<EnemyStats>().AttackM();
                        other.GetComponent<BossStats_>().isAlive = false;
                        heroStats.ExpNum += 250;
                    }
                    if (other.GetComponent<KnightStats_>().knightHp <= 0)
                    {
                        other.GetComponent<EnemyStats>().AttackM();
                        other.GetComponent<KnightStats_>().isAlive = false;
                        heroStats.ExpNum += 500;
                    }
                }
            }
            if (other.CompareTag("Boss"))
            {
                if (!other.GetComponent<KnightStats_>().isBeenAttacked && other.GetComponent<KnightStats_>().isAlive)
                {
                    tempEffect = Instantiate(effect, other.transform.position, Quaternion.identity);
                    other.GetComponent<KnightStats_>().AttackM(hero.GetComponent<MainHeroHp>().damage);
                    audio.PlayOneShot(sound);
                    other.GetComponent<KnightStats_>().knightHp -= (Random.Range(20, 40) + heroStats.damage);
                    if (other.GetComponent<KnightStats_>().knightHp <= 0)
                    {
                        other.GetComponent<EnemyStats>().AttackM();
                        other.GetComponent<KnightStats_>().isAlive = false;
                        heroStats.ExpNum += 500;
                    }
                }
            }
            StartCoroutine(EffectFade(tempEffect));
        }

    }
    IEnumerator StopOnAttack(float tempSpeed)
    {
        yield return new WaitForSeconds(1f);
        hero.GetComponent<Player>().speed = tempSpeed;
    }
    IEnumerator EffectFade(GameObject effect)
    {
        yield return new WaitForSeconds(2);
        Destroy(effect);
    }
}
