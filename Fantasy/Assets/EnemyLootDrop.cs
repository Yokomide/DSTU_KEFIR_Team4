using System.Collections.Generic;
using UnityEngine;

public class EnemyLootDrop : MonoBehaviour
{
    public LootManager lootShell;
    public int EnemyItemsDropCount = 3;
    public List<Items> itemsToDrop;
    public int countItemsToDrop;

    private bool _itemsAreDroped = false;
    private void Start()
    {
        countItemsToDrop = Random.Range(0, EnemyItemsDropCount + 1);
        itemsToDrop = new List<Items>();
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
    private void Update()
    {
        if (gameObject.GetComponent<EnemyStats>().hp<0 && !_itemsAreDroped)
        {
            for (int i = 0; i < countItemsToDrop; i++)
            {
                GameObject temp =  Instantiate(itemsToDrop[i].gameObject,gameObject.GetComponent<Transform>().position,Quaternion.identity);
                temp.GetComponent<Rigidbody>().AddForce(Vector3.up*255);
            }
            _itemsAreDroped = true;
        }
    }
}
