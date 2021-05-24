using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSaveSecondLocation : MonoBehaviour
{
    public GameObject inventory;
    private void Start()
    {
        
        gameObject.GetComponent<MainHeroHp>().LoadPlayer();
        inventory.GetComponent<Inventory>().LoadInventory();
    }

}
