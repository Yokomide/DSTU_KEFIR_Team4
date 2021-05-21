using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSaveSecondLocation : MonoBehaviour
{
    public GameObject inventory;
    private void Start()
    {
        inventory.GetComponent<Inventory>().LoadInventory();
        gameObject.GetComponent<MainHeroHp>().LoadPlayer();
    }

}
