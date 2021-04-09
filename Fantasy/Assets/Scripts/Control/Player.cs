using UnityEngine;


public class Player : MonoBehaviour
{
    public AttackRadiusTrigger AttackRadiusT;
    public float speed = 2f; // скорость нашего персонажа
    public float shiftSpeedMultiplier = 1.6f; // скорость при нажатии Shift
    public float sensetivity = 5f; //скорость поворота камеры при вращении мышки



    private void FixedUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        Vector3 direction = new Vector3(h, 0, v);

        //бег при нажатии клавиши Shift
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * speed * shiftSpeedMultiplier * Time.deltaTime);
        }
        else
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * speed * Time.deltaTime);
        }  

        //—мерть √√
        if (GetComponent<MainHeroHp>().HeroHp <=0)
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            GetComponent<Rigidbody>().freezeRotation = true;
            Destroy(GetComponent <MouseFollow>());
            Destroy(GetComponent<AnimMoveset>());
            Destroy(GetComponent<SphereCollider>());
            Destroy(AttackRadiusT.GetComponent<SphereCollider>());

        }

    }

}

