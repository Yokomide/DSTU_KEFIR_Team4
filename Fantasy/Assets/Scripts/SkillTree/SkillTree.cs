using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTree : MonoBehaviour
{
    public MainHeroHp player;

    int hpBoostCount;
    int adBoostCount;
    int apBoostCount;

    public GameObject passiveSkillTree;
    public GameObject activeSkillTree;

    public Text hpText;
    public Text adText;
    public Text apText;
    public Text SkillPoints;
    public Text SkillPassivePoints;

    private void Start()
    {
        passiveSkillTree.SetActive(false);
    }

    public void hpBoost()
    {
        if (player.SkillPoint > 0 && player.money > 50)
        {
            player.SkillPoint--;
            player.money-=50;
            player.maxHeroHp += 50;
            hpBoostCount++;
            hpText.text = "Улучшено: " + hpBoostCount + " раз";
        }
    }

    public void adBoost()
    {
        if (player.SkillPoint > 0 && player.money > 100)
        {
            player.damage += 20;
            player.SkillPoint--;
            player.money -= 100;
            adBoostCount++;
            adText.text = "Улучшено: " + adBoostCount + " раз";
        }
    }

    public void apBoost()
    {
        if (player.SkillPoint > 0 && player.money > 100)
        {
            player.SkillPoint--;
            player.money -= 100;
            apBoostCount++;
            apText.text = "Улучшено: " + apBoostCount+" раз";
        }
    }
    private void Update()
    {
        ToggleActivePassiveTree();
    }
    void ToggleActivePassiveTree()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SkillPoints.text = "Души: " + player.SkillPoint;
            SkillPassivePoints.text = "Души: " + player.SkillPoint;
            if (passiveSkillTree.activeSelf || activeSkillTree.activeSelf)
            {
                passiveSkillTree.SetActive(false);
                activeSkillTree.SetActive(false);
            }
            else
            {
                passiveSkillTree.SetActive(true);
            }
        }
    }

    public void nextTree()
    {
        SkillPoints.text = "Души: " + player.SkillPoint;
        SkillPassivePoints.text = "Души: " + player.SkillPoint;
        if (passiveSkillTree.activeSelf)
        {
            passiveSkillTree.SetActive(false);
            activeSkillTree.SetActive(true);
        }
        else
        {
            passiveSkillTree.SetActive(true);
            activeSkillTree.SetActive(false);
        }
    }
}
