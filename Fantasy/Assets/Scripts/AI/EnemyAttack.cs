using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

    public AudioClip sound;
    private Animator animator;
    AudioSource audio;

    public float _attackCoolDown = -2f;
    public MainHeroHp player;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        gameObject.AddComponent<AudioSource>();
        audio = gameObject.GetComponent<AudioSource>();
        gameObject.GetComponent<AudioSource>().clip = sound;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MainHeroHp>();
    }

    private void Update()
    {
        if (gameObject.GetComponent<EnemyStats>().isAlive)
        {
            _attackCoolDown += Time.deltaTime;
            if (_attackCoolDown > 1f && Vector3.Distance(gameObject.GetComponent<EnemyStats>().GetComponent<Transform>().position, player.GetComponent<Transform>().position) < 4f
                && animator.IsInTransition(0))
            {
                _attackCoolDown = 0;
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.3f);
        audio.PlayOneShot(sound);
        player.HeroHp -= Random.Range(10, 15);
        if (player.GetComponent<HealingSkill>().lerping)
        {
            player.GetComponent<HealingSkill>().endHealingAmount -= Random.Range(10, 15);
        }
    }
}
