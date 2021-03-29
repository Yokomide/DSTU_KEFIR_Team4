using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimMoveset : MonoBehaviour
{
    // Start is called before the first frame update
    
    public Animator anim;
    public TrailRenderer weaponTrail;
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        
        anim.SetFloat("Ver", Input.GetAxis("Vertical"));

        anim.SetFloat("Hor", Input.GetAxis("Horizontal"));

        if (Input.GetMouseButtonDown(0)){
   
                anim.Play("Attack");
                weaponTrail.enabled = true;
                StartCoroutine(TrailOff());
        }
    }
    IEnumerator TrailOff()
    {
        yield return new WaitForSeconds(2f);

        weaponTrail.enabled = false;
    }
}
       
    

