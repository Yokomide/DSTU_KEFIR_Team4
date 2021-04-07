using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    Animator animAi;
    public GameObject player;

    public GameObject GetPlayer()
    {
        return player;
    }

    void Start()
    {
        animAi = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.CompareTag("Enemy"))
        {
            animAi.SetFloat("distance", Vector3.Distance(transform.position, player.transform.position));
        }
    }
}
