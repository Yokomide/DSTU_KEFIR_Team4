using UnityEngine;
using UnityEngine.EventSystems;
public class CurrentItemMerchant : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    public int index;
    GameObject inventoryObject;
    Inventory inventory;
    [HideInInspector]
    public Merchant merchant;

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
            if (!inventory.full)
            {
                if (merchant._merchantsItems[index].id != 0)
                {
                    if (merchant._merchantsItems[index].countItem > 1)
                    {
                        for (int i = 0; i < inventory.items.Count; i++)
                        {
                            if (inventory.items[i].id == 0)
                            {
                                merchant._merchantsItems[index].countItem--;
                                inventory.AddItem(inventory.CloneItems(merchant._merchantsItems[index]));
                                break;
                            }
                            else if (merchant._merchantsItems[index].id == inventory.items[i].id && inventory.items[i].countItem != inventory.items[i].maxStackSize)
                            {
                                merchant._merchantsItems[index].countItem--;
                                inventory.AddItem(inventory.CloneItems(merchant._merchantsItems[index]));
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (merchant._merchantsItems[index].isStackable)
                        {
                            for (int i = 0; i < inventory.items.Count; i++)
                            {
                                if (inventory.items[i].id == 0)
                                {
                                inventory.AddItem(inventory.CloneItems(merchant._merchantsItems[index]));
                                    merchant._merchantsItems[index] = new Items();
                                    break;
                                }
                                else if (merchant._merchantsItems[index].id == inventory.items[i].id)
                                {
                                inventory.AddItem(inventory.CloneItems(merchant._merchantsItems[index]));
                                    merchant._merchantsItems[index] = new Items();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < inventory.items.Count; i++)
                            {
                                if (inventory.items[i].id == 0)
                                {
                                inventory.AddItem(inventory.CloneItems(merchant._merchantsItems[index]));
                                    merchant._merchantsItems[index] = new Items();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            merchant.DisplayItem();
            inventory.DisplayItem();
        }
        //Shift fo full stack
    }
}
