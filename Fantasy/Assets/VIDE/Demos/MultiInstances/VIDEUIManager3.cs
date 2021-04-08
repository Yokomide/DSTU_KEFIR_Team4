using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using VIDE_Data; //<--- Import to use VD2 class

/*
 * This is another example of a dialogue UImanager
 * But this one uses the VD2 class that has no static members
 * You can use the VD2 to create multiple instances and have multiple dialogues running at the same time
 */

public class VIDEUIManager3 : MonoBehaviour
{
    public Text[] npcText; //References to UI elements
    IEnumerator[] textAnim = new IEnumerator[4]; 
    public int animatingText = -1; //With this we'll know if and what we are animating

    //A list that will contain instances of VD2, which will handle dialogue data
    public List<VD2> dialogueDataInstances = new List<VD2>();

    void Start()
    {
        //Let's just add 4 instances of VD2 for our dialogues
        for (int i = 0; i < 4; i++)
            dialogueDataInstances.Add(new VD2());
    }

    //Called by UI buttons, sends the dialogue index
    //Will start the convo or update it
    public void ButtonAction(int dialogueIndex)
    {
        var data = dialogueDataInstances[dialogueIndex];
        if (!data.isActive)
        {
            data.OnEnd += End; //Required events
            data.BeginDialogue(GetComponent<VIDE_Assign>());
            //This will begin the dialogue for that instance
        }
        else
        {
            if (animatingText != -1)
            {
                CutTextAnim();
                return;
            } else
            {
                data.Next();
                //Next for that instance
            }
        }

        if (data.isActive) //Check again if loaded as we might reach the end when calling Next
            UpdateDialogue(dialogueIndex);
    }

    //This will update the interface, which is NPC text only
    void UpdateDialogue(int dialogueIndex)
    {
        textAnim[dialogueIndex] = ShowNPCText(dialogueIndex);
        StartCoroutine(textAnim[dialogueIndex]);
    }

    //Animate the npc text
    IEnumerator ShowNPCText(int dialogueIndex)
    {
        animatingText = dialogueIndex;
        var data = dialogueDataInstances[dialogueIndex];
        string text = string.Empty;
        npcText[dialogueIndex].text = text;
        while (text.Length < data.nodeData.comments[data.nodeData.commentIndex].Length)
        {
            text += data.nodeData.comments[data.nodeData.commentIndex][text.Length];
            npcText[dialogueIndex].text = text;
            yield return new WaitForSeconds(0.01f);
        }
        animatingText = -1;
    }

    //Stop animating the text
    void CutTextAnim()
    {
        var data = dialogueDataInstances[animatingText];
        StopCoroutine(textAnim[animatingText]);
        npcText[animatingText].text = data.nodeData.comments[data.nodeData.commentIndex]; //Now just copy full text		
        animatingText = -1;
    }

    //OnEnd will trigger this which will end the conversation for that instance
    void End(VD2 data)
    {       
        //You can use the returned VD2 to find it within a list and get its index.
        npcText[dialogueDataInstances.IndexOf(data)].text = string.Empty;
        data.OnEnd -= End;
        data.EndDialogue();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            VD.SetCurrentLanguage("English");
        }
    }
}
