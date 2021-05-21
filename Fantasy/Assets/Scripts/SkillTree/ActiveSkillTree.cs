using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSkillTree : MonoBehaviour
{
    public MainHeroHp player;

    int hpBoostCount;
    int adBoostCount;
    int apBoostCount;

    public GameObject activeSkillTree;
    public GameObject passiveSkillTree;

    private void Start()
    {
        activeSkillTree.SetActive(false);
    }


    private void Update()
    {
        
    }


    public void nextTree()
    {
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
