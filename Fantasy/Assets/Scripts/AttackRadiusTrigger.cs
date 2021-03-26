using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRadiusTrigger : MonoBehaviour
{
    public bool isTriggered = false;
    private float _attackCoolDown = 0f;

    private void Update()
    {
        _attackCoolDown += Time.deltaTime;
    }
    private void OnTriggerStay(Collider other)
    {
        isTriggered = true;
        if (isTriggered && Input.GetKey(KeyCode.Mouse0) && !other.CompareTag("Player") && _attackCoolDown > 2f)
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
        isTriggered = false;
    }
}
