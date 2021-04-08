/*
 *  This is script is only meant to be demonstrate various ways of handling data to create a Dialogue/UI Manager
 *  VIDE doesn't focus on the actual interface, but rather on the system and the data handling
 *  This script is basically handling the node data from nodeData in its own, customized way
 *  Creating a customized in-game Dialogue/UI manager is up to you
 *  Of course, you can absolutely use this script as a start point by adding, modifying, optimizing, or simplifying it to your needs.
 *  If you are experiencing strange behaviours or have any issues or questions, don't hesitate on contacting me at https://videdialogues.wordpress.com/contact/
 *  Need help programming your own UI Manager? Check out the scripting tutorial: https://videdialogues.wordpress.com/tutorial/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using VIDE_Data; //<--- Import to use easily call VD class

public class VIDEUIManager1 : MonoBehaviour
{

    //This script will handle everything related to dialogue interface
    //It will use the VD class to load dialogues and retrieve node data

    #region VARS

    //These are the references to UI components and containers in the scene
    public GameObject dialogueContainer;
    public GameObject NPC_Container;
    public GameObject playerContainer;
    public GameObject itemPopUp;

    public Text NPC_Text;
    public Text NPC_label;
    public Image NPCSprite;
    public GameObject playerChoicePrefab;
    public Image playerSprite;
    public Text playerLabel;

    bool dialoguePaused = false; //Custom variable to prevent the manager from calling VD.Next
    bool animatingText = false; //Will help us know when text is currently being animated

    //Reference to the player script
    public VIDEDemoPlayer player;

    //We'll be using this to store references of the current player choices
    private List<Text> currentChoices = new List<Text>();

    //With this we can start a coroutine and stop it. Used to animate text
    IEnumerator NPC_TextAnimator;

    #endregion

    #region MAIN

    void Awake()
    {
       // VD.LoadDialogues(); //Load all dialogues to memory so that we dont spend time doing so later
        //An alternative to this can be preloading dialogues from the VIDE_Assign component!

        //Loads the saved state of VIDE_Assigns and dialogues.
        VD.LoadState("VIDEDEMOScene1", true);
    }

    //This begins the dialogue and progresses through it (Called by VIDEDemoPlayer.cs)
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
        } else
        {
            CallNext();
        }      
    }

    //This begins the conversation
    void Begin(VIDE_Assign dialogue)
    {
        //Let's reset the NPC text variables
        NPC_Text.text = "";
        NPC_label.text = "";
        playerLabel.text = "";

        //First step is to call BeginDialogue, passing the required VIDE_Assign component 
        //This will store the first Node data in VD.nodeData
        //But before we do so, let's subscribe to certain events that will allow us to easily
        //Handle the node-changes
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
        } else
        {
            //Disable item popup and disable pause
            if (itemPopUp.activeSelf)
            {
                dialoguePaused = false;
                itemPopUp.SetActive(false);
            }
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
            if (!data.pausedAction && data.isPlayer)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    if (data.commentIndex < currentChoices.Count - 1)
                        data.commentIndex++;
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if (data.commentIndex > 0)
                        data.commentIndex--;
                }

                //Color the Player options. Blue for the selected one
                for (int i = 0; i < currentChoices.Count; i++)
                {
                    currentChoices[i].color = Color.white;
                    if (i == data.commentIndex) currentChoices[i].color = Color.yellow;
                }
            }
        }

        //Note you could also use Unity's Navi system
    }

    //When we call VD.Next, nodeData will change. When it changes, OnNodeChange event will fire
    //We subscribed our UpdateUI method to the event in the Begin method
    //Here's where we update our UI
    void UpdateUI(VD.NodeData data)
    {
        //Reset some variables
        //Destroy the current choices
        foreach (Text op in currentChoices)
            Destroy(op.gameObject);
        currentChoices = new List<UnityEngine.UI.Text>();
        NPC_Text.text = "";
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

            SetOptions(data.comments);

            //If it has a tag, show it, otherwise let's use the alias we set in the VIDE Assign
            if (data.tag.Length > 0)
                playerLabel.text = data.tag;
            else
                playerLabel.text = player.playerName;

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

            //This coroutine animates the NPC text instead of displaying it all at once
            NPC_TextAnimator = DrawText(data.comments[data.commentIndex], 0.02f);
            StartCoroutine(NPC_TextAnimator);

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
    public void SetOptions(string[] choices)
    {
        //Create the choices. The prefab comes from a dummy gameobject in the scene
        //This is a generic way of doing it. You could instead have a fixed number of choices referenced.
        for (int i = 0; i < choices.Length; i++)
        {
            GameObject newOp = Instantiate(playerChoicePrefab.gameObject, playerChoicePrefab.transform.position, Quaternion.identity) as GameObject;
            newOp.transform.SetParent(playerChoicePrefab.transform.parent, true);
            newOp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20 - (20 * i));
            newOp.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            newOp.GetComponent<UnityEngine.UI.Text>().text = choices[i];
            newOp.SetActive(true);

            currentChoices.Add(newOp.GetComponent<UnityEngine.UI.Text>()); 
        }
    }

    //Unsuscribe from everything, disable UI, and end dialogue
    //Called automatically because we subscribed to the OnEnd event
    void EndDialogue(VD.NodeData data)
    {
        CheckTasks();
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        dialogueContainer.SetActive(false);
        VD.EndDialogue();

        VD.SaveState("VIDEDEMOScene1", true); //Saves VIDE stuff related to EVs and override start nodes
        QuestChartDemo.SaveProgress(); //saves OUR custom game data
    }

    void OnDisable()
    {
        //If the script gets destroyed, let's make sure we force-end the dialogue to prevent errors
        //We do not save changes
        CheckTasks();
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        if (dialogueContainer != null)
        dialogueContainer.SetActive(false);
        VD.EndDialogue();
    }

    #endregion

    #region DIALOGUE CONDITIONS 

    //DIALOGUE CONDITIONS --------------------------------------------

    //When this returns true, it means that we did something that alters the progression of the dialogue
    //And we don't want to call Next() this time
    bool PreConditions(VIDE_Assign dialogue)
    {
        var data = VD.nodeData;

        if (VD.isActive) //Stuff we check while the dialogue is active
        {
            //Check for extra variables
            //This one finds a key named "item" which has the value of the item thats gonna be given
            //If there's an 'item' key, then we will assume there's also an 'itemLine' key and use it
            if (!data.isPlayer)
            {
                if (data.extraVars.ContainsKey("item") && !data.dirty)
                {
                    if (data.commentIndex == (int)data.extraVars["itemLine"])
                    {
                        if (data.extraVars.ContainsKey("item++")) //If we have this key, we use it to increment the value of 'item' by 'item++'
                        {
                            Dictionary<string, object> newVars = data.extraVars; //Clone the current extraVars content
                            int newItem = (int)newVars["item"]; //Retrieve the value we want to change
                            newItem += (int)data.extraVars["item++"]; //Change it as we desire
                            newVars["item"] = newItem; //Set it back   
                            VD.SetExtraVariables(25, newVars); //Send newVars through UpdateExtraVariable method
                        }

                        //If it's CrazyCap, check his stock before continuing
                        //If out of stock, change override start node
                        if (VD.assigned.alias == "CrazyCap")
                            if ((int)data.extraVars["item"] + 1 >= player.demo_Items.Count)
                                VD.assigned.overrideStartNode = 28;


                        if (!player.demo_ItemInventory.Contains(player.demo_Items[(int)data.extraVars["item"]]))
                        {
                            GiveItem((int)data.extraVars["item"]);
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (data.extraVars.ContainsKey("outCondition"))
                {
                    if (data.extraVars.ContainsKey("condInfo"))
                    {
                        int[] nodeIDs = VD.ToIntArray((string)data.extraVars["outCondition"]);
                        if (VD.assigned.interactionCount < nodeIDs.Length)
                            VD.SetNode(nodeIDs[VD.assigned.interactionCount]);
                        else
                            VD.SetNode(nodeIDs[nodeIDs.Length - 1]);
                        return true;
                    }
                }

            }
        } else //Stuff we do right before the dialogue begins
        {
            //Get the item from CrazyCap to trigger this one on Charlie
            if (dialogue.alias == "Charlie")
            {
                if (player.demo_ItemInventory.Count > 0 && dialogue.overrideStartNode == -1)
                {
                    dialogue.overrideStartNode = 16;
                    return false;
                }
            }
        }
        return false;
    }

    //Conditions we check after VD.Next was called but before we update the UI
    void PostConditions(VD.NodeData data)
    {
        //Don't conduct extra variable actions if we are waiting on a paused action
        if (data.pausedAction) return;

        if (!data.isPlayer) //For player nodes
        {
            //Replace [WORDS]
            ReplaceWord(data);

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
        }
    }

    //This will replace any "[NAME]" with the name of the gameobject holding the VIDE_Assign
    //Will also replace [WEAPON] with a different variable
    void ReplaceWord(VD.NodeData data)
    {
        if (data.comments[data.commentIndex].Contains("[NAME]"))
            data.comments[data.commentIndex] = data.comments[data.commentIndex].Replace("[NAME]", VD.assigned.gameObject.name);

        if (data.comments[data.commentIndex].Contains("[WEAPON]"))
            data.comments[data.commentIndex] = data.comments[data.commentIndex].Replace("[WEAPON]", player.demo_ItemInventory[0].ToLower());
    }

    #endregion

    #region EVENTS AND HANDLERS

    //Just so we know when we finished loading all dialogues, then we unsubscribe
    void OnLoadedAction()
    {
        Debug.Log("Finished loading all dialogues");
        VD.OnLoaded -= OnLoadedAction;
    }

    //Another way to handle Action Nodes is to listen to the OnActionNode event, which sends the ID of the action node
    void ActionHandler(int actionNodeID)
    {
        //Debug.Log("ACTION TRIGGERED: " + actionNodeID.ToString());
    }

    //Adds item to demo inventory, shows item popup, and pauses dialogue
    void GiveItem(int itemIndex)
    {
        player.demo_ItemInventory.Add(player.demo_Items[itemIndex]);
        itemPopUp.SetActive(true);
        string text = "You've got a <color=yellow>" + player.demo_Items[itemIndex] + "</color>!";
        itemPopUp.transform.GetChild(0).GetComponent<Text>().text = text;
        dialoguePaused = true;
    }

    IEnumerator DrawText(string text, float time)
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
                yield return new WaitForSeconds(time);
            }
        }
        NPC_Text.text = text;
        animatingText = false;
    }

    void CutTextAnim()
    {
        StopCoroutine(NPC_TextAnimator);
        NPC_Text.text = VD.nodeData.comments[VD.nodeData.commentIndex]; //Now just copy full text		
        animatingText = false;
    }

    //Check task progression
    void CheckTasks()
    {
        if (player.demo_ItemInventory.Count == 5)
            QuestChartDemo.SetQuest(2, false);

        QuestChartDemo.CheckTaskCompletion(VD.nodeData);
    }

    #endregion

	//Utility note: If you're on MonoDevelop. Go to Tools > Options > General and enable code folding.
	//That way you can exapnd and collapse the regions and methods

}
