using System.Collections.Generic;
using References.Location.Item;
using References.Location;
using UnityEngine;

namespace References.Inventory
{
    [CreateAssetMenu(fileName = "InventoryDescription", menuName = "References/InventoryDescription")]
    public class InventoryDescription : ScriptableObject
    {
        public List<ItemDescription> Items;
        public bool full;
    }
}