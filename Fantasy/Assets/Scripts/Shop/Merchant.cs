using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Merchant : MonoBehaviour
{
    private bool _isHere = false;
    public bool _showShop = false;
    private Canvas _canvas;
    [HideInInspector]
    public List<Items> _merchantsItems;
    public GameObject shopContainer;
    public GameObject player;
    public GameObject menu;
    public bool full = false;
    public List<Items> itemsInShop;


    public AudioClip sound;
    AudioSource audio;

    private void Start()
    {
        gameObject.AddComponent<AudioSource>();
        audio = gameObject.GetComponent<AudioSource>();
        gameObject.GetComponent<AudioSource>().clip = sound;
        shopContainer.SetActive(false);
        _canvas = GetComponent<Canvas>();
        _merchantsItems = new List<Items>();
        for (int i = 0; i < shopContainer.transform.childCount; i++)
        {
            _merchantsItems.Add(new Items());
        }
        for (int i = 0; i < shopContainer.transform.childCount; i++)
        {
            shopContainer.transform.GetChild(i).GetComponent<CurrentItemMerchant>().index = i;
        }
        for(int i = 0; i < itemsInShop.Count; i++)
        {
            AddItem(itemsInShop[i]);
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
            shopContainer.SetActive(_isHere);
        }
    }
    private void ToggleShopMenu()
    {
        if (_isHere && Input.GetKeyDown(KeyCode.T) && !menu.activeSelf)
        {
            _showShop = !_showShop;
            shopContainer.SetActive(_showShop);
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
        for (int i = 0; i < _merchantsItems.Count; i++)
        {
            if (_merchantsItems[i].id == item.id)
            {
                if (_merchantsItems[i].countItem < item.maxStackSize)
                {
                    _merchantsItems[i].countItem++;
                    if(_merchantsItems.Count-1 == i)
                    {
                        _merchantsItems[_merchantsItems.Count-1]=new Items();
                    }
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
                if(_merchantsItems.Count-1 == i)
                    {
                        _merchantsItems[_merchantsItems.Count-1]=new Items();
                    }
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
    public void SellPushButton()
    {
        audio.PlayOneShot(sound);
        for (int i = 0; i < _merchantsItems.Count; i++)
        {
            if (_merchantsItems[i].id != 0)
            {
                if (_merchantsItems[i].isStackable && _merchantsItems[i].countItem > 1)
                {
                    while (_merchantsItems[i].countItem > 1)
                    {
                        player.GetComponent<MainHeroHp>().money += _merchantsItems[i].cost;
                        _merchantsItems[i].countItem--;
                    }
                }
                if (_merchantsItems[i].countItem == 1)
                {
                    player.GetComponent<MainHeroHp>().money += _merchantsItems[i].cost;
                    _merchantsItems[i] = new Items();
                }
            }
        }
        DisplayItem();
    }
}
