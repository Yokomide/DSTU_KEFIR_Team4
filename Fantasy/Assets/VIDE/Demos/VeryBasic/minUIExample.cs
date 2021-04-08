using UnityEngine;
using VIDE_Data;

public class minUIExample : MonoBehaviour {

    void Start()
    {
        gameObject.AddComponent<VD>();
    }

    void OnDisable()
    {
        VD.EndDialogue();
    }

    void OnGUI () {
	    if (VD.isActive)
        {
            var data = VD.nodeData; //Quick reference
            if (data.isPlayer) // If it's a player node, let's show all of the available options as buttons
            {
                for (int i = 0; i < data.comments.Length; i++)
                {
                    if (GUILayout.Button(data.comments[i])) //When pressed, set the selected option and call Next()
                    {
                        data.commentIndex = i;
                        VD.Next();
                    }
                }
            } else //if it's a NPC node, Let's show the comment and add a button to continue
            {
                GUILayout.Label(data.comments[data.commentIndex]);

                if (GUILayout.Button(">")){
                    VD.Next();
                }
            }
			if (data.isEnd) // If it's the end, let's just call EndDialogue
                {
                    VD.EndDialogue();
                }
        } else // Add a button to begin conversation if it isn't started yet
        {
            if (GUILayout.Button("Start Convo"))
            {
                VD.BeginDialogue(GetComponent<VIDE_Assign>()); //We've attached a VIDE_Assign to this same gameobject, so we just call the component
            }
        }
	}
}
