using UnityEngine;


public class Player : MonoBehaviour
{

    public float speed = 2f; // �������� ������ ���������
    public float shiftSpeed = 3.5f; // �������� ��� ������� Shift
    public float sensetivity = 5f; //�������� �������� ������ ��� �������� �����



    private void FixedUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        Vector3 direction = new Vector3(h, 0, v);

        //��� ��� ������� ������� Shift
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * shiftSpeed * Time.deltaTime);
        }
        else
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * speed * Time.deltaTime);
        }


        //���������� �������� ��������� � ����� 
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

