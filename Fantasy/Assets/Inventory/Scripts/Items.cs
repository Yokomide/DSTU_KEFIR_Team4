using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public List<Items> itemsOnTrigger;

    void Start()
    {
        itemsOnTrigger = new List<Items>();
        inventoryObject = GameObject.FindGameObjectWithTag("InventoryTag");
        inventory = inventoryObject.GetComponent<Inventory>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        { 
            itemsOnTrigger.Add(gameObject.GetComponent<Items>());
        }
    }
    void Update()
    {
        if (itemsOnTrigger.Count != 0)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                 inventory.AddItem(itemsOnTrigger[0]);
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            itemsOnTrigger.Remove(gameObject.GetComponent<Items>());
        }
    }
}
