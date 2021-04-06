using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainHeroHp : MonoBehaviour
{
    public float HeroHp = 250f;
    public float Lvl = 0;

    public Text level;
    private void Update()
    {
        if (Input.GetKey(KeyCode.Minus))
        {
            HeroHp -= 0.1f;
        }

        level.text = "LEVEL: " + Lvl;
        
    }
}
