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

        int combo = Random.Range(1, 4);
        switch (combo) {
            case 1:
                MainCharacterAnimation.Play("Attack");
                break;
            case 2:
                MainCharacterAnimation.Play("Attack1");
                break;
            case 3:
                MainCharacterAnimation.Play("Attack2");
                break;
        }
        WeaponTrail.enabled = true;
        StartCoroutine(TrailOff());
    }
    IEnumerator TrailOff()
    {
        yield return new WaitForSeconds(2f);

        WeaponTrail.enabled = false;
    }
}



