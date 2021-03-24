using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anime : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator anim;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("Ver", Input.GetAxis("Vertical"));
        anim.SetFloat("Hor", Input.GetAxis("Horizontal"));
    }
}
