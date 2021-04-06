using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public float rayDistance;
    [HideInInspector]
    public List<Items> items;
    public List<Items> itemsOnTrigger;

    public GameObject cellContainer;


    void Start()
    {
        cellContainer.SetActive(false);
        itemsOnTrigger = new List<Items>();
        items = new List<Items>();
        for (int i = 0; i < cellContainer.transform.childCount; i++)
        {
            items.Add(new Items());
        }
        for (int i = 0; i < cellContainer.transform.childCount; i++)
        {
            cellContainer.transform.GetChild(i).GetComponent<CurrentItem>().index = i;
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(transform.position, ray.direction);
        RaycastHit hit;
        ToggleInventory();
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<Items>())
                {
                    AddItem(hit.collider.GetComponent<Items>());
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
                    Destroy(item.gameObject);
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
                Destroy(item.gameObject);
                itemsOnTrigger.Remove(item);
                break;
            }
        }
    }

    void ToggleInventory()
    {
        if (Input.GetKeyDown(KeyCode.I))
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
}
