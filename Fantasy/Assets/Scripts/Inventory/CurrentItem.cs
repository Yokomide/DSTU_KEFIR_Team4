using UnityEngine;
using UnityEngine.EventSystems;
public class CurrentItem : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int index;
    public GameObject shop;
    GameObject playerObject;
    GameObject inventoryObject;
    Inventory inventory;
    Merchant merchant;
    Bag bag;
    
    void Start()
    {
        inventoryObject = GameObject.FindGameObjectWithTag("InventoryTag");
        inventory = inventoryObject.GetComponent<Inventory>();
        merchant = inventoryObject.GetComponent<Merchant>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (inventory.items[index].id != 0)
            {
                if (!shop.activeSelf)
                {
                    GameObject droppedObject = Instantiate(Resources.Load<GameObject>(inventory.items[index].pathPrefab));
                    playerObject = GameObject.FindGameObjectWithTag("Player");
                    droppedObject.transform.position = playerObject.transform.position + new Vector3(-1, +1, 0);
                    if (inventory.items[index].countItem > 1)
                    {
                        inventory.items[index].countItem--;
                    }
                    else
                    {
                        inventory.items[index] = new Items();
                    }
                }
                else
                {
                    if (inventory.items[index].countItem > 1)
                    {
                        for (int i = 0; i < merchant._merchantsItems.Count; i++)
                        {

                            if (merchant._merchantsItems[i].id == 0)
                            {
                                inventory.items[index].countItem--;
                                merchant.AddItem(CloneClass(inventory.items[index]));
                                break;
                            }
                            else if (merchant._merchantsItems[i].id == inventory.items[index].id && merchant._merchantsItems[i].countItem != merchant._merchantsItems[i].maxStackSize)
                            {
                                inventory.items[index].countItem--;
                                merchant.AddItem(CloneClass(inventory.items[index]));
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (inventory.items[index].isStackable)
                        {
                            for (int i = 0; i < merchant._merchantsItems.Count; i++)
                            {

                                if (merchant._merchantsItems[i].id == 0)
                                {
                                    merchant.AddItem(CloneClass(inventory.items[index]));
                                    inventory.items[index] = new Items();
                                    break;
                                }
                                else if (merchant._merchantsItems[i].id == inventory.items[index].id && merchant._merchantsItems[i].countItem != merchant._merchantsItems[i].maxStackSize)
                                {
                                    merchant.AddItem(CloneClass(inventory.items[index]));
                                    inventory.items[index] = new Items();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < merchant._merchantsItems.Count; i++)
                            {
                                if (merchant._merchantsItems[i].id == 0)
                                {
                                    merchant.AddItem(CloneClass(inventory.items[index]));
                                    inventory.items[index] = new Items();
                                    break;
                                }
                            }
                        }
                    }
                }
                //Shift fo full stack
                merchant.DisplayItem();
                inventory.DisplayItem();
            }
        }
    }

    public Items CloneClass(Items item)
    {
        Items new_item =item;
        new_item.id = item.id;
        new_item.cost = item.cost;
        new_item.nameItem = item.nameItem;
        new_item.countItem = 1;
        new_item.isStackable = item.isStackable;
        new_item.maxStackSize = item.maxStackSize;
        new_item.lootType = item.lootType;
        new_item.descriptionItem = item.descriptionItem;
        new_item.pathPrefab=item.pathPrefab;
        new_item.pathIcon=item.pathIcon;
        return new_item;
    }
}
