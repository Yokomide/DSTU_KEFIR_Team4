using System.Collections.Generic;
using UnityEngine;

public class Bag : MonoBehaviour
{
    [System.Serializable]
    public class Bag_Items
    {
        public string name;
        public GameObject item;
        public int count_Items;
        public int drop_Rarity;
        public string pathItem;
        public string pathIcon;
    }
    public List<Bag_Items> bag_Items = new List<Bag_Items>();
}

