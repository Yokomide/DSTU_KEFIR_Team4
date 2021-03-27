using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackRadiusTrigger : MonoBehaviour
{
    private bool _isTriggered = false;
    private float _attackCoolDown = 0f;

    public float coolDownTimer = 2f;


    private void Update()
    {
        _attackCoolDown += Time.deltaTime;
    }
    private void OnTriggerStay(Collider other)
    {
        _isTriggered = true;
        
        if (_isTriggered && Input.GetKey(KeyCode.Mouse0) && !other.CompareTag("Player") && _attackCoolDown > coolDownTimer && other.CompareTag("Enemy"))
        {
            _attackCoolDown = 0f;
            other.GetComponent<EnemyStats>().hp -= Random.Range(10, 20);
            
            Debug.Log(other.GetComponent<EnemyStats>().hp);
            if (other.GetComponent<EnemyStats>().hp < 0)
            {

                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _isTriggered = false;
    }
}
