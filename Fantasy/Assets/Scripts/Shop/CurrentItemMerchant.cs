using UnityEngine;
using UnityEngine.EventSystems;
public class CurrentItemMerchant : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    public int index;
    GameObject inventoryObject;
    Inventory inventory;
    Merchant merchant;

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
            if (merchant._merchantsItems[index].id != 0)
            {
                if (merchant._merchantsItems[index].countItem > 1)
                {
                    merchant._merchantsItems[index].countItem--;
                    inventory.AddItem(Instantiate(Resources.Load<Items>(merchant._merchantsItems[index].pathPrefab)));
                }
                else
                {
                    inventory.AddItem(Instantiate(Resources.Load<Items>(merchant._merchantsItems[index].pathPrefab)));
                    merchant._merchantsItems[index] = new Items();
                }
                merchant.DisplayItem();
                inventory.DisplayItem();
            }
        }
        //Shift fo full stack
    }
}
