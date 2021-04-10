using System.Collections;
using UnityEngine;

public class AnimMoveset : MonoBehaviour
{
    public GameObject player;
    public Animator MainCharacter_Animation;
    public TrailRenderer WeaponTrail;

    private void Update()
    {
        MainCharacter_Animation.SetFloat("Ver", Input.GetAxis("Vertical"));
        MainCharacter_Animation.SetFloat("Hor", Input.GetAxis("Horizontal"));
    }
    public void MoveAnimation()
    {
        MainCharacter_Animation.SetFloat("Ver", Input.GetAxis("Vertical"));
        MainCharacter_Animation.SetFloat("Hor", Input.GetAxis("Horizontal"));
    }
    public void DeathAnimation()
    {
        MainCharacter_Animation.SetTrigger("Death");
    }
    public void AttackAnimation()
    {
        MainCharacter_Animation.Play("Attack");
        WeaponTrail.enabled = true;
        StartCoroutine(TrailOff());
    }
    IEnumerator TrailOff()
    {
        yield return new WaitForSeconds(2f);

        WeaponTrail.enabled = false;
    }
}



