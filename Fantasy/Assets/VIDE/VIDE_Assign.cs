using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using MiniJSON_VIDE;
using VIDE_Data;

public class VIDE_Assign : MonoBehaviour, ISerializationCallbackReceiver
{
    /*
     * This script component should be attached to every game object you will be interacting with.
     * When interacting with the NPC or object, you will have to call the BeginDialogue() method on
     * the DialogueData component and pass this script.
     * It will safely load the assigned script and keep track of the amount of times you've interacted with it.
     * It will also allow you to set a start point override and to modify the assigned dialogue.
     */

    public List<string> diags = new List<string>();

    public int assignedIndex = -1;
    public int assignedID = 0;
    public string assignedDialogue = "";

    public int interactionCount = 0;
    public string alias = "";

    public int overrideStartNode = -1;

    public Sprite defaultNPCSprite;
    public Sprite defaultPlayerSprite;

    public GameObject targetManager;

    void OnEnable()
    {
        //Sends preloaded data
        if (preload)
        {
            VD.LoadFromVA(this);
        }
    }

    /// <summary>
    /// Returns the name of the currently assigned dialogue.
    /// </summary>
    /// <returns></returns>
    public string GetAssigned()
    {
        return diags[assignedIndex];
    }

    /// <summary>
    /// Assigns a new dialogue to these component.
    /// </summary>
    /// <param name="Dialogue name"></param>
    /// <returns></returns>
    public bool AssignNew(string newFile)
    {
        loadFiles();

        if (!diags.Contains(newFile))
        {
            Debug.LogError("Dialogue not found! Make sure the name is correct and has no extension");
            return false;
        }

        assignedIndex = diags.IndexOf(newFile);
        assignedDialogue = diags[assignedIndex];

        return true;
    }

    private void loadFiles()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        diags = new List<string>();
        assignedIndex = 0;

        if (files.Length < 1) return;

        foreach (TextAsset f in files)
        {
            diags.Add(f.name);
        }

        diags.Sort();

    }

    /// <summary>
    /// Saves the current state of this VA component.
    /// </summary>
    /// <param name="filename">Name to save under.</param>
    public void SaveState(string filename)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("alias", alias);
        dict.Add("ovr", overrideStartNode);
        dict.Add("icount", interactionCount);

        dict.Add("aIndex", assignedIndex);
        dict.Add("aID", assignedID);
        dict.Add("aDialogue", assignedDialogue);

        SerializeHelper.WriteToFile(dict as Dictionary<string, object>, filename + ".json");
    }

    /// <summary>
    /// Loads a state to this VA component.
    /// </summary>
    /// <param name="filename">Name of the state to load.</param>
    public void LoadState(string filename)
    {

        string fileDataPath = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer ? Application.persistentDataPath : Application.dataPath);
        if (!File.Exists(fileDataPath + "/VIDE/saves/VA/" + filename + ".json"))
        {
            Debug.LogWarning("Could not find save state!");
            return;
        }

        Dictionary<string, object> dict = SerializeHelper.ReadState(filename) as Dictionary<string, object>;

        alias = (string)dict["alias"];
        overrideStartNode = ((int)((long)dict["ovr"]));
        interactionCount = ((int)((long)dict["icount"]));

        assignedIndex = ((int)((long)dict["aIndex"]));
        assignedID = ((int)((long)dict["aID"]));
        assignedDialogue = (string)dict["aDialogue"];
    }

    class SerializeHelper
    {
        static string fileDataPath = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer ? Application.persistentDataPath : Application.dataPath);

        public static object ReadState(string filename)
        {
            string jsonString = File.ReadAllText(fileDataPath + "/VIDE/saves/VA/" + filename + ".json");
            return MiniJSON_VIDE.DiagJson.Deserialize(jsonString);
        }

        public static void WriteToFile(object data, string filename)
        {
            if (!Directory.Exists(fileDataPath + "/VIDE"))
                Directory.CreateDirectory(fileDataPath + "/VIDE");
            if (!Directory.Exists(fileDataPath + "/VIDE/saves"))
                Directory.CreateDirectory(fileDataPath + "/VIDE/saves");
            if (!Directory.Exists(fileDataPath + "/VIDE/saves/VA"))
                Directory.CreateDirectory(fileDataPath + "/VIDE/saves/VA");

            string outString = MiniJSON_VIDE.DiagJson.Serialize(data);
            File.WriteAllText(fileDataPath + "/VIDE/saves/VA/" + filename, outString);
        }
    }

    /* PRELOAD DATA */

    public bool preload;
    public bool notuptodate = false;
    public int startp;
    public string loadtag; 

    public List<DialogueNode> playerDiags = new List<DialogueNode>();
    public List<ActionNode> actionNodes = new List<ActionNode>();
    public List<VIDE_Localization.VLanguage> langs = new List<VIDE_Localization.VLanguage>();

    /*
    public class DialogueNode
    {
        public List<Comment> comment;
        public int ID;
        public string playerTag;
        public bool isPlayer = false;

        public Sprite sprite;
        public List<string> vars = new List<string>();
        public List<string> varKeys = new List<string>();

        public DialogueNode()
        {
            comment = new List<Comment>();
        }

        public DialogueNode(int comSize, int id, string tag)
        {
            comment = new List<Comment>();
            ID = id;
            playerTag = tag;
            for (int i = 0; i < comSize; i++)
                comment.Add(new Comment());
        }

        public DialogueNode(DialogueNode n)
        {
            if (n == null) return;
            ID = n.ID;
            playerTag = n.playerTag;
            isPlayer = n.isPlayer;
            sprite = n.sprite;
            vars.AddRange(n.vars);
            varKeys.AddRange(n.varKeys);
            comment = new List<Comment>();
            foreach (Comment c in n.comment)
            {
                comment.Add(c);
            }
        }
    }

    public class Comment
    {
        public string text;
        public string extraData;
        public DialogueNode inputSet;
        public DialogueNode outNode;
        public ActionNode outAction;

        public Sprite sprites;
        public AudioClip audios;
        public bool visible = true;

        public Comment()
        {
            text = "";
            extraData = "";
            outNode = null;
        }
        public Comment(DialogueNode id)
        {
            outNode = null;
            inputSet = id;
            text = "Comment...";
            extraData = "ExtraData...";
        }
        public Comment(Comment c)
        {
            if (c == null) return;
            text = c.text;
            extraData = c.extraData;
            sprites = c.sprites;
            audios = c.audios;
            visible = c.visible;
            inputSet = c.inputSet;
            outNode = c.outNode;
            outAction = c.outAction;
        }
    }

    public class ActionNode
    {
        public bool pauseHere = false;
        public string gameObjectName;
        public string methodName;
        public int paramType;

        public int gotoNode = -1;

        public bool param_bool;
        public string param_string;
        public int param_int;
        public float param_float;

        public int ID;
        public DialogueNode outPlayer;
        public ActionNode outAction;

        public int ovrStartNode = -1;
        public string renameDialogue = string.Empty;

        public ActionNode(int id, string meth, string goMeth, bool pau, bool pb, string ps, int pi, float pf)
        {
            pauseHere = pau;
            methodName = meth;
            gameObjectName = goMeth;

            param_bool = pb;
            param_string = ps;
            param_int = pi;
            param_float = pf;

            outPlayer = null;
            outAction = null;
            ID = id;
        }

        public ActionNode()
        {
            //:D
        }

        public ActionNode(ActionNode a)
        {
            if (a == null) return;
            pauseHere = a.pauseHere;
            gameObjectName = a.gameObjectName;
            methodName = a.methodName;
            paramType = a.paramType;
            gotoNode = a.gotoNode;
            param_bool = a.param_bool;
            param_string = a.param_string;
            param_int = a.param_int;
            param_float = a.param_float;
            ID = a.ID;
            outPlayer = a.outPlayer;
            outAction = a.outAction;
            ovrStartNode = a.ovrStartNode;
            renameDialogue = a.renameDialogue;
        }

    }
    */

    //SERIALIZATION...

    public List<Serialized_playerDiags> S_playerDiags;
    public List<Serialized_actionNodes> S_actionNodes;


    public void OnBeforeSerialize()
    {
        PlayerSerialize();
        ActionSerialize();
    }

    public void OnAfterDeserialize()
    {
        if (S_playerDiags.Count > 0)
            playerDiags = PlayerDeserialize();
        else
            playerDiags = new List<DialogueNode>();

        if (S_actionNodes.Count > 0)
            actionNodes = ActionDeserialize();
        else
            actionNodes = new List<ActionNode>();

        ConnectNodes();
    }

    [Serializable]
    public struct Serialized_actionNodes
    {
        public bool pauseHere;
        public string gameObjectName;
        public string methodName;
        public int paramType;

        public int gotoNode;

        public bool param_bool;
        public string param_string;
        public int param_int;
        public float param_float;

        public int ID;

        public int outPlayerIndex;
        public int outActionIndex;

        public int ovrStartNode;
        public string renameDialogue;
    }
    [Serializable]
    public struct Serialized_playerDiags
    {
        public int commentCount;
        public List<Serialized_comment> s_comment;
        public int ID;
        public string pTag;
        public Sprite sprite;
        public List<string> vars;
        public List<string> varKeys;
        public bool isPlayer;
    }
    [Serializable]
    public struct Serialized_comment
    {
        public string text;
        public string extraData;
        public int inputSetIndex;
        public int outputNodeIndex;
        public int outActionIndex;
        public Sprite sprites;
        public AudioClip audios;
        public bool visible;
    }

    void PlayerSerialize()
    {
        List<Serialized_playerDiags> S_playerDiag = new List<Serialized_playerDiags>();

        //Serialize DialogueNodes
        foreach (var child in playerDiags)
        {
            Serialized_playerDiags np = new Serialized_playerDiags()
            {
                commentCount = child.comment.Count,
                ID = child.ID,
                pTag = child.playerTag,
                sprite = child.sprite,
                vars = child.vars,
                isPlayer = child.isPlayer,
                varKeys = child.varKeys
            };
            //Serialize comments inside this set
            np.s_comment = new List<Serialized_comment>();
            for (int i = 0; i < np.commentCount; i++)
            {
                Serialized_comment sc = new Serialized_comment()
                {
                    text = child.comment[i].text,
                    outActionIndex = actionNodes.IndexOf(child.comment[i].outAction),
                    outputNodeIndex = playerDiags.IndexOf(child.comment[i].outNode),
                    inputSetIndex = playerDiags.IndexOf(child),
                    audios = child.comment[i].audios,
                    sprites = child.comment[i].sprites,
                    visible = child.comment[i].visible,
                    extraData = child.comment[i].extraData
                };
                np.s_comment.Add(sc);
            }

            S_playerDiag.Add(np);
        }

        S_playerDiags = S_playerDiag;
    }

    void ActionSerialize()
    {
        List<Serialized_actionNodes> S_actionNode = new List<Serialized_actionNodes>();
        foreach (var child in actionNodes)
        {
            Serialized_actionNodes np = new Serialized_actionNodes()
            {
                gameObjectName = child.gameObjectName,
                pauseHere = child.pauseHere,
                methodName = child.methodName,
                paramType = child.paramType,
                param_bool = child.param_bool,
                param_float = child.param_float,
                param_int = child.param_int,
                param_string = child.param_string,
                ID = child.ID,
                outPlayerIndex = playerDiags.IndexOf(child.outPlayer),
                outActionIndex = actionNodes.IndexOf(child.outAction),
                ovrStartNode = child.ovrStartNode,
                renameDialogue = child.renameDialogue,
                gotoNode = child.gotoNode
            };
            S_actionNode.Add(np);
        }
        S_actionNodes = S_actionNode;
    }

    List<DialogueNode> PlayerDeserialize()
    {
        List<DialogueNode> temp_playerDiags = new List<DialogueNode>();
        foreach (var child in S_playerDiags)
        {
            temp_playerDiags.Add(new DialogueNode());
            var x = temp_playerDiags[temp_playerDiags.Count - 1];
            x.ID = child.ID;
            x.playerTag = child.pTag;
            x.sprite = child.sprite;
            x.vars = child.vars;
            x.varKeys = child.varKeys;
            x.isPlayer = child.isPlayer;

            for (int i = 0; i < child.commentCount; i++)
            {
                DialogueNode s = temp_playerDiags[temp_playerDiags.Count - 1];
                s.comment.Add(new Comment());
                s.comment[i].text = child.s_comment[i].text;
                s.comment[i].sprites = child.s_comment[i].sprites;
                s.comment[i].audios = child.s_comment[i].audios;
                s.comment[i].extraData = child.s_comment[i].extraData;
                s.comment[i].visible = child.s_comment[i].visible;
            }
        }

        return temp_playerDiags;
    }

    List<ActionNode> ActionDeserialize()
    {
        List<ActionNode> temp_actionNodes = new List<ActionNode>();
        foreach (var child in S_actionNodes)
        {
            temp_actionNodes.Add(new ActionNode());
            var x = temp_actionNodes[temp_actionNodes.Count - 1];
            x.gameObjectName = child.gameObjectName;
            x.methodName = child.methodName;
            x.paramType = child.paramType;
            x.param_string = child.param_string;
            x.param_int = child.param_int;
            x.param_float = child.param_float;
            x.param_bool = child.param_bool;
            x.pauseHere = child.pauseHere;
            x.gotoNode = child.gotoNode;
            x.ID = child.ID;
            x.ovrStartNode = child.ovrStartNode;
            x.renameDialogue = child.renameDialogue;
        }

        return temp_actionNodes;
    }

    //Now we can connect all of the nodes 
    void ConnectNodes()
    {
        for (int i = 0; i < playerDiags.Count; i++) //Connect Player Nodes
        {
            for (int ii = 0; ii < playerDiags[i].comment.Count; ii++)
            {
                playerDiags[i].comment[ii].inputSet = playerDiags[i];

                if (S_playerDiags[i].s_comment[ii].outputNodeIndex >= 0)
                    playerDiags[i].comment[ii].outNode = playerDiags[S_playerDiags[i].s_comment[ii].outputNodeIndex];
                else
                    playerDiags[i].comment[ii].outNode = null;

                if (S_playerDiags[i].s_comment[ii].outActionIndex >= 0)
                    playerDiags[i].comment[ii].outAction = actionNodes[S_playerDiags[i].s_comment[ii].outActionIndex];
                else
                    playerDiags[i].comment[ii].outAction = null;
            }
        }

        for (int i = 0; i < actionNodes.Count; i++) //Connect Action Nodes
        {
            if (S_actionNodes[i].outPlayerIndex >= 0)
                actionNodes[i].outPlayer = playerDiags[S_actionNodes[i].outPlayerIndex];
            else
                actionNodes[i].outPlayer = null;

            if (S_actionNodes[i].outActionIndex >= 0)
                actionNodes[i].outAction = actionNodes[S_actionNodes[i].outActionIndex];
            else
                actionNodes[i].outAction = null;
        }

    }

}


