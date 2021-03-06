using UnityEngine;

public class Items : MonoBehaviour
{
    public int cost;
    public string nameItem;
    public float id;
    [HideInInspector]
    public float countItem;
    public bool isStackable;
    public int maxStackSize;
    [Multiline(5)]
    public string descriptionItem;
    public string pathIcon;
    public string pathPrefab;
    GameObject inventoryObject;
    Inventory inventory;

    void Start()
    {

        inventoryObject = GameObject.FindGameObjectWithTag("InventoryTag");
        inventory = inventoryObject.GetComponent<Inventory>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            inventory.itemsOnTrigger.Add(gameObject.GetComponent<Items>());
        }
        if (other.tag == "Item")
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
