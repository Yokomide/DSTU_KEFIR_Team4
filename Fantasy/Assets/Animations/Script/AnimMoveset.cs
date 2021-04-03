using System.Collections;
using UnityEngine;

public class AnimMoveset : MonoBehaviour
{
    public GameObject player;
    public Animator MainCharacter_Animation;
    public TrailRenderer WeaponTrail;


    void Update()
    {
        MainCharacter_Animation.SetFloat("Ver", Input.GetAxis("Vertical"));

        MainCharacter_Animation.SetFloat("Hor", Input.GetAxis("Horizontal"));

        if (Input.GetMouseButtonDown(0))
        {

            MainCharacter_Animation.Play("Attack");
            WeaponTrail.enabled = true;
            StartCoroutine(TrailOff());
        }
        if (GetComponent<MainHeroHp>().HeroHp <= 0f)
        {
            MainCharacter_Animation.SetTrigger("Death");
        }
    }
    IEnumerator TrailOff()
    {
        yield return new WaitForSeconds(2f);

        WeaponTrail.enabled = false;
    }
}



