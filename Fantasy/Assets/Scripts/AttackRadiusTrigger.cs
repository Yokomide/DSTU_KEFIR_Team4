using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class AttackRadiusTrigger : MonoBehaviour
{ 

    private bool _isTriggered = false;
    private float _attackCoolDown = 0f;

    public float coolDownTimer = 2f;
    public Animator _deathAnim;

    private void Update()
    {
        _attackCoolDown += Time.deltaTime;

    }

    private void OnTriggerStay(Collider other)
    {
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        _deathAnim = other.GetComponent<Animator>();
        _isTriggered = true;
        if (_isTriggered && Input.GetKey(KeyCode.Mouse0) && !other.CompareTag("Player") && _attackCoolDown > coolDownTimer && other.CompareTag("Enemy"))
        {
            _attackCoolDown = 0f;

            other.GetComponent<EnemyStats>().hp -= Random.Range(10, 20);
            Debug.Log(other.GetComponent<EnemyStats>().hp);

            //������ �������� ��������� ����� � ���������

            StartCoroutine(DeathAnimDelay());

            if (other.GetComponent<EnemyStats>().hp < 0)
            {

                //��������� �������� ��� ������ �������� ������.

                _deathAnim.SetTrigger("Active");
                
                //������������ ������ ����

                other.transform.gameObject.tag = "Dead";

                //��������� ���������� � �����

                agent.isStopped = true;

                //������. ������� ����������, ��������� ������� � �������� ����� �����������������.


                Destroy(other.GetComponent<EnemyStats>());
                Destroy(other.GetComponent<BoxCollider>());
                Destroy(other.GetComponent<NavMove>());
                Destroy(other.GetComponent<Rigidbody>());
                Destroy(other.GetComponent<MobMoving>());

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _isTriggered = false;
    }


    IEnumerator DeathAnimDelay()
    {
        //�������� �������� ��������� �����

        yield return new WaitForSeconds(0.4f);

        _deathAnim.Play("Hit");

    }

}
