using UnityEngine;
using System.Collections.Generic;
using VIDE_Data;

public class CylinderGuy : MonoBehaviour {

	//The dialogue uses an Action Node to call this function
    //The Action Node also uses the 'Go to Node' predefined action to continue on to the desired node 
    //This method retrieves the Extra Variables of a node
    //Then uses its contents to modify the default comment of a Dialogue node before we get to it
    //GetExtraVariables and Update Comment: Remember the dialogue doesn't necessarily require to be active, only loaded. 
	public void ModifyText () {
        string newText;
        Dictionary<string, object> options = VD.GetExtraVariables(VD.assigned.assignedDialogue, 0);
        List<string> keys = new List<string>(options.Keys); 
        int randomPick = Random.Range(0, keys.Count);
        newText = (string) options[keys[randomPick]];
        VD.SetComment(VD.assigned.assignedDialogue, 0, 0, newText);

        QuestChartDemo.CylinderGuyAddInteraction(randomPick);

    }
}
