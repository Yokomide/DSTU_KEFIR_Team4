using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using VIDE_Data;

/*
 * This is another example script that handles the data obtained from nodeData
 * It handles localization data.
 * Refer to Example3Dialogue dialogue in the VIDE Editor.
 * It is simpler and focused on showing both NPC and Player text at the same time
 * It doesn't require any VIDE_Data or VIDE_Assign component to be in the scene
 */

public class VIDEUIManager2 : MonoBehaviour
{
    public string dialogueNameToLoad;
    public Text[] playerChoices;
    public Text npcText;
    public Image flag;
    public AudioSource audioPlayer;

    void Start()
    {
        //Sets the temp VIDE_Assign’s variables for the given dialogue.
        VD.SetAssigned(dialogueNameToLoad, "LocalizationTest", -1, null, null);
    }

    //Called by UI button
    public void Begin()
    {
        if (!VD.isActive)
        {
            transform.GetChild(1).gameObject.SetActive(true); //UI stuff
            transform.GetChild(0).gameObject.SetActive(false); //UI stuff
            VD.OnNodeChange += NodeChangeAction; //Required events
            VD.OnEnd += End; //Required events
            VD.BeginDialogue(dialogueNameToLoad);
        }
    }

    //Called by UI buttons, every button sends a different choice index
    public void ButtonChoice(int choice)
    {
        VD.nodeData.commentIndex = choice; //Set commentIndex as it acts as the picked choice
        if (VD.nodeData.extraVars.ContainsKey("loadLang"))
        {
            if (VD.nodeData.commentIndex < (int) VD.nodeData.extraVars["loadLang"]) //Don't count index 3 as language
			{
				VD.OnLanguageChange += UpdateWithNewLanguage;
				VD.SetCurrentLanguage(VD.GetLanguages()[VD.nodeData.commentIndex]); 
			}
			else 
			{
				VD.Next(); 
			}
        }
    }

    void OnDisable()
    {
        //If the script gets destroyed, let's make sure we force-end the dialogue to prevent errors
        End(null);
    }

    //This will trigger with the OnLanguageChange event
    //It will make sure the current text being displayed will be updated with the new localization
    void UpdateWithNewLanguage() {
        npcText.text = VD.GetNodeData(0).comments[0];
        flag.sprite = VD.GetNodeData(0).sprite;
        SetPlayerChoices();
        audioPlayer.clip = VD.GetNodeData(0).audios[0];
        audioPlayer.Play();
        VD.OnLanguageChange -= UpdateWithNewLanguage;
    }

    //Called by the OnNodeChange event
    void NodeChangeAction(VD.NodeData data)
    {
        if (data.isPlayer)
        {
            SetPlayerChoices();
        }
        else
        {
            WipePlayerChoices();
            StartCoroutine(ShowNPCText());
        }
    }

    void WipePlayerChoices()
    {
        for (int i = 0; i < playerChoices.Length; i++)
        {
            playerChoices[i].transform.parent.gameObject.SetActive(false);
        }
    }

    void SetPlayerChoices()
    {
        for (int i = 0; i < playerChoices.Length; i++)
        {
            if (i < VD.nodeData.comments.Length)
            {
                playerChoices[i].transform.parent.gameObject.SetActive(true);
                playerChoices[i].text = VD.nodeData.comments[i];
            }
            else
            {
                playerChoices[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator ShowNPCText()
    {
        if (VD.GetExtraVariables(VD.nodeData.nodeID).ContainsKey("flag"))
            flag.sprite = VD.nodeData.sprite;

        string text = string.Empty;
        npcText.text = text;
        while (text.Length < VD.nodeData.comments[VD.nodeData.commentIndex].Length)
        {
            text += VD.nodeData.comments[VD.nodeData.commentIndex][text.Length];
            npcText.text = text;
            yield return new WaitForSeconds(0.01f);
        }

        //Automatically call next.
        yield return new WaitForSeconds(1f);
        VD.Next();
    }

    void End(VD.NodeData data)
    {
        WipePlayerChoices();
        npcText.text = string.Empty;
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
        VD.OnNodeChange -= NodeChangeAction;
        VD.OnEnd -= End;
        VD.EndDialogue();
    }


}
