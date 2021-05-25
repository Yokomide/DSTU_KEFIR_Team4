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

            if (hero.GetComponent<MainHeroHp>().HeroHp > 0)
            {

                if (other.CompareTag("Enemy") || other.CompareTag("Citizen"))
                {

                    tempEffect = Instantiate(effect, other.transform.position, Quaternion.Euler(-90, 0, 0));
                    other.GetComponent<EnemyStats>().AttackM(hero.GetComponent<MainHeroHp>().damage);
                    audio.PlayOneShot(sound);
                    other.GetComponent<EnemyStats>().enemyHp -= Random.Range(10, 20);
                    if (other.GetComponent<EnemyStats>().enemyHp <= 0)
                    {
                        heroStats.ExpNum += Random.Range(50, 70);
                    }

                }
                if (other.CompareTag("Boss"))
                {
                    tempEffect = Instantiate(effect, other.transform.position, Quaternion.Euler(-90, 0, 0));
                    other.GetComponent<BossStats_>().AttackM(hero.GetComponent<MainHeroHp>().damage);
                    audio.PlayOneShot(sound);
                    other.GetComponent<BossStats_>().bossHp -= Random.Range(10, 20);
                    if (other.GetComponent<BossStats_>().bossHp <= 0)
                    {
                        heroStats.ExpNum += 250;
                    }
                }
                StartCoroutine(EffectFade(tempEffect));
            }
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
