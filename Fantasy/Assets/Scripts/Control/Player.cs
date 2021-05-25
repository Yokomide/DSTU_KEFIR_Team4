using UnityEngine;


public class Player : MonoBehaviour
{
    public float speed = 2f;
    public float shiftSpeedMultiplier = 1.6f;
    public float sensetivity = 5f;

    [HideInInspector]
    public bool revived = false;

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



        if (GetComponent<MainHeroHp>().HeroHp <= 0)
        {
            _rb.constraints = RigidbodyConstraints.FreezePosition;
            _rb.freezeRotation = true;
            gameObject.GetComponent<AnimMoveset>().DeathAnimation();
            revived = false;
        }
        else if (revived)
        {
            _rb.constraints = RigidbodyConstraints.None;
            _rb.freezeRotation = false;

        }

    }

}

