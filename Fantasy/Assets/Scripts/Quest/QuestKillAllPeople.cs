using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestKillAllPeople : MonoBehaviour
{
    public new List<EnemyStats> enemies;
    int deathCount = 0;
    public BossStats_ boss;
    [HideInInspector]
    public bool firstQuestIsEnded = false;

    public GameObject canvasStats;

    public Text questName;
    public new List<Text> enemiesKilled;
    List<bool> enemyIsKilled = new List<bool> { false, false, false, false, false, false };

    private void Start()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemiesKilled[i].text = enemies[i].GetComponent<EnemyStats>().enemyStats.enemyName;
        }
    }
    private void Update()
    {
        if (!firstQuestIsEnded)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].isAlive == false && !enemyIsKilled[i])
                {
                    deathCount++;
                    enemiesKilled[i].color = Color.green;
                    enemyIsKilled[i] = true;
                }
            }

            if (deathCount == enemies.Count)
            {
                questName.color = Color.green;
                firstQuestIsEnded = true;
                StartCoroutine(ToBossQuest());
            }
        }
        if (!boss.isAlive)
        {
            questName.color = Color.green;
            StartCoroutine(ToCastle());
        }
 
    }

    IEnumerator ToBossQuest()
    {
        gameObject.GetComponent<QuestDisplay>().questOnSceneUI.SetActive(false);
        yield return new WaitForSeconds(2f);
        questName.color = Color.white;
        questName.text = "Убей босса";
    }
    IEnumerator ToCastle()
    {
        yield return new WaitForSeconds(2f);
        Destroy(questName);
    }
}
