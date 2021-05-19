using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveScript : MonoBehaviour
{
   Player player;

    private void Start()
    {
        player = gameObject.GetComponent<Player>();
    }
    public void Revive()
    {
        player.revived = true;
    }
}
