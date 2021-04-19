using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : ScriptableObject
{
    public Image questImage;
    public new string questName;
    public List<string> questSubQuests;
    public List<int> expToGetFromEachQuest;
    public ScriptableObject Quest;
    public Font font;

}
