using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    public MainHeroHp player;

    int hpBoostCount;
    public void hpBoost()
    {
        if (player.SkillPoint > 0 && player.Lvl>2 && player.money > 50)
        {
            player.SkillPoint--;
            player.money-=50;
            player.maxHeroHp += 25;
            hpBoostCount++;
        }
    }

    public void adBoost()
    {
        if (player.SkillPoint > 0 && player.Lvl > 2 && player.money > 50)
        {
            player.SkillPoint--;
            player.money -= 100;
        }
    }

    public void apBoost()
    {
        if (player.SkillPoint > 0 && player.Lvl > 2 && player.money > 50)
        {
            player.SkillPoint--;
            player.money -= 100;
        }
    }
}
