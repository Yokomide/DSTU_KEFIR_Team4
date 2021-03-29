using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AttackRadiusTrigger : MonoBehaviour
{

    public bool isTriggered = false;
    private float _attackCoolDown = 0f;
    public GameObject player;

    public float coolDownTimer = 2f;
    public Animator _deathAnim;

    private float _NavMeshSpeedTemp = 3f;


    private void Start()
    {
    }
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
            _deathAnim = other.GetComponent<Animator>();
            isTriggered = true;
            if (Input.GetKey(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer)
            {
                _attackCoolDown = 0f;

                other.GetComponent<EnemyStats>().hp -= Random.Range(10, 20);
                Debug.Log(other.GetComponent<EnemyStats>().hp);

                //Запуск анимации получения урона с задержкой

                StartCoroutine(DeathAnimDelay(other));


                if (other.GetComponent<EnemyStats>().hp < 0)
                {

                    //Активация триггера для начала анимации смерти.

                    _deathAnim.SetTrigger("Active");


                    //Остановка следования к точке

                    agent.isStopped = true;

                    //Смерть. Убирает компоненты, благодаря которым с объектом можно взаимодействовать.


                    Destroy(other.GetComponent<EnemyStats>());
                    Destroy(other.GetComponent<NavMove>());
                    Destroy(other.GetComponent<Rigidbody>());
                    Destroy(other.GetComponent<MobMoving>());

                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isTriggered = false;
    }


    IEnumerator DeathAnimDelay(Collider other)
    {
        //Задержка анимации получения урона
        other.GetComponent<NavMeshAgent>().speed = 0f;
        yield return new WaitForSeconds(0.4f);
        other.GetComponent<NavMeshAgent>().speed = _NavMeshSpeedTemp;
        _deathAnim.Play("Hit");

    }

    IEnumerator StopOnAttack()
    {
        player.GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(1f);
        player.GetComponent<Rigidbody>().isKinematic = false;
    }

}
