using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FreezeSkill : MonoBehaviour
{
    private MainHeroHp _heroStats;
    private GameObject tempObject;
    public GameObject effect;

    private float _freezeTime;
    public float freezeTimeCounter;

    private GameObject tempEffect;
    public List<GameObject> enemies = new List<GameObject>();

    public AudioClip sound;
    AudioSource audio;

    private void Start()
    {
        gameObject.AddComponent<AudioSource>();
        audio = gameObject.GetComponent<AudioSource>();
        gameObject.GetComponent<AudioSource>().clip = sound;
        _heroStats = gameObject.GetComponentInParent<MainHeroHp>();
        _freezeTime = 0;
    }

    public void Update()
    {
        _freezeTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.Alpha1) && _freezeTime > freezeTimeCounter && _heroStats.HeroHp > 0) Freeze();
    }



    public void Freeze()
    {
        audio.PlayOneShot(sound);
        tempEffect = Instantiate(effect, gameObject.transform.position, Quaternion.Euler(-90, 0, 0));
        foreach (GameObject i in enemies)
        {
            StartCoroutine(FreezeCount(i));
        }
        StartCoroutine(EffectFade(tempEffect));
        _freezeTime = 0;
    }

    IEnumerator FreezeCount(GameObject enemy)
    {
        enemy.GetComponent<Rigidbody>().isKinematic = true;

        if (enemy.CompareTag("Enemy"))
        {
            enemy.GetComponent<EnemyAttack>().enabled = false;
            enemy.GetComponent<Animator>().enabled = false;
            enemy.GetComponent<MobAI>().enabled = false;
            enemy.GetComponent<MobMoving>().enabled = false;
        }

        if (enemy.CompareTag("Citizen"))
        {
              enemy.GetComponent<NavMeshAgent>().enabled = false;
              enemy.GetComponent<Animator>().enabled = false;
              enemy.GetComponent<MobAI>().enabled = false;
              enemy.GetComponent<MobMoving>().enabled = false;
          }

       else if (enemy.CompareTag("Boss"))
        {
            enemy.GetComponent<BossAttack>().enabled = false;
            enemy.GetComponent<Animator>().enabled = false;
            enemy.GetComponent<Boss_AI>().enabled = false;
            enemy.GetComponent<Boss_Move>().enabled = false;
        }
        
        yield return new WaitForSeconds(2);

        Destroy(tempEffect);

        enemy.GetComponent<Rigidbody>().isKinematic = false;

        if (enemy.CompareTag("Enemy"))
        {
            enemy.GetComponent<EnemyAttack>().enabled = true;
            enemy.GetComponent<Animator>().enabled = true;
            enemy.GetComponent<MobAI>().enabled = true;
            enemy.GetComponent<MobMoving>().enabled = true;
        }
        

        if (enemy.CompareTag("Citizen"))
        {
            enemy.GetComponent<NavMeshAgent>().enabled = true;
            enemy.GetComponent<Animator>().enabled = true;
            enemy.GetComponent<MobAI>().enabled = true;
            enemy.GetComponent<MobMoving>().enabled = true;
        }
        else if (enemy.CompareTag("Boss"))
        {
            enemy.GetComponent<BossAttack>().enabled = true;
            enemy.GetComponent<Animator>().enabled = true;
            enemy.GetComponent<Boss_AI>().enabled = true;
            enemy.GetComponent<Boss_Move>().enabled = true;
        }
      
    }
    IEnumerator EffectFade(GameObject effect)
    {
        yield return new WaitForSeconds(2);
        Destroy(effect);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyStats>())
        {
            if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")) && (other.GetComponent<EnemyStats>().isAlive))
            {
                bool _isHere = false;
                tempObject = other.gameObject;
                foreach (GameObject i in enemies)
                {
                    if (i == tempObject)
                    {
                        _isHere = true;
                        break;
                    }

                }

                if (!_isHere) enemies.Add(tempObject);
            }
        }
        
        
        if (other.GetComponent<BossStats_>())
        {
            if ((other.CompareTag("Boss")) && (other.GetComponent<BossStats_>().isAlive))
            {
                bool _isHere = false;
                tempObject = other.gameObject;
                foreach (GameObject i in enemies)
                {
                    if (i == tempObject)
                    {
                        _isHere = true;
                        break;
                    }

                }

                if (!_isHere) enemies.Add(tempObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BossStats_>())
        {
            if ((other.CompareTag("Boss")) && (other.GetComponent<BossStats_>().isAlive))
            {
                tempObject = other.gameObject;
                foreach (GameObject i in enemies)
                {
                    if (i == tempObject)
                    {
                        enemies.Remove(i);
                        break;
                    }

                }
            }
        }
        if (other.GetComponent<EnemyStats>())
        {
            if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")) &&  (other.GetComponent<EnemyStats>().isAlive))
            {
                tempObject = other.gameObject;
                foreach (GameObject i in enemies)
                {
                    if (i == tempObject)
                    {
                        enemies.Remove(i);
                        break;
                    }

                }
            }
        }
    }
}
