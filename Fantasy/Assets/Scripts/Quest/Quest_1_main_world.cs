using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class Quest_1_main_world
{
    public new List<EnemyStats> enemies;
    new List<Text> enemiesTextOnUI;
    new List<bool> enemiesAreKilled;

    int deathCount = 0;
    bool questIsEnded;

    public GameObject canvasStats;

}
