using UnityEngine;


public class Player : MonoBehaviour
{
    public AttackRadiusTrigger AttackRadiusT;
    public float speed = 2f;
    public float shiftSpeedMultiplier = 1.6f;
    public float sensetivity = 5f;



    private void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        Vector3 direction = new Vector3(h, 0, v);

        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * speed * shiftSpeedMultiplier * Time.deltaTime);
        }
        else
        {
            transform.GetComponent<Rigidbody>().MovePosition(transform.GetComponent<Rigidbody>().position + direction * speed * Time.deltaTime);
        }  

        
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

