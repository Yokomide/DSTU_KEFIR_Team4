using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "ScriptableObjects/EnemyStats", order = 5)]
public class ScrObjEnemyStats : ScriptableObject
{
    public string enemyName = "враг";
    public float maxHp = 100;
    public float damage;
    public float minDamage;
    public float maxDamage;
    public float hp;
}
