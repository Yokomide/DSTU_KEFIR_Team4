using UnityEngine;
using UnityEngine.AI;

public class EnemyIsAttacked : MonoBehaviour
{
    public GameObject playerAttackSphere;
    private float _NavMeshSpeedTemp = 3f;
    private void Start()
    {
        _NavMeshSpeedTemp = this.gameObject.GetComponent<NavMeshAgent>().speed;
    }
    private void Update()
    {
        if (playerAttackSphere.GetComponent<AttackRadiusTrigger>().isAttacked &&
            playerAttackSphere.GetComponent<AttackRadiusTrigger>().attackCoolDown < playerAttackSphere.GetComponent<AttackRadiusTrigger>().coolDownTimer)
        {
            this.gameObject.GetComponent<NavMeshAgent>().speed = 0f;
        }
        else
        {
            this.gameObject.GetComponent<NavMeshAgent>().speed = _NavMeshSpeedTemp;
        }
    }
}
