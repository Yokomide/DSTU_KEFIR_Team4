using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public GameObject boss;
    public AudioClip sound;
    public GameObject effect;
    public float _attackCoolDown = -2f;
    public MainHeroHp player;

    AudioSource audio;

    private Animator animator;
    private GameObject tempEffect;


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
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack3") || animator.GetCurrentAnimatorStateInfo(0).IsName("Kill") || animator.GetCurrentAnimatorStateInfo(0).IsName("Kill1"))
        {
            Vector3 y = new Vector3(0f, 2f, 0);
            if (boss.GetComponent<BossStats_>().isAlive)
            {
                if (other.CompareTag("Player"))
                {
                    tempEffect = Instantiate(effect, other.transform.position + y, Quaternion.Euler(-90, 0, 0));
                    if (other.GetComponent<HealingSkill>().lerping)
                    {
                        player.GetComponent<HealingSkill>().endHealingAmount -= Random.Range(10, 15);
                    }
                    audio.PlayOneShot(sound);
                    player.HeroHp -= Random.Range(40, 80);
                }
                StartCoroutine(EffectFade(tempEffect));
            }
        }
    }
    IEnumerator EffectFade(GameObject effect)
    {
        yield return new WaitForSeconds(2);
        Destroy(effect);
    }
}
