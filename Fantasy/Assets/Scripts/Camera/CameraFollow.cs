using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Player player;
    public float cameraHeight = 8f;
    public float cameraZoffset = 2f;

    private void Update()
    {
        Vector3 pos = new Vector3(player.transform.position.x, player.transform.position.y+cameraHeight, player.transform.position.z-cameraZoffset);
        transform.GetComponent<Rigidbody>().MovePosition(pos);
    }
}
