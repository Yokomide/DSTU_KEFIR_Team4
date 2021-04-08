using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VIDE_EditorDB : MonoBehaviour, ISerializationCallbackReceiver
{

    /*
     * Here is were we store all of the temporal data generated on the VIDE Editor. 
     * When the VIDE Editor saves, it will save all of the data we store here into a json file.
     * Since the VIDE Editor allows the creation of endless structures, this script also handles
     * serialization and deserialization of data in order to avoid object composition cycles and
     * to be able to implement Undo/Redo. 
     */

    public class ActionNode
    {
        public bool editorRefreshed = false;
        public bool pauseHere = false;
        public string gameObjectName = "[No object]";
        public string methodName = "[No method]";
        public int methodIndex;
        public int nameIndex;
        public int paramType;
        public List<string> nameOpts = new List<string>() { "[No object]" };
        public string[] opts = new string[] { "[No method]" };
        public Dictionary<string, string> methods = new Dictionary<string, string>();

        public int gotoNode = -1;

        public bool param_bool;
        public string param_string;
        public int param_int;
        public float param_float;

        public Rect rect;
        public Rect rectRaw;
        public int ID;
        public DialogueNode outPlayer;
        public ActionNode outAction;

        public bool more = false;
        public int ovrStartNode = -1;
        public string renameDialogue = String.Empty;

        public void Clean()
        {
            pauseHere = false;
            gameObjectName = "[NONE]";
            methodName = "[NONE]";
            methodIndex = 0;
            nameIndex = 0;
            ovrStartNode = -1;
            renameDialogue = String.Empty;
            more = false;
            paramType = -1;
        }

        public ActionNode(Rect pos, int id)
        {
            pauseHere = false;
            gameObjectName = "";
            methodName = "";
            ID = id;
            outAction = null;
            outPlayer = null;
            rect = new Rect(pos.x, pos.y + 200, 300, 50);
        }
        public ActionNode(Vector2 pos, int id)
        {
            pauseHere = false;
            gameObjectName = "";
            methodName = "";
            ID = id;
            outAction = null;
            outPlayer = null;
            rect = new Rect(pos.x - 100, pos.y - 75, 300, 50);
        }
        public ActionNode()
        {
            pauseHere = false;
            gameObjectName = "";
            methodName = "";
            outAction = null;
            outPlayer = null;
        }
        public ActionNode(Vector2 rPos, int id, string meth, string goMeth, bool pau, bool pb, string ps, int pi, float pf)
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
            rect = new Rect(rPos.x, rPos.y, 300, 50);
            ID = id;
        }

    }

    public class DialogueNode
    {
        [NonSerialized]
        public List<Comment> comment;
        public Rect rect;
        public Rect rectRaw;
        public int ID;
        public string playerTag = "";

        public bool isPlayer = false;
        public Sprite sprite;

        public bool expand;
        public List<string> vars = new List<string>();
        public List<string> varKeys = new List<string>();
		
		

        public DialogueNode()
        {
            comment = new List<Comment>();
            rect = new Rect(20, 200, 300, 100);
        }

        public DialogueNode(Rect pos, int id)
        {
            comment = new List<Comment>();
            comment.Add(new Comment());
            rect = new Rect(pos.x, pos.y + 200, 300, 100);
            ID = id;
        }

        public DialogueNode(Vector2 pos, int id)
        {
            comment = new List<Comment>();
            comment.Add(new Comment());
            rect = new Rect(pos.x - 150, pos.y - 50, 300, 100);
            ID = id;
        }

        public DialogueNode(Vector2 rectPos, int comSize, int id, string tag, bool endC)
        {
            rect = new Rect(rectPos.x, rectPos.y, 300, 100);
            comment = new List<Comment>();
            ID = id;
            playerTag = tag;
            for (int i = 0; i < comSize; i++)
                comment.Add(new Comment());
        }
    }
	
	public class NodeSelection
    {
        public VIDE_EditorDB.DialogueNode dNode;
        public VIDE_EditorDB.ActionNode aNode;

        public NodeSelection(VIDE_EditorDB.DialogueNode d)
        {
            dNode = d;
        }
        public NodeSelection(VIDE_EditorDB.ActionNode d)
        {
            aNode = d;
        }
		
		   public NodeSelection()
        {
            dNode = null;
            aNode = null;
        }
    }

    public class Comment
    {
        public string text;
        public string extraData = "ExtraData";
        public DialogueNode inputSet;
        public DialogueNode outNode;
        public ActionNode outAction;
        public Rect outRect;

        public bool visible = true;
        public AudioClip audios;
        public Sprite sprites;
        public bool showmore;

        public Comment()
        {
            outNode = null;
            inputSet = null;
            outAction = null;
            text = "Comment...";
            extraData = "ExtraData";

        }
        public Comment(DialogueNode id)
        {
            outNode = null;
            outAction = null;
            inputSet = id;
            text = "Comment...";
            extraData = "ExtraData";
        }
    }
     
    public List<DialogueNode> playerDiags = new List<DialogueNode>();
    public List<ActionNode> actionNodes = new List<ActionNode>();
    public List<NodeSelection> selectedNodes = new List<NodeSelection>();
	
    public void CopyLastActionNode(object copy)
    {
        ActionNode ac = actionNodes[actionNodes.Count - 1];
        ActionNode c = (ActionNode)copy;

        ac.editorRefreshed = c.editorRefreshed;
        ac.pauseHere = c.pauseHere;
        ac.gameObjectName = c.gameObjectName;
        ac.methodName = c.methodName;
        ac.methodIndex = c.methodIndex;
        ac.nameIndex = c.nameIndex;
        ac.paramType = c.paramType;
        ac.nameOpts = c.nameOpts;
        ac.opts = c.opts;
        ac.methods = c.methods;

        ac.gotoNode = c.gotoNode;

        ac.param_bool = c.param_bool;
        ac.param_string = c.param_string;
        ac.param_int = c.param_int;
        ac.param_float = c.param_float;

        ac.outPlayer = c.outPlayer;
        ac.outAction = c.outAction;

        ac.more = c.more;
        ac.ovrStartNode = c.ovrStartNode;
        ac.renameDialogue = c.renameDialogue;
    }

    public void CopyLastDialogueNode(object copy)
    {
        DialogueNode dn = playerDiags[playerDiags.Count - 1];
        DialogueNode c = (DialogueNode)copy;

        dn.playerTag = c.playerTag;

        dn.isPlayer = c.isPlayer;
        dn.sprite = c.sprite;

        dn.expand = c.expand;
        dn.vars.AddRange(c.vars);
        dn.varKeys.AddRange(c.varKeys);

        dn.comment = new List<Comment>();

        for (int i = 0; i < c.comment.Count; i++)
        {
            dn.comment.Add(new Comment());

            dn.comment[i].text = c.comment[i].text;
            dn.comment[i].extraData = c.comment[i].extraData;
            dn.comment[i].inputSet =  dn;
            dn.comment[i].outNode = c.comment[i].outNode;
            dn.comment[i].outAction = c.comment[i].outAction;
            dn.comment[i].outRect = c.comment[i].outRect;
            dn.comment[i].audios = c.comment[i].audios;
            dn.comment[i].sprites = c.comment[i].sprites;
            dn.comment[i].showmore = c.comment[i].showmore;
            dn.comment[i].visible = c.comment[i].visible;
        }

    }

    /* Editor */
    public static string videRoot;
    public int fileIndex = 0;
    public int skinIndex = 0;
    public string loadTag = string.Empty;
    public int startID = 0;
    public int curFocusID = 0;
    public int currentID;
    public bool autosave = true;
    public bool previewPanning;
    public int currentDiag = 0;
    public bool locEdit = false;
    public DialogueNode pNode;
    public int pNodeID;
    public ActionNode aNode;
    public int aNodeID;

    //SERIALIZATION...

    public List<Serialized_playerDiags> S_playerDiags;
    public List<Serialized_actionNodes> S_actionNodes;
    public List<Serialized_selNodes> S_selNodes;


    public void OnBeforeSerialize()
    {
        playerSerialize();
        actionSerialize();
    }

    public void OnAfterDeserialize()
    {
        if (S_playerDiags.Count > 0)
            playerDiags = playerDeserialize();
        else
            playerDiags = new List<DialogueNode>();

        if (S_actionNodes.Count > 0)
            actionNodes = actionDeserialize();
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
        public int nameIndex;
        public int methodIndex;
        public int paramIndex;
        public string[] opts;
        public List<string> nameOpts;

        public bool param_bool;
        public string param_string;
        public int param_int;
        public float param_float;

        public Rect rect;
        public int ID;

        public int outPlayerIndex;
        public int outActionIndex;

        public bool more;
        public int ovrStartNode;
        public string renameDialogue;
    }
    [Serializable]
    public struct Serialized_playerDiags
    {
        public int commentCount;
        public List<Serialized_comment> s_comment;
        public int ID;
        public Rect rect;
        public string pTag;
        public bool expand;
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
        public bool showmore;
        public Rect outRect;
        public Sprite sprites;
        public AudioClip audios;
        public bool visible;
    }
	[Serializable]
    public struct Serialized_selNodes
    {
        public VIDE_EditorDB.DialogueNode dNode;
        public VIDE_EditorDB.ActionNode aNode;
    }


    void playerSerialize()
    {
        List<Serialized_playerDiags> S_playerDiag = new List<Serialized_playerDiags>();

        //Serialize DialogueNodes
        foreach (var child in playerDiags)
        {
            Serialized_playerDiags np = new Serialized_playerDiags()
            {
                commentCount = child.comment.Count,
                ID = child.ID,
                rect = child.rect,
                pTag = child.playerTag,
                expand = child.expand,
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
                    outRect = child.comment[i].outRect,
                    outActionIndex = actionNodes.IndexOf(child.comment[i].outAction),
                    outputNodeIndex = playerDiags.IndexOf(child.comment[i].outNode),
                    inputSetIndex = playerDiags.IndexOf(child),
                    audios = child.comment[i].audios,
                    sprites = child.comment[i].sprites,
                    showmore = child.comment[i].showmore,
                    visible = child.comment[i].visible,
                    extraData = child.comment[i].extraData
                };
                np.s_comment.Add(sc);
            }

            S_playerDiag.Add(np);
        }

        S_playerDiags = S_playerDiag;
    }

    void actionSerialize()
    {
        List<Serialized_actionNodes> S_actionNode = new List<Serialized_actionNodes>();
        foreach (var child in actionNodes)
        {
            Serialized_actionNodes np = new Serialized_actionNodes()
            {
                gameObjectName = child.gameObjectName,
                pauseHere = child.pauseHere,
                methodName = child.methodName,
                methodIndex = child.methodIndex,
                opts = child.opts,
                nameOpts = child.nameOpts,
                paramIndex = child.paramType,
                nameIndex = child.nameIndex,
                param_bool = child.param_bool,
                param_float = child.param_float,
                param_int = child.param_int,
                param_string = child.param_string,
                ID = child.ID,
                rect = child.rect,
                outPlayerIndex = playerDiags.IndexOf(child.outPlayer),
                outActionIndex = actionNodes.IndexOf(child.outAction),
                more = child.more,
                ovrStartNode = child.ovrStartNode,
                renameDialogue = child.renameDialogue
            };
            S_actionNode.Add(np);
        }
        S_actionNodes = S_actionNode;
    }
	
	void selSerialize()
    {
        List<Serialized_selNodes> S_selNode = new List<Serialized_selNodes>();
        foreach (var child in selectedNodes)
        {
            Serialized_selNodes np = new Serialized_selNodes()
            {
                dNode = child.dNode,
                aNode = child.aNode,			
            };
            S_selNode.Add(np);
        }
        S_selNodes = S_selNode;
    }

    List<DialogueNode> playerDeserialize()
    {
        List<DialogueNode> temp_playerDiags = new List<DialogueNode>();
        foreach (var child in S_playerDiags)
        {
            temp_playerDiags.Add(new DialogueNode());
            var x = temp_playerDiags[temp_playerDiags.Count - 1];
            x.ID = child.ID;
            x.rect = child.rect;
            x.playerTag = child.pTag;
            x.sprite = child.sprite;
            x.vars = child.vars;
            x.varKeys = child.varKeys;
            x.expand = child.expand;
            x.isPlayer = child.isPlayer;

            for (int i = 0; i < child.commentCount; i++)
            {
                DialogueNode s = temp_playerDiags[temp_playerDiags.Count - 1];
                s.comment.Add(new Comment());
                s.comment[i].text = child.s_comment[i].text;
                s.comment[i].sprites = child.s_comment[i].sprites;
                s.comment[i].audios = child.s_comment[i].audios;
                s.comment[i].extraData = child.s_comment[i].extraData;
                s.comment[i].outRect = child.s_comment[i].outRect;
                s.comment[i].showmore = child.s_comment[i].showmore;
                s.comment[i].visible = child.s_comment[i].visible;
            }
        }

        return temp_playerDiags;
    }

    List<ActionNode> actionDeserialize()
    {
        List<ActionNode> temp_actionNodes = new List<ActionNode>();
        foreach (var child in S_actionNodes)
        {
            temp_actionNodes.Add(new ActionNode());
            var x = temp_actionNodes[temp_actionNodes.Count - 1];
            x.gameObjectName = child.gameObjectName;
            x.methodName = child.methodName;
            x.methodIndex = child.methodIndex;
            x.opts = child.opts;
            x.nameOpts = child.nameOpts;
            x.paramType = child.paramIndex;
            x.nameIndex = child.nameIndex;
            x.param_string = child.param_string;
            x.param_int = child.param_int;
            x.param_float = child.param_float;
            x.param_bool = child.param_bool;
            x.pauseHere = child.pauseHere;
            x.ID = child.ID;
            x.rect = child.rect;
            x.more = child.more;
            x.ovrStartNode = child.ovrStartNode;
            x.renameDialogue = child.renameDialogue;

        }

        return temp_actionNodes;
    }
	
	List<NodeSelection> selDeserialize()
    {
        List<NodeSelection> temp_selNodes = new List<NodeSelection>();
        foreach (var child in S_selNodes)
        {
            temp_selNodes.Add(new NodeSelection());
            var x = temp_selNodes[temp_selNodes.Count - 1];
			
            x.dNode = child.dNode;
            x.aNode = child.aNode;
        }

        return temp_selNodes;
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
