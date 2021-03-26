using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    GameObject It;
    void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            It.GetComponent<Inventory>();
            Inventory inv = GetComponent<Inventory>();
            Debug.Log("Да ты че...");
            Destroy(gameObject);
        }
    } 
    public string nameItem;
    public int id;
    public int countItem;
    public bool isStackable;
    [Multiline(5)]
    public string descriptionItem;

    public string pathIcon;
    public string pathPrefab;
    void Start()
    {

        It = GameObject.Find("Inventory");//ссылка на другой скрипт
    }
    
}
