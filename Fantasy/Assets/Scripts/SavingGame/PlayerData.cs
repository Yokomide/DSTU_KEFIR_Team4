using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerData
{
    public float HeroHp;
    public float maxHeroHp;
    public float Lvl;
    public float SkillPoint;
    public float money;
    public float damage;
    public float ExpNum;
    public float magicDamage;


    public PlayerData(MainHeroHp player)
    {
        HeroHp = player.HeroHp;
        maxHeroHp = player.maxHeroHp;
        SkillPoint = player.SkillPoint;
        Lvl = player.Lvl;
        money = player.money;
        damage = player.damage;
        ExpNum = player.ExpNum;
        magicDamage = player.magicDamage;
    }
    
}
