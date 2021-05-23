using System.Collections.Generic;
using UnityEngine;

public class EnemyLootDrop : MonoBehaviour
{
    public LootManager lootShell;
    public int EnemyItemsDropCount = 3;
    public List<Items> itemsToDrop = new List<Items>();
    public int countItemsToDrop;

    private GameObject item;
    GameObject tempItem;
    private void Start()
    {
        item = GameObject.Find("Items");
        if (gameObject.CompareTag("Boss"))
        {
            countItemsToDrop = 2;
        }
        else
        {

            countItemsToDrop = Random.Range(0, EnemyItemsDropCount + 1);

            for (int i = 0; i < countItemsToDrop; i++)
            {
                switch (Random.Range(1, 4))
                {
                    case 1:
                        itemsToDrop.Add(lootShell.CommonItems[Random.Range(1, lootShell.CommonItems.Count + 1) - 1].GetComponent<Items>());
                        break;
                    case 2:
                        itemsToDrop.Add(lootShell.RareItems[Random.Range(1, lootShell.RareItems.Count + 1) - 1].GetComponent<Items>());
                        break;
                    case 3:
                        itemsToDrop.Add(lootShell.EpicItems[Random.Range(1, lootShell.EpicItems.Count + 1) - 1].GetComponent<Items>());
                        break;
                }
            }
        }
    }
    public void DropItems()
    {
        for (int i = 0; i < countItemsToDrop; i++)
        {
            tempItem = Instantiate(itemsToDrop[i].gameObject, gameObject.GetComponent<Transform>().position, Quaternion.identity);
            tempItem.GetComponent<Rigidbody>().AddForce(Vector3.up * 255);
        }
    }
}
