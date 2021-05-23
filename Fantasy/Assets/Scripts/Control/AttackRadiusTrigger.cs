using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackRadiusTrigger : MonoBehaviour
{
    public GameObject player;
    [HideInInspector]
    public Animator DeathAnim;

    public GameObject bloodSplat;
    [HideInInspector]
    public Transform bloodPos;

    public bool isTriggered = false;
    public float coolDownTimer = 1.5f;

    private float _NavMeshSpeedTemp = 3f;
    private float _attackCoolDown = 0f;
    private float _tempSpeed;
    public List<GameObject> enemies = new List<GameObject>();
    private MainHeroHp heroStats;
    private EnemyStats _enemyStats;
    private BossStats_ _bossStats;
    private Player _playerMove;
    private NavMeshAgent agent;

    public bool isAttacking = false;

    GameObject tempObject;

    public AudioClip soundAttack;
    public AudioClip soundMiss;
    AudioSource audio;


    private void Start(){
    
        gameObject.AddComponent<AudioSource>();
        audio = gameObject.GetComponent<AudioSource>();
        gameObject.GetComponent<AudioSource>().clip = soundMiss;
        _playerMove = player.GetComponent<Player>();
        heroStats = player.GetComponent<MainHeroHp>();
        _attackCoolDown = coolDownTimer;
        _tempSpeed = _playerMove.speed;
    }

    private void Update()
    {
        _attackCoolDown += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer && heroStats.HeroHp > 0)
        {
            StopAllCoroutines();
            gameObject.GetComponentInParent<AnimMoveset>().AttackAnimation();
            _attackCoolDown = 0f;
            _tempSpeed = _playerMove.speed;
            _playerMove.speed = 0.2f;
            StartCoroutine(StopOnAttack(_tempSpeed));
            if (enemies.Count != 0)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    
                    agent = enemies[i].GetComponent<NavMeshAgent>();
                    DeathAnim = enemies[i].GetComponent<Animator>();
                    if (enemies[i].GetComponent<EnemyStats>())
                    {
                        _enemyStats = enemies[i].GetComponent<EnemyStats>();
                        _enemyStats.Attacked(player.GetComponent<MainHeroHp>().damage);
                        if (_enemyStats.isAlive)
                        {
                            StartCoroutine(HitAnimDelay(enemies[i].GetComponent<Collider>()));
                        }
                    }
                    else if (enemies[i].GetComponent<BossStats_>())

                    {
                        _bossStats = enemies[i].GetComponent<BossStats_>();
                        _bossStats.Attacked(player.GetComponent<MainHeroHp>().damage);
                        if (_bossStats.isAlive)
                        {
                            StartCoroutine(HitAnimDelay(enemies[i].GetComponent<Collider>()));
                        }
                    }

                    else 
                    {
                        heroStats.ExpNum += Random.Range(50, 70);
                        DeathAnim.SetTrigger("Active");
                        agent.isStopped = true;
                        enemies.RemoveAt(i);
                    }
                }
            }
            else
            {
                StartCoroutine(AttackMiss());
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        isTriggered = true;
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
            if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")) && (other.GetComponent<BossStats_>().isAlive))
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
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<EnemyStats>())
        {
            if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")) && (other.GetComponent<EnemyStats>().isAlive))
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

            if (enemies.Count == 0)
            {
                isTriggered = false;
            }
        }
        
        if (other.GetComponent<BossStats_>())
        {
            if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")) && (other.GetComponent<BossStats_>().isAlive))
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

            if (enemies.Count == 0)
            {
                isTriggered = false;
            }
        }
    }




    IEnumerator HitAnimDelay(Collider other)
    {
        
        other.GetComponent<NavMeshAgent>().speed = 0f;
        yield return new WaitForSeconds(0.3f);
        audio.PlayOneShot(soundAttack);
        yield return new WaitForSeconds(0.1f);
        bloodPos = other.GetComponent<Transform>();
        Instantiate(bloodSplat, bloodPos);
        DeathAnim.Play("TakeDmg");
        other.GetComponent<NavMeshAgent>().speed = _NavMeshSpeedTemp;

    }

    IEnumerator AttackMiss()
    {
        yield return new WaitForSeconds(0.3f);
        audio.PlayOneShot(soundMiss);
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator StopOnAttack(float tempSpeed)
    {
        yield return new WaitForSeconds(1f);
        _playerMove.speed = tempSpeed;
    }

}
