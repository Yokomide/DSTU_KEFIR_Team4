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
}
