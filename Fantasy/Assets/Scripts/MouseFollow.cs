using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    public Player player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float hitdist = 0.0f;
        Plane playerPlane = new Plane(Vector3.up, player.transform.position);
        if (playerPlane.Raycast(ray, out hitdist))
        {
            Vector3 targetPoint = ray.GetPoint(hitdist);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - player.transform.position);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.speed * Time.deltaTime * player.sensetivity);
        }
    }
}
