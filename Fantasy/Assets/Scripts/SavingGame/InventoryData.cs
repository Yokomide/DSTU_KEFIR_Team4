using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class InventoryData
{
    public float[] ids = new float[21];
    public float[] counters = new float[21];



    public InventoryData(Inventory inventory)
    {

        for (int i = 0; i < 21; i++)
        {
            ids[i] = inventory.items[i].id;
            counters[i] = inventory.items[i].countItem;
        }

    }

}
