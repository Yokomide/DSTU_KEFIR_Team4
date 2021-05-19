using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyShopMenu : MonoBehaviour
{
    private bool _isHere = false;
    public bool _showShop = false;
    private Canvas _canvas;
    public List<Items> _merchantbuyItems;
    public GameObject buyshopContainer;
    public GameObject player;
    public GameObject menu;
    public bool full = false;

    private void Start()
    {
        buyshopContainer.SetActive(false);
        _canvas = GetComponent<Canvas>();
        _merchantbuyItems = new List<Items>();
        for (int i = 0; i < buyshopContainer.transform.childCount; i++)
        {
            _merchantbuyItems.Add(new Items());
        }
        for (int i = 0; i < buyshopContainer.transform.childCount; i++)
        {
            buyshopContainer.transform.GetChild(i).GetComponent<CurrentItemMerchant>().index = i;
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
        if (other.CompareTag("Player"))
        {
            _isHere = false;
            buyshopContainer.SetActive(_isHere);
        }
    }
    private void ToggleShopMenu()
    {
        if (_isHere && Input.GetKeyDown(KeyCode.T) )
        {
            _showShop = !_showShop;
            buyshopContainer.SetActive(_showShop);
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
        for (int i = 0; i < _merchantbuyItems.Count; i++)
        {
            if (_merchantbuyItems[i].id == item.id)
            {
                if (_merchantbuyItems[i].countItem < item.maxStackSize)
                {
                    _merchantbuyItems[i].countItem++;
                    DisplayItem();
                    return;
                }
            }
        }
        AddUnstackableItem(item);
    }

    void AddUnstackableItem(Items item)
    {
        for (int i = 0; i < _merchantbuyItems.Count; i++)
        {
            if (_merchantbuyItems[i].id == 0)
            {
                _merchantbuyItems[i] = item;
                _merchantbuyItems[i].countItem = 1;
                DisplayItem();
                break;
            }
        }
    }
    public void DisplayItem()
    {
        for (int i = 0; i < _merchantbuyItems.Count; i++)
        {
            Transform cell = buyshopContainer.transform.GetChild(i);
            Transform icon = cell.GetChild(0);
            Transform count = icon.GetChild(0);
            Text txt = count.GetComponent<Text>();
            Image img = icon.GetComponent<Image>();
            if (_merchantbuyItems[i].id != 0)
            {
                img.enabled = true;
                img.sprite = Resources.Load<Sprite>(_merchantbuyItems[i].pathIcon);
                if (_merchantbuyItems[i].countItem > 1)
                {
                    txt.text = _merchantbuyItems[i].countItem.ToString();
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
    public void SellPushButton()
    {
        for (int i = 0; i < _merchantbuyItems.Count; i++)
        {
            if (_merchantbuyItems[i].id != 0)
            {
                if (_merchantbuyItems[i].isStackable && _merchantbuyItems[i].countItem > 1)
                {
                    while (_merchantbuyItems[i].countItem > 1)
                    {
                        player.GetComponent<MainHeroHp>().money += _merchantbuyItems[i].cost;
                        _merchantbuyItems[i].countItem--;
                    }
                }
                if (_merchantbuyItems[i].countItem == 1)
                {
                    player.GetComponent<MainHeroHp>().money += _merchantbuyItems[i].cost;
                    _merchantbuyItems[i] = new Items();
                }
            }
        }
        DisplayItem();
    }
}
