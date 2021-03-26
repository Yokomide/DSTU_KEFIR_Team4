using UnityEngine;


public class Player : MonoBehaviour
{

    public float speed = 2f; // скорость нашего персонажа
    public float shiftSpeed = 3.5f; // скорость при нажатии Shift
    public float sensetivity = 5f; //скорость поворота камеры при вращении мышки



    private void FixedUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        Vector3 direction = new Vector3(h, 0, v);

        //бег при нажатии клавиши Shift
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * shiftSpeed * Time.deltaTime);
        }
        else
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * speed * Time.deltaTime);
        }


        //реализация поворота персонажа к мышке 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float hitdist = 0.0f;
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        if (playerPlane.Raycast(ray, out hitdist))
        {
            Vector3 targetPoint = ray.GetPoint(hitdist);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime * sensetivity);
        }
    }

}

