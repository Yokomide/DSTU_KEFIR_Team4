using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Data.Inventory;
using References.Inventory;
using References.Location;
using References.Location.Item;
using Data.Location;
using Data.Location.Item;

namespace Controllers.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [HideInInspector] public List<ItemDescription> items;
        [HideInInspector] public List<ItemDescription> itemsOnTrigger;
        public GameObject cellContainer;
        public GameObject shopContainer;
        public GameObject menu;


        void Start()
        {
            cellContainer.SetActive(false);
            for (int i = 0; i < cellContainer.transform.childCount; i++)
            {
                items.Add(new ItemDescription());
            }
            for (int i = 0; i < cellContainer.transform.childCount; i++)
            {
                cellContainer.transform.GetChild(i).GetComponent<CurrentItem>().index = i;
            }
        }

        void Update()
        {
            ToggleInventory();
        }


        void ToggleInventory()
        {
            if (Input.GetKeyDown(KeyCode.I) && !menu.activeSelf)
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

        public void DisplayItems()
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
}
