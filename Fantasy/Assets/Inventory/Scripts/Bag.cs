using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bag : MonoBehaviour, IPointerClickHandler
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

    [HideInInspector]
    public int index;

    GameObject playerObject;
    GameObject inventoryObject;
    Inventory inventory;

    void Start()
    {
        inventoryObject = GameObject.FindGameObjectWithTag("InventoryTag");
        inventory = inventoryObject.GetComponent<Inventory>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) // нопки мыши : Left Right Middle
        {
            GameObject droppedObject = Instantiate(Resources.Load<GameObject>(bag_Items[Random.Range(0,3)].pathItem));
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
            inventory.DisplayItem();
        }
    }
}

