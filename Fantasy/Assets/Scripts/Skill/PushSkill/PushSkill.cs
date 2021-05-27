using System.Collections;
using UnityEngine;

public class PushSkill : MonoBehaviour
{
    private MainHeroHp _heroStats;
    private float _pushTime = 0;
    private GameObject tempEffect;
    private PushCoolDown coolDown;

    public float pushTimeCounter = 6;
    public int pushForce = 25;
    public GameObject effect;
    public FreezeSkill freeze;

    public AudioClip sound;
    AudioSource audio;

    private void Start()
    {
        freeze = gameObject.GetComponentInChildren<FreezeSkill>();
        coolDown = gameObject.GetComponent<PushCoolDown>();
        audio = gameObject.GetComponent<AudioSource>();
        _heroStats = gameObject.GetComponent<MainHeroHp>();
    }
    private void Update()
    {
        _pushTime += Time.deltaTime;

        if (Input.GetKey(KeyCode.Alpha3) && _pushTime > pushTimeCounter && _heroStats.HeroHp > 0)
        {
            audio.PlayOneShot(sound);
            Vector3 pos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 3, gameObject.transform.position.z);
            tempEffect = Instantiate(effect, pos, Quaternion.identity, gameObject.transform);
            StartCoroutine(pushBack(tempEffect));
        }
    }

    IEnumerator pushBack(GameObject tempEffect)
    {
        _pushTime = 0;
        gameObject.GetComponent<PushCoolDown>().timeElapsed = 0;
        foreach (GameObject enemy in freeze.enemies)
        {
            enemy.GetComponent<EnemyStats>().enemyHp -= pushForce;
            enemy.GetComponent<EnemyStats>().AttackM();
        }
        yield return new WaitForSeconds(1.5f);
        Destroy(tempEffect);

    }
}
