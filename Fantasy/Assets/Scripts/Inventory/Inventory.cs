using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [HideInInspector] public List<Items> items;
    [HideInInspector] public List<Items> itemsOnTrigger;
    public GameObject cellContainer;
    public GameObject shopContainer;
    public GameObject menu;
    public bool full = false;

    List<Items> itemsMain;

    void Start()
    {
        InventoryMain inventoryMain = InventoryMain.Initialization();
        if (InventoryMain.ReturnList() == null)
        {
            itemsMain = InventoryMain.ListInit();
        }
        else
        {
            itemsMain = InventoryMain.ReturnList();
        }

        cellContainer.SetActive(false);
        itemsOnTrigger = new List<Items>();
        items = new List<Items>();
        for (int i = 0; i < cellContainer.transform.childCount; i++)
        {
            items.Add(itemsMain[i]);
        }

        for (int i = 0; i < cellContainer.transform.childCount; i++)
        {
            cellContainer.transform.GetChild(i).GetComponent<CurrentItem>().index = i;
        }
        DisplayItem();
    }

    void Update()
    {
        ToggleInventory();
        if (itemsOnTrigger.Count != 0 && !full)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!shopContainer.activeSelf)
                {
                    Destroy(itemsOnTrigger[0].gameObject);
                    InventoryMain.AddItem(CloneItems(itemsOnTrigger[0]));
                    AddItem(itemsOnTrigger[0]);

                }
            }
        }
    }


    public void AddItem(Items item)
    {
        if (item.isStackable)
        {
            AddStackableItem(item);
        }
        else
        {
            AddUnstackableItem(item);
        }
    }

    void AddStackableItem(Items item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == item.id)
            {
                if (items[i].countItem < item.maxStackSize)
                {
                    items[i].countItem++;
                    DisplayItem();
                    itemsOnTrigger.Remove(item);
                    return;
                }
            }
        }
        AddUnstackableItem(item);
    }

    void AddUnstackableItem(Items item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == 0)
            {
                items[i] = item;
                items[i].countItem = 1;
                DisplayItem();
                itemsOnTrigger.Remove(item);
                break;
            }
        }
    }

    void ToggleInventory()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !menu.activeSelf)
        {
            if (cellContainer.activeSelf)
            {
                cellContainer.SetActive(false);
            }
            else
            {
                cellContainer.SetActive(true);

            }
        }
    }

    public void DisplayItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            Transform cell = cellContainer.transform.GetChild(i);
            Transform icon = cell.GetChild(0);
            Transform count = icon.GetChild(0);
            Text txt = count.GetComponent<Text>();

            Image img = icon.GetComponent<Image>();
            if (items[i].id != 0)
            {
                img.enabled = true;
                img.sprite = Resources.Load<Sprite>(items[i].pathIcon);
                if (items[i].countItem > 1)
                {
                    txt.text = items[i].countItem.ToString();
                }
                else
                {
                    txt.text = null;
                }
            }
            else
            {
                img.enabled = false;
                img.sprite = null;
                txt.text = null;

            }
        }
    }

    public Items CloneItems(Items item)
    {
        Items cloneitem = new Items();
        cloneitem.cost = item.cost;
        cloneitem.nameItem = item.nameItem;
        cloneitem.id = item.id;
        cloneitem.countItem = 1;
        cloneitem.maxStackSize = item.maxStackSize;
        cloneitem.descriptionItem = item.descriptionItem;
        cloneitem.pathIcon = item.pathIcon;
        cloneitem.pathPrefab = item.pathPrefab;
        cloneitem.isStackable = item.isStackable;
        return cloneitem;
    }

}

