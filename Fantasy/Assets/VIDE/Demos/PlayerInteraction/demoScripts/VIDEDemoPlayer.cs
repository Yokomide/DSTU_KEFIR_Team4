using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VIDE_Data;

public class VIDEDemoPlayer : MonoBehaviour
{
    //This script handles player movement and interaction with other NPC game objects

    public string playerName = "VIDE User";

    //Reference to our diagUI script for quick access
    public VIDEUIManager1 diagUI;
    public QuestChartDemo questUI;
    public Animator blue;

    //Stored current VA when inside a trigger
    public VIDE_Assign inTrigger;

    //DEMO variables for item inventory
    //Crazy cap NPC in the demo has items you can collect
    public List<string> demo_Items = new List<string>();
    public List<string> demo_ItemInventory = new List<string>();

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<VIDE_Assign>() != null)
            inTrigger = other.GetComponent<VIDE_Assign>();
    }

    void OnTriggerExit()
    {
        inTrigger = null;
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        //Only allow player to move and turn if there are no dialogs loaded
        if (!VD.isActive)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * 5, 0);
            float move = Input.GetAxisRaw("Vertical");
            transform.position += transform.forward * 7 * move * Time.deltaTime;
            blue.SetFloat("speed", move);
        }

        //Interact with NPCs when pressing E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        //Hide/Show cursor
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.visible = !Cursor.visible;
            if (Cursor.visible)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    //Casts a ray to see if we hit an NPC and, if so, we interact
    void TryInteract()
    {
        /* Prioritize triggers */

        if (inTrigger)
        {
            diagUI.Interact(inTrigger);
            return;
        }

        /* If we are not in a trigger, try with raycasts */

        RaycastHit rHit;

        if (Physics.Raycast(transform.position, transform.forward, out rHit, 2))
        {
            //Lets grab the NPC's VIDE_Assign script, if there's any
            VIDE_Assign assigned;
            if (rHit.collider.GetComponent<VIDE_Assign>() != null)
                assigned = rHit.collider.GetComponent<VIDE_Assign>();
            else return;

            if (assigned.alias == "QuestUI")
            {
                questUI.Interact(); //Begins interaction with Quest Chart
            } else
            {
                diagUI.Interact(assigned); //Begins interaction
            }
        }
    }
}
