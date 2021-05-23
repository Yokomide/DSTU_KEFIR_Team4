using System.Collections;
using UnityEngine;

public class HeroAttack : MonoBehaviour
{
    public GameObject hero;
    public AudioClip sound;
    public AudioClip miss;
    public Animator animator;
    AudioSource audio;
    public float _attackCoolDown = -2f;
    public float coolDownTimer = 1.5f;

    private bool _missattack;
    private void Start()
    {
        _missattack = true;
        _attackCoolDown = coolDownTimer;
    }

    private void Awake()
    {
        animator = hero.GetComponent<Animator>();
        hero.AddComponent<AudioSource>();
        audio = hero.GetComponent<AudioSource>();
        hero.GetComponent<AudioSource>().clip = sound;

    }
    private void Update()
    {
        _attackCoolDown += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer && hero.GetComponent<MainHeroHp>().HeroHp > 0)
            {
            hero.GetComponent<AnimMoveset>().AttackAnimation();
            _attackCoolDown = 0f;
            audio.PlayOneShot(miss);
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

                    other.GetComponent<EnemyStats>().AttackM(hero.GetComponent<MainHeroHp>().damage);
                    audio.PlayOneShot(sound);
                    other.GetComponent<EnemyStats>().enemyHp -= Random.Range(20, 40);

                }
                if (other.CompareTag("Boss"))
                {
                    other.GetComponent<BossStats_>().AttackM(hero.GetComponent<MainHeroHp>().damage);
                    audio.PlayOneShot(sound);
                    other.GetComponent<BossStats_>().bossHp -= Random.Range(20, 40);

                }
            }
        }
    }
}
