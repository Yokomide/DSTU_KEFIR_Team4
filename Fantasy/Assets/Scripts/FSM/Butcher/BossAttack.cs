using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public GameObject boss;
    public AudioClip sound;
    private Animator animator;
    AudioSource audio;

    public float _attackCoolDown = -2f;
    public MainHeroHp player;
    private void Awake()
    {
        animator = boss.GetComponent<Animator>();
        boss.AddComponent<AudioSource>();
        audio = boss.GetComponent<AudioSource>();
        boss.GetComponent<AudioSource>().clip = sound;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MainHeroHp>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (boss.GetComponent<BossStats_>().isAlive)
        {
            if (other.CompareTag("Player"))
            {
                if (other.GetComponent<HealingSkill>().lerping)
                {
                    player.GetComponent<HealingSkill>().endHealingAmount -= Random.Range(10, 15);
                }
                audio.PlayOneShot(sound);
                player.HeroHp -= Random.Range(20, 40);
            }
        }
    }
}
