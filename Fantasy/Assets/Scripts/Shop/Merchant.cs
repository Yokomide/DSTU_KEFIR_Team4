using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Merchant : MonoBehaviour
{
    private bool _isHere = false;
    private bool _showShop = false;

    public Inventory inventory;

    private Canvas _canvas;
    public List<Items> _merchantsItems;
    public List<Items> itemsOnTrigger;

    public GameObject shopContainer;
    public GameObject player;



    private void Start()
    {
        shopContainer.SetActive(false);
        _canvas = GetComponent<Canvas>();
        itemsOnTrigger = new List<Items>();
        _merchantsItems = new List<Items>();
        for (int i = 0; i < shopContainer.transform.childCount; i++)
        {
            _merchantsItems.Add(new Items());
        }
        for (int i = 0; i < shopContainer.transform.childCount; i++)
        {
            //shopContainer.transform.GetChild(i).GetComponent<CurrentItem>().index = i;
        }
    }
    private void Update()
    {
        ToggleShopMenu();

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _isHere = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _isHere = false;
        shopContainer.SetActive(_isHere);
    }

    private void ToggleShopMenu()
    {
        if (_isHere && Input.GetKeyDown(KeyCode.T))
        {
            _showShop = !_showShop;
            shopContainer.SetActive(_showShop);
        }
    }
    public void AddItem(Items item)
    {
        player.GetComponent<MainHeroHp>().money += item.cost;
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
        for (int i = 0; i < _merchantsItems.Count; i++)
        {
            if (_merchantsItems[i].id == item.id)
            {
                if (_merchantsItems[i].countItem < item.maxStackSize)
                {
                    _merchantsItems[i].countItem++;
                    DisplayItem();
                    return;
                }
            }
        }
        AddUnstackableItem(item);
    }

    void AddUnstackableItem(Items item)
    {
        for (int i = 0; i < _merchantsItems.Count; i++)
        {
            if (_merchantsItems[i].id == 0)
            {
                _merchantsItems[i] = item;
                _merchantsItems[i].countItem = 1;

                DisplayItem();
                break;
            }
        }
    }


    public void DisplayItem()
    {
        for (int i = 0; i < _merchantsItems.Count; i++)
        {
            Transform cell = shopContainer.transform.GetChild(i);
            Transform icon = cell.GetChild(0);
            Transform count = icon.GetChild(0);
            Text txt = count.GetComponent<Text>();

            Image img = icon.GetComponent<Image>();
            if (_merchantsItems[i].id != 0)
            {
                img.enabled = true;
                img.sprite = Resources.Load<Sprite>(_merchantsItems[i].pathIcon);
                if (_merchantsItems[i].countItem > 1)
                {
                    txt.text = _merchantsItems[i].countItem.ToString();
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
