using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerData
{
    public float level;
    public float health;
    public float[] position;
    public float expNum;
    public float gold;

    public PlayerData(MainHeroHp player)
    {
        level = player.Lvl;
        health = player.HeroHp;
        expNum = player.ExpNum;
        gold = player.money;

        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;
    }
    
}
