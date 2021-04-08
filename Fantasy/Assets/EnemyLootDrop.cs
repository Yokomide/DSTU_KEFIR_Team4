using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLootDrop : MonoBehaviour
{
    public LootManager lootShell;
    public int EnemyItemsDropCount=3;
    public List<Items> itemsToDrop;
    public int countItemsToDrop;

    private void Start()
    {
        countItemsToDrop = Random.Range(0, EnemyItemsDropCount + 1);
        itemsToDrop = new List<Items>();
        for (int i = 0; i < countItemsToDrop; i++)
        {
            switch (Random.Range(1, 4))
            {
                case 1:
                    itemsToDrop.Add(lootShell.CommonItems[Random.Range(1, lootShell.CommonItems.Count + 1)-1].GetComponent<Items>());
                    Debug.Log(Random.Range(1, lootShell.CommonItems.Count + 1) - 1);
                    Debug.Log("common");
                    Debug.Log(i);
                    break;
                case 2:
                    itemsToDrop.Add(lootShell.RareItems[Random.Range(1, lootShell.RareItems.Count + 1)-1].GetComponent<Items>());
                    Debug.Log(Random.Range(1, lootShell.RareItems.Count + 1) - 1);
                    Debug.Log("rare");
                    Debug.Log(i);
                    break;
                case 3:
                    itemsToDrop.Add(lootShell.EpicItems[Random.Range(1, lootShell.EpicItems.Count + 1)-1].GetComponent<Items>());
                    Debug.Log(Random.Range(1, lootShell.EpicItems.Count + 1) - 1);
                    Debug.Log("epic");
                    Debug.Log(i);
                    break;
            }
        }
    }
}
