using UnityEngine;
using UnityEngine.EventSystems;



public class CurrentItem : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    public int index;
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
                }
                else
                {
                    merchant.AddItem(inventory.items[index]);
                }
                if (inventory.items[index].countItem > 1)
                {
                    inventory.items[index].countItem--;
                }
                else
                {
                    inventory.items[index] = new Items();
                }
                merchant.DisplayItem();
                inventory.DisplayItem();
            }
        }

    }
}
