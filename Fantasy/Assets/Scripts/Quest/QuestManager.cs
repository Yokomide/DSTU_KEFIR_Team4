using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Quest", menuName = "ScriptableObjects/Quest", order = 2)]
public class QuestManager : ScriptableObject
{
    public Image questImage;
    public new string questName;
    public List<string> questSubQuests;
    public List<int> expToGetFromEachQuest;
    public Font font;

}
