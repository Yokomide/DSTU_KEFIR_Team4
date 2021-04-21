using UnityEngine;

public class AttackHeroEvent : MonoBehaviour
{
    public HeroStatsScriptableOvject hero;
    public ScrObjEnemyStats enemy;
    public void OnEnable()
    {
        EventManager.OnStatsChange += DealDamage;
    }
    private void OnDestroy()
    {
        EventManager.OnStatsChange -= DealDamage;
    }

    void DealDamage()
    {
        hero.HeroHp -= Random.Range(enemy.minDamage, enemy.maxDamage);
        Debug.Log("Получил");
    }
}
