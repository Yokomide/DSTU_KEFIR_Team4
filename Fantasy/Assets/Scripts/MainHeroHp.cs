using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainHeroHp : MonoBehaviour
{
    public float HeroHp = 250f;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.Mouse1))
        {
            HeroHp -= 0.1f;
        }
    }
}
