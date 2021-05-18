using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using References.Inventory;
using References.Location;
using References.Location.Item;
using Data.Location;
using Data.Location.Item;
//ненужные удалить!

namespace Data.Inventory
{
    public class InventoryData 
    {
       public List<ItemData> Items;
       public List<ItemDescription> ItemsOnTrigger;

       public InventoryData(InventoryDescription description)
       {
           Items = new List<ItemData>();
       }

       public ItemData CreateItem(ItemDescription description){
           var item = new ItemData(description);
           Items.Add(item);
           
           return item;
       }
    }
}

