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
    public float coolDownTimer = 2f;

    private float _NavMeshSpeedTemp = 3f;
    private float _attackCoolDown = 0f;
    private List<GameObject> _enemies = new List<GameObject>();

    private string _enemyTemp;


    private void Start()
    {
        _attackCoolDown = coolDownTimer;
    }

    private void Update()
    {
        _attackCoolDown += Time.deltaTime;

        if (Input.GetKey(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer)
        {
            _attackCoolDown = 0f;
            player.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(StopOnAttack());
            if (_enemies.Count != 0)
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    NavMeshAgent agent = _enemies[i].GetComponent<NavMeshAgent>();
                    DeathAnim = _enemies[i].GetComponent<Animator>();

                    _enemies[i].GetComponent<EnemyStats>().Attacked();

                    //Запуск анимации получения урона с задержкой

                    StartCoroutine(HitAnimDelay(_enemies[i].GetComponent<Collider>()));

                    if (_enemies[i].GetComponent<EnemyStats>().hp < 0)
                    {
                        //Активация триггера для начала анимации смерти.
                        DeathAnim.SetTrigger("Active");

                        //Остановка следования к точке
                        agent.isStopped = true;

                        //Смерть. Убирает компоненты, благодаря которым с объектом можно взаимодействовать.

                        Destroy(_enemies[i].GetComponent<EnemyStats>());
                        Destroy(_enemies[i].GetComponent<NavMove>());
                        Destroy(_enemies[i].GetComponent<Rigidbody>());
                        Destroy(_enemies[i].GetComponent<MobMoving>());
                        Destroy(_enemies[i].GetComponent<BoxCollider>());
                        _enemies.RemoveAt(i);

                    }

                }
            }
            else
            {

            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        isTriggered = true;
        if (other.CompareTag("Enemy"))
        {
            bool _isHere = false;
            GameObject temp = other.gameObject;
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] == temp)
                {
                    _isHere = true;
                    break;
                }

            }
            if (!_isHere) _enemies.Add(temp);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameObject temp = other.gameObject;
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] == temp)
                {
                    _enemies.RemoveAt(i);
                    break;
                }

            }
        }
        isTriggered = false;
    }




    IEnumerator HitAnimDelay(Collider other)
    {

        other.GetComponent<NavMeshAgent>().speed = 0f;
        //Задержка анимации получения урона
        yield return new WaitForSeconds(0.4f);
        DeathAnim.Play("Hit");
        bloodPos = other.GetComponent<Transform>();
        Instantiate(bloodSplat, bloodPos);
        other.GetComponent<NavMeshAgent>().speed = _NavMeshSpeedTemp;

    }

    IEnumerator StopOnAttack()
    {
        yield return new WaitForSeconds(1f);
        player.GetComponent<Rigidbody>().isKinematic = false;
    }

}
