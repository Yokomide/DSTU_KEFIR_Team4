using UnityEngine;

namespace References.Location.Item
{
    [CreateAssetMenu(fileName = "ItemDescription", menuName = "References/ItemDescription")]
    public class ItemDescription : ScriptableObject
    {
        public string nameItem; 
        public int id;
        public int cost;
        public bool isStackable;
        public int maxStackSize;
        public int countItem;// может быть инвентарь должен иметь количество вещей в ячейке ,а не предмет ?
        public string lootType;
        [Multiline(5)]public string descriptionItem;
        public string pathIcon;
        public string pathPrefab;
    }
}

