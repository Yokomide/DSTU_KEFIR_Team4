using UnityEngine;

public class Items : MonoBehaviour
{
    public int cost;
    public string nameItem;
    public int id;
    public int countItem;
    public bool isStackable;
    public int maxStackSize;
    public string lootType;
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

    void Update()
    {
        if (inventory.itemsOnTrigger.Count != 0)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                inventory.AddItem(inventory.itemsOnTrigger[0]);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            inventory.itemsOnTrigger.Add(gameObject.GetComponent<Items>());
        }
        if(other.tag == "Item")
        {
            Physics.IgnoreCollision(gameObject.GetComponent<MeshCollider>(), other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            inventory.itemsOnTrigger.Remove(gameObject.GetComponent<Items>());
        }
    }
}