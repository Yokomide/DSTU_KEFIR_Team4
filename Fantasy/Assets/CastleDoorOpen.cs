using System.Collections;
using UnityEngine;

public class CastleDoorOpen : MonoBehaviour
{
    static float t = 0.0f;
    public Inventory inventory;
    public GameObject leftDoor;
    public GameObject rightDoor;
    public BossStats_ boss;

    private bool isDoorOpen = false;

    Quaternion right = new Quaternion(0, -105, 0, 0);
    Quaternion left = new Quaternion(0, 105, 0, 0);

    private void Update()
    {
        if (isDoorOpen)
        {
            right.y = Mathf.Lerp(rightDoor.transform.rotation.y, right.y, t);
            left.y = Mathf.Lerp(leftDoor.transform.rotation.y, left.y, t);
            t += 0.005f * Time.deltaTime;
            leftDoor.transform.rotation = left;
            rightDoor.transform.rotation = right;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDoorOpen)
        {
            if (!boss.isAlive)
            {
                isDoorOpen = true;
            }
        }
    }

}
