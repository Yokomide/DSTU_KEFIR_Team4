using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera camera;
    public float speed = 1.2f;
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.GetComponent<Rigidbody>().AddForce(Vector3.forward * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.GetComponent<Rigidbody>().AddForce(Vector3.forward *- speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.GetComponent<Rigidbody>().AddForce(Vector3.left * speed);

        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.GetComponent<Rigidbody>().AddForce(Vector3.right * speed);
        }
        camera.transform.position = new Vector3(transform.position.x,6, transform.position.z);
    }
}
