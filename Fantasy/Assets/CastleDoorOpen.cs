using System.Collections;
using UnityEngine;

public class CastleDoorOpen : MonoBehaviour
{

    public Inventory inventory;
    public GameObject leftDoor;
    public GameObject rightDoor;
    public BossStats_ boss;

    private bool isDoorOpen = false;

    public AudioClip sound;
    AudioSource audio;

    private void Start()
    {
        audio = gameObject.GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (isDoorOpen)
        {
            leftDoor.transform.rotation = Quaternion.RotateTowards(leftDoor.transform.rotation, Quaternion.Euler(leftDoor.transform.rotation.x, 200, leftDoor.transform.rotation.z), 20 * Time.deltaTime);
            Debug.Log(leftDoor.transform.rotation.eulerAngles.y);
            rightDoor.transform.rotation = Quaternion.RotateTowards(rightDoor.transform.rotation, Quaternion.Euler(rightDoor.transform.rotation.x, -20, rightDoor.transform.rotation.z),  20 * Time.deltaTime);
            if (leftDoor.transform.rotation.eulerAngles.y == 200)
            {
                audio.Stop();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDoorOpen)
        {
            if (!boss.isAlive)
            {
                isDoorOpen = true;
                audio.PlayOneShot(sound);
            }
        }
    }

}
