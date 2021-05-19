using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerData
{
    public float level;
    public float expNum;
    public float gold;



    public PlayerData(MainHeroHp player)
    {
        level = player.Lvl;
        expNum = player.ExpNum;
        gold = player.money;

    }
    
}
