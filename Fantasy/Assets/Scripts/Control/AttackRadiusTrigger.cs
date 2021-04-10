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

    private string _enemyTemp;


    private void Start()
    {
        _attackCoolDown = coolDownTimer;
    }

    private void Update()
    {
        _attackCoolDown += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Mouse0) && _attackCoolDown > coolDownTimer)
        {
            StopAllCoroutines();
            _attackCoolDown = 0f;
            _tempSpeed = player.GetComponent<Player>().speed;
            player.GetComponent<Player>().speed = 0.2f;
            StartCoroutine(StopOnAttack(_tempSpeed));
            if (enemies.Count != 0)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    NavMeshAgent agent = enemies[i].GetComponent<NavMeshAgent>();
                    DeathAnim = enemies[i].GetComponent<Animator>();

                    enemies[i].GetComponent<EnemyStats>().Attacked(player.GetComponent<MainHeroHp>().damage);

                    //Запуск анимации получения урона с задержкой
                    if (enemies[i].GetComponent<EnemyStats>().isAlive)
                    {
                        StartCoroutine(HitAnimDelay(enemies[i].GetComponent<Collider>()));
                    }

                    if (!enemies[i].GetComponent<EnemyStats>().isAlive)
                    {
                        player.GetComponent<MainHeroHp>()._ExpNum += Random.Range(50, 70);
                        //Активация триггера для начала анимации смерти.
                        DeathAnim.SetTrigger("Active");

                        //Остановка следования к точке
                        agent.isStopped = true;
                        enemies.RemoveAt(i);

                    }

                }
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        isTriggered = true;
        if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")) && other.GetComponent<EnemyStats>().isAlive)
        {
            bool _isHere = false;
            GameObject temp = other.gameObject;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == temp)
                {
                    _isHere = true;
                    break;
                }

            }
            if (!_isHere) enemies.Add(temp);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Enemy") || other.CompareTag("Citizen")) && other.GetComponent<EnemyStats>().isAlive)
        {
            GameObject temp = other.gameObject;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == temp)
                {
                    enemies.RemoveAt(i);
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
        yield return new WaitForSeconds(0.3f);
        DeathAnim.Play("TakeDmg");
        yield return new WaitForSeconds(0.1f);
        bloodPos = other.GetComponent<Transform>();
        Instantiate(bloodSplat, bloodPos);
        other.GetComponent<NavMeshAgent>().speed = _NavMeshSpeedTemp;

    }

    IEnumerator StopOnAttack(float tempSpeed)
    {
        yield return new WaitForSeconds(1f);
        player.GetComponent<Player>().speed = tempSpeed;
    }

}
