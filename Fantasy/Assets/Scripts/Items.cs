using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    public string nameItem;
    public int id;
    public int countItem;
    public bool isStackable;
    [Multiline(5)]
    public string descriptionItem;

    public string pathIcon;
    public string pathPrefab;

    GameObject inventoryObject;
    Inventory inventory;

    public string playerTag = "Player";
    public GameObject Inventory;

    void Start()
    {
        inventoryObject = GameObject.FindGameObjectWithTag("InventoryTag");
        inventory = inventoryObject.GetComponent<Inventory>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Вы можете подобрать предмет!");
    }
    
    void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            if (other.tag == playerTag)
            {
                inventory.AddItem(gameObject.GetComponent<Items>());
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log("Вы потеряли предмет из виду предмет!");
    }
}
