using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
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
        if (gameObject.GetComponent<BossStats_>().isAlive)
        {
            _attackCoolDown += Time.deltaTime;
            if (_attackCoolDown > 2f && Vector3.Distance(gameObject.GetComponent<BossStats_>().GetComponent<Transform>().position, player.GetComponent<Transform>().position) < 6f
                && animator.IsInTransition(0))
            {
                _attackCoolDown = 0;
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(1f);
        audio.PlayOneShot(sound);
        player.HeroHp -= Random.Range(20, 40);
        if (player.GetComponent<HealingSkill>().lerping)
        {
            player.GetComponent<HealingSkill>().endHealingAmount -= Random.Range(10, 15);
        }
    }
}
