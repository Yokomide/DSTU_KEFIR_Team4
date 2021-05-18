using References.Location.Item;

namespace Data.Location.Item
{
    public class ItemData 
    {
        public int CostItem { get; private set; }
        public ItemData(ItemDescription description)
        {
            CostItem = description.cost;
        }
    }
}

