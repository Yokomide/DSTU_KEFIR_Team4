using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace VIDE_Data
{
    /*
    * This class will manage dialogues at runtime.
    * You should never have any need of modifying any variable within this script. 
    * It will manage the flow of the conversation based on the current node data stored in a variable called nodeData.
    * Include the VIDE_Data namespace in your scripts, then enter VD to access the methods, variables, and events.
    * Call BeginDialogue first to initiate a dialogue.
    * The rest is up to the Next() method to advance in the conversation up until you call EndDialogue()
    * Check VIDEUIManager1.cs for an already-setup UI manager example.
    */

    class SerializeHelper
    {
        public static TextAsset[] files;

        //static string fileDataPath = Application.dataPath + "/VIDE/dialogues/";
        public static object ReadFromFile(string filename)
        {
            TextAsset file = null;
            foreach (TextAsset t in files)
            {
                if (t.name == filename)
                {
                    file = t;
                    break;
                }

            }

            if (file != null)
            {
                string jsonString = file.text;
                return MiniJSON_VIDE.DiagJson.Deserialize(jsonString);
            }
            else
            {
                return null;
            }

        }

        static string fileDataPath = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer ? Application.persistentDataPath : Application.dataPath);

        public static object ReadState(string filename)
        {
            string jsonString = File.ReadAllText(fileDataPath + "/VIDE/saves/" + filename + ".json");
            return MiniJSON_VIDE.DiagJson.Deserialize(jsonString);
        }

        public static void WriteToFile(object data, string filename)
        {

            if (!Directory.Exists(fileDataPath + "/VIDE"))
                Directory.CreateDirectory(fileDataPath + "/VIDE");
            if (!Directory.Exists(fileDataPath + "/VIDE/saves"))
                Directory.CreateDirectory(fileDataPath + "/VIDE/saves");

            string outString = MiniJSON_VIDE.DiagJson.Serialize(data);
            File.WriteAllText(fileDataPath + "/VIDE/saves/" + filename, outString);
        }
    }

    public class Diags
    {
        public string name = string.Empty;
        public bool loaded = false;
        public int start = -1;
        public string loadTag;
        public List<DialogueNode> playerNodes = new List<DialogueNode>();
        public List<ActionNode> actionNodes = new List<ActionNode>();
        public VIDE_Assign VA;

        public Diags(string s, string t, VIDE_Assign vd)
        {
            name = s;
            loadTag = t;
            VA = vd;
        }

        public Diags()
        {
            name = "";
            loadTag = "";
            VA = null;
        }
    }


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

    public class VD : MonoBehaviour
    {

        /// <summary>
        /// Saves current VD state, which includes current language and modified EVs and comment visibility for all loaded dialogues. 
        /// </summary>
        /// <param name="filename">Name to save the JSON file under.</param>
        /// <param name="saveAssigned">Optionally, save every VA state found within the current scene.</param>
        public static void SaveState(string filename, bool saveAssigned)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (_currentLanguage != null)
                dict.Add("locCur", currentLanguage);

            for (int i = 0; i < diags.Count; i++)
            {
                if (diags[i].loaded)
                {
                    for (int n = 0; n < diags[i].playerNodes.Count; n++)
                    {
                        for (int k = 0; k < diags[i].playerNodes[n].vars.Count; k++)
                        {
                            dict.Add(diags[i].name + "_node" + n.ToString() + "_var" + k.ToString(), diags[i].playerNodes[n].vars[k]);
                        }
                        for (int c = 0; c < diags[i].playerNodes[n].comment.Count; c++)
                        {
                            dict.Add(diags[i].name + "_node" + n.ToString() + "_com" + c.ToString() + "_vis", diags[i].playerNodes[n].comment[c].visible);
                        }
                    }
                }
            }

            if (saveAssigned)
            {
                VIDE_Assign[] gos = Resources.FindObjectsOfTypeAll<VIDE_Assign>();
                foreach (VIDE_Assign va in gos)
                    va.SaveState(va.gameObject.name + "_state");
            }


            SerializeHelper.WriteToFile(dict as Dictionary<string, object>, filename + ".json");
        }

        /// <summary>
        /// Loads the specified state to VD. 
        /// </summary>
        /// <param name="filename">The filename of the state to load.</param>
        /// <param name="loadAssigned">Optionally, load the state of every VA found in the scene if filename follows the 'gameObjectName_state' naming convention.</param>
        public static void LoadState(string filename, bool loadAssigned)
        {
            string fileDataPath = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer ? Application.persistentDataPath : Application.dataPath);
            if (!File.Exists(fileDataPath + "/VIDE/saves/" + filename + ".json"))
            {
                Debug.LogWarning("Save file '" + filename + "' not found!");
                return;
            }

            if (diags.Count < 1)
                FetchDiags();

            Dictionary<string, object> dict = SerializeHelper.ReadState(filename) as Dictionary<string, object>;

            if (localizationEnabled && dict.ContainsKey("locCur"))
            {
                if (currentLanguage != (string)dict["locCur"])
                    SetCurrentLanguage((string)dict["locCur"]);
            }


            for (int i = 0; i < diags.Count; i++)
            {
                if (diags[i].loaded)
                {
                    for (int n = 0; n < diags[i].playerNodes.Count; n++)
                    {
                        for (int k = 0; k < diags[i].playerNodes[n].vars.Count; k++)
                        {
                            if (dict.ContainsKey(diags[i].name + "_node" + n.ToString() + "_var" + k.ToString()))
                                diags[i].playerNodes[n].vars[k] = (string)dict[diags[i].name + "_node" + n.ToString() + "_var" + k.ToString()];
                        }
                        for (int c = 0; c < diags[i].playerNodes[n].comment.Count; c++)
                        {
                            if (dict.ContainsKey(diags[i].name + "_node" + n.ToString() + "_com" + c.ToString() + "_vis"))
                                diags[i].playerNodes[n].comment[c].visible = (bool)dict[diags[i].name + "_node" + n.ToString() + "_com" + c.ToString() + "_vis"];
                        }
                    }
                }
            }

            if (loadAssigned)
            {
                VIDE_Assign[] gos = Resources.FindObjectsOfTypeAll<VIDE_Assign>();
                foreach (VIDE_Assign va in gos)
                    va.LoadState(va.gameObject.name + "_state");
            }

        }

        public class NodeData
        {
            public bool isPlayer;
            public bool pausedAction;
            public bool isEnd;
            public int nodeID;
            public Sprite sprite;

            public string[] comments;
            public Comment[] creferences;
            public string[] extraData;
            public AudioClip[] audios;
            public Sprite[] sprites;
            public Dictionary<string, object> extraVars;

            public int commentIndex;

            public string tag;
            public bool dirty;

            public NodeData()
            {
                isPlayer = true;
                isEnd = false;
                extraVars = new Dictionary<string, object>();
                nodeID = -1;
                commentIndex = 0;               
            }

            public NodeData(NodeData nd)
            {
                isPlayer = nd.isPlayer;
                pausedAction = nd.pausedAction;
                isEnd = nd.isEnd;
                nodeID = nd.nodeID;
                sprite = nd.sprite;
                comments = nd.comments;
                extraData = nd.extraData;
                audios = nd.audios;
                sprites = nd.sprites;
                extraVars = nd.extraVars;
                commentIndex = nd.commentIndex;
                tag = nd.tag;
                dirty = nd.dirty;
            }

        }

        public int assignedIndex = 0;
        private static DialogueNode currentPlayerStep;
        private static ActionNode currentActionNode;
        private static ActionNode lastActionNode;
        private static int startPoint = -1;
        private static VIDE_Assign _assigned;
        private static bool cancelAction;
        private static bool _localizationEnabled;
        private static VIDE_Localization.VLanguage _defaultLanguage;
        private static VIDE_Localization.VLanguage _currentLanguage;
        private static List<Diags> diags = new List<Diags>();

        public static Sprite[] spriteDatabase;
        public static AudioClip[] audioDatabase;

        public static bool isActive;
        public static NodeData nodeData;
        public static int currentDiag = -1;
        public static VIDE_Assign assigned
        {
            get
            {
                return (VIDE_Assign)_assigned;
            }
        }
        public static List<Diags> saved
        {
            get
            {
                return diags;
            }
        }
        public static int startNode
        {
            get
            {
                return startPoint;
            }
        }

        public static string currentLanguage
        {
            get
            {
                return _currentLanguage.name;
            }
        }

        public static VIDE_Localization.VLanguage GetCurLan
        {
            get
            {
                return _currentLanguage;
            }
        }

        private static VIDE_Localization.VLanguage currentLanguageSET
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                _currentLanguage = value;
            }
        }

        public static string defaultLanguage
        {
            get
            {
                return _defaultLanguage.name;
            }
        }

        public static VIDE_Localization.VLanguage dflset 
        {
            get
            {
                return _defaultLanguage;
            }
            set
            {
                _defaultLanguage = value;

            }
        }

        public static bool localizationEnabled
        {
            get
            {
                return _localizationEnabled;
            }
        }

        public static bool localizationEnabledSET
        {
            get
            {
                return _localizationEnabled;
            }
            set
            {
                _localizationEnabled = value;
            }
        }


        /* Events */

        public delegate void ActionEvent(int nodeID);
        public static event ActionEvent OnActionNode;

        public delegate void LangEvent();
        public static event LangEvent OnLanguageChange;

        public delegate void NodeChange(NodeData data);
        public static event NodeChange OnNodeChange;
        public static event NodeChange OnEnd;

        public delegate void LoadUnload();
        public static event LoadUnload OnLoaded;
        public static event LoadUnload OnUnloaded;

        static int pauseGotoNode = -1;

        public static void LoadLocalized(bool onlyLoadDefault)
        {
            VIDE_Localization.VLanguage cur = _currentLanguage;

            if (onlyLoadDefault)
                cur = _defaultLanguage;

            if (cur != null)
                if (cur.playerDiags != null)
                    for (int i = 0; i < cur.playerDiags.Count; i++)
                    {
                        diags[currentDiag].playerNodes[i].sprite = cur.playerDiags[i].sprite;
                        diags[currentDiag].playerNodes[i].playerTag = cur.playerDiags[i].playerTag;
                        for (int ii = 0; ii < cur.playerDiags[i].comment.Count; ii++)
                        {
                            diags[currentDiag].playerNodes[i].comment[ii].text = cur.playerDiags[i].comment[ii].text;
                            diags[currentDiag].playerNodes[i].comment[ii].audios = cur.playerDiags[i].comment[ii].audios;
                            diags[currentDiag].playerNodes[i].comment[ii].sprites = cur.playerDiags[i].comment[ii].sprites;
                        }
                    }
        }

        public static void FetchDiags()
        {
            if (diags.Count > 0) return;

            VIDE_Localization.LoadSettings();
            _localizationEnabled = VIDE_Localization.enabledInGame;
            _defaultLanguage = VIDE_Localization.defaultLanguage;
            _currentLanguage = _defaultLanguage;

            SerializeHelper.files = Resources.LoadAll<TextAsset>("Dialogues");
            List<string> names = new List<string>();
            for (int i = 0; i < SerializeHelper.files.Length; i++)
            {
                names.Add(SerializeHelper.files[i].name);
            }

            names.Sort();

            for (int i = 0; i < names.Count; i++)
            {
                string ttag = "";
                diags.Add(new Diags(names[i], ttag, null));
            }
        }

        static void DoAction()
        {

            if (OnActionNode != null)
                OnActionNode(currentActionNode.ID);

            //Do predefined actions
            if (currentActionNode.ovrStartNode > -1)
                _assigned.overrideStartNode = currentActionNode.ovrStartNode;
            if (currentActionNode.renameDialogue.Length > 0)
                _assigned.alias = currentActionNode.renameDialogue;

            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == currentActionNode.gameObjectName);

            foreach (GameObject g in objects)
            {
                if (currentActionNode.paramType < 1)
                {
                    g.SendMessage(currentActionNode.methodName, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 1)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_bool, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 2)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_string, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 3)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_int, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 4)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_float, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
            }

            if (currentActionNode.paramType > -1)
                if (cancelAction)
                {
                    cancelAction = false;
                    return;
                }


            if (!currentActionNode.pauseHere)
            {
                if (currentActionNode.gotoNode > -1)
                    SetNode(lastActionNode.gotoNode);
                else
                    Next();
            }
            else
            {
                if (currentActionNode.gotoNode > -1)
                    pauseGotoNode = currentActionNode.gotoNode;
                nodeData.pausedAction = true;
            }

        }

        private static string[] GetOptions(DialogueNode diagNode)
        {
            List<string> op = new List<string>();

            if (diagNode == null)
            {
                return op.ToArray();
            }

            for (int i = 0; i < diagNode.comment.Count; i++)
            {
                op.Add(diagNode.comment[i].text);
            }

            return op.ToArray();
        }

        private static string[] GetExtraData(DialogueNode diagNode)
        {
            List<string> op = new List<string>();

            if (diagNode == null)
            {
                return op.ToArray();
            }

            for (int i = 0; i < diagNode.comment.Count; i++)
            {
                if (diagNode.comment[i].extraData.Length > 0)
                    op.Add(diagNode.comment[i].extraData);
                else
                    op.Add(string.Empty);
            }

            return op.ToArray();
        }

        void addComment(DialogueNode id)
        {
            id.comment.Add(new Comment(id));
        }

        static void addSet(int cSize, int id, string tag)
        {
            diags[currentDiag].playerNodes.Add(new DialogueNode(cSize, id, tag));
        }

        public static void LoadFromVA(VIDE_Assign diagToLoad)
        {
            FetchDiags();


            int theIndex = -1;
            for (int i = 0; i < diags.Count; i++)
            {
                if (diagToLoad.assignedDialogue == diags[i].name)
                    theIndex = i;
            }

            if (theIndex == -1)
            {
                Debug.LogError("'" + diagToLoad.assignedDialogue + "' dialogue assigned to " + diagToLoad.gameObject.name + " not found! Did you delete the dialogue?");
                return;
            }

            if (!diags[theIndex].loaded)
            {
                diags[theIndex].start = diagToLoad.startp;
                diags[theIndex].loadTag = diagToLoad.loadtag;
                diags[theIndex].playerNodes = diagToLoad.playerDiags;
                diags[theIndex].actionNodes = diagToLoad.actionNodes;

                VIDE_Localization.languages = diagToLoad.langs;
                LoadLocalized(!_localizationEnabled);

                diags[theIndex].loaded = true;

                VIDE_Localization.VLanguage cur = _currentLanguage;

                if (cur != null)
                    if (cur.playerDiags != null)
                        for (int i = 0; i < cur.playerDiags.Count; i++)
                        {
                            diags[theIndex].playerNodes[i].sprite = cur.playerDiags[i].sprite;
                            diags[theIndex].playerNodes[i].playerTag = cur.playerDiags[i].playerTag;
                            for (int ii = 0; ii < cur.playerDiags[i].comment.Count; ii++)
                            {
                                diags[theIndex].playerNodes[i].comment[ii].text = cur.playerDiags[i].comment[ii].text;
                                diags[theIndex].playerNodes[i].comment[ii].audios = cur.playerDiags[i].comment[ii].audios;
                                diags[theIndex].playerNodes[i].comment[ii].sprites = cur.playerDiags[i].comment[ii].sprites;
                            }
                        }
            }
        }

        /// <summary>
        /// Internal. Do not use.
        /// </summary>
        /// <returns></returns>
        public static bool Load(string dName)
        {
            if (diags[currentDiag].loaded)
            {
                return false;
            }

            diags[currentDiag] = new Diags(diags[currentDiag].name, diags[currentDiag].loadTag, diags[currentDiag].VA);

            Dictionary<string, object> dict = SerializeHelper.ReadFromFile(dName) as Dictionary<string, object>;

            int pDiags = (int)((long)dict["playerDiags"]);
            int nDiags = 0;
            if (dict.ContainsKey("npcDiags"))
                nDiags = (int)((long)dict["npcDiags"]);

            int aDiags = 0;
            if (dict.ContainsKey("actionNodes")) aDiags = (int)((long)dict["actionNodes"]);

            diags[currentDiag].start = (int)((long)dict["startPoint"]);

            if (dict.ContainsKey("loadTag"))
            {
                diags[currentDiag].loadTag = (string)dict["loadTag"];
            }

            Sprite[] sprites;
            AudioClip[] audios;
            if (spriteDatabase != null && spriteDatabase.Length > 0)
                sprites = spriteDatabase;
            else
                sprites = Resources.LoadAll<Sprite>("");

            if (audioDatabase != null && audioDatabase.Length > 0)
                audios = audioDatabase;
            else
                audios = Resources.LoadAll<AudioClip>("");


            List<string> spriteNames = new List<string>();
            List<string> audioNames = new List<string>();
            foreach (Sprite t in sprites)
                spriteNames.Add(t.name);
            foreach (AudioClip t in audios)
                audioNames.Add(t.name);

            //Create first...
            for (int i = 0; i < pDiags; i++)
            {
                string tagt = "";

                if (dict.ContainsKey("pd_pTag_" + i.ToString()))
                    tagt = (string)dict["pd_pTag_" + i.ToString()];


                addSet(
                    (int)((long)dict["pd_comSize_" + i.ToString()]),
                    (int)((long)dict["pd_ID_" + i.ToString()]),
                    tagt
                    );

                DialogueNode com = diags[currentDiag].playerNodes[diags[currentDiag].playerNodes.Count - 1];

                if (dict.ContainsKey("pd_isp_" + i.ToString()))
                    com.isPlayer = (bool)dict["pd_isp_" + i.ToString()];
                else
                    com.isPlayer = true;

                if (dict.ContainsKey("pd_sprite_" + i.ToString()))
                {
                    string name = Path.GetFileNameWithoutExtension((string)dict["pd_sprite_" + i.ToString()]);
                    if (spriteNames.Contains(name))
                        com.sprite = sprites[spriteNames.IndexOf(name)];
                    else if (name != string.Empty)
                        Debug.LogError("'" + name + "' not found in any Resources folder!");
                }


                if (dict.ContainsKey("pd_vars" + i.ToString()))
                {
                    for (int v = 0; v < (int)(long)dict["pd_vars" + i.ToString()]; v++)
                    {
                        com.vars.Add((string)dict["pd_var_" + i.ToString() + "_" + v.ToString()]);
                        com.varKeys.Add((string)dict["pd_varKey_" + i.ToString() + "_" + v.ToString()]);
                    }
                }
            }

            int npcIndexStart = diags[currentDiag].playerNodes.Count;

            for (int i = 0; i < nDiags; i++)
            {
                string tagt = "";

                if (dict.ContainsKey("nd_tag_" + i.ToString()))
                    tagt = (string)dict["nd_tag_" + i.ToString()];

                diags[currentDiag].playerNodes.Add(new DialogueNode());

                var npc = diags[currentDiag].playerNodes[diags[currentDiag].playerNodes.Count - 1];
                npc.ID = (int)((long)dict["nd_ID_" + i.ToString()]);
                npc.playerTag = tagt;

                npc.isPlayer = false;

                List<string> texts = new List<string>();

                string text = (string)dict["nd_text_" + i.ToString()];
                if (text.Contains("<br>"))
                {
                    string[] splitText = Regex.Split(text, "<br>");
                    texts = new List<string>();
                    foreach (string s in splitText)
                    {
                        texts.Add(s.Trim());
                    }
                }
                else
                {
                    texts.Add(text);
                }

                foreach (string s in texts)
                {
                    npc.comment.Add(new Comment());
                    npc.comment[npc.comment.Count - 1].text = s;
                }

                if (dict.ContainsKey("nd_sprite_" + i.ToString()))
                {
                    string name = Path.GetFileNameWithoutExtension((string)dict["nd_sprite_" + i.ToString()]);

                    if (spriteNames.Contains(name))
                        npc.sprite = sprites[spriteNames.IndexOf(name)];
                    else if (name != string.Empty)
                        Debug.LogError("'" + name + "' not found in any Resources folder!");
                }

                if (dict.ContainsKey("nd_vars" + i.ToString()))
                {
                    for (int v = 0; v < (int)(long)dict["nd_vars" + i.ToString()]; v++)
                    {
                        npc.vars.Add((string)dict["nd_var_" + i.ToString() + "_" + v.ToString()]);
                        npc.varKeys.Add((string)dict["nd_varKey_" + i.ToString() + "_" + v.ToString()]);
                    }
                }
            }


            for (int i = 0; i < aDiags; i++)
            {
                float pFloat;
                var pfl = dict["ac_pFloat_" + i.ToString()];
                if (pfl.GetType() == typeof(System.Double))
                    pFloat = System.Convert.ToSingle(pfl);
                else
                    pFloat = (float)(long)pfl;


                diags[currentDiag].actionNodes.Add(new ActionNode(
                    (int)((long)dict["ac_ID_" + i.ToString()]),
                    (string)dict["ac_meth_" + i.ToString()],
                    (string)dict["ac_goName_" + i.ToString()],
                    (bool)dict["ac_pause_" + i.ToString()],
                    (bool)dict["ac_pBool_" + i.ToString()],
                    (string)dict["ac_pString_" + i.ToString()],
                    (int)((long)dict["ac_pInt_" + i.ToString()]),
                    pFloat
                    ));

                if (dict.ContainsKey("ac_ovrStartNode_" + i.ToString()))
                    diags[currentDiag].actionNodes[diags[currentDiag].actionNodes.Count - 1].ovrStartNode = (int)((long)dict["ac_ovrStartNode_" + i.ToString()]);

                if (dict.ContainsKey("ac_renameDialogue_" + i.ToString()))
                    diags[currentDiag].actionNodes[diags[currentDiag].actionNodes.Count - 1].renameDialogue = (string)dict["ac_renameDialogue_" + i.ToString()];

                if (dict.ContainsKey("ac_goto_" + i.ToString()))
                    diags[currentDiag].actionNodes[diags[currentDiag].actionNodes.Count - 1].gotoNode = (int)((long)dict["ac_goto_" + i.ToString()]);
            }

            //Connect now...
            for (int i = 0; i < diags[currentDiag].playerNodes.Count - nDiags; i++)
            {
                for (int ii = 0; ii < diags[currentDiag].playerNodes[i].comment.Count; ii++)
                {
                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "text"))
                        diags[currentDiag].playerNodes[i].comment[ii].text = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "text"];

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "visible"))
                        diags[currentDiag].playerNodes[i].comment[ii].visible = (bool)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "visible"];

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"))
                    {
                        string name = Path.GetFileNameWithoutExtension((string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"]);

                        if (spriteNames.Contains(name))
                            diags[currentDiag].playerNodes[i].comment[ii].sprites = sprites[spriteNames.IndexOf(name)];
                        else if (name != "")
                            Debug.LogError("'" + name + "' not found in any Resources folder!");
                    }

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"))
                    {
                        string name = Path.GetFileNameWithoutExtension((string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"]);

                        if (audioNames.Contains(name))
                            diags[currentDiag].playerNodes[i].comment[ii].audios = audios[audioNames.IndexOf(name)];
                        else if (name != "")
                            Debug.LogError("'" + name + "' not found in any Resources folder!");
                    }

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "extraD"))
                        diags[currentDiag].playerNodes[i].comment[ii].extraData = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "extraD"];

                    int index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "iSet"]);

                    if (index != -1)
                        diags[currentDiag].playerNodes[i].comment[ii].inputSet = diags[currentDiag].playerNodes[index];

                    index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAns"]);

                    if (index != -1)
                    {
                        if (nDiags > 0)
                        {
                            diags[currentDiag].playerNodes[i].comment[ii].outNode = diags[currentDiag].playerNodes[(diags[currentDiag].playerNodes.Count - nDiags) + index];
                        }
                        else
                        {
                            diags[currentDiag].playerNodes[i].comment[ii].outNode = diags[currentDiag].playerNodes[index];
                        }
                    }


                    index = -1;
                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "oAct"))
                        index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAct"]);

                    if (index != -1)
                        diags[currentDiag].playerNodes[i].comment[ii].outAction = diags[currentDiag].actionNodes[index];
                }
            }

            for (int i = npcIndexStart; i < diags[currentDiag].playerNodes.Count; i++)
            {
                int x = i - npcIndexStart;
                int index = -1;
                if (dict.ContainsKey("nd_oSet_" + x.ToString()))
                    index = (int)((long)dict["nd_oSet_" + x.ToString()]);

                if (index != -1)
                    diags[currentDiag].playerNodes[i].comment[0].outNode = diags[currentDiag].playerNodes[index];

                if (dict.ContainsKey("nd_oAct_" + x.ToString()))
                {
                    index = -1;
                    index = (int)((long)dict["nd_oAct_" + x.ToString()]);
                    if (index != -1)
                        diags[currentDiag].playerNodes[i].comment[0].outAction = diags[currentDiag].actionNodes[index];
                }
            }

            for (int i = 0; i < diags[currentDiag].actionNodes.Count; i++)
            {
                diags[currentDiag].actionNodes[i].paramType = (int)((long)dict["ac_paramT_" + i.ToString()]);

                int index = -1;
                index = (int)((long)dict["ac_oSet_" + i.ToString()]);

                if (index != -1)
                    diags[currentDiag].actionNodes[i].outPlayer = diags[currentDiag].playerNodes[index];

                if (dict.ContainsKey("ac_oAct_" + i.ToString()))
                {
                    index = -1;
                    index = (int)((long)dict["ac_oAct_" + i.ToString()]);
                    if (index != -1)
                        diags[currentDiag].actionNodes[i].outAction = diags[currentDiag].actionNodes[index];
                }
            }

            //Here we load the localized version
            if (_localizationEnabled)
            {
                VIDE_Localization.LoadLanguages(diags[currentDiag].name, false);
                LoadLocalized(false);
            }
            else
            {
                VIDE_Localization.LoadLanguages(diags[currentDiag].name, true);
                LoadLocalized(true);
            }


            diags[currentDiag].loaded = true;
            return true;
        }

        public static Diags PreloadLoad(string dName)
        {
            SerializeHelper.files = Resources.LoadAll<TextAsset>("Dialogues");

            Diags theDiag = new Diags();

            Dictionary<string, object> dict = SerializeHelper.ReadFromFile(dName) as Dictionary<string, object>;

            int pDiags = (int)((long)dict["playerDiags"]);
            int nDiags = 0;
            if (dict.ContainsKey("npcDiags"))
                nDiags = (int)((long)dict["npcDiags"]);

            int aDiags = 0;
            if (dict.ContainsKey("actionNodes")) aDiags = (int)((long)dict["actionNodes"]);

            theDiag.start = (int)((long)dict["startPoint"]);

            if (dict.ContainsKey("loadTag"))
            {
                theDiag.loadTag = (string)dict["loadTag"];
            }

            Sprite[] sprites;
            AudioClip[] audios;
            if (spriteDatabase != null && spriteDatabase.Length > 0)
                sprites = spriteDatabase;
            else
                sprites = Resources.LoadAll<Sprite>("");

            if (audioDatabase != null && audioDatabase.Length > 0)
                audios = audioDatabase;
            else
                audios = Resources.LoadAll<AudioClip>("");


            List<string> spriteNames = new List<string>();
            List<string> audioNames = new List<string>();
            foreach (Sprite t in sprites)
                spriteNames.Add(t.name);
            foreach (AudioClip t in audios)
                audioNames.Add(t.name);

            //Create first...
            for (int i = 0; i < pDiags; i++)
            {
                string tagt = "";

                if (dict.ContainsKey("pd_pTag_" + i.ToString()))
                    tagt = (string)dict["pd_pTag_" + i.ToString()];


                theDiag.playerNodes.Add(new DialogueNode((int)((long)dict["pd_comSize_" + i.ToString()]),
                    (int)((long)dict["pd_ID_" + i.ToString()]),
                    tagt));

                DialogueNode com = theDiag.playerNodes[theDiag.playerNodes.Count - 1];

                if (dict.ContainsKey("pd_isp_" + i.ToString()))
                    com.isPlayer = (bool)dict["pd_isp_" + i.ToString()];
                else
                    com.isPlayer = true;

                if (dict.ContainsKey("pd_sprite_" + i.ToString()))
                {
                    string name = Path.GetFileNameWithoutExtension((string)dict["pd_sprite_" + i.ToString()]);
                    if (spriteNames.Contains(name))
                        com.sprite = sprites[spriteNames.IndexOf(name)];
                    else if (name != string.Empty)
                        Debug.LogError("'" + name + "' not found in any Resources folder!");
                }


                if (dict.ContainsKey("pd_vars" + i.ToString()))
                {
                    for (int v = 0; v < (int)(long)dict["pd_vars" + i.ToString()]; v++)
                    {
                        com.vars.Add((string)dict["pd_var_" + i.ToString() + "_" + v.ToString()]);
                        com.varKeys.Add((string)dict["pd_varKey_" + i.ToString() + "_" + v.ToString()]);
                    }
                }
            }

            int npcIndexStart = theDiag.playerNodes.Count;

            for (int i = 0; i < nDiags; i++)
            {
                string tagt = "";

                if (dict.ContainsKey("nd_tag_" + i.ToString()))
                    tagt = (string)dict["nd_tag_" + i.ToString()];

                theDiag.playerNodes.Add(new DialogueNode());

                var npc = theDiag.playerNodes[theDiag.playerNodes.Count - 1];
                npc.ID = (int)((long)dict["nd_ID_" + i.ToString()]);
                npc.playerTag = tagt;

                npc.isPlayer = false;

                List<string> texts = new List<string>();

                string text = (string)dict["nd_text_" + i.ToString()];
                if (text.Contains("<br>"))
                {
                    string[] splitText = Regex.Split(text, "<br>");
                    texts = new List<string>();
                    foreach (string s in splitText)
                    {
                        texts.Add(s.Trim());
                    }
                }
                else
                {
                    texts.Add(text);
                }

                foreach (string s in texts)
                {
                    npc.comment.Add(new Comment());
                    npc.comment[npc.comment.Count - 1].text = s;
                }

                if (dict.ContainsKey("nd_sprite_" + i.ToString()))
                {
                    string name = Path.GetFileNameWithoutExtension((string)dict["nd_sprite_" + i.ToString()]);

                    if (spriteNames.Contains(name))
                        npc.sprite = sprites[spriteNames.IndexOf(name)];
                    else if (name != string.Empty)
                        Debug.LogError("'" + name + "' not found in any Resources folder!");
                }

                if (dict.ContainsKey("nd_vars" + i.ToString()))
                {
                    for (int v = 0; v < (int)(long)dict["nd_vars" + i.ToString()]; v++)
                    {
                        npc.vars.Add((string)dict["nd_var_" + i.ToString() + "_" + v.ToString()]);
                        npc.varKeys.Add((string)dict["nd_varKey_" + i.ToString() + "_" + v.ToString()]);
                    }
                }
            }


            for (int i = 0; i < aDiags; i++)
            {
                float pFloat;
                var pfl = dict["ac_pFloat_" + i.ToString()];
                if (pfl.GetType() == typeof(System.Double))
                    pFloat = System.Convert.ToSingle(pfl);
                else
                    pFloat = (float)(long)pfl;


                theDiag.actionNodes.Add(new ActionNode(
                    (int)((long)dict["ac_ID_" + i.ToString()]),
                    (string)dict["ac_meth_" + i.ToString()],
                    (string)dict["ac_goName_" + i.ToString()],
                    (bool)dict["ac_pause_" + i.ToString()],
                    (bool)dict["ac_pBool_" + i.ToString()],
                    (string)dict["ac_pString_" + i.ToString()],
                    (int)((long)dict["ac_pInt_" + i.ToString()]),
                    pFloat
                    ));

                if (dict.ContainsKey("ac_ovrStartNode_" + i.ToString()))
                    theDiag.actionNodes[theDiag.actionNodes.Count - 1].ovrStartNode = (int)((long)dict["ac_ovrStartNode_" + i.ToString()]);

                if (dict.ContainsKey("ac_renameDialogue_" + i.ToString()))
                    theDiag.actionNodes[theDiag.actionNodes.Count - 1].renameDialogue = (string)dict["ac_renameDialogue_" + i.ToString()];

                if (dict.ContainsKey("ac_goto_" + i.ToString()))
                    theDiag.actionNodes[theDiag.actionNodes.Count - 1].gotoNode = (int)((long)dict["ac_goto_" + i.ToString()]);
            }

            //Connect now...
            for (int i = 0; i < theDiag.playerNodes.Count - nDiags; i++)
            {
                for (int ii = 0; ii < theDiag.playerNodes[i].comment.Count; ii++)
                {
                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "text"))
                        theDiag.playerNodes[i].comment[ii].text = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "text"];

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "visible"))
                        theDiag.playerNodes[i].comment[ii].visible = (bool)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "visible"];

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"))
                    {
                        string name = Path.GetFileNameWithoutExtension((string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"]);

                        if (spriteNames.Contains(name))
                            theDiag.playerNodes[i].comment[ii].sprites = sprites[spriteNames.IndexOf(name)];
                        else if (name != "")
                            Debug.LogError("'" + name + "' not found in any Resources folder!");
                    }

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"))
                    {
                        string name = Path.GetFileNameWithoutExtension((string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"]);

                        if (audioNames.Contains(name))
                            theDiag.playerNodes[i].comment[ii].audios = audios[audioNames.IndexOf(name)];
                        else if (name != "")
                            Debug.LogError("'" + name + "' not found in any Resources folder!");
                    }

                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "extraD"))
                        theDiag.playerNodes[i].comment[ii].extraData = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "extraD"];

                    int index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "iSet"]);

                    if (index != -1)
                        theDiag.playerNodes[i].comment[ii].inputSet = theDiag.playerNodes[index];

                    index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAns"]);

                    if (index != -1)
                    {
                        if (nDiags > 0)
                        {
                            theDiag.playerNodes[i].comment[ii].outNode = theDiag.playerNodes[(theDiag.playerNodes.Count - nDiags) + index];
                        }
                        else
                        {
                            theDiag.playerNodes[i].comment[ii].outNode = theDiag.playerNodes[index];
                        }
                    }


                    index = -1;
                    if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "oAct"))
                        index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAct"]);

                    if (index != -1)
                        theDiag.playerNodes[i].comment[ii].outAction = theDiag.actionNodes[index];
                }
            }

            for (int i = npcIndexStart; i < theDiag.playerNodes.Count; i++)
            {
                int x = i - npcIndexStart;
                int index = -1;
                if (dict.ContainsKey("nd_oSet_" + x.ToString()))
                    index = (int)((long)dict["nd_oSet_" + x.ToString()]);

                if (index != -1)
                    theDiag.playerNodes[i].comment[0].outNode = theDiag.playerNodes[index];

                if (dict.ContainsKey("nd_oAct_" + x.ToString()))
                {
                    index = -1;
                    index = (int)((long)dict["nd_oAct_" + x.ToString()]);
                    if (index != -1)
                        theDiag.playerNodes[i].comment[0].outAction = theDiag.actionNodes[index];
                }
            }

            for (int i = 0; i < theDiag.actionNodes.Count; i++)
            {
                theDiag.actionNodes[i].paramType = (int)((long)dict["ac_paramT_" + i.ToString()]);

                int index = -1;
                index = (int)((long)dict["ac_oSet_" + i.ToString()]);

                if (index != -1)
                    theDiag.actionNodes[i].outPlayer = theDiag.playerNodes[index];

                if (dict.ContainsKey("ac_oAct_" + i.ToString()))
                {
                    index = -1;
                    index = (int)((long)dict["ac_oAct_" + i.ToString()]);
                    if (index != -1)
                        theDiag.actionNodes[i].outAction = theDiag.actionNodes[index];
                }
            }

            //Here we load the localized version
            /*if (_localizationEnabled)
            {
                VIDE_Localization.LoadLanguages(diags[currentDiag].name, false);
                LoadLocalized(false);
            }
            else
            {
                VIDE_Localization.LoadLanguages(diags[currentDiag].name, true);
                LoadLocalized(true);
            }*/

            theDiag.loaded = true;
            return theDiag;
        }


        public static string GetFirstTag(bool searchPlayer)
        {
            if (!isActive)
            {
                Debug.LogError("No dialogue loaded!");
                return string.Empty;
            }

            string firstTag = string.Empty;
            if (searchPlayer)
            {
                for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                {
                    if (diags[currentDiag].playerNodes[i].isPlayer)
                    {
                        firstTag = diags[currentDiag].playerNodes[i].playerTag;
                        if (!string.IsNullOrEmpty(firstTag))
                            break;
                    }
                    else
                    {
                        firstTag = diags[currentDiag].playerNodes[i].playerTag;
                        if (!string.IsNullOrEmpty(firstTag))
                            break;
                    }
                }
            }

            return firstTag;
        }

        /// <summary>
        /// Convert a string to an int array. This is useful when handling certain EVs.
        /// </summary>
        /// <param name="stringToConvert">The string to convert.</param>
        /// <returns>The int array</returns>
        public static int[] ToIntArray(string stringToConvert)
        {
            List<int> ints = new List<int>();
            char[] delimeters = { ',', '-', '_', ' ' };
            string[] split = stringToConvert.Split(delimeters);

            foreach (string i in split)
            {
                int outInt = -1;
                int.TryParse(i, out outInt);
                ints.Add(outInt);
            }
            return ints.ToArray();
        }

        /// <summary>
        /// Convert a string to a string array. This is useful when handling certain EVs.
        /// </summary>
        /// <param name="stringToConvert">The string to convert.</param>
        /// <returns>The string array</returns>
        public static string[] ToStringArray(string stringToConvert)
        {
            char[] delimeters = { ',', '-', '_', ' ' };
            string[] split = stringToConvert.Split(delimeters);
            return split;
        }

        static AudioClip[] GetPlayerAudios(DialogueNode diagNode)
        {
            List<AudioClip> au = new List<AudioClip>();
            foreach (Comment c in diagNode.comment)
                au.Add(c.audios);

            return au.ToArray();
        }

        static Sprite[] GetPlayerSprites(DialogueNode diagNode)
        {
            List<Sprite> sp = new List<Sprite>();
            foreach (Comment c in diagNode.comment)
                sp.Add(c.sprites);

            return sp.ToArray();
        }

        private static NodeData GetNodeDataForPlayer(DialogueNode diagNode)
        {
            NodeData nd = new NodeData();

            nd.isPlayer = diagNode.isPlayer;
            nd.pausedAction = false;
            nd.isEnd = false;
            nd.nodeID = diagNode.ID;

            nd.comments = GetOptions(diagNode);

            List<Comment> cRefs = new List<Comment>();
            foreach (Comment c in diagNode.comment)
                cRefs.Add(c);
            nd.creferences = cRefs.ToArray();

            nd.extraData = GetExtraData(diagNode);

            nd.commentIndex = 0;

            nd.tag = diagNode.playerTag;
            nd.dirty = false;

            nd.extraVars = GetExtraVars(diagNode.varKeys.ToArray(), diagNode.vars.ToArray());
            nd.sprite = diagNode.sprite;

            nd.audios = GetPlayerAudios(diagNode);
            nd.sprites = GetPlayerSprites(diagNode);
            SetVisibility(nd, diagNode);

            return nd;
        }

        static void SetVisibility(NodeData nd, DialogueNode diagNode)
        {
            List<string> coms = new List<string>();
            List<Comment> cRefs = new List<Comment>();
            List<string> exd = new List<string>();
            List<AudioClip> aud = new List<AudioClip>();
            List<Sprite> spr = new List<Sprite>();

            for (int i = 0; i < diagNode.comment.Count; i++)
            {
                if (diagNode.comment[i].visible)
                {
                    coms.Add(nd.comments[i]);
                    cRefs.Add(nd.creferences[i]);
                    exd.Add(nd.extraData[i]);
                    aud.Add(nd.audios[i]);
                    spr.Add(nd.sprites[i]);
                }
            }

            nd.comments = coms.ToArray();
            nd.creferences = cRefs.ToArray();
            nd.extraData = exd.ToArray();
            nd.audios = aud.ToArray();
            nd.sprites = spr.ToArray();
        }

        static Dictionary<string, object> GetExtraVars(string[] key, string[] val)
        {
            Dictionary<string, object> objs = new Dictionary<string, object>();

            for (int i = 0; i < val.Length; i++)
            {
                string st = val[i].ToLower();
                //Bools
                if (st.Contains("false"))
                {
                    objs.Add(key[i], false);
                    continue;
                }
                if (st.Contains("true"))
                {
                    objs.Add(key[i], true);
                    continue;
                }
                //Int
                int sInt = -1;
                if (System.Int32.TryParse(st, out sInt))
                {
                    objs.Add(key[i], sInt);
                    continue;
                }
                //Float
                float sFloat = -1;
                System.Globalization.NumberStyles style = System.Globalization.NumberStyles.AllowDecimalPoint;
                System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;
                if (System.Single.TryParse(st, style, culture, out sFloat))
                {
                    objs.Add(key[i], sFloat);
                    continue;
                }
                //String1
                objs.Add(key[i], val[i]);
            }

            return objs;
        }

        /// <summary>
        /// Sets the visibility of a node comment. Dialogue doesn't have to be active.
        /// </summary>
        /// <param name="dialogueName">Dialogue name</param>
        /// <param name="nodeID">ID of the node</param>
        /// <param name="commentIndex">Comment index</param>
        /// <param name="visible">Is it visible?</param>
        public static void SetVisible(string dialogueName, int nodeID, int commentIndex, bool visible)
        {
            int diag = -1;
            DialogueNode playerNode = null;
            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (commentIndex >= playerNode.comment.Count || commentIndex < 0)
            {
                Debug.LogError("Comment not found! Check the commentIndex.");
                return;
            }

            playerNode.comment[commentIndex].visible = visible;
        }

        /// <summary>
        /// Sets the visibility of a node comment in the currently active dialogue.
        /// </summary>
        /// <param name="nodeID">ID of the node</param>
        /// <param name="commentIndex">Comment index</param>
        /// <param name="visible">Is it visible?</param>
        public static void SetVisible(int nodeID, int commentIndex, bool visible)
        {
            if (!isActive)
            {
                Debug.LogError("There's no active dialogue!");
            }

            int node = currentPlayerStep.comment.IndexOf(nodeData.creferences[nodeData.commentIndex]);
            int diag = currentDiag;
            DialogueNode playerNode = null;

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (commentIndex >= playerNode.comment.Count || commentIndex < 0)
            {
                Debug.LogError("Comment not found! Check the commentIndex.");
                return;
            }

            playerNode.comment[node].visible = visible;
        }

        /// <summary>
        /// Loads the desired dialogue(s) to memory. Use both fields for a more specific search.
        /// </summary>
        /// <param name='dialogueName'>
        /// The name of the dialogue file. Leave empty to only search under load tag.
        /// </param>
        public static void LoadDialogues(string dialogueName)
        {
            FetchDiags();
            bool didLoad = false;

            foreach (Diags d in diags)
            {
                currentDiag = diags.IndexOf(d);

                if (d.name == dialogueName)
                {
                    Load(d.name);
                    didLoad = true;
                    break;
                }

            }

            if (!didLoad)
                Debug.LogError("Found no dialogue(s) to load!");

            if (OnLoaded != null)
                OnLoaded();

            currentDiag = -1;
        }

        /// <summary>
        /// Loads all of the dialogues to memory.
        /// </summary>
        public static void LoadDialogues()
        {
            FetchDiags();
            foreach (Diags d in diags)
            {
                currentDiag = diags.IndexOf(d);
                Load(d.name);
            }

            if (OnLoaded != null)
                OnLoaded();

            currentDiag = -1;
        }

        /// <summary>
        /// Unloads all of the dialogues from memory.
        /// </summary>
        public static void UnloadDialogues()
        {
            foreach (Diags d in diags)
            {
                d.playerNodes = new List<DialogueNode>();
                d.actionNodes = new List<ActionNode>();
                d.start = -1;
                d.loaded = false;
            }
            if (OnUnloaded != null)
                OnUnloaded();
        }

        /// <summary>
        /// Modify the Extra Variables of a dialogue node.
        /// </summary>
        /// <param name="dialogueName">The dialogue to modify. Use VIDE_Data.assign.assignedDialogue to use currently active dialogue</param>
        /// <param name="nodeID">The node to modify. Make sure it exists.</param>
        /// <param name="newVars">A dictionary with the new content</param>
        public static void SetExtraVariables(string dialogueName, int nodeID, Dictionary<string, object> newVars)
        {
            int diag = -1;
            DialogueNode playerNode = null;
            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (playerNode != null)
            {
                List<string> newStrings = new List<string>();
                foreach (KeyValuePair<string, object> entry in newVars)
                {
                    newStrings.Add(entry.Value.ToString());
                }
                playerNode.vars = newStrings;
            }

            if (isActive && nodeData != null)
            {
                nodeData.dirty = true;
            }

        }

        /// <summary>
        /// Modify the active dialogue's Extra Variables.
        /// </summary>
        /// <param name='nodeID'>
        /// The node to modify. Make sure it exists.
        /// </param>
        /// <param name='newVars'>
        /// A dictionary with the new content
        /// </param>
        public static void SetExtraVariables(int nodeID, Dictionary<string, object> newVars)
        {
            if (!isActive)
            {
                Debug.LogError("There's no active dialogue!");
            }

            int diag = currentDiag;
            DialogueNode playerNode = null;

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }


            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (playerNode != null)
            {
                List<string> newStrings = new List<string>();
                foreach (KeyValuePair<string, object> entry in newVars)
                {
                    newStrings.Add(entry.Value.ToString());
                }
                playerNode.vars = newStrings;
            }

            if (isActive && nodeData != null)
            {
                nodeData.dirty = true;
            }

        }

        /// <summary>
        /// Update a Dialogue node comment.
        /// </summary>
        /// <param name="dialogueName">Name of the dialogue to modify a node from</param>
        /// <param name="nodeID">The ID of the Dialogue node</param>
        /// <param name="commentIndex">The comment index of a Player node</param>
        /// <param name="newComment">The new comment</param>
        public static void SetComment(string dialogueName, int nodeID, int commentIndex, string newComment)
        {
            int diag = -1;

            DialogueNode playerNode = null;

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }
            else
            {
                if (commentIndex > -1 && commentIndex < playerNode.comment.Count)
                {
                    playerNode.comment[commentIndex].text = newComment;
                }
                else
                {
                    Debug.LogError("Index out of range!");
                    return;
                }
            }

        }

        /// <summary>
        /// Get the Extra Variables of a loaded dialogue node.
        /// </summary>
        /// <param name="dialogueName">The loaded dialogue to get Extra Variables from</param>
        /// <param name="nodeID">The node ID</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetExtraVariables(string dialogueName, int nodeID)
        {
            int diag = -1;

            DialogueNode playerNode = null;

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return null;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return null;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return null;
            }

            if (playerNode != null)
            {
                return GetExtraVars(playerNode.varKeys.ToArray(), playerNode.vars.ToArray());
            }

            return null;
        }

        /// <summary>
        /// Get the Extra Variables of an active dialogue node.
        /// </summary>
        /// <param name="nodeID">The node ID</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetExtraVariables(int nodeID)
        {
            if (!isActive)
            {
                Debug.LogError("No dialogue currently active!");
                return null;
            }

            DialogueNode playerNode = null;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                if (diags[currentDiag].playerNodes[i].ID == nodeID) { playerNode = diags[currentDiag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return null;
            }

            if (playerNode != null)
            {
                return GetExtraVars(playerNode.varKeys.ToArray(), playerNode.vars.ToArray());
            }

            return null;
        }

        /// <summary>
        /// Switch the current language. Will automatically reload any loaded dialogues.
        /// </summary>
        /// <param name="lang">The language name. Make sure it exists.</param>
        public static void SetCurrentLanguage(string lang)
        {

            if (!_localizationEnabled)
            {
                Debug.LogError("Localization is disabled! Enable it from the VIDE Editor.");
                return;
            }

            VIDE_Localization.VLanguage vl = null;
            foreach (VIDE_Localization.VLanguage l in VIDE_Localization.languages)
            {
                if (l.name == lang)
                {
                    vl = l;
                }
            }
            if (vl == null)
            {
                Debug.LogError("Could not find '" + lang + "'!");
                return;
            }
            else
            {
                _currentLanguage = vl;
            }

            //Reload loaded dialogues
            int cur = currentDiag;
            //int idx = diags[currentDiag].playerNodes.IndexOf(currentPlayerStep);

            foreach (Diags d in diags)
            {
                if (d.loaded)
                {
                    currentDiag = diags.IndexOf(d);
                    VIDE_Localization.LoadLanguages(diags[currentDiag].name, false);
                    LoadLocalized(false);

                }
            }
            currentDiag = cur;

            /* Update active NodeData */
            int curID;
            if (nodeData != null && currentPlayerStep != null)
            {
                curID = nodeData.nodeID;
                for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                {
                    if (diags[currentDiag].playerNodes[i].ID == curID)
                    {
                        currentPlayerStep = diags[currentDiag].playerNodes[i];
                    }
                }
                nodeData.comments = GetOptions(currentPlayerStep);
                nodeData.tag = currentPlayerStep.playerTag;
                nodeData.sprite = currentPlayerStep.sprite;
                nodeData.audios = GetPlayerAudios(currentPlayerStep);
                nodeData.sprites = GetPlayerSprites(currentPlayerStep);

                List<Comment> cRefs = new List<Comment>();
                foreach (Comment c in currentPlayerStep.comment)
                    cRefs.Add(c);
                nodeData.creferences = cRefs.ToArray();

                if (OnNodeChange != null) OnNodeChange(nodeData);
            }

            if (OnLanguageChange != null) OnLanguageChange();
        }

        /// <summary>
        /// Gets the dialogue's non-component VIDE_Assign.
        /// </summary>
        /// <param name="dialogueName">Name of the dialogue. Make sure it exists.</param>
        /// <returns></returns>
        public static VIDE_Assign GetAssigned(string dialogueName)
        {
            if (diags.Count < 1)
                FetchDiags();

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].VA != null)
                    {
                        return diags[i].VA;
                    }
                    else
                    {
                        Debug.LogError("Variable is Null! Initiate it first by calling SetAssigned()");
                        return null;
                    }
                }

            Debug.LogError("'" + dialogueName + "' not found!");
            return null;
        }

        /// <summary>
        /// Sets the dialogue's non-component VIDE_Assign.
        /// </summary>
        /// <param name="dialogueName">Name of the dialogue. Make sure it exists.</param>
        /// <param name="alias">alias</param>
        /// <param name="ovr">override Start Node. -1 is default.</param>
        /// <param name="playerSprite">Default player Sprite. Null is default.</param>
        /// <param name="npcSprite">Default NPC Sprite. Null is default.</param>
        public static void SetAssigned(string dialogueName, string alias, int ovr, Sprite playerSprite, Sprite npcSprite)
        {
            if (diags.Count < 1)
                FetchDiags();

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].VA == null)
                    {
                        GameObject go = new GameObject();
                        go.name = "temp_" + diags[i].name;
                        go.hideFlags = HideFlags.HideInHierarchy;
                        diags[i].VA = go.AddComponent<VIDE_Assign>();
                        diags[i].VA.assignedDialogue = diags[i].name;
                        diags[i].VA.assignedIndex = diags.IndexOf(diags[i]);
                    }
                    diags[i].VA.alias = alias;
                    diags[i].VA.overrideStartNode = ovr;
                    diags[i].VA.defaultNPCSprite = npcSprite;
                    diags[i].VA.defaultPlayerSprite = playerSprite;
                    return;
                }

            Debug.LogError("'" + dialogueName + "' not found!");
            return;
        }

        /// <summary>
        /// Returns the list of created dialogues.
        /// </summary>
        /// <returns>
        /// The NodeData package.
        /// </returns>
        public static string[] GetDialogues()
        {
            TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
            List<string> names = new List<string>();
            for (int i = 0; i < files.Length; i++)
                names.Add(files[i].name);
            names.Sort();

            return names.ToArray();
        }

        /// <summary>
        /// Get a list of the available languages created. 
        /// Add them from the Localization menu inside the VIDE Editor.
        /// </summary>
        /// <returns></returns>
        public static string[] GetLanguages()
        {
            List<string> langs = new List<string>();
            foreach (VIDE_Localization.VLanguage l in VIDE_Localization.languages)
            {
                langs.Add(l.name);
            }

            return langs.ToArray();
        }

        /// <summary>
        /// Returns the NodeData package of the specified Dialogue Node. Dialogue doesn't have to be active.
        /// </summary>
        /// <returns>
        /// The NodeData package.
        /// </returns>
        /// <param name='dialogueName'>
        /// The name of the dialogue to search. You can use GetDialogues() to get a list.
        /// </param>
        /// <param name='id'>
        /// The ID of the Dialogue Node. Get it from the VIDE Editor.
        /// </param>
        /// <param name='forceLoad'>
        /// Will load the dialogue to memory if its not loaded. 
        /// </param>
        public static NodeData GetNodeData(string dialogueName, int id, bool forceLoad)
        {
            int dIndex = -1;

            if (forceLoad)
                FetchDiags();

            foreach (Diags d in diags)
            {
                if (d.name == dialogueName)
                {
                    if (!d.loaded)
                    {
                        if (forceLoad)
                        {
                            dIndex = diags.IndexOf(d);
                            currentDiag = diags.IndexOf(d);
                            Load(d.name);
                            if (OnLoaded != null)
                                OnLoaded();
                        }
                        else
                        {
                            Debug.LogError("'" + dialogueName + "' not loaded!");
                            return null;
                        }
                    }
                    else
                    {
                        dIndex = diags.IndexOf(d);
                    }
                    break;
                }
            }

            currentDiag = -1;
            DialogueNode realNode = currentPlayerStep;

            if (dIndex == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found! Is it loaded?");
                return null;
            }

            //Look for Node with given ID
            bool foundID = false;

            for (int i = 0; i < diags[dIndex].playerNodes.Count; i++)
            {
                if (diags[dIndex].playerNodes[i].ID == id)
                {
                    currentPlayerStep = diags[dIndex].playerNodes[i];
                    foundID = true;
                }
            }

            if (!foundID)
            {
                Debug.LogError("Could not find a Node with ID " + id.ToString());
                return null;
            }

            /* Action end */

            NodeData nd = GetNodeDataForPlayer(currentPlayerStep);
            currentPlayerStep = realNode;
            return nd;
        }

        /// <summary>
        /// Returns the NodeData package of the specified active Dialogue Node. The current progression of the dialogue is unaffected by this call.
        /// nodeData is also unaffected.
        /// </summary>
        /// <returns>
        /// The NodeData package.
        /// </returns>
        /// <param name='id'>
        /// The ID of the Dialogue Node. Get it from the VIDE Editor.
        /// </param>
        public static NodeData GetNodeData(int id)
        {
            if (!isActive)
            {
                Debug.LogError("You must call the 'BeginDialogue()' method before calling this method!");
                return null;
            }

            //Look for Node with given ID
            bool foundID = false;
            DialogueNode realNode = currentPlayerStep;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
            {
                if (diags[currentDiag].playerNodes[i].ID == id)
                {
                    currentPlayerStep = diags[currentDiag].playerNodes[i];
                    foundID = true;
                }
            }

            if (!foundID)
            {
                Debug.LogError("Could not find a Node with ID " + id.ToString());
                return null;
            }

            /* Action end */

            NodeData nd = GetNodeDataForPlayer(currentPlayerStep);
            currentPlayerStep = realNode;
            return nd;

        }

        /// <summary>
        /// Returns the total number of nodes of the active dialogue.
        /// </summary>
        /// <returns>
        /// Number of nodes.
        /// </returns>
        /// <param name='includeActionNodes'>
        /// Also count action nodes?
        /// </param>
        public static int GetNodeCount(bool includeActionNodes)
        {
            if (!isActive)
            {
                Debug.LogError("You must call the 'BeginDialogue()' method before calling this method!");
                return -1;
            }

            /* Action end */

            int count = diags[currentDiag].playerNodes.Count;
            if (includeActionNodes) count += diags[currentDiag].actionNodes.Count;

            return count;

        }

        /// <summary>
        /// Ignores current state of nodeData and jumps directly to specified node. 
        /// </summary>
        /// <returns>
        /// The NodeData package.
        /// </returns>
        /// <param name='id'>
        /// The ID of the Node. Get it from the VIDE Editor.
        /// </param>
        public static NodeData SetNode(int id)
        {
            if (!isActive)
            {
                Debug.LogError("You must call the 'BeginDialogue()' method before calling this method!");
                return null;
            }

            pauseGotoNode = -1;
            currentActionNode = null;
            lastActionNode = null;

            //Look for Node with given ID
            bool foundID = false;
            bool isAct = false;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
            {
                if (diags[currentDiag].playerNodes[i].ID == id)
                {
                    currentPlayerStep = diags[currentDiag].playerNodes[i];
                    foundID = true;
                }
            }
            if (!foundID)
            {
                for (int i = 0; i < diags[currentDiag].actionNodes.Count; i++)
                {
                    if (diags[currentDiag].actionNodes[i].ID == id)
                    {
                        currentActionNode = diags[currentDiag].actionNodes[i];
                        foundID = true;
                        isAct = true;
                    }
                }
            }
            if (!foundID)
            {
                Debug.LogError("Could not find a Node with ID " + id.ToString());
                return null;
            }

            cancelAction = true;

            /* Action node */

            if (isAct)
            {
                lastActionNode = currentActionNode;
                nodeData = new NodeData();
                DoAction();
                return nodeData;
            }

            /* Action end */

            nodeData = GetNodeDataForPlayer(currentPlayerStep);
            if (OnNodeChange != null) OnNodeChange(nodeData);
            return nodeData;

        }

        /// <summary>
        /// Populates nodeData with the data from next Node based on the current nodeData.
        /// </summary>
        /// <returns></returns>
        public static NodeData Next()
        {
            cancelAction = false;

            if (!isActive)
            {
                Debug.LogError("You must call the 'BeginDialogue()' method before calling the 'Next()' method!");
                return null;
            }

            if (pauseGotoNode > -1)
            {
                SetNode(pauseGotoNode);
                return null;
            }

            int option = 0;

            if (currentPlayerStep != null)
            {
                option = currentPlayerStep.comment.IndexOf(nodeData.creferences[nodeData.commentIndex]);
            }

            if (currentPlayerStep != null)
            {
                if (currentPlayerStep.isPlayer)
                {
                    if (currentPlayerStep.comment[option].outNode == null && currentPlayerStep.comment[option].outAction == null)
                    {
                        nodeData.isEnd = true;
                        if (OnEnd != null) OnEnd(nodeData);
                        return nodeData;
                    }
                }
                else
                {
                    if (currentPlayerStep.comment[0].outNode == null && currentPlayerStep.comment[0].outAction == null && nodeData.commentIndex == nodeData.comments.Length - 1)
                    {
                        nodeData.isEnd = true;
                        if (OnEnd != null) OnEnd(nodeData);
                        return nodeData;
                    }
                }

            }


            //If action node is connected to nothing, then it's the end
            if (lastActionNode != null)
            {
                if (lastActionNode.outPlayer == null && lastActionNode.outAction == null)
                {
                    if (lastActionNode.gotoNode == -1)
                    {
                        nodeData.isEnd = true;
                        if (OnEnd != null) OnEnd(nodeData);
                        return nodeData;
                    }

                }
            }

            /* Action Node? */

            if (currentActionNode == null)
            {
                if (currentPlayerStep.isPlayer)
                {
                    currentActionNode = currentPlayerStep.comment[option].outAction;
                }
                else
                {
                    if (nodeData.commentIndex == nodeData.comments.Length - 1)
                        currentActionNode = currentPlayerStep.comment[0].outAction;
                }
            }
            else
            {
                currentActionNode = currentActionNode.outAction;
            }

            //If we found action node, let's go to it.
            if (currentActionNode != null)
            {
                lastActionNode = currentActionNode;
                DoAction();
                return nodeData;
            }

            /* END Action Node */

            if (!nodeData.isPlayer)
            {
                if (nodeData.comments.Length > 0)
                {
                    if (nodeData.commentIndex != nodeData.comments.Length - 1)
                    {
                        nodeData.commentIndex++;
                        lastActionNode = null;
                        if (OnNodeChange != null) OnNodeChange(nodeData);
                        return nodeData;
                    }
                }

                if (lastActionNode != null)
                    if (lastActionNode.outPlayer != null)
                    {
                        currentPlayerStep = lastActionNode.outPlayer;
                        lastActionNode = null;
                        nodeData = GetNodeDataForPlayer(currentPlayerStep);
                        if (OnNodeChange != null) OnNodeChange(nodeData);
                        return nodeData;
                    }

                lastActionNode = null;
                currentPlayerStep = currentPlayerStep.comment[0].outNode;
                nodeData = GetNodeDataForPlayer(currentPlayerStep);
                if (OnNodeChange != null) OnNodeChange(nodeData);


                return nodeData;
            }
            else
            {
                if (lastActionNode == null)
                {
                    currentPlayerStep = currentPlayerStep.comment[option].outNode;
                }
                else
                {
                    if (lastActionNode.outPlayer != null)
                    {
                        currentPlayerStep = lastActionNode.outPlayer;
                        lastActionNode = null;
                        nodeData = GetNodeDataForPlayer(currentPlayerStep);
                        if (OnNodeChange != null) OnNodeChange(nodeData);
                        return nodeData;
                    }
                }

                lastActionNode = null;
                nodeData = GetNodeDataForPlayer(currentPlayerStep);
                if (OnNodeChange != null) OnNodeChange(nodeData);
                return nodeData;
            }
        }

        /// <summary>
        /// Simulates Next() method. Returns next node's NodeData package based on current nodeData. Will not fire any events. Will not modify current nodeData. Will return current nodeData if next isEnd or an ActionNode.  
        /// </summary>
        /// <param name="returnNextComment">Should we follow the current comment array?</param>
        /// <param name="callAction">If next is an ActionNode, should the actions be called?</param>
        /// <returns>NodeData package</returns>
        public static NodeData GetNext(bool returnNextComment, bool callAction)
        {
            if (!isActive)
            {
                Debug.LogError("A dialogue must be active!");
                return null;
            }

            DialogueNode currentPlayerStepB = new DialogueNode(currentPlayerStep);
            ActionNode currentActionNodeB = new ActionNode(currentActionNode);
            ActionNode lastActionNodeB = new ActionNode(lastActionNode);

            if (currentPlayerStep == null) currentPlayerStepB = null;
            if (currentActionNode == null) currentActionNodeB = null;
            if (lastActionNode == null) lastActionNodeB = null;

            int option = 0;

            if (nodeData != null)
                option = nodeData.commentIndex;

            if (currentPlayerStepB != null)
            {
                if (currentPlayerStepB.isPlayer)
                {
                    if (currentPlayerStepB.comment[option].outNode == null && currentPlayerStepB.comment[option].outAction == null)
                    {
                        NodeData nd = new NodeData(nodeData);
                        nd.isEnd = true;
                        return nd;
                    }
                }
                else
                {
                    if (returnNextComment)
                    {
                        if (currentPlayerStepB.comment[0].outNode == null && currentPlayerStepB.comment[0].outAction == null && nodeData.commentIndex == nodeData.comments.Length - 1)
                        {
                            NodeData nd = new NodeData(nodeData);
                            nd.isEnd = true;
                            return nd;
                        }
                    }
                    else
                    {
                        if ((currentPlayerStepB.comment[0].outNode == null && currentPlayerStepB.comment[0].outAction == null))
                        {
                            NodeData nd = new NodeData(nodeData);
                            nd.isEnd = true;
                            return nd;
                        }
                    }

                }
            }


            //If action node is connected to nothing, then it's the end
            if (lastActionNodeB != null)
            {
                if (lastActionNodeB.outPlayer == null && lastActionNodeB.outAction == null)
                {
                    if (lastActionNodeB.gotoNode == -1)
                    {
                        NodeData nd = new NodeData(nodeData);
                        nd.isEnd = true;
                        return nd;
                    }

                }
            }

            /* Action Node? */

            if (currentActionNodeB == null)
            {
                if (currentPlayerStepB.isPlayer)
                {
                    currentActionNodeB = currentPlayerStepB.comment[option].outAction;
                }
                else
                {
                    if (nodeData.commentIndex == nodeData.comments.Length - 1)
                        currentActionNodeB = currentPlayerStepB.comment[0].outAction;
                }
            }
            else
            {
                currentActionNodeB = currentActionNodeB.outAction;
            }

            //If we found action node, let's go to it.
            if (currentActionNodeB != null)
            {
                lastActionNodeB = currentActionNodeB;
                if (callAction) DoAction();
                return nodeData;
            }

            /* END Action Node */

            if (!nodeData.isPlayer)
            {
                if (returnNextComment)
                {
                    if (nodeData.comments.Length > 0)
                    {
                        if (nodeData.commentIndex != nodeData.comments.Length - 1)
                        {
                            NodeData nd = new NodeData(nodeData);
                            nd.commentIndex++;
                            return nd;
                        }
                    }
                }


                if (lastActionNodeB != null)
                    if (lastActionNodeB.outPlayer != null)
                    {
                        currentPlayerStepB = lastActionNodeB.outPlayer;
                        lastActionNodeB = null;
                        NodeData nd = GetNodeDataForPlayer(currentPlayerStepB);
                        return nd;
                    }

                lastActionNodeB = null;
                currentPlayerStepB = currentPlayerStepB.comment[0].outNode;
                NodeData nd2 = GetNodeDataForPlayer(currentPlayerStepB);
                return nd2;
            }
            else
            {
                if (lastActionNodeB == null)
                {
                    currentPlayerStepB = currentPlayerStepB.comment[option].outNode;
                }
                else
                {
                    if (lastActionNodeB.outPlayer != null)
                    {
                        currentPlayerStepB = lastActionNodeB.outPlayer;
                        lastActionNodeB = null;
                        NodeData nd = GetNodeDataForPlayer(currentPlayerStepB);
                        return nd;
                    }
                }

                lastActionNodeB = null;
                NodeData nd2 = GetNodeDataForPlayer(currentPlayerStepB);
                return nd2;
            }
        }

        /// <summary>
        /// Activates the dialogue _assigned to VIDE_Assign. Populates the nodeData variable with the first Node based on the Start Node. Also returns the current NodeData package.
        /// </summary>
        /// <param name="diagToLoad"></param>
        /// <returns>NodeData</returns>
        public static NodeData BeginDialogue(VIDE_Assign diagToLoad)
        {
            if (diags.Count < 1)
                FetchDiags();

            if (diagToLoad.assignedIndex < 0 || diagToLoad.assignedIndex > diagToLoad.diags.Count - 1)
            {
                Debug.LogError("No dialogue assigned to VIDE_Assign!");
                return null;
            }

            int theIndex = -1;
            for (int i = 0; i < diags.Count; i++)
            {
                if (diagToLoad.assignedDialogue == diags[i].name)
                    theIndex = i;
            }

            if (theIndex == -1)
            {
                Debug.LogError("'" + diagToLoad.assignedDialogue + "' dialogue assigned to " + diagToLoad.gameObject.name + " not found! Did you delete the dialogue?");
                return null;
            }

            currentDiag = theIndex; //assign current dialogue index
            _assigned = diagToLoad;

            //Check if the dialogue is already loaded
            if (!diags[currentDiag].loaded)
            {
                //Let's load the dialogue 
                if (Load(diagToLoad.assignedDialogue))
                {
                    isActive = true;
                }
                else
                {
                    isActive = false;
                    currentDiag = -1;
                    Debug.LogError("Failed to load '" + diagToLoad.diags[diagToLoad.assignedIndex] + "'");
                    return null;
                }
            }
            else
            {
                isActive = true;
            }

            //Make sure that variables were correctly reset after last conversation
            if (nodeData != null)
            {
                Debug.LogError("You forgot to call 'EndDialogue()' on last conversation!");
                return null;
            }

            startPoint = diags[currentDiag].start;

            if (_assigned.overrideStartNode != -1)
                startPoint = _assigned.overrideStartNode;

            int startIndex = -1;
            bool isAct = false;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                if (startPoint == diags[currentDiag].playerNodes[i].ID) { startIndex = i; break; }

            for (int i = 0; i < diags[currentDiag].actionNodes.Count; i++)
                if (startPoint == diags[currentDiag].actionNodes[i].ID)
                {
                    startIndex = i;
                    currentActionNode = diags[currentDiag].actionNodes[i]; isAct = true; break;
                }

            /* Action node */

            if (isAct)
            {
                lastActionNode = currentActionNode;
                nodeData = new NodeData();
                DoAction();
                return nodeData;
            }

            /* Action end */

            if (startIndex == -1)
            {
                Debug.LogError("Start point not found! Check your IDs!");
                return null;
            }

            currentPlayerStep = diags[currentDiag].playerNodes[startIndex];
            lastActionNode = null;
            nodeData = GetNodeDataForPlayer(currentPlayerStep);
            if (OnNodeChange != null) OnNodeChange(nodeData);
            return nodeData;

        }

        /// <summary>
        /// Activates the dialogue just sent. Populates the nodeData variable with the first Node based on the Start Node. Also returns the current NodeData package.
        /// VIDE_Assign variables will only be available as long as the dialogue is loaded. 
        /// </summary>
        /// <param name="diagToLoad"></param>
        /// <returns>NodeData</returns>
        public static NodeData BeginDialogue(string diagName)
        {
            if (diags.Count < 1)
                FetchDiags();

            int theIndex = -1;
            for (int i = 0; i < diags.Count; i++)
            {
                if (diagName == diags[i].name)
                    theIndex = i;
            }

            if (theIndex == -1)
            {
                Debug.LogError("'" + diagName + " not found!");
                return null;
            }

            currentDiag = theIndex; //assign current dialogue index

            //Check if the dialogue is already loaded
            if (!diags[currentDiag].loaded)
            {
                //Let's load the dialogue 
                if (Load(diagName))
                {
                    isActive = true;
                }
                else
                {
                    isActive = false;
                    currentDiag = -1;
                    Debug.LogError("Failed to load '" + diagName + "'");
                    return null;
                }
            }
            else
            {
                isActive = true;
            }

            //Because we have no VIDE_Assign and we need one for the user
            //We'll create a temporal object with one
            //The data will prevail until the dialogue is unloaded
            if (diags[currentDiag].VA == null)
            {
                GameObject go = new GameObject();
                go.name = "temp_" + diags[currentDiag].name;
                go.hideFlags = HideFlags.HideInHierarchy;
                diags[currentDiag].VA = go.AddComponent<VIDE_Assign>();
                diags[currentDiag].VA.assignedDialogue = diags[currentDiag].name;
                diags[currentDiag].VA.assignedIndex = diags.IndexOf(diags[currentDiag]);
            }

            //Make sure that variables were correctly reset after last conversation
            if (nodeData != null)
            {
                Debug.LogError("You forgot to call 'EndDialogue()' on last conversation!");
                return null;
            }

            _assigned = diags[currentDiag].VA;
            startPoint = diags[currentDiag].start;

            if (_assigned.overrideStartNode != -1)
                startPoint = _assigned.overrideStartNode;

            int startIndex = -1;
            bool isAct = false;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                if (startPoint == diags[currentDiag].playerNodes[i].ID) { startIndex = i; break; }

            for (int i = 0; i < diags[currentDiag].actionNodes.Count; i++)
                if (startPoint == diags[currentDiag].actionNodes[i].ID)
                {
                    startIndex = i;
                    currentActionNode = diags[currentDiag].actionNodes[i]; isAct = true; break;
                }

            /* Action node */

            if (isAct)
            {
                lastActionNode = currentActionNode;
                nodeData = new NodeData();
                DoAction();
                return nodeData;
            }

            /* Action end */

            if (startIndex == -1)
            {
                Debug.LogError("Start point not found! Check your IDs!");
                return null;
            }

            currentPlayerStep = diags[currentDiag].playerNodes[startIndex];

            lastActionNode = null;
            nodeData = GetNodeDataForPlayer(currentPlayerStep);
            if (OnNodeChange != null) OnNodeChange(nodeData);
            return nodeData;

        }

        /// <summary>
        /// Wipes out all data and unloads the current VIDE_Assign, raising its interactionCount.
        /// </summary>
        public static void EndDialogue()
        {
            nodeData = null;
            if (_assigned != null)
                _assigned.interactionCount++;
            _assigned = null;
            startPoint = -1;
            isActive = false;
            currentDiag = -1;
            currentPlayerStep = null;
            currentActionNode = null;
            lastActionNode = null;
            pauseGotoNode = -1;
            cancelAction = false;

        }

        //Get Extra Variables
        #region
        // INT
        public static int GetInt(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(int))
                    {
                        return (int)dic[key];
                    }
                }
            }
            return -1;
        }
        public static int GetInt(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(VD.currentPlayerStep.varKeys.ToArray(), VD.currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(int))
                    {
                        return (int)dic[key];
                    }
                }
            }
            return -1;
        }
        // FLOAT
        public static float GetFloat(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(float))
                    {
                        return (float)dic[key];
                    }
                }
            }
            return -1;
        }
        public static float GetFloat(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(VD.currentPlayerStep.varKeys.ToArray(), VD.currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(float))
                    {
                        return (float)dic[key];
                    }
                }
            }
            return -1;
        }
        // BOOL
        public static bool GetBool(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(bool))
                    {
                        return (bool)dic[key];
                    }
                }
            }
            return false;
        }
        public static bool GetBool(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(VD.currentPlayerStep.varKeys.ToArray(), VD.currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(bool))
                    {
                        return (bool)dic[key];
                    }
                }
            }
            return false;
        }
        // STRING
        public static string GetString(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(string))
                    {
                        return (string)dic[key];
                    }
                }
            }
            return string.Empty;
        }
        public static string GetString(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(VD.currentPlayerStep.varKeys.ToArray(), VD.currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(string))
                    {
                        return (string)dic[key];
                    }
                }
            }
            return string.Empty;
        }

        static DialogueNode GetPN(string dialogueName, int nodeID)
        {
            int diag = -1;

            DialogueNode playerNode = null;

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i;
                        break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return null;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return null;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID)
                {
                    playerNode = diags[diag].playerNodes[i];
                    break;
                }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return null;
            }
            return playerNode;
        }
        #endregion

    }

    public class VD2
    {
        public VD2()
        {
            FetchDiags();
        }

        /// <summary>
        /// Saves current VD2 state which includes modified EVs and comment visibility. 
        /// </summary>
        /// <param name="filename">Name to save the JSON file under.</param>
        /// <param name="saveAssigned">Optionally, save every VA state found within the current scene.</param>
        public void SaveState(string filename, bool saveAssigned)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (VD.GetCurLan != null)
                dict.Add("locCur", VD.currentLanguage);

            for (int i = 0; i < diags.Count; i++)
            {
                if (diags[i].loaded)
                {
                    for (int n = 0; n < diags[i].playerNodes.Count; n++)
                    {
                        for (int k = 0; k < diags[i].playerNodes[n].vars.Count; k++)
                        {
                            dict.Add(diags[i].name + "_node" + n.ToString() + "_var" + k.ToString(), diags[i].playerNodes[n].vars[k]);
                        }
                        for (int c = 0; c < diags[i].playerNodes[n].comment.Count; c++)
                        {
                            dict.Add(diags[i].name + "_node" + n.ToString() + "_com" + c.ToString() + "_vis", diags[i].playerNodes[n].comment[c].visible);
                        }
                    }
                }
            }

            if (saveAssigned)
            {
                VIDE_Assign[] gos = Resources.FindObjectsOfTypeAll<VIDE_Assign>();
                foreach (VIDE_Assign va in gos)
                    va.SaveState(va.gameObject.name + "_state");
            }


            SerializeHelper.WriteToFile(dict as Dictionary<string, object>, filename + ".json");
        }

        /// <summary>
        /// Loads the specified state to VD2. 
        /// </summary>
        /// <param name="filename">The filename of the state to load.</param>
        /// <param name="loadAssigned">Optionally, load the state of every VA found in the scene if filename follows the 'gameObjectName_state' naming convention.</param>
        public void LoadState(string filename, bool loadAssigned)
        {
            if (!File.Exists(Application.dataPath + "/VIDE/saves/" + filename + ".json"))
            {
                return;
            }

            if (diags.Count < 1)
                FetchDiags();

            Dictionary<string, object> dict = SerializeHelper.ReadState(filename) as Dictionary<string, object>;

            if (VD.localizationEnabled && dict.ContainsKey("locCur"))
            {
                if (VD.currentLanguage != (string)dict["locCur"])
                    VD.SetCurrentLanguage((string)dict["locCur"]);
            }


            for (int i = 0; i < diags.Count; i++)
            {
                if (diags[i].loaded)
                {
                    for (int n = 0; n < diags[i].playerNodes.Count; n++)
                    {
                        for (int k = 0; k < diags[i].playerNodes[n].vars.Count; k++)
                        {
                            if (dict.ContainsKey(diags[i].name + "_node" + n.ToString() + "_var" + k.ToString()))
                                diags[i].playerNodes[n].vars[k] = (string)dict[diags[i].name + "_node" + n.ToString() + "_var" + k.ToString()];
                        }
                        for (int c = 0; c < diags[i].playerNodes[n].comment.Count; c++)
                        {
                            if (dict.ContainsKey(diags[i].name + "_node" + n.ToString() + "_com" + c.ToString() + "_vis"))
                                diags[i].playerNodes[n].comment[c].visible = (bool)dict[diags[i].name + "_node" + n.ToString() + "_com" + c.ToString() + "_vis"];
                        }
                    }
                }
            }

            if (loadAssigned)
            {
                VIDE_Assign[] gos = Resources.FindObjectsOfTypeAll<VIDE_Assign>();
                foreach (VIDE_Assign va in gos)
                    va.LoadState(va.gameObject.name + "_state");
            }

        }


        public class NodeData
        {
            public bool isPlayer;
            public bool pausedAction;
            public bool isEnd;
            public int nodeID;
            public Sprite sprite;

            public string[] comments;
            public Comment[] creferences;
            public string[] extraData;
            public AudioClip[] audios;
            public Sprite[] sprites;
            public Dictionary<string, object> extraVars;

            public int commentIndex;

            public string tag;
            public bool dirty;

            public NodeData()
            {
                isPlayer = true;
                isEnd = false;
                extraVars = new Dictionary<string, object>();
                nodeID = -1;
                commentIndex = 0;
            }
            public NodeData(NodeData nd)
            {
                isPlayer = nd.isPlayer;
                pausedAction = nd.pausedAction;
                isEnd = nd.isEnd;
                nodeID = nd.nodeID;
                sprite = nd.sprite;
                comments = nd.comments;
                extraData = nd.extraData;
                audios = nd.audios;
                sprites = nd.sprites;
                extraVars = nd.extraVars;
                commentIndex = nd.commentIndex;
                tag = nd.tag;
                dirty = nd.dirty;
            }


        }

        public int assignedIndex = 0;

        private DialogueNode currentPlayerStep;
        private ActionNode currentActionNode;
        private ActionNode lastActionNode;
        private int startPoint = -1;
        private VIDE_Assign _assigned;
        private bool cancelAction;
        private List<Diags> diags = new List<Diags>();

        public bool isActive;
        public NodeData nodeData;
        public int currentDiag = -1;
        public VIDE_Assign assigned
        {
            get
            {
                return (VIDE_Assign)_assigned;
            }
        }
        public List<Diags> saved
        {
            get
            {
                return diags;
            }
        }

        /* Events */

        public delegate void ActionEvent(int nodeID);
        public event ActionEvent OnActionNode;

        public delegate void LangEvent();

        public delegate void NodeChange(VD2 data);
        public event NodeChange OnNodeChange;
        public event NodeChange OnEnd;

        public delegate void LoadUnload();
        public event LoadUnload OnLoaded;

        int pauseGotoNode = -1;

        void FetchDiags()
        {
            VD.FetchDiags();
            diags.AddRange(VD.saved);

        }

        void DoAction()
        {

            if (OnActionNode != null)
                OnActionNode(lastActionNode.ID);

            //Do predefined actions
            if (lastActionNode.ovrStartNode > -1)
                _assigned.overrideStartNode = lastActionNode.ovrStartNode;
            if (lastActionNode.renameDialogue.Length > 0)
                _assigned.alias = lastActionNode.renameDialogue;

            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == currentActionNode.gameObjectName);

            foreach (GameObject g in objects)
            {
                if (currentActionNode.paramType < 1)
                {
                    g.SendMessage(currentActionNode.methodName, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 1)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_bool, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 2)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_string, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 3)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_int, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
                if (currentActionNode.paramType == 4)
                {
                    g.SendMessage(currentActionNode.methodName, currentActionNode.param_float, SendMessageOptions.DontRequireReceiver);
                    if (currentActionNode == null) return;
                    continue;
                }
            }

            if (currentActionNode.paramType > -1)
                if (cancelAction)
                {
                    cancelAction = false;
                    return;
                }


            if (!currentActionNode.pauseHere)
            {
                if (lastActionNode.gotoNode > -1)
                    SetNode(lastActionNode.gotoNode);
                else
                    Next();
            }
            else
            {
                if (lastActionNode.gotoNode > -1)
                    pauseGotoNode = lastActionNode.gotoNode;
                nodeData.pausedAction = true;
            }

        }

        private string[] GetOptions(DialogueNode diagNode)
        {
            List<string> op = new List<string>();

            if (diagNode == null)
            {
                return op.ToArray();
            }

            for (int i = 0; i < diagNode.comment.Count; i++)
            {
                op.Add(diagNode.comment[i].text);
            }

            return op.ToArray();
        }

        private string[] GetExtraData(DialogueNode diagNode)
        {
            List<string> op = new List<string>();

            if (diagNode == null)
            {
                return op.ToArray();
            }

            for (int i = 0; i < diagNode.comment.Count; i++)
            {
                if (diagNode.comment[i].extraData.Length > 0)
                    op.Add(diagNode.comment[i].extraData);
                else
                    op.Add(string.Empty);
            }

            return op.ToArray();
        }

        void addComment(DialogueNode id)
        {
            id.comment.Add(new Comment(id));
        }

        void addSet(int cSize, int id, string tag)
        {
            diags[currentDiag].playerNodes.Add(new DialogueNode(cSize, id, tag));
        }

        /// <summary>
        /// Internal. Do not use.
        /// </summary>
        /// <returns></returns>
        bool Load(string dName)
        {
            VD.currentDiag = currentDiag;
            VD.Load(dName);
            diags[currentDiag] = VD.saved[currentDiag];

            //Here we load the localized version
            if (VD.localizationEnabledSET)
            {
                VIDE_Localization.LoadLanguages(VD.saved[currentDiag].name, false);
                LoadLocalized(false);
            }
            else
            {
                VIDE_Localization.LoadLanguages(VD.saved[currentDiag].name, true);
                LoadLocalized(true);
            }

            VD.currentDiag = -1;

            return true;
        }

        public int startNode
        {
            get
            {
                return startPoint;
            }
        }

        public string GetFirstTag(bool searchPlayer)
        {
            if (!isActive)
            {
                Debug.LogError("No dialogue loaded!");
                return string.Empty;
            }

            string firstTag = string.Empty;
            if (searchPlayer)
            {
                for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                {
                    if (diags[currentDiag].playerNodes[i].isPlayer)
                    {
                        firstTag = diags[currentDiag].playerNodes[i].playerTag;
                        if (!string.IsNullOrEmpty(firstTag))
                            break;
                    }
                    else
                    {
                        firstTag = diags[currentDiag].playerNodes[i].playerTag;
                        if (!string.IsNullOrEmpty(firstTag))
                            break;
                    }
                }
            }

            return firstTag;
        }

        AudioClip[] GetPlayerAudios(DialogueNode diagNode)
        {
            List<AudioClip> au = new List<AudioClip>();
            foreach (Comment c in diagNode.comment)
                au.Add(c.audios);

            return au.ToArray();
        }

        Sprite[] GetPlayerSprites(DialogueNode diagNode)
        {
            List<Sprite> sp = new List<Sprite>();
            foreach (Comment c in diagNode.comment)
                sp.Add(c.sprites);

            return sp.ToArray();
        }

        private NodeData GetNodeDataForPlayer(DialogueNode diagNode)
        {
            NodeData nd = new NodeData();

            nd.isPlayer = diagNode.isPlayer;
            nd.pausedAction = false;
            nd.isEnd = false;
            nd.nodeID = diagNode.ID;

            nd.comments = GetOptions(diagNode);

            List<Comment> cRefs = new List<Comment>();
            foreach (Comment c in diagNode.comment)
                cRefs.Add(c);
            nd.creferences = cRefs.ToArray();

            nd.extraData = GetExtraData(diagNode);

            nd.commentIndex = 0;

            nd.tag = diagNode.playerTag;
            nd.dirty = false;

            nd.extraVars = GetExtraVars(diagNode.varKeys.ToArray(), diagNode.vars.ToArray());
            nd.sprite = diagNode.sprite;

            nd.audios = GetPlayerAudios(diagNode);
            nd.sprites = GetPlayerSprites(diagNode);
            SetVisibility(nd, diagNode);

            return nd;
        }

        Dictionary<string, object> GetExtraVars(string[] key, string[] val)
        {
            Dictionary<string, object> objs = new Dictionary<string, object>();

            for (int i = 0; i < val.Length; i++)
            {
                string st = val[i].ToLower();
                //Bools
                if (st.Contains("false"))
                {
                    objs.Add(key[i], false);
                    continue;
                }
                if (st.Contains("true"))
                {
                    objs.Add(key[i], true);
                    continue;
                }
                //Int
                int sInt = -1;
                if (System.Int32.TryParse(st, out sInt))
                {
                    objs.Add(key[i], sInt);
                    continue;
                }
                //Float
                float sFloat = -1;
                if (System.Single.TryParse(st, out sFloat))
                {
                    objs.Add(key[i], sFloat);
                    continue;
                }
                //String
                objs.Add(key[i], val[i]);
            }

            return objs;
        }

        void SetVisibility(NodeData nd, DialogueNode diagNode)
        {
            List<string> coms = new List<string>();
            List<Comment> cRefs = new List<Comment>();
            List<string> exd = new List<string>();
            List<AudioClip> aud = new List<AudioClip>();
            List<Sprite> spr = new List<Sprite>();

            for (int i = 0; i < diagNode.comment.Count; i++)
            {
                if (diagNode.comment[i].visible)
                {
                    coms.Add(nd.comments[i]);
                    cRefs.Add(nd.creferences[i]);
                    exd.Add(nd.extraData[i]);
                    aud.Add(nd.audios[i]);
                    spr.Add(nd.sprites[i]);
                }
            }

            nd.comments = coms.ToArray();
            nd.creferences = cRefs.ToArray();
            nd.extraData = exd.ToArray();
            nd.audios = aud.ToArray();
            nd.sprites = spr.ToArray();
        }

        /// <summary>
        /// Sets the visibility of a node comment. Dialogue doesn't have to be active.
        /// </summary>
        /// <param name="dialogueName">Dialogue name</param>
        /// <param name="nodeID">ID of the node</param>
        /// <param name="commentIndex">Comment index</param>
        /// <param name="visible">Is it visible?</param>
        public void SetVisible(string dialogueName, int nodeID, int commentIndex, bool visible)
        {
            int diag = -1;
            DialogueNode playerNode = null;
            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (commentIndex >= playerNode.comment.Count || commentIndex < 0)
            {
                Debug.LogError("Comment not found! Check the commentIndex.");
                return;
            }

            playerNode.comment[commentIndex].visible = visible;
        }

        /// <summary>
        /// Sets the visibility of a node comment in the currently active dialogue.
        /// </summary>
        /// <param name="nodeID">ID of the node</param>
        /// <param name="commentIndex">Comment index</param>
        /// <param name="visible">Is it visible?</param>
        public void SetVisible(int nodeID, int commentIndex, bool visible)
        {
            if (!isActive)
            {
                Debug.LogError("There's no active dialogue!");
            }

            int node = currentPlayerStep.comment.IndexOf(nodeData.creferences[nodeData.commentIndex]);

            int diag = currentDiag;
            DialogueNode playerNode = null;

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (commentIndex >= playerNode.comment.Count || commentIndex < 0)
            {
                Debug.LogError("Comment not found! Check the commentIndex.");
                return;
            }

            playerNode.comment[node].visible = visible;
        }

        /// <summary>
        /// Modify the Extra Variables of a dialogue node.
        /// </summary>
        /// <param name="dialogueName">The dialogue to modify. Use VIDE_Data.assign.assignedDialogue to use currently active dialogue</param>
        /// <param name="nodeID">The node to modify. Make sure it exists.</param>
        /// <param name="newVars">A dictionary with the new content</param>
        public void SetExtraVariables(string dialogueName, int nodeID, Dictionary<string, object> newVars)
        {
            int diag = -1;
            DialogueNode playerNode = null;
            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }


            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (playerNode != null)
            {
                List<string> newStrings = new List<string>();
                foreach (KeyValuePair<string, object> entry in newVars)
                {
                    newStrings.Add(entry.Value.ToString());
                }
                playerNode.vars = newStrings;
            }

            if (isActive && nodeData != null)
            {
                nodeData.dirty = true;
            }

        }

        /// <summary>
        /// Modify the active dialogue's Extra Variables.
        /// </summary>
        /// <param name='nodeID'>
        /// The node to modify. Make sure it exists.
        /// </param>
        /// <param name='newVars'>
        /// A dictionary with the new content
        /// </param>
        public void SetExtraVariables(int nodeID, Dictionary<string, object> newVars)
        {
            if (!isActive)
            {
                Debug.LogError("There's no active dialogue!");
            }

            int diag = currentDiag;
            DialogueNode playerNode = null;

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }


            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }

            if (playerNode != null)
            {
                List<string> newStrings = new List<string>();
                foreach (KeyValuePair<string, object> entry in newVars)
                {
                    newStrings.Add(entry.Value.ToString());
                }
                playerNode.vars = newStrings;
            }

            if (isActive && nodeData != null)
            {
                nodeData.dirty = true;
            }

        }

        /// <summary>
        /// Update a Dialogue node comment.
        /// </summary>
        /// <param name="dialogueName">Name of the dialogue to modify a node from</param>
        /// <param name="nodeID">The ID of the Dialogue node</param>
        /// <param name="commentIndex">The comment index of a Player node</param>
        /// <param name="newComment">The new comment</param>
        public void SetComment(string dialogueName, int nodeID, int commentIndex, string newComment)
        {
            int diag = -1;

            DialogueNode playerNode = null;

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return;
            }
            else
            {
                if (commentIndex > -1 && commentIndex < playerNode.comment.Count)
                {
                    playerNode.comment[commentIndex].text = newComment;
                }
                else
                {
                    Debug.LogError("Index out of range!");
                    return;
                }
            }

        }

        /// <summary>
        /// Get the Extra Variables of a loaded dialogue node.
        /// </summary>
        /// <param name="dialogueName">The loaded dialogue to get Extra Variables from</param>
        /// <param name="nodeID">The node ID</param>
        /// <returns></returns>
        public Dictionary<string, object> GetExtraVariables(string dialogueName, int nodeID)
        {
            int diag = -1;

            DialogueNode playerNode = null;

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i; break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return null;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return null;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID) { playerNode = diags[diag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return null;
            }

            if (playerNode != null)
            {
                return GetExtraVars(playerNode.varKeys.ToArray(), playerNode.vars.ToArray());
            }

            return null;
        }

        /// <summary>
        /// Get the Extra Variables of an active dialogue node.
        /// </summary>
        /// <param name="nodeID">The node ID</param>
        /// <returns></returns>
        public Dictionary<string, object> GetExtraVariables(int nodeID)
        {
            if (!isActive)
            {
                Debug.LogError("No dialogue currently active!");
                return null;
            }

            DialogueNode playerNode = null;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                if (diags[currentDiag].playerNodes[i].ID == nodeID) { playerNode = diags[currentDiag].playerNodes[i]; break; }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return null;
            }

            if (playerNode != null)
            {
                return GetExtraVars(playerNode.varKeys.ToArray(), playerNode.vars.ToArray());
            }

            return null;
        }

        /// <summary>
        /// Gets the dialogue's non-component VIDE_Assign.
        /// </summary>
        /// <param name="dialogueName">Name of the dialogue. Make sure it exists.</param>
        /// <returns></returns>
        public VIDE_Assign GetAssigned(string dialogueName)
        {
            if (diags.Count < 1)
                FetchDiags();

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].VA != null)
                    {
                        return diags[i].VA;
                    }
                    else
                    {
                        Debug.LogError("Variable is Null! Initiate it first by calling SetAssigned()");
                        return null;
                    }
                }

            Debug.LogError("'" + dialogueName + "' not found!");
            return null;
        }

        /// <summary>
        /// Sets the dialogue's non-component VIDE_Assign.
        /// </summary>
        /// <param name="dialogueName">Name of the dialogue. Make sure it exists.</param>
        /// <param name="alias">alias</param>
        /// <param name="ovr">override Start Node. -1 is default.</param>
        /// <param name="playerSprite">Default player Sprite. Null is default.</param>
        /// <param name="npcSprite">Default NPC Sprite. Null is default.</param>
        public void SetAssigned(string dialogueName, string alias, int ovr, Sprite playerSprite, Sprite npcSprite)
        {
            if (diags.Count < 1)
                FetchDiags();

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].VA == null)
                    {
                        GameObject go = new GameObject();
                        go.name = "temp_" + diags[i].name;
                        go.hideFlags = HideFlags.HideInHierarchy;
                        diags[i].VA = go.AddComponent<VIDE_Assign>();
                        diags[i].VA.assignedDialogue = diags[i].name;
                        diags[i].VA.assignedIndex = diags.IndexOf(diags[i]);
                    }
                    diags[i].VA.alias = alias;
                    diags[i].VA.overrideStartNode = ovr;
                    diags[i].VA.defaultNPCSprite = npcSprite;
                    diags[i].VA.defaultPlayerSprite = playerSprite;
                    return;
                }

            Debug.LogError("'" + dialogueName + "' not found!");
            return;
        }

        /// <summary>
        /// Returns the list of created dialogues.
        /// </summary>
        /// <returns></returns>
        public string[] GetDialogues()
        {
            TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
            List<string> names = new List<string>();
            for (int i = 0; i < files.Length; i++)
                names.Add(files[i].name);
            names.Sort();

            return names.ToArray();
        }

        /// <summary>
        /// Get a list of the available languages created. 
        /// Add them from the Localization menu inside the VIDE Editor.
        /// </summary>
        /// <returns></returns>
        public string[] GetLanguages()
        {
            List<string> langs = new List<string>();
            foreach (VIDE_Localization.VLanguage l in VIDE_Localization.languages)
            {
                langs.Add(l.name);
            }

            return langs.ToArray();
        }

        /// <summary>
        /// Returns the list of created dialogues.
        /// </summary>
        /// <returns>
        /// The NodeData package.
        /// </returns>
        /// <param name='dialogueName'>
        /// The name of the dialogue to search. You can use GetDialogues() to get a list.
        /// </param>
        /// <param name='id'>
        /// The ID of the Dialogue Node. Get it from the VIDE Editor.
        /// </param>
        /// <param name='forceLoad'>
        /// Will load the dialogue to memory if its not loaded. 
        /// </param>
        public NodeData GetNodeData(string dialogueName, int id, bool forceLoad)
        {
            int dIndex = -1;

            if (forceLoad)
                FetchDiags();

            foreach (Diags d in diags)
            {
                if (d.name == dialogueName)
                {
                    if (!d.loaded)
                    {
                        if (forceLoad)
                        {
                            dIndex = diags.IndexOf(d);
                            currentDiag = diags.IndexOf(d);
                            Load(d.name);
                            if (OnLoaded != null)
                                OnLoaded();
                        }
                        else
                        {
                            Debug.LogError("'" + dialogueName + "' not loaded!");
                            return null;
                        }
                    }
                    else
                    {
                        dIndex = diags.IndexOf(d);
                    }
                    break;
                }
            }

            currentDiag = -1;
            DialogueNode realNode = currentPlayerStep;
            if (dIndex == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found! Is it loaded?");
                return null;
            }

            //Look for Node with given ID
            bool foundID = false;

            for (int i = 0; i < diags[dIndex].playerNodes.Count; i++)
            {
                if (diags[dIndex].playerNodes[i].ID == id)
                {
                    currentPlayerStep = diags[dIndex].playerNodes[i];
                    foundID = true;
                }
            }

            if (!foundID)
            {
                Debug.LogError("Could not find a Node with ID " + id.ToString());
                return null;
            }

            /* Action end */

            NodeData nd = GetNodeDataForPlayer(currentPlayerStep);
            currentPlayerStep = realNode;
            return nd;
        }

        /// <summary>
        /// Returns the NodeData package of the specified Dialogue Node. The current progression of the dialogue is unaffected by this call.
        /// nodeData is also unaffected.
        /// </summary>
        /// <returns>
        /// The NodeData package.
        /// </returns>
        /// <param name='id'>
        /// The ID of the Dialogue Node. Get it from the VIDE Editor.
        /// </param>
        public NodeData GetNodeData(int id)
        {
            if (!isActive)
            {
                Debug.LogError("You must call the 'BeginDialogue()' method before calling this method!");
                return null;
            }

            //Look for Node with given ID
            bool foundID = false;
            DialogueNode realNode = currentPlayerStep;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
            {
                if (diags[currentDiag].playerNodes[i].ID == id)
                {
                    currentPlayerStep = diags[currentDiag].playerNodes[i];
                    foundID = true;
                }
            }

            if (!foundID)
            {
                Debug.LogError("Could not find a Node with ID " + id.ToString());
                return null;
            }

            /* Action end */

            NodeData nd = GetNodeDataForPlayer(currentPlayerStep);
            currentPlayerStep = realNode;
            return nd;

        }

        /// <summary>
        /// Ignores current state of nodeData and jumps directly to specified node. 
        /// </summary>
        /// <returns>
        /// The NodeData package.
        /// </returns>
        /// <param name='id'>
        /// The ID of the Node. Get it from the VIDE Editor.
        /// </param>
        public NodeData SetNode(int id)
        {
            if (!isActive)
            {
                Debug.LogError("You must call the 'BeginDialogue()' method before calling this method!");
                return null;
            }

            pauseGotoNode = -1;
            currentActionNode = null;
            lastActionNode = null;

            //Look for Node with given ID
            bool foundID = false;
            bool isAct = false;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
            {
                if (diags[currentDiag].playerNodes[i].ID == id)
                {
                    currentPlayerStep = diags[currentDiag].playerNodes[i];
                    foundID = true;
                }
            }
            if (!foundID)
            {
                for (int i = 0; i < diags[currentDiag].actionNodes.Count; i++)
                {
                    if (diags[currentDiag].actionNodes[i].ID == id)
                    {
                        currentActionNode = diags[currentDiag].actionNodes[i];
                        foundID = true;
                        isAct = true;
                    }
                }
            }
            if (!foundID)
            {
                Debug.LogError("Could not find a Node with ID " + id.ToString());
                return null;
            }

            cancelAction = true;

            /* Action node */

            if (isAct)
            {
                lastActionNode = currentActionNode;
                nodeData = new NodeData();
                DoAction();
                return nodeData;
            }

            /* Action end */

            nodeData = GetNodeDataForPlayer(currentPlayerStep);
            if (OnNodeChange != null) OnNodeChange(this);
            return nodeData;

        }

        /// <summary>
        /// Populates nodeData with the data from next Node based on the current nodeData.
        /// </summary>
        /// <returns></returns>
        public NodeData Next()
        {
            cancelAction = false;

            if (!isActive)
            {
                Debug.LogError("You must call the 'BeginDialogue()' method before calling the 'Next()' method!");
                return null;
            }

            if (pauseGotoNode > -1)
            {
                SetNode(pauseGotoNode);
                return null;
            }

            int option = 0;

            if (nodeData != null)
            {
                option = currentPlayerStep.comment.IndexOf(nodeData.creferences[nodeData.commentIndex]);
            }

            if (currentPlayerStep != null)
            {
                if (currentPlayerStep.isPlayer)
                {
                    if (currentPlayerStep.comment[option].outNode == null && currentPlayerStep.comment[option].outAction == null)
                    {
                        nodeData.isEnd = true;
                        if (OnEnd != null) OnEnd(this);
                        return nodeData;
                    }
                }
                else
                {
                    if (currentPlayerStep.comment[0].outNode == null && currentPlayerStep.comment[0].outAction == null && nodeData.commentIndex == nodeData.comments.Length - 1)
                    {
                        nodeData.isEnd = true;
                        if (OnEnd != null) OnEnd(this);
                        return nodeData;
                    }
                }

            }


            //If action node is connected to nothing, then it's the end
            if (lastActionNode != null)
            {
                if (lastActionNode.outPlayer == null && lastActionNode.outAction == null)
                {
                    if (lastActionNode.gotoNode == -1)
                    {
                        nodeData.isEnd = true;
                        if (OnEnd != null) OnEnd(this);
                        return nodeData;
                    }

                }
            }

            /* Action Node? */

            if (currentActionNode == null)
            {
                if (currentPlayerStep.isPlayer)
                {
                    currentActionNode = currentPlayerStep.comment[option].outAction;
                }
                else
                {
                    if (nodeData.commentIndex == nodeData.comments.Length - 1)
                        currentActionNode = currentPlayerStep.comment[0].outAction;
                }
            }
            else
            {
                currentActionNode = currentActionNode.outAction;
            }

            //If we found action node, let's go to it.
            if (currentActionNode != null)
            {

                lastActionNode = currentActionNode;
                DoAction();
                return nodeData;
            }

            /* END Action Node */

            if (!nodeData.isPlayer)
            {
                if (nodeData.comments.Length > 0)
                {
                    if (nodeData.commentIndex != nodeData.comments.Length - 1)
                    {
                        nodeData.commentIndex++;
                        lastActionNode = null;
                        if (OnNodeChange != null) OnNodeChange(this);
                        return nodeData;
                    }
                }

                if (lastActionNode != null)
                    if (lastActionNode.outPlayer != null)
                    {
                        currentPlayerStep = lastActionNode.outPlayer;
                        lastActionNode = null;
                        nodeData = GetNodeDataForPlayer(currentPlayerStep);
                        if (OnNodeChange != null) OnNodeChange(this);
                        return nodeData;
                    }

                lastActionNode = null;
                currentPlayerStep = currentPlayerStep.comment[0].outNode;
                nodeData = GetNodeDataForPlayer(currentPlayerStep);
                if (OnNodeChange != null) OnNodeChange(this);


                return nodeData;
            }
            else
            {
                if (lastActionNode == null)
                {
                    currentPlayerStep = currentPlayerStep.comment[option].outNode;
                }
                else
                {
                    if (lastActionNode.outPlayer != null)
                    {
                        currentPlayerStep = lastActionNode.outPlayer;
                        lastActionNode = null;
                        nodeData = GetNodeDataForPlayer(currentPlayerStep);
                        if (OnNodeChange != null) OnNodeChange(this);
                        return nodeData;
                    }
                }

                lastActionNode = null;
                nodeData = GetNodeDataForPlayer(currentPlayerStep);
                if (OnNodeChange != null) OnNodeChange(this);
                return nodeData;
            }
        }

        /// <summary>
        /// Simulates Next() method. Returns next node's NodeData package based on current nodeData. Will not fire any events. Will not modify current nodeData. Will return current nodeData if next isEnd or an ActionNode.  
        /// </summary>
        /// <param name="returnNextComment">Should we follow the current comment array?</param>
        /// <param name="callAction">If next is an ActionNode, should the actions be called?</param>
        /// <returns>NodeData package</returns>
        public NodeData GetNext(bool returnNextComment, bool callAction)
        {

            if (!isActive)
            {
                Debug.LogError("A dialogue must be active!");
                return null;
            }

            DialogueNode currentPlayerStepB = new DialogueNode(currentPlayerStep);
            ActionNode currentActionNodeB = new ActionNode(currentActionNode);
            ActionNode lastActionNodeB = new ActionNode(lastActionNode);

            if (currentPlayerStep == null) currentPlayerStepB = null;
            if (currentActionNode == null) currentActionNodeB = null;
            if (lastActionNode == null) lastActionNodeB = null;

            int option = 0;

            if (nodeData != null)
                option = nodeData.commentIndex;

            if (currentPlayerStepB != null)
            {
                if (currentPlayerStepB.isPlayer)
                {
                    if (currentPlayerStepB.comment[option].outNode == null && currentPlayerStepB.comment[option].outAction == null)
                    {
                        NodeData nd = new NodeData(nodeData);
                        nd.isEnd = true;
                        return nd;
                    }
                }
                else
                {
                    if (returnNextComment)
                    {
                        if (currentPlayerStepB.comment[0].outNode == null && currentPlayerStepB.comment[0].outAction == null && nodeData.commentIndex == nodeData.comments.Length - 1)
                        {
                            NodeData nd = new NodeData(nodeData);
                            nd.isEnd = true;
                            return nd;
                        }
                    }
                    else
                    {
                        if ((currentPlayerStepB.comment[0].outNode == null && currentPlayerStepB.comment[0].outAction == null))
                        {
                            NodeData nd = new NodeData(nodeData);
                            nd.isEnd = true;
                            return nd;
                        }
                    }

                }
            }


            //If action node is connected to nothing, then it's the end
            if (lastActionNodeB != null)
            {
                if (lastActionNodeB.outPlayer == null && lastActionNodeB.outAction == null)
                {
                    if (lastActionNodeB.gotoNode == -1)
                    {
                        NodeData nd = new NodeData(nodeData);
                        nd.isEnd = true;
                        return nd;
                    }

                }
            }

            /* Action Node? */

            if (currentActionNodeB == null)
            {
                if (currentPlayerStepB.isPlayer)
                {
                    currentActionNodeB = currentPlayerStepB.comment[option].outAction;
                }
                else
                {
                    if (nodeData.commentIndex == nodeData.comments.Length - 1)
                        currentActionNodeB = currentPlayerStepB.comment[0].outAction;
                }
            }
            else
            {
                currentActionNodeB = currentActionNodeB.outAction;
            }

            //If we found action node, let's go to it.
            if (currentActionNodeB != null)
            {
                lastActionNodeB = currentActionNodeB;
                if (callAction) DoAction();
                return nodeData;
            }

            /* END Action Node */

            if (!nodeData.isPlayer)
            {
                if (returnNextComment)
                {
                    if (nodeData.comments.Length > 0)
                    {
                        if (nodeData.commentIndex != nodeData.comments.Length - 1)
                        {
                            NodeData nd = new NodeData(nodeData);
                            nd.commentIndex++;
                            return nd;
                        }
                    }
                }


                if (lastActionNodeB != null)
                    if (lastActionNodeB.outPlayer != null)
                    {
                        currentPlayerStepB = lastActionNodeB.outPlayer;
                        lastActionNodeB = null;
                        NodeData nd = GetNodeDataForPlayer(currentPlayerStepB);
                        return nd;
                    }

                lastActionNodeB = null;
                currentPlayerStepB = currentPlayerStepB.comment[0].outNode;
                NodeData nd2 = GetNodeDataForPlayer(currentPlayerStepB);
                return nd2;
            }
            else
            {
                if (lastActionNodeB == null)
                {
                    currentPlayerStepB = currentPlayerStepB.comment[option].outNode;
                }
                else
                {
                    if (lastActionNodeB.outPlayer != null)
                    {
                        currentPlayerStepB = lastActionNodeB.outPlayer;
                        lastActionNodeB = null;
                        NodeData nd = GetNodeDataForPlayer(currentPlayerStepB);
                        return nd;
                    }
                }

                lastActionNodeB = null;
                NodeData nd2 = GetNodeDataForPlayer(currentPlayerStepB);
                return nd2;
            }
        }

        /// <summary>
        /// Activates the dialogue _assigned to VIDE_Assign. Populates the nodeData variable with the first Node based on the Start Node. Also returns the current NodeData package.
        /// </summary>
        /// <param name="diagToLoad"></param>
        /// <returns>NodeData</returns>
        public NodeData BeginDialogue(VIDE_Assign diagToLoad)
        {
            if (diags.Count < 1)
                FetchDiags();

            if (diagToLoad.assignedIndex < 0 || diagToLoad.assignedIndex > diagToLoad.diags.Count - 1)
            {
                Debug.LogError("No dialogue assigned to VIDE_Assign!");
                return null;
            }

            int theIndex = -1;
            for (int i = 0; i < diags.Count; i++)
            {
                if (diagToLoad.assignedDialogue == diags[i].name)
                    theIndex = i;
            }

            if (theIndex == -1)
            {
                Debug.LogError("'" + diagToLoad.assignedDialogue + "' dialogue assigned to " + diagToLoad.gameObject.name + " not found! Did you delete the dialogue?");
                return null;
            }

            currentDiag = theIndex; //assign current dialogue index

            if (VD.saved.Count > 0 && VD.saved[currentDiag].loaded)
            {
                diags[currentDiag] = VD.saved[currentDiag];
            }

            //Check if the dialogue is already loaded
            if (!diags[currentDiag].loaded)
            {
                //Let's load the dialogue 
                if (Load(diagToLoad.assignedDialogue))
                {
                    diags[currentDiag] = VD.saved[currentDiag];
                    isActive = true;
                }
                else
                {
                    isActive = false;
                    currentDiag = -1;
                    Debug.LogError("Failed to load '" + diagToLoad.assignedDialogue + "'");
                    return null;
                }
            }
            else
            {
                isActive = true;
            }

            //Make sure that variables were correctly reset after last conversation
            if (nodeData != null)
            {
                Debug.LogError("You forgot to call 'EndDialogue()' on last conversation!");
                return null;
            }

            _assigned = diagToLoad;
            startPoint = diags[currentDiag].start;

            if (_assigned.overrideStartNode != -1)
                startPoint = _assigned.overrideStartNode;

            int startIndex = -1;
            bool isAct = false;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                if (startPoint == diags[currentDiag].playerNodes[i].ID) { startIndex = i; break; }

            for (int i = 0; i < diags[currentDiag].actionNodes.Count; i++)
                if (startPoint == diags[currentDiag].actionNodes[i].ID)
                {
                    startIndex = i;
                    currentActionNode = diags[currentDiag].actionNodes[i]; isAct = true; break;
                }

            /* Action node */

            if (isAct)
            {
                lastActionNode = currentActionNode;
                nodeData = new NodeData();
                DoAction();
                return nodeData;
            }

            /* Action end */

            if (startIndex == -1)
            {
                Debug.LogError("Start point not found! Check your IDs!");
                return null;
            }

            currentPlayerStep = diags[currentDiag].playerNodes[startIndex];

            lastActionNode = null;
            nodeData = GetNodeDataForPlayer(currentPlayerStep);
            if (OnNodeChange != null) OnNodeChange(this);
            return nodeData;

        }

        /// <summary>
        /// Activates the dialogue just sent. Populates the nodeData variable with the first Node based on the Start Node. Also returns the current NodeData package.
        /// VIDE_Assign variables will only be available as long as the dialogue is loaded. 
        /// </summary>
        /// <param name="diagToLoad"></param>
        /// <returns>NodeData</returns>
        public NodeData BeginDialogue(string diagName)
        {
            if (diags.Count < 1)
                FetchDiags();

            int theIndex = -1;
            for (int i = 0; i < diags.Count; i++)
            {
                if (diagName == diags[i].name)
                    theIndex = i;
            }

            if (theIndex == -1)
            {
                Debug.LogError("'" + diagName + " not found!");
                return null;
            }

            currentDiag = theIndex; //assign current dialogue index

            if (VD.saved.Count > 0 && VD.saved[currentDiag].loaded)
            {
                diags[currentDiag] = VD.saved[currentDiag];
            }

            //Check if the dialogue is already loaded
            if (!diags[currentDiag].loaded)
            {
                //Let's load the dialogue 
                if (Load(diagName))
                {
                    isActive = true;
                }
                else
                {
                    isActive = false;
                    currentDiag = -1;
                    Debug.LogError("Failed to load '" + diagName + "'");
                    return null;
                }
            }
            else
            {
                isActive = true;
            }

            //Because we have no VIDE_Assign and we need one for the user
            //We'll create a temporal object with one
            //The data will prevail until the dialogue is unloaded
            if (diags[currentDiag].VA == null)
            {
                GameObject go = new GameObject();
                go.name = "temp_" + diags[currentDiag].name;
                go.hideFlags = HideFlags.HideInHierarchy;
                diags[currentDiag].VA = go.AddComponent<VIDE_Assign>();
                diags[currentDiag].VA.assignedDialogue = diags[currentDiag].name;
                diags[currentDiag].VA.assignedIndex = diags.IndexOf(diags[currentDiag]);
            }

            //Make sure that variables were correctly reset after last conversation
            if (nodeData != null)
            {
                Debug.LogError("You forgot to call 'EndDialogue()' on last conversation!");
                return null;
            }

            _assigned = diags[currentDiag].VA;
            startPoint = diags[currentDiag].start;

            if (_assigned.overrideStartNode != -1)
                startPoint = _assigned.overrideStartNode;

            int startIndex = -1;
            bool isAct = false;

            for (int i = 0; i < diags[currentDiag].playerNodes.Count; i++)
                if (startPoint == diags[currentDiag].playerNodes[i].ID) { startIndex = i; break; }

            for (int i = 0; i < diags[currentDiag].actionNodes.Count; i++)
                if (startPoint == diags[currentDiag].actionNodes[i].ID)
                {
                    startIndex = i;
                    currentActionNode = diags[currentDiag].actionNodes[i]; isAct = true; break;
                }

            /* Action node */

            if (isAct)
            {
                lastActionNode = currentActionNode;
                nodeData = new NodeData();
                DoAction();
                return nodeData;
            }

            /* Action end */

            if (startIndex == -1)
            {
                Debug.LogError("Start point not found! Check your IDs!");
                return null;
            }

            currentPlayerStep = diags[currentDiag].playerNodes[startIndex];

            lastActionNode = null;
            nodeData = GetNodeDataForPlayer(currentPlayerStep);
            if (OnNodeChange != null) OnNodeChange(this);
            return nodeData;

        }

        /// <summary>
        /// Wipes out all data and unloads the current VIDE_Assign, raising its interactionCount.
        /// </summary>
        public void EndDialogue()
        {
            nodeData = null;
            if (_assigned != null)
                _assigned.interactionCount++;
            _assigned = null;
            startPoint = -1;
            isActive = false;
            currentDiag = -1;
            currentPlayerStep = null;
            currentActionNode = null;
            lastActionNode = null;
            pauseGotoNode = -1;
            cancelAction = false;

        }

        public void LoadLocalized(bool onlyLoadDefault)
        {
            VIDE_Localization.VLanguage cur = VD.GetCurLan;

            if (onlyLoadDefault)
                cur = VD.dflset;

            if (cur != null)
                if (cur.playerDiags != null)
                    for (int i = 0; i < cur.playerDiags.Count; i++)
                    {
                        diags[currentDiag].playerNodes[i].sprite = cur.playerDiags[i].sprite;
                        diags[currentDiag].playerNodes[i].playerTag = cur.playerDiags[i].playerTag;
                        for (int ii = 0; ii < cur.playerDiags[i].comment.Count; ii++)
                        {
                            diags[currentDiag].playerNodes[i].comment[ii].text = cur.playerDiags[i].comment[ii].text;
                            diags[currentDiag].playerNodes[i].comment[ii].audios = cur.playerDiags[i].comment[ii].audios;
                            diags[currentDiag].playerNodes[i].comment[ii].sprites = cur.playerDiags[i].comment[ii].sprites;
                        }
                    }
        }

        //Get extra variables 
        #region
        // INT
        public int GetInt(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(int))
                    {
                        return (int)dic[key];
                    }
                }
            }
            return -1;
        }
        public int GetInt(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(currentPlayerStep.varKeys.ToArray(), currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(int))
                    {
                        return (int)dic[key];
                    }
                }
            }
            return -1;
        }
        // FLOAT
        public float GetFloat(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(float))
                    {
                        return (float)dic[key];
                    }
                }
            }
            return -1;
        }
        public float GetFloat(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(currentPlayerStep.varKeys.ToArray(), currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(float))
                    {
                        return (float)dic[key];
                    }
                }
            }
            return -1;
        }
        // BOOL
        public bool GetBool(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(bool))
                    {
                        return (bool)dic[key];
                    }
                }
            }
            return false;
        }
        public bool GetBool(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(currentPlayerStep.varKeys.ToArray(), currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(bool))
                    {
                        return (bool)dic[key];
                    }
                }
            }
            return false;
        }
        // STRING
        public string GetString(string dialogueName, int nodeID, string key)
        {
            DialogueNode evs = GetPN(dialogueName, nodeID);
            if (evs != null)
            {
                Dictionary<string, object> dic = GetExtraVars(evs.varKeys.ToArray(), evs.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(string))
                    {
                        return (string)dic[key];
                    }
                }
            }
            return string.Empty;
        }
        public string GetString(string key)
        {
            if (VD.isActive && VD.nodeData != null)
            {
                Dictionary<string, object> dic = GetExtraVars(currentPlayerStep.varKeys.ToArray(), currentPlayerStep.vars.ToArray());
                if (dic.ContainsKey(key))
                {
                    if (dic[key].GetType() == typeof(string))
                    {
                        return (string)dic[key];
                    }
                }
            }
            return string.Empty;
        }

        DialogueNode GetPN(string dialogueName, int nodeID)
        {
            int diag = -1;

            DialogueNode playerNode = null;

            for (int i = 0; i < diags.Count; i++)
                if (diags[i].name == dialogueName)
                {
                    if (diags[i].loaded)
                    {
                        diag = i;
                        break;
                    }
                    else
                    {
                        Debug.LogError("'" + dialogueName + "' not loaded! Load it first by calling LoadDialogues()");
                        return null;
                    }
                }

            if (diag == -1)
            {
                Debug.LogError("'" + dialogueName + "' not found!");
                return null;
            }

            for (int i = 0; i < diags[diag].playerNodes.Count; i++)
                if (diags[diag].playerNodes[i].ID == nodeID)
                {
                    playerNode = diags[diag].playerNodes[i];
                    break;
                }

            if (playerNode == null)
            {
                Debug.LogError("Node ID " + nodeID.ToString() + " not found within the Dialogue nodes!");
                return null;
            }
            return playerNode;
        }
        #endregion Extra Variables

    }


}








