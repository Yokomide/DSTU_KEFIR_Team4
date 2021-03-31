using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



public class CurrentItem : MonoBehaviour,IPointerClickHandler
{
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
        if (eventData.button == PointerEventData.InputButton.Right) // нопки мыши : Left Right Middle
        {
            if (inventory.items[index].id != 0)
            {
                GameObject droppedObject = Instantiate(Resources.Load<GameObject>(inventory.items[index].pathPrefab));
                playerObject = GameObject.FindGameObjectWithTag("Player");
                droppedObject.transform.position = playerObject.transform.position+new Vector3(-1, +1, 0);
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
}
