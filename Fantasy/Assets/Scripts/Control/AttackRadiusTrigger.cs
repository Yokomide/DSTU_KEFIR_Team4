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
    private List<GameObject> _enemies;

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
            _attackCoolDown = 0f;
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
        if (other.CompareTag("Enemy"))
        {
            GameObject temp = other.gameObject;
            _enemies.Add(temp);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        isTriggered = false;
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case "Enemy":
                NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
                DeathAnim = other.GetComponent<Animator>();
                isTriggered = true;

                if (Input.GetKey(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer)
                {

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
                break;
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
