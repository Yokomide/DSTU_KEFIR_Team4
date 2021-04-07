using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMove : MonoBehaviour
{

    private Camera _mainCamera;
    private NavMeshAgent agent;
    

    void Start()
    {
        _mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RaycastHit hit;
            if (Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                agent.SetDestination(hit.point);
            }
        }
    }
}