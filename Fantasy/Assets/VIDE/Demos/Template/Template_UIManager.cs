/*
 *  This is a template verison of the VIDEUIManager1.cs script. Check that script out and the "Player Interaction" demo for more reference.
 *  This one doesn't include an item popup as that demo was mostly hard coded.
 *  Doesn't include reference to a player script or gameobject. How you handle that is up to you.
 *  Doesn't save dialogue and VA state.
 *  Player choices are not instantiated. You need to set the references manually.
    
 *  You are NOT limited to what this script can do. This script is only for convenience. You are completely free to write your own manager or build from this one.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using VIDE_Data; //<--- Import to use VD class

public class Template_UIManager : MonoBehaviour
{
    #region VARS

    //These are the references to UI components and containers in the scene
    [Header("References")]
    public GameObject dialogueContainer;
    public GameObject NPC_Container;
    public GameObject playerContainer;

    public Text NPC_Text;
    public Text NPC_label;
    public Image NPCSprite;
    public Image playerSprite;
    public Text playerLabel;

    public List<Button> maxPlayerChoices = new List<Button>();

    [Tooltip("Attach an Audio Source and reference it if you want to play audios")]
    public AudioSource audioSource;

    [Header("Options")]
    public KeyCode interactionKey;
    public bool NPC_animateText;
    public bool player_animateText;
    public float NPC_secsPerLetter;
    public float player_secsPerLetter;
    public float choiceInterval;
    [Tooltip("Tick this if using Navigation. Will prevent mixed input.")]
    public bool useNavigation;


    bool dialoguePaused = false; //Custom variable to prevent the manager from calling VD.Next
    bool animatingText = false; //Will help us know when text is currently being animated
    int availableChoices = 0;

    IEnumerator TextAnimator;

    #endregion

    #region MAIN

    void Awake()
    {

        VD.LoadDialogues(); //Load all dialogues to memory so that we dont spend time doing so later
        //An alternative to this can be preloading dialogues from the VIDE_Assign component!
    }

    //Call this to begin the dialogue and advance through it
    public void Interact(VIDE_Assign dialogue)
    {
        //Sometimes, we might want to check the ExtraVariables and VAs before moving forward
        //We might want to modify the dialogue or perhaps go to another node, or dont start the dialogue at all
        //In such cases, the function will return true
        var doNotInteract = PreConditions(dialogue);
        if (doNotInteract) return;

        if (!VD.isActive)
        {
            Begin(dialogue);
        }
        else
        {
            CallNext();
        }
    }

    //This begins the conversation. 
    void Begin(VIDE_Assign dialogue)
    {
        //Let's reset the NPC text variables
        NPC_Text.text = "";
        NPC_label.text = "";
        playerLabel.text = "";

        //Subscribe to events
        VD.OnActionNode += ActionHandler;
        VD.OnNodeChange += UpdateUI;
        VD.OnEnd += EndDialogue;

        VD.BeginDialogue(dialogue); //Begins dialogue, will call the first OnNodeChange

        dialogueContainer.SetActive(true); //Let's make our dialogue container visible
    }
    
    //Calls next node in the dialogue
    public void CallNext()
    {
        //Let's not go forward if text is currently being animated, but let's speed it up.
        if (animatingText) { CutTextAnim(); return; }

        if (!dialoguePaused) //Only if
        {
            VD.Next(); //We call the next node and populate nodeData with new data. Will fire OnNodeChange.
        }
        else
        {
            //Stuff we can do instead if dialogue is paused
        }
    }

    //If not using local input, then the UI buttons are going to call this method when you tap/click them!
    //They will send along the choice index
    public void SelectChoice(int choice)
    {
        VD.nodeData.commentIndex = choice;

        if (Input.GetMouseButtonUp(0))
        {
            Interact(VD.assigned);
        }
    }

    //Input related stuff (scroll through player choices and update highlight)
    void Update()
    {
        //Lets just store the Node Data variable for the sake of fewer words
        var data = VD.nodeData;

        if (VD.isActive) //If there is a dialogue active
        {
            //Scroll through Player dialogue options if dialogue is not paused and we are on a player node
            //For player nodes, NodeData.commentIndex is the index of the picked choice
            if (!data.pausedAction && !animatingText && data.isPlayer && !useNavigation)
            {
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (data.commentIndex < availableChoices - 1)
                        data.commentIndex++;
                }
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (data.commentIndex > 0)
                        data.commentIndex--;
                }
                //Color the Player options. Blue for the selected one
                for (int i = 0; i < maxPlayerChoices.Count; i++)
                {
                    maxPlayerChoices[i].transform.GetChild(0).GetComponent<Text>().color = Color.white;
                    if (i == data.commentIndex) maxPlayerChoices[i].transform.GetChild(0).GetComponent<Text>().color = Color.yellow;
                }
            }

            //Detect interact key
            if (Input.GetKeyDown(interactionKey))
            {
                Interact(VD.assigned);
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (animatingText)
                {
                    Interact(VD.assigned);
                }
                else if (!data.isPlayer)
                {
                    Interact(VD.assigned);
                }
            }
        }
        //Note you could also use Unity's Navi system, in which case you would tick the useNavigation flag.
    }

    //When we call VD.Next, nodeData will change. When it changes, OnNodeChange event will fire
    //We subscribed our UpdateUI method to the event in the Begin method
    //Here's where we update our UI
    void UpdateUI(VD.NodeData data)
    {
        //Reset some variables
        NPC_Text.text = "";
        foreach (Button b in maxPlayerChoices) { b.transform.GetChild(0).GetComponent<Text>().text = ""; b.transform.GetChild(0).GetComponent<Text>().color = Color.white; }
        NPC_Container.SetActive(false);
        playerContainer.SetActive(false);
        playerSprite.sprite = null;
        NPCSprite.sprite = null;

        //Look for dynamic text change in extraData
        PostConditions(data);

        //If this new Node is a Player Node, set the player choices offered by the node
        if (data.isPlayer)
        {
            //Set node sprite if there's any, otherwise try to use default sprite
            if (data.sprite != null)
                playerSprite.sprite = data.sprite;
            else if (VD.assigned.defaultPlayerSprite != null)
                playerSprite.sprite = VD.assigned.defaultPlayerSprite;

            SetChoices(data.comments);

            //If it has a tag, show it, otherwise let's use the alias we set in the VIDE Assign
            if (data.tag.Length > 0)
                playerLabel.text = data.tag;

            //Sets the player container on
            playerContainer.SetActive(true);

        }
        else  //If it's an NPC Node, let's just update NPC's text and sprite
        {
            //Set node sprite if there's any, otherwise try to use default sprite
            if (data.sprite != null)
            {
                //For NPC sprite, we'll first check if there's any "sprite" key
                //Such key is being used to apply the sprite only when at a certain comment index
                //Check CrazyCap dialogue for reference
                if (data.extraVars.ContainsKey("sprite"))
                {
                    if (data.commentIndex == (int)data.extraVars["sprite"])
                        NPCSprite.sprite = data.sprite;
                    else
                        NPCSprite.sprite = VD.assigned.defaultNPCSprite; //If not there yet, set default dialogue sprite
                }
                else //Otherwise use the node sprites
                {
                    NPCSprite.sprite = data.sprite;
                }
            } //or use the default sprite if there isnt a node sprite at all
            else if (VD.assigned.defaultNPCSprite != null)
                NPCSprite.sprite = VD.assigned.defaultNPCSprite;

            if (NPC_animateText)
            {
                //This coroutine animates the NPC text instead of displaying it all at once
                TextAnimator = AnimateNPCText(data.comments[data.commentIndex]);
                StartCoroutine(TextAnimator);
            } else
            {
                NPC_Text.text = data.comments[data.commentIndex];
            }
            
            if (data.audios[data.commentIndex] != null)
            {
                audioSource.clip = data.audios[data.commentIndex];
                audioSource.Play();
            }

            //If it has a tag, show it, otherwise let's use the alias we set in the VIDE Assign
            if (data.tag.Length > 0)
                NPC_label.text = data.tag;
            else
                NPC_label.text = VD.assigned.alias;

            //Sets the NPC container on
            NPC_Container.SetActive(true);
        }
    }

    //This uses the returned string[] from nodeData.comments to create the UIs for each comment
    //It first cleans, then it instantiates new choices
    public void SetChoices(string[] choices)
    {
        availableChoices = 0;

        foreach (UnityEngine.UI.Button choice in maxPlayerChoices)
            choice.gameObject.SetActive(false);

        if (player_animateText)
        {
            //This coroutine animates the Player choices instead of displaying it all at once
            TextAnimator = AnimatePlayerText(choices);
            StartCoroutine(TextAnimator);
        } else
        {
            for (int i = 0; i < choices.Length; i++)
            {
                maxPlayerChoices[i].transform.GetChild(0).GetComponent<Text>().text = choices[i]; //Assumes first child of button gameobject is text gameobject
                maxPlayerChoices[i].gameObject.SetActive(true);
                availableChoices++;
            }
            //Highlight the button. Used by Navi
            if (useNavigation)
                maxPlayerChoices[0].Select();
        }
    }

    //Unsuscribe from everything, disable UI, and end dialogue
    //Called automatically because we subscribed to the OnEnd event
    void EndDialogue(VD.NodeData data)
    {
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);
        VD.EndDialogue();
    }

    //To prevent errors
    void OnDisable()
    {
        EndDialogue(null);
    }

    #endregion

    #region DIALOGUE CONDITIONS 

    //DIALOGUE CONDITIONS --------------------------------------------

    bool PreConditions(VIDE_Assign assigned)
    {
        var data = VD.nodeData;
        if (VD.isActive)
        {
            if (!data.isPlayer)
            {

            }
            else
            {

            }
        } else
        {

        }
        
        return false;
    }

    //Conditions we check after VD.Next was called but before we update the UI
    void PostConditions(VD.NodeData data)
    {
        //Don't conduct extra variable actions if we are waiting on a paused action
        if (data.pausedAction) return;

        ReplaceWord(data);

        if (!data.isPlayer) //For player nodes
        {
            //Checks for extraData that concerns font size (CrazyCap node 2)
            if (data.extraData[data.commentIndex].Contains("fs"))
            {
                int fSize = 14;

                string[] fontSize = data.extraData[data.commentIndex].Split(","[0]);
                int.TryParse(fontSize[1], out fSize);
                NPC_Text.fontSize = fSize;
            }
            else
            {
                NPC_Text.fontSize = 14;
            }
        } else
        {

        }
    }

    void ReplaceWord(VD.NodeData data)
    {
        if (data.comments[data.commentIndex].Contains("[NAME]"))
            data.comments[data.commentIndex] = data.comments[data.commentIndex].Replace("[NAME]", VD.assigned.gameObject.name);

        if (data.comments[data.commentIndex].Contains("[WEAPON]"))
            data.comments[data.commentIndex] = data.comments[data.commentIndex].Replace("[WEAPON]", "sword");
    }

    #endregion

    #region EVENTS AND HANDLERS

    //Called when dialogue sare finished loading
    void OnLoadedAction()
    {
        //Debug.Log("Finished loading all dialogues");
        VD.OnLoaded -= OnLoadedAction;
    }

    //Another way to handle Action Nodes is to listen to the OnActionNode event, which sends the ID of the action node
    void ActionHandler(int actionNodeID)
    {
        //Debug.Log("ACTION TRIGGERED: " + actionNodeID.ToString());
    }

    IEnumerator AnimatePlayerText(string[] choices)
    {
        animatingText = true;

        for (int c = 0; c < choices.Length; c++)
        {
            maxPlayerChoices[c].gameObject.SetActive(true);
            string[] words = choices[c].Split(' ');
            Text txtref = maxPlayerChoices[c].transform.GetChild(0).GetComponent<Text>();
            txtref.text = "";

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (i != words.Length - 1) word += " ";

                string previousText = txtref.text;

                float lastHeight = txtref.preferredHeight;
                txtref.text += word;
                if (txtref.preferredHeight > lastHeight)
                {
                    previousText += System.Environment.NewLine;
                }

                for (int j = 0; j < word.Length; j++)
                {
                    txtref.text = previousText + word.Substring(0, j + 1);
                    yield return new WaitForSeconds(player_secsPerLetter);
                }
            }
            availableChoices++;
            txtref.text = choices[c];
            yield return new WaitForSeconds(choiceInterval);
        }

        //Highlight the button. Used by Navi
        if (useNavigation)
            maxPlayerChoices[0].Select();
        animatingText = false;
    }

    IEnumerator AnimateNPCText(string text)
    {
        animatingText = true;

        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (i != words.Length - 1) word += " ";

            string previousText = NPC_Text.text;

            float lastHeight = NPC_Text.preferredHeight;
            NPC_Text.text += word;
            if (NPC_Text.preferredHeight > lastHeight)
            {
                previousText += System.Environment.NewLine;
            }

            for (int j = 0; j < word.Length; j++)
            {
                NPC_Text.text = previousText + word.Substring(0, j + 1);
                yield return new WaitForSeconds(NPC_secsPerLetter);
            }
        }
        NPC_Text.text = text;
        animatingText = false;
    }

    void CutTextAnim()
    {
        StopCoroutine(TextAnimator);
        if (VD.nodeData.isPlayer)
        {
                availableChoices = 0;
            for (int i = 0; i < VD.nodeData.comments.Length; i++)
            {
                maxPlayerChoices[i].transform.GetChild(0).GetComponent<Text>().text = VD.nodeData.comments[i]; //Assumes first child of button gameobject is text gameobject
                maxPlayerChoices[i].gameObject.SetActive(true);
                availableChoices++;
            }
            if (useNavigation)
                maxPlayerChoices[0].Select();
        }
        else
        {
            NPC_Text.text = VD.nodeData.comments[VD.nodeData.commentIndex]; //Now just copy full text	
        }
        animatingText = false;
    }


    #endregion

    //Utility note: If you're on MonoDevelop. Go to Tools > Options > General and enable code folding.
    //That way you can exapnd and collapse the regions and methods

}
