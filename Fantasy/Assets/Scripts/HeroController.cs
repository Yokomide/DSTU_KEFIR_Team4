using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroController : MonoBehaviour

{
    public int speed = 5;
    public GameObject player;//здесь ми указываем персонажа как игровой Object;

    void Start()
    {
        player = (GameObject)this.gameObject; //тут присваиваем персонажа к игровому Object или как-то так.
    }
    // Ах да вместо player надо ставить имя твоего перса которое записано в Unity;
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            player.transform.position += player.transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            player.transform.position -= player.transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            player.transform.position += player.transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            player.transform.position -= player.transform.right * speed * Time.deltaTime;
                                                        
        }
        if (Input.GetKey(KeyCode.Space))
        {
            player.transform.position += player.transform.up * speed * Time.deltaTime;

        }
        
    }
}
/*
{

    public Transform _target; 
    public float moveSpeed = 0.1f; 
    public float verticalSpeed = 0.05f;

    void Start()
    {
    }

    void Update()
    {
        float forwardMove = Input.GetAxis("Vertical") * moveSpeed;
        float sideMove = Input.GetAxis("Horizontal") * moveSpeed;
        float verticalMove = Input.GetAxis("Jump") * verticalSpeed;
        _target.position += _target.forward * forwardMove +
                            _target.right * sideMove +
                            _target.up * verticalMove;
    }
}
*/