using System.Collections;
using UnityEngine;

public class AnimMoveset : MonoBehaviour
{
    public GameObject player;
    public Animator MainCharacterAnimation;
    public TrailRenderer WeaponTrail;

    private void Update()
    {
        MainCharacterAnimation.SetFloat("Ver", Input.GetAxis("Vertical"));
        MainCharacterAnimation.SetFloat("Hor", Input.GetAxis("Horizontal"));
    }
    public void MoveAnimation()
    {
        MainCharacterAnimation.SetFloat("Ver", Input.GetAxis("Vertical"));
        MainCharacterAnimation.SetFloat("Hor", Input.GetAxis("Horizontal"));
    }
    public void DeathAnimation()
    {
        MainCharacterAnimation.SetTrigger("Death");
    }
    public void AttackAnimation()
    {
        MainCharacterAnimation.Play("Attack");
        WeaponTrail.enabled = true;
        StartCoroutine(TrailOff());
    }
    IEnumerator TrailOff()
    {
        yield return new WaitForSeconds(2f);

        WeaponTrail.enabled = false;
    }
}



