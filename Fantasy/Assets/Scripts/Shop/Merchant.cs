using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Merchant : MonoBehaviour
{
    private bool _isHere = false;
    private bool _showShop = false;

    public Inventory inventory;


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
        if (inventory.isActiveAndEnabled && Input.GetMouseButtonDown(1))
        {

        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _isHere = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _isHere = false;
        cellContainer.SetActive(_isHere);
    }

    private void ToggleShopMenu()
    {
        if (_isHere && Input.GetKeyDown(KeyCode.T))
        {
            _showShop = !_showShop;
            cellContainer.SetActive(_showShop);
        }
    }
}
