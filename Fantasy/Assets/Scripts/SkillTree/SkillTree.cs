using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTree : MonoBehaviour
{
    public MainHeroHp player;

    public GameObject passiveSkillTree;


    public Text hpText;
    public Text adText;
    public Text SkillPoints;

    private void Start()
    {
        hpText.text = player.maxHeroHp.ToString();
        adText.text = player.damage.ToString();
        passiveSkillTree.SetActive(false);

    }

    public void hpBoost()
    {
        if (player.SkillPoint > 0 && player.money > 50)
        {
            player.SkillPoint--;
            player.money-=50;
            player.maxHeroHp += 50;
            hpText.text = player.maxHeroHp.ToString();
            SkillPoints.text = player.SkillPoint.ToString();
        }
    }

    public void adBoost()
    {
        if (player.SkillPoint > 0 && player.money > 100)
        {
            player.damage += 20;
            player.SkillPoint--;
            player.money -= 100;
            adText.text = player.damage.ToString();
            SkillPoints.text = player.SkillPoint.ToString();
        }
    }

    public void apBoost()
    {
        if (player.SkillPoint > 0 && player.money > 100)
        {
            player.SkillPoint--;
            player.money -= 100;
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
            SkillPoints.text =player.SkillPoint.ToString();
            if (passiveSkillTree.activeSelf)
            {
                passiveSkillTree.SetActive(false);
            }
            else
            {
                passiveSkillTree.SetActive(true);
            }
        }
    }
}
