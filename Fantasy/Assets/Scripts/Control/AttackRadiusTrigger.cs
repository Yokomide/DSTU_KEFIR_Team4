using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackRadiusTrigger : MonoBehaviour
{
    public GameObject player;
    public Animator DeathAnim;

    public bool isTriggered = false;
    public float coolDownTimer = 2f;

    private float _NavMeshSpeedTemp = 3f;
    private float _attackCoolDown = 0f;
    private List<string> _enemies;

    private  string _enemyTemp;


    private void Start()
    {
        _attackCoolDown = coolDownTimer;
    }

    private void Update()
    {
        _attackCoolDown += Time.deltaTime;

        if (Input.GetKey(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer )
        {
            player.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(StopOnAttack());
        }

        if (Input.GetKey(KeyCode.L))
        {
            Debug.Log(_enemies);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Enemy"))
        //{
        //}
    }
    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Enemy"))
        //{
            _enemies.Remove(other.GetComponent<Rigidbody>().name);
        //}
        isTriggered = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            DeathAnim = other.GetComponent<Animator>();
            isTriggered = true;

            if (Input.GetKey(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer)
            {
                
                    isTriggered = true;
                    _attackCoolDown = 0f;

                    other.GetComponent<EnemyStats>().hp -= Random.Range(10, 20);
                    Debug.Log(other.GetComponent<EnemyStats>().hp);

                    //Запуск анимации получения урона с задержкой

                        StartCoroutine(HitAnimDelay(other));
                    
                    if (other.GetComponent<EnemyStats>().hp < 0)
                    {
                        //Активация триггера для начала анимации смерти.
                        DeathAnim.SetTrigger("Active");

                        //Остановка следования к точке
                        agent.isStopped = true;

                        //Смерть. Убирает компоненты, благодаря которым с объектом можно взаимодействовать.

                        Destroy(other.GetComponent<EnemyStats>());
                        Destroy(other.GetComponent<NavMove>());
                        Destroy(other.GetComponent<Rigidbody>());
                        Destroy(other.GetComponent<MobMoving>());
                        Destroy(other.GetComponent<BoxCollider>());

                    
                }
            }
        }
    }


    IEnumerator HitAnimDelay(Collider other)
    {
        
            other.GetComponent<NavMeshAgent>().speed = 0f;
            //Задержка анимации получения урона
            yield return new WaitForSeconds(0.4f);
            DeathAnim.Play("Hit");
            other.GetComponent<NavMeshAgent>().speed = _NavMeshSpeedTemp;

    }

    IEnumerator StopOnAttack()
    {
        yield return new WaitForSeconds(1f);
        player.GetComponent<Rigidbody>().isKinematic = false;
    }

}
