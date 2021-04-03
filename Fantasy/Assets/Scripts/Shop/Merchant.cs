using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Merchant : MonoBehaviour
{
    private bool _isHere = false;
    private bool _showShop = false;


    private Canvas _canvas;
    private List<Items> _merchantsItems;

    public GameObject cellContainer;


    private void Start()
    {
        cellContainer.SetActive(false);
        _canvas = GetComponent<Canvas>();
        _merchantsItems = new List<Items>();
        for (int i = 0; i < cellContainer.transform.childCount; i++)
        {
            _merchantsItems.Add(new Items());
        }
        for (int i = 0; i < cellContainer.transform.childCount; i++)
        {
            cellContainer.transform.GetChild(i).GetComponent<CurrentItem>().index = i;
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
        cellContainer.SetActive(false);
    }

    private void ToggleShopMenu()
    {
        if (_isHere && Input.GetKeyDown(KeyCode.T))
        {
            cellContainer.SetActive(true);
            _showShop = true;
        }
        else if (Input.GetKeyDown(KeyCode.T) && _showShop)
        {
            cellContainer.SetActive(false);
            _showShop = false;
        }
    }
}
