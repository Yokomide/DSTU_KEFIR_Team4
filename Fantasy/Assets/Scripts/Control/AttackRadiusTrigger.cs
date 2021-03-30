using System.Collections;
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

    private void Update()
    {
        _attackCoolDown += Time.deltaTime;

        if (Input.GetKey(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer)
        {
            StartCoroutine(StopOnAttack());
        }

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

    private void OnTriggerExit(Collider other)
    {
        isTriggered = false;
    }

    IEnumerator HitAnimDelay(Collider other)
    {
        
            other.GetComponent<NavMeshAgent>().speed = 0f;
            //Задержка анимации получения урона
            yield return new WaitForSeconds(0.4f);
            DeathAnim.Play("Hit");
            other.GetComponent<NavMeshAgent>().speed = 0f;

    }

    IEnumerator StopOnAttack()
    {
        player.GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(1f);
        player.GetComponent<Rigidbody>().isKinematic = false;
    }

}
