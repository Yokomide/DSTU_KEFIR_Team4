using UnityEngine;

[CreateAssetMenu(fileName = "HeroStats", menuName = "ScriptableObjects/HeroStats", order = 4)]
public class HeroStatsScriptableOvject : ScriptableObject
{
    public float HeroHp;
    public float maxHeroHp;
    public float Lvl;
    public float money;
    public float damage;
    public float ExpNum;

}
