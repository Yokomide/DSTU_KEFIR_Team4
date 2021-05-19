using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerData
{
    public float level;
    public float health;
    public float expNum;
    public float gold;



    public PlayerData(MainHeroHp player)
    {
        level = player.Lvl;
        health = player.HeroHp;
        expNum = player.ExpNum;
        gold = player.money;

    }
    
}
