using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSaveSecondLocation : MonoBehaviour
{

    private void Start()
    {
        gameObject.GetComponent<MainHeroHp>().LoadPlayer();
    }

}
