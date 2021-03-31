using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    public string nameItem;
    public int id;
    public int countItem;
    public bool isStackable;
    public int maxStackSize;
    [Multiline(5)]
    public string descriptionItem;

    public string pathIcon;
    public string pathPrefab;

    GameObject inventoryObject;
    Inventory inventory;

    public GameObject Inventory;

    void Start()
    {
        inventoryObject = GameObject.FindGameObjectWithTag("InventoryTag");
        inventory = inventoryObject.GetComponent<Inventory>();
    }

    
    void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            if (other.tag == "Player")
            {
                inventory.AddItem(gameObject.GetComponent<Items>());
            }
        }
    }
}
