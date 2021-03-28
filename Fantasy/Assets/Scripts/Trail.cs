using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    private TrailRenderer weaponTrail;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        weaponTrail = GetComponent<TrailRenderer>();
        if (Input.GetMouseButtonDown(0))
        {
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
