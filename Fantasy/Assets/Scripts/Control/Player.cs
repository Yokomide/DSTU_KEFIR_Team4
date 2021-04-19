using UnityEngine;


public class Player : MonoBehaviour
{
    public AttackRadiusTrigger AttackRadiusT;
    public float speed = 2f;
    public float shiftSpeedMultiplier = 1.6f;
    public float sensetivity = 5f;

    private Rigidbody _rb;
    float v;
    float h;


    private void Start()
    {
        _rb = transform.GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");
        Vector3 direction = new Vector3(h, 0, v);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _rb.MovePosition(_rb.position + direction * speed * shiftSpeedMultiplier * Time.fixedDeltaTime);
        }
        else
        {
            _rb.MovePosition(_rb.position + direction * speed * Time.fixedDeltaTime);
        }


        if (GetComponent<MainHeroHp>().heroStats.HeroHp <= 0)
        {
            _rb.constraints = RigidbodyConstraints.FreezePosition;
            _rb.freezeRotation = true;
            gameObject.GetComponent<AnimMoveset>().DeathAnimation();
        }

    }

}

