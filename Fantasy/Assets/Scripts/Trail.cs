using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    private TrailRenderer weaponTrail;
    // Start is called before the first frame update
    void Start()
    {
        weaponTrail = GetComponent<TrailRenderer>();
        weaponTrail.enabled = true;
    }

    IEnumerator TrailOff()
    {
        yield return new WaitForSeconds(2f);

        weaponTrail.enabled = false;
    }
}
