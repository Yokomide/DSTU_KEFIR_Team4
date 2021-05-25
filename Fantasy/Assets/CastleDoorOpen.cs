using System.Collections;
using UnityEngine;

public class CastleDoorOpen : MonoBehaviour
{

    public Inventory inventory;
    public GameObject leftDoor;
    public GameObject rightDoor;
    public BossStats_ boss;

    private bool isDoorOpen = false;


    private void Update()
    {
        if (isDoorOpen)
        {

            leftDoor.transform.rotation = Quaternion.RotateTowards(leftDoor.transform.rotation, Quaternion.Euler(leftDoor.transform.rotation.x, 200, leftDoor.transform.rotation.z), 20 * Time.deltaTime);
            rightDoor.transform.rotation = Quaternion.RotateTowards(rightDoor.transform.rotation, Quaternion.Euler(rightDoor.transform.rotation.x, -20, rightDoor.transform.rotation.z),  20 * Time.deltaTime);
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
