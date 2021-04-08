using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MiniJSON_VIDE;
using System.Reflection;
using bf = System.Reflection.BindingFlags;
using System.Text.RegularExpressions;
using System.Linq;

public class VIDE_Editor : EditorWindow
{

    //This script will draw the VIDE Editor window and all of its content
    //It comunicates with VIDE_EditorDB to store the data

    //Blacklist for namespaces or class names. 
    //For Action Nodes: add here the namespaces or classes you don't wish to see fetched in the list.
    //Any namespace or class CONTAINING any of the below strings will be discarded from the search.
    public string[] namespaceBlackList = new string[]{
        "UnityEngine",
        //TMP       
    };

    VIDE_EditorDB db;
    GameObject dbObj;
    Color defaultColor;
    Color32[] colors;

    VIDE_EditorDB.Comment draggedCom;
    VIDE_EditorDB.ActionNode draggedAction;

    Rect canvas = new Rect(-4000, -4000, 20000, 20000);
    bool lerpFocusTime = false;
    Vector2 goalScrollPos;
    Vector2 scrollArea;
    Vector2 dragStart;
    Vector2 assignScroll;
    int gotofocus = 0;
    int gridSize = 10;
    Rect fWin = new Rect();
    Rect startDiag;

    bool updateNodesRectsOnce = true;
    bool spyView = false;
    bool showSettings;
    bool assignMenu;
    bool assignMenuShowMore;
    bool localizationMenu;
    int deletingLanguage = -1;
    string assignMenuFilter = "";
    float focusTimer;

    string newsHeadline;
    string newsHeadlineLink;

    bool searchingForDialogue = false;
    string searchWord;
    Vector2 searchScrollView;

    WWW news;
    List<string> saveNames = new List<string>() { };
    List<string> saveNamesFull = new List<string>() { };

    int areYouSureIndex = 0;
    Texture2D lineIcon;
    Texture2D newNodeIcon;
    Texture2D newNodeIcon3;
    Texture2D twitIcon;
    int dragNewNode = 0;
    object copiedNode = null;
    Rect dragNewNodeRect = new Rect(20, 20, 100, 40);

    bool draggingLine = false;
    bool dragnWindows = false;
    bool repaintLines = false;
    bool editEnabled = true;
    bool newFile = false;
    bool overwritePopup = false;
    bool deletePopup = false;
    bool needSave = false;
    bool playerReady = false;
    bool areYouSure = false;
    bool showError = false;
    bool hasID = false;
    bool insideNode = false;
    bool willDeselect = false;
    bool copyFromDefSure = false;
    bool editingColors = false;
    bool showHelp = false;
    string newFileName = "My Dialogue";
    string errorMsg = "";
    string lastTextFocus;

    bool draggingNode = false;

    GUIStyle txtComst;
    GUIStyle text1st;

    Vector2 languageScrollArea;
    VIDE_Localization.VLanguage selLang;
    Rect langOptionsRect;

    int selectNodeDelayed = -1;
    int selectANodeDelayed = -1;

    VIDE_Editor_Skin skinscr;

    List<string> existingTags = new List<string>();

    List<Vector2> balls = new List<Vector2>();
    List<Vector2> ballsGravity = new List<Vector2>();
    bool holdingBall = false;

    //Add VIDE Editor to Window...
    [MenuItem("Window/VIDE Editor")]
    static void ShowEditor()
    {
        VIDE_Editor editor = EditorWindow.GetWindow<VIDE_Editor>();
        editor.Init("", false);
    }

    void OnEnable()
    {
        VIDE_EditorDB.videRoot = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
        VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
        VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
        dbObj = (GameObject)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/Editor/db.prefab", typeof(GameObject));
        db = dbObj.GetComponent<VIDE_EditorDB>();

        GameObject go = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/Editor/editorstyles.prefab", typeof(GameObject));
        VIDE_Editor_Skin.instance = go.GetComponent<VIDE_Editor_Skin>();
        skinscr = go.GetComponent<VIDE_Editor_Skin>();

        lineIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/lineIcon.png", typeof(Texture2D));
        newNodeIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/newNode.png", typeof(Texture2D));
        newNodeIcon3 = (Texture2D)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/newNode2.png", typeof(Texture2D));
        twitIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/twit.jpg", typeof(Texture2D));
        gridTex = (Texture2D)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/backTex.jpg", typeof(Texture2D));
        mapTex = (Texture2D)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/uiBack.png", typeof(Texture2D));

        mapTex.SetPixel(0, 0, new Color(0, 0, 0, 0.25f)); mapTex.Apply();
        gridTex.SetPixel(0, 0, VIDE_Editor_Skin.GetColor(7, db.skinIndex));
        gridTex.Apply();
        selLang = VIDE_Localization.LoadSettings();
        txtComst = skinscr.mm_box_default;
        text1st = skinscr.mm_labels;
        spyView = false;
        CheckNews();

        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        saveNames = new List<string>();
        saveNamesFull = new List<string>();
        foreach (TextAsset f in files)
        {
            saveNames.Add(f.name);
            saveNamesFull.Add(AssetDatabase.GetAssetPath(f));
        }
        saveNames.Sort();

        //Listen to localization events
        VIDE_Localization.LoadSettings();

        loadEditorSettings();
        loadFiles(db.currentDiag);
        Load(true);

        repaintLines = true;
        CenterAll(false, db.startID, true);
    }

    //Save progress if autosave is on
    void OnLostFocus()
    {
        dragnWindows = false;
        Repaint();
        repaintLines = true;

        if (db.autosave)
        {
            Save();
            AssetDatabase.Refresh();
            saveEditorSettings(db.currentDiag);
        }
    }

    //Set all start variables
    public void Init(string dName, bool loadFromIndex)
    {
        VIDE_EditorDB.videRoot = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));

        VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
        VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
        VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();

#if UNITY_5_0
        EditorWindow.GetWindow<VIDE_Editor>().title = "VIDE Editor";
#else
        Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/assignIcon.png", typeof(Texture2D));
        GUIContent titleContent = new GUIContent(" VIDE Editor", icon);
        EditorWindow.GetWindow<VIDE_Editor>().titleContent = titleContent;
#endif


        dbObj = (GameObject)AssetDatabase.LoadAssetAtPath(VIDE_EditorDB.videRoot + "/Data/Editor/db.prefab", typeof(GameObject));
        db = dbObj.GetComponent<VIDE_EditorDB>();
        startDiag = new Rect(20f, 50f, 300f, 50f);

        CheckNews();

        VIDE_Editor editor = EditorWindow.GetWindow<VIDE_Editor>();
        editor.position = new Rect(50f, 50f, 1027f, 768);

        scrollArea = new Vector2(4000, 4000);

        //Update diag list
        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        saveNames = new List<string>();
        saveNamesFull = new List<string>();
        foreach (TextAsset f in files)
        {
            saveNames.Add(f.name);
            saveNamesFull.Add(AssetDatabase.GetAssetPath(f));
        }
        saveNames.Sort();

        //Get correct index of sent diag
        int theIndex = 0;
        for (int i = 0; i < saveNames.Count; i++)
        {
            if (saveNames[i] == dName)
                theIndex = i;
        }

        //Listen to localization events
        VIDE_Localization.LoadSettings();

        if (loadFromIndex)
        {
            db.fileIndex = theIndex;
            loadFiles(theIndex);
            saveEditorSettings(db.currentDiag);
            Load(true);
        }
        else
        {
            loadEditorSettings();
            loadFiles(db.currentDiag);
            Load(true);
        }


        repaintLines = true;

        CenterAll(false, db.startID, true);
    }

    void CheckNews()
    {
        newsHeadline = "Checking...";
        news = new WWW("https://chrishendersonb.com/vide/videnews.txt");
    }

    double lastTime;
    double deltatime;
    void Update()
    {
        double currenttime = EditorApplication.timeSinceStartup;
        deltatime = currenttime - lastTime;

        if (news != null)
            if (news.isDone)
            {
                if (news.text.Length > 10)
                {
                    string[] st = news.text.Split(","[0]);
                    newsHeadline = st[0];
                    if (st.Length > 1)
                        newsHeadlineLink = st[1];
                    news = null;

                }
                else
                {
                    newsHeadlineLink = string.Empty;
                    newsHeadline = "Could not connect";
                    news = null;
                }
            }

        if (lerpFocusTime)
        {
            float timer = ((Time.realtimeSinceStartup - focusTimer) / 5);
            scrollArea = Vector2.Lerp(scrollArea, goalScrollPos, timer);
            Repaint();
            if (timer > 0.2f)
            {
                lerpFocusTime = false;
            }

            if (Vector2.Distance(new Vector2(scrollArea.x, scrollArea.y), new Vector2(goalScrollPos.x, goalScrollPos.y)) < 0.1f)
            {
                lerpFocusTime = false;
            }
        }

        for (int i = 0; i < balls.Count; i++)
        {
            ballsGravity[i] += new Vector2(0, 15f );
            balls[i] += ballsGravity[i] * (float)deltatime;
        }

        lastTime = currenttime;

    }

    public class SerializeHelper
    {
        static string fileDataPath = Application.dataPath + "/../";
        static string SettingsDataPath = Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/";

        public static void WriteToFile(object data, string filename)
        {
            string outString = DiagJson.Serialize(data);
            File.WriteAllText(fileDataPath + filename, outString);
        }

        public static object ReadFromFile(string filename)
        {
            string jsonString = File.ReadAllText(fileDataPath + filename);
            return DiagJson.Deserialize(jsonString);
        }
        public static void WriteSettings(object data, string filename)
        {
            string outString = DiagJson.Serialize(data);
            File.WriteAllText(SettingsDataPath + filename, outString);
        }
        public static object ReadSettings(string filename)
        {
            string jsonString = File.ReadAllText(SettingsDataPath + filename);
            return DiagJson.Deserialize(jsonString);
        }
    }

    #region Main Methods

    public void addComment(VIDE_EditorDB.DialogueNode id)
    {
        Undo.RecordObject(db, "Added Comment");
        int setX = db.playerDiags.IndexOf(id);
        id.comment.Add(new VIDE_EditorDB.Comment(id));

        if (VIDE_Localization.isEnabled)
        {
            for (int i = 0; i < VIDE_Localization.languages.Count; i++)
            {
                if (VIDE_Localization.languages[i].playerDiags != null)
                    VIDE_Localization.languages[i].playerDiags[setX].comment.Add(new VIDE_EditorDB.Comment(VIDE_Localization.languages[i].playerDiags[setX]));
            }
        }
    }

    public void addSet(Vector2 rPos, int cSize, int id, string pTag, bool endC)
    {
        db.playerDiags.Add(new VIDE_EditorDB.DialogueNode(rPos, cSize, id, pTag, endC));

        if (VIDE_Localization.isEnabled)
        {
            for (int i = 0; i < VIDE_Localization.languages.Count; i++)
            {
                if (VIDE_Localization.languages[i].playerDiags != null)
                {
                    VIDE_Localization.languages[i].playerDiags.Add(new VIDE_EditorDB.DialogueNode(rPos, cSize, id, pTag, endC));
                }
            }
        }
    }

    public void removeSet(VIDE_EditorDB.DialogueNode id)
    {
        Undo.RecordObject(db, "Removed Set");
        if (VIDE_Localization.isEnabled)
        {
            for (int i = 0; i < VIDE_Localization.languages.Count; i++)
            {
                if (VIDE_Localization.languages[i].playerDiags != null)
                {
                    VIDE_Localization.languages[i].playerDiags.RemoveAt(db.playerDiags.IndexOf(id));
                }
            }
        }

        db.playerDiags.Remove(id);

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                if (db.playerDiags[i].comment[ii].outNode == id)
                {
                    db.playerDiags[i].comment[ii].outNode = null;
                }
            }

        }

        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            if (db.actionNodes[i].outPlayer == id)
            {
                db.actionNodes[i].outPlayer = null;
            }
        }
    }

    public void removeComment(VIDE_EditorDB.Comment idx)
    {
        Undo.RecordObject(db, "Removed Comment");
        int setX = db.playerDiags.IndexOf(idx.inputSet);
        int cX = idx.inputSet.comment.IndexOf(idx);

        idx.inputSet.comment.Remove(idx);

        if (VIDE_Localization.isEnabled)
        {
            for (int i = 0; i < VIDE_Localization.languages.Count; i++)
            {
                if (VIDE_Localization.languages[i].playerDiags != null)
                    VIDE_Localization.languages[i].playerDiags[setX].comment.RemoveAt(cX);
            }
        }
    }

    public void removeAction(VIDE_EditorDB.ActionNode id)
    {
        Undo.RecordObject(db, "Added Action");
        db.actionNodes.Remove(id);

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                if (db.playerDiags[i].comment[ii].outAction == id)
                {
                    db.playerDiags[i].comment[ii].outAction = null;
                }
            }
        }

        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            if (db.actionNodes[i].outAction == id)
            {
                db.actionNodes[i].outAction = null;
            }
        }
    }

    void ArrangeComment(VIDE_EditorDB.DialogueNode node, int dir, int oriIndex)
    {
        VIDE_EditorDB.Comment other = node.comment[oriIndex + dir];
        node.comment[oriIndex + dir] = node.comment[oriIndex];
        node.comment[oriIndex] = other;

        int setX = db.playerDiags.IndexOf(node);

        if (VIDE_Localization.isEnabled)
        {
            for (int i = 0; i < VIDE_Localization.languages.Count; i++)
            {
                if (VIDE_Localization.languages[i].playerDiags != null)
                {
                    VIDE_EditorDB.DialogueNode nodeB = VIDE_Localization.languages[i].playerDiags[setX];
                    VIDE_EditorDB.Comment otherB = nodeB.comment[oriIndex + dir];
                    nodeB.comment[oriIndex + dir] = nodeB.comment[oriIndex];
                    nodeB.comment[oriIndex] = otherB;
                }
            }
        }
    }

    //This will break the node connections
    public void breakConnection(int type, VIDE_EditorDB.Comment commID, VIDE_EditorDB.ActionNode aID)
    {
        Undo.RecordObject(db, "Broke Connection");

        //Type 0 = VIDE_EditorDB.DialogueNode
        //Type 1 = VIDE_EditorDB.ActionNode

        if (type == 0)
        {
            commID.outNode = null;
            commID.outAction = null;
        }

        if (type == 1)
        {
            aID.outPlayer = null;
            aID.outAction = null;
        }

    }

    //Connect DialogueNode to others
    //Create node if released on empty space
    public void TryConnectToDialogueNode(Vector2 mPos, VIDE_EditorDB.Comment commID)
    {
        if (commID == null) return;

        Undo.RecordObject(db, "Connected Node");

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (db.playerDiags[i].rect.Contains(mPos))
            {
                commID.outNode = db.playerDiags[i];
                Repaint();
                return;
            }
        }
        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            if (db.actionNodes[i].rect.Contains(mPos))
            {
                commID.outAction = db.actionNodes[i];
                Repaint();
                return;
            }
        }

        int id = setUniqueID();
        db.playerDiags.Add(new VIDE_EditorDB.DialogueNode(new Rect(mPos.x - 150, mPos.y - 200, 0, 0), id));
        commID.outNode = db.playerDiags[db.playerDiags.Count - 1];

        if (VIDE_Localization.isEnabled)
        {
            for (int i = 0; i < VIDE_Localization.languages.Count; i++)
            {
                if (VIDE_Localization.languages[i].playerDiags != null)
                {
                    VIDE_Localization.languages[i].playerDiags.Add(new VIDE_EditorDB.DialogueNode(new Rect(mPos.x - 150, mPos.y - 200, 0, 0), 0));
                }
            }
        }

        repaintLines = true;
        Repaint();
        GUIUtility.hotControl = 0;
    }

    //Connect Action node to Dialogue Node/Action node
    //Create Action node if released on empty space
    public void TryConnectAction(Vector2 mPos, VIDE_EditorDB.ActionNode aID)
    {
        if (aID == null) return;
        Undo.RecordObject(db, "Connected Node");

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (db.playerDiags[i].rect.Contains(mPos))
            {
                aID.outPlayer = db.playerDiags[i];
                Repaint();
                return;
            }
        }
        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            if (db.actionNodes[i].rect.Contains(mPos))
            {
                if (db.actionNodes[i] == aID) { return; }

                aID.outAction = db.actionNodes[i];
                Repaint();
                return;
            }
        }
        int id = setUniqueID();
        db.actionNodes.Add(new VIDE_EditorDB.ActionNode(new Rect(mPos.x - 150, mPos.y - 200, 0, 0), id));
        aID.outAction = db.actionNodes[db.actionNodes.Count - 1];
        repaintLines = true;
        Repaint();
        GUIUtility.hotControl = 0;
    }

    //Sets a unique ID for the node
    public int setUniqueID()
    {
        int tempID = 0;
        while (!searchIDs(tempID))
        {
            tempID++;
        }
        return tempID;
    }

    //Searches for a unique ID
    public bool searchIDs(int id)
    {
        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (db.playerDiags[i].ID == id) return false;
        }
        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            if (db.actionNodes[i].ID == id) return false;
        }
        return true;
    }

    bool HasUniqueID(int id)
    {
        //Retrieve all IDs
        foreach (string s in saveNames)
        {
            if (s == saveNames[db.currentDiag]) continue;

            int currentName = -1;

            /* Get file location based on name */
            for (int i = 0; i < saveNames.Count; i++)
            {
                if (saveNamesFull[i].Contains(saveNames[saveNames.IndexOf(s)] + ".json"))
                    currentName = i;
            }

            if (currentName == -1)
            {
                return true;
            }

            if (File.Exists(Application.dataPath + "/../" + saveNamesFull[currentName]))
            {
                Dictionary<string, object> dict = SerializeHelper.ReadFromFile(saveNamesFull[currentName]) as Dictionary<string, object>;
                if (dict.ContainsKey("dID"))
                    if (id == ((int)((long)dict["dID"])))
                        return false;
            }
        }
        return true;
    }

    int AssignDialogueID()
    {
        List<int> ids = new List<int>();
        int newID = Random.Range(0, 99999);

        //Retrieve all IDs
        foreach (string s in saveNames)
        {
            int currentName = -1;

            /* Get file location based on name */
            for (int i = 0; i < saveNames.Count; i++)
            {
                if (saveNamesFull[i].Contains(saveNames[saveNames.IndexOf(s)] + ".json"))
                    currentName = i;
            }

            if (currentName == -1)
            {
                return 0;
            }
            if (File.Exists(Application.dataPath + "/../" + saveNamesFull[currentName]))
            {
                Dictionary<string, object> dict = SerializeHelper.ReadFromFile(saveNamesFull[currentName]) as Dictionary<string, object>;
                if (dict.ContainsKey("dID"))
                    ids.Add((int)((long)dict["dID"]));
            }
        }

        //Make sure ID is unique
        while (ids.Contains(newID))
        {
            newID = Random.Range(0, 99999);
        }

        return newID;
    }

    //Try create a new dialogue file
    public bool tryCreate(string fName)
    {

        if (saveNames.Contains(fName))
        {
            return false;
        }
        else
        {
            Undo.RecordObject(db, "Created Dialogue");
            saveNames.Add(fName);
            saveNamesFull.Add(VIDE_EditorDB.videRoot + "/Resources/Dialogues/" + fName + ".json");

            saveNames.Sort();
            db.currentDiag = saveNames.IndexOf(fName);
            db.startID = 0;
            db.autosave = true;
            return true;
        }
    }

    //Deletes dialogue
    public void DeleteDiag()
    {
        Undo.RecordObject(db, "Deleted Dialogue");
        File.Delete(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Dialogues/" + saveNames[db.currentDiag] + ".json");
        File.Delete(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Dialogues/" + saveNames[db.currentDiag] + ".json.meta");
        if (File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/" + "LOC_" + saveNames[db.currentDiag] + ".json"))
        {
            File.Delete(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/" + "LOC_" + saveNames[db.currentDiag] + ".json");
            File.Delete(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/" + "LOC_" + saveNames[db.currentDiag] + ".json.meta");
        }
        AssetDatabase.Refresh();
        loadFiles(0);
        Load(true);
    }

    public Rect IDExists(bool resetOnMax)
    {
        int highestID = 0;
        foreach (VIDE_EditorDB.DialogueNode c in db.playerDiags)
        {
            if (c.ID > highestID) { highestID = c.ID; }
        }
        foreach (VIDE_EditorDB.ActionNode c in db.actionNodes)
        {
            if (c.ID > highestID) { highestID = c.ID; }
        }

        for (int i = 0; i < 99999; i++)
        {
            if (db.curFocusID > highestID)
            {
                if (resetOnMax)
                    db.curFocusID = 0;
                else
                    db.curFocusID = highestID;
            }

            foreach (VIDE_EditorDB.DialogueNode c in db.playerDiags)
            {
                if (c.ID == db.curFocusID) { return c.rect; }
            }
            foreach (VIDE_EditorDB.ActionNode c in db.actionNodes)
            {
                if (c.ID == db.curFocusID) { return c.rect; }
            }
            db.curFocusID++;
        }
        return new Rect(0, 0, 0, 0);
    }

    //Centers nodes
    public void CenterAll(bool increment, int specific, bool instantly)
    {
        Rect nodePos = new Rect();
        if (specific > -1)
        {
            db.curFocusID = specific;
            nodePos = IDExists(false);
        }

        if (increment)
        {
            db.curFocusID++;
            nodePos = IDExists(true);
        }

        if (instantly)
        {
            scrollArea = new Vector2(Mathf.Abs(canvas.x) + nodePos.x - (position.width / 2) + nodePos.width / 2, Mathf.Abs(canvas.y) + nodePos.y - (position.height / 2) + nodePos.height / 2 + 50);
            return;
        }

        focusTimer = Time.realtimeSinceStartup;
        goalScrollPos = new Vector2(Mathf.Abs(canvas.x) + nodePos.x - (position.width / 2) + nodePos.width / 2, Mathf.Abs(canvas.y) + nodePos.y - (position.height / 2) + nodePos.height / 2 + 50);
        lerpFocusTime = true;
    }
    #endregion

    #region File Handling

    public void SaveToLanguage()
    {
        GUIUtility.hotControl = 0;
        GUIUtility.keyboardControl = 0;

        VIDE_Localization.VLanguage cur = VIDE_Localization.currentLanguage;
        if (db.playerDiags.Count > 0)
            cur.playerDiags = new List<VIDE_EditorDB.DialogueNode>();

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            cur.playerDiags.Add(new VIDE_EditorDB.DialogueNode());

            cur.playerDiags[i].sprite = db.playerDiags[i].sprite;
            cur.playerDiags[i].playerTag = db.playerDiags[i].playerTag;

            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                cur.playerDiags[i].comment.Add(new VIDE_EditorDB.Comment());
                cur.playerDiags[i].comment[ii].text = db.playerDiags[i].comment[ii].text;
                cur.playerDiags[i].comment[ii].audios = db.playerDiags[i].comment[ii].audios;
                cur.playerDiags[i].comment[ii].sprites = db.playerDiags[i].comment[ii].sprites;
            }
        }

        VIDE_Localization.SaveLanguages(saveNames[db.currentDiag]);
    }

    public void LoadLocalizedFromDefault()
    {
        VIDE_Localization.VLanguage cur = VIDE_Localization.defaultLanguage;
        if (cur != null)
            if (cur.playerDiags != null)
                for (int i = 0; i < cur.playerDiags.Count; i++)
                {
                    if (db.playerDiags.Count < 1) return;
                    if (db.playerDiags[i] == null) return;
                    db.playerDiags[i].sprite = cur.playerDiags[i].sprite;
                    db.playerDiags[i].playerTag = cur.playerDiags[i].playerTag;
                    for (int ii = 0; ii < cur.playerDiags[i].comment.Count; ii++)
                    {
                        if (ii >= db.playerDiags[i].comment.Count)
                        {
                            return;
                        }
                        db.playerDiags[i].comment[ii].text = cur.playerDiags[i].comment[ii].text;
                        db.playerDiags[i].comment[ii].audios = cur.playerDiags[i].comment[ii].audios;
                        db.playerDiags[i].comment[ii].sprites = cur.playerDiags[i].comment[ii].sprites;
                    }
                }
    }

    public void LoadLocalized()
    {
        VIDE_Localization.VLanguage cur = VIDE_Localization.currentLanguage;
        if (cur != null)
            if (cur.playerDiags != null)
                for (int i = 0; i < cur.playerDiags.Count; i++)
                {
                    if (db.playerDiags.Count < 1) return;
                    if (db.playerDiags[i] == null) return;
                    db.playerDiags[i].sprite = cur.playerDiags[i].sprite;
                    db.playerDiags[i].playerTag = cur.playerDiags[i].playerTag;
                    for (int ii = 0; ii < cur.playerDiags[i].comment.Count; ii++)
                    {
                        if (ii >= db.playerDiags[i].comment.Count)
                        {
                            return;
                        }
                        db.playerDiags[i].comment[ii].text = cur.playerDiags[i].comment[ii].text;
                        db.playerDiags[i].comment[ii].audios = cur.playerDiags[i].comment[ii].audios;
                        db.playerDiags[i].comment[ii].sprites = cur.playerDiags[i].comment[ii].sprites;
                    }
                }
    }

    //This will save the current data base status
    public void Save()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        GUIUtility.keyboardControl = 0;

        if (VIDE_Localization.isEnabled)
        {
            SaveToLanguage();
            VIDE_Localization.SaveSettings();
        }

        if (saveNames.Count < 1)
            return;

        if (db.currentDiag >= saveNames.Count)
        {
            Debug.LogError("Dialogue file not found! Loading default.");
            db.currentDiag = 0;
        }

        int theID = -1;
        int currentName = -1;

        /* Get file location based on name */
        for (int i = 0; i < saveNames.Count; i++)
        {
            if (saveNamesFull[i].Contains(saveNames[db.currentDiag] + ".json"))
                currentName = i;
        }
        if (currentName == -1)
        {
            return;
        }

        if (File.Exists(Application.dataPath + "/../" + saveNamesFull[currentName]))
        {
            Dictionary<string, object> dictl = SerializeHelper.ReadFromFile(saveNamesFull[currentName]) as Dictionary<string, object>;
            if (dictl.ContainsKey("dID"))
            {
                theID = ((int)((long)dictl["dID"]));

            }
        }

        if (theID == -1)
        {
            dict.Add("dID", AssignDialogueID());
        }
        else
        {
            if (!HasUniqueID(theID))
                dict.Add("dID", AssignDialogueID());
            else
                dict.Add("dID", theID);
        }

        dict.Add("playerDiags", db.playerDiags.Count);
        dict.Add("actionNodes", db.actionNodes.Count);
        dict.Add("startPoint", db.startID);
        dict.Add("loadTag", db.loadTag);
        dict.Add("previewPanning", db.previewPanning);
        dict.Add("autosave", db.autosave);
        dict.Add("locEdit", VIDE_Localization.isEnabled);

        dict.Add("showSettings", showSettings);

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            dict.Add("pd_isp_" + i.ToString(), db.playerDiags[i].isPlayer);
            dict.Add("pd_rect_" + i.ToString(), new int[] { (int)db.playerDiags[i].rect.x, (int)db.playerDiags[i].rect.y });
            dict.Add("pd_comSize_" + i.ToString(), db.playerDiags[i].comment.Count);
            dict.Add("pd_ID_" + i.ToString(), db.playerDiags[i].ID);

            dict.Add("pd_pTag_" + i.ToString(), db.playerDiags[i].playerTag);

            if (db.playerDiags[i].sprite != null)
                dict.Add("pd_sprite_" + i.ToString(), AssetDatabase.GetAssetPath(db.playerDiags[i].sprite));
            else
                dict.Add("pd_sprite_" + i.ToString(), string.Empty);


            dict.Add("pd_expand_" + i.ToString(), db.playerDiags[i].expand);
            dict.Add("pd_vars" + i.ToString(), db.playerDiags[i].vars.Count);

            for (int v = 0; v < db.playerDiags[i].vars.Count; v++)
            {
                dict.Add("pd_var_" + i.ToString() + "_" + v.ToString(), db.playerDiags[i].vars[v]);
                dict.Add("pd_varKey_" + i.ToString() + "_" + v.ToString(), db.playerDiags[i].varKeys[v]);
            }

            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "iSet", db.playerDiags.FindIndex(idx => idx == db.playerDiags[i].comment[ii].inputSet));
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "oAns", db.playerDiags.FindIndex(idx => idx == db.playerDiags[i].comment[ii].outNode));
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "oAct", db.actionNodes.FindIndex(idx => idx == db.playerDiags[i].comment[ii].outAction));
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "text", db.playerDiags[i].comment[ii].text);
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "extraD", db.playerDiags[i].comment[ii].extraData);
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "showmore", db.playerDiags[i].comment[ii].showmore);
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "visible", db.playerDiags[i].comment[ii].visible);

                if (db.playerDiags[i].comment[ii].audios != null)
                    dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "audio", AssetDatabase.GetAssetPath(db.playerDiags[i].comment[ii].audios));
                else
                    dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "audio", string.Empty);
                if (db.playerDiags[i].comment[ii].sprites != null)
                    dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite", AssetDatabase.GetAssetPath(db.playerDiags[i].comment[ii].sprites));
                else
                    dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite", string.Empty);

            }
        }

        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            dict.Add("ac_rect_" + i.ToString(), new int[] { (int)db.actionNodes[i].rect.x, (int)db.actionNodes[i].rect.y });
            dict.Add("ac_ID_" + i.ToString(), db.actionNodes[i].ID);
            dict.Add("ac_pause_" + i.ToString(), db.actionNodes[i].pauseHere);

            dict.Add("ac_goName_" + i.ToString(), db.actionNodes[i].gameObjectName);
            dict.Add("ac_nIndex_" + i.ToString(), db.actionNodes[i].nameIndex);

            dict.Add("ac_goto_" + i.ToString(), db.actionNodes[i].gotoNode);

            dict.Add("ac_optsCount_" + i.ToString(), db.actionNodes[i].opts.Length);
            for (int ii = 0; ii < db.actionNodes[i].opts.Length; ii++)
                dict.Add("ac_opts_" + ii.ToString() + "_" + i.ToString(), db.actionNodes[i].opts[ii]);

            dict.Add("ac_namesCount_" + i.ToString(), db.actionNodes[i].nameOpts.Count);
            for (int ii = 0; ii < db.actionNodes[i].nameOpts.Count; ii++)
                dict.Add("ac_names_" + ii.ToString() + "_" + i.ToString(), db.actionNodes[i].nameOpts[ii]);

            List<string> keyList = new List<string>(db.actionNodes[i].methods.Keys);
            dict.Add("ac_methCount_" + i.ToString(), keyList.Count);

            for (int ii = 0; ii < db.actionNodes[i].methods.Count; ii++)
            {
                dict.Add("ac_meth_key_" + i.ToString() + "_" + ii.ToString(), keyList[ii]);
                dict.Add("ac_meth_val_" + i.ToString() + "_" + ii.ToString(), db.actionNodes[i].methods[keyList[ii]]);
            }

            dict.Add("ac_meth_" + i.ToString(), db.actionNodes[i].methodName);
            dict.Add("ac_paramT_" + i.ToString(), db.actionNodes[i].paramType);
            dict.Add("ac_methIndex_" + i.ToString(), db.actionNodes[i].methodIndex);

            dict.Add("ac_pString_" + i.ToString(), db.actionNodes[i].param_string);
            dict.Add("ac_pBool_" + i.ToString(), db.actionNodes[i].param_bool);
            dict.Add("ac_pInt_" + i.ToString(), db.actionNodes[i].param_int);
            dict.Add("ac_pFloat_" + i.ToString(), db.actionNodes[i].param_float);

            dict.Add("ac_oSet_" + i.ToString(), db.playerDiags.FindIndex(idx => idx == db.actionNodes[i].outPlayer));
            dict.Add("ac_oAct_" + i.ToString(), db.actionNodes.FindIndex(idx => idx == db.actionNodes[i].outAction));

            dict.Add("ac_ovrStartNode_" + i.ToString(), db.actionNodes[i].ovrStartNode);
            dict.Add("ac_renameDialogue_" + i.ToString(), db.actionNodes[i].renameDialogue);
            dict.Add("ac_more_" + i.ToString(), db.actionNodes[i].more);

        }

        //Check if Va needs preload again
        if (needSave)
        {
            VIDE_Assign[] gos = Resources.FindObjectsOfTypeAll<VIDE_Assign>();
            foreach (VIDE_Assign v in gos)
            {
                if (v.assignedDialogue == saveNames[db.currentDiag])
                {
                    if (v.preload)
                    {
                        v.notuptodate = true;
                    }
                }
            }
        }

        needSave = false;
        SerializeHelper.WriteToFile(dict as Dictionary<string, object>, saveNamesFull[currentName]);

      


    }

    public static string colorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    public static Color hexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    public void saveEditorSettings(int cd)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("db.currentDiagEdited", cd);

        dict.Add("db.curSkin", db.skinIndex);

        //Save skin colors
        dict.Add("colors", "colors");
        List<VIDE_Editor_Skin.Skin> skins = VIDE_Editor_Skin.instance.skins;
        for (int i = 0; i < skins.Count; i++)
        {
            dict.Add("Skin_" + i.ToString() + "pNodeColor", colorToHex(skins[i].player_NodeColor));
            dict.Add("Skin_" + i.ToString() + "pNodeColor2", colorToHex(skins[i].player_NodeColorSecondary));
            dict.Add("Skin_" + i.ToString() + "nNodeColor", colorToHex(skins[i].npc_NodeColor));
            dict.Add("Skin_" + i.ToString() + "nNodeColor2", colorToHex(skins[i].npc_NodeColorSecondary));
            dict.Add("Skin_" + i.ToString() + "aNodeColor", colorToHex(skins[i].action_NodeColor));
            dict.Add("Skin_" + i.ToString() + "aNodeColor2", colorToHex(skins[i].action_NodeColorSecondary));
            dict.Add("Skin_" + i.ToString() + "back", colorToHex(skins[i].background_color));
            dict.Add("Skin_" + i.ToString() + "grid", colorToHex(skins[i].grid_color));
            dict.Add("Skin_" + i.ToString() + "connectors", colorToHex(skins[i].connectors_color));
            dict.Add("Skin_" + i.ToString() + "pText", colorToHex(skins[i].playerText));
            dict.Add("Skin_" + i.ToString() + "nText", colorToHex(skins[i].npcText));
            dict.Add("Skin_" + i.ToString() + "aText", colorToHex(skins[i].actionText));
            dict.Add("Skin_" + i.ToString() + "pText2", colorToHex(skins[i].playerText2));
            dict.Add("Skin_" + i.ToString() + "nText2", colorToHex(skins[i].npcText2));
            dict.Add("Skin_" + i.ToString() + "aText2", colorToHex(skins[i].actionText2));
        }

        SerializeHelper.WriteSettings(dict as Dictionary<string, object>, "EditorSettings" + ".json");
    }

    public void loadEditorSettings()
    {
        if (!File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/" + "EditorSettings" + ".json"))
            return;

        Dictionary<string, object> dict = SerializeHelper.ReadSettings("EditorSettings" + ".json") as Dictionary<string, object>;
        if (dict.ContainsKey("db.currentDiagEdited"))
        {
            db.currentDiag = (int)((long)dict["db.currentDiagEdited"]);
            db.fileIndex = db.currentDiag;
        }
        else
        {
            db.currentDiag = 0;
            db.fileIndex = 0;
        }

        if (dict.ContainsKey("db.curSkin"))
        {
            db.skinIndex = (int)((long)dict["db.curSkin"]);
        }

        //Load skin colors
        if (dict.ContainsKey("colors"))
        {
            List<VIDE_Editor_Skin.Skin> skins = VIDE_Editor_Skin.instance.skins;
            for (int i = 0; i < skins.Count; i++)
            {
                skins[i].player_NodeColor = hexToColor((string)dict["Skin_" + i.ToString() + "pNodeColor"]);
                skins[i].player_NodeColorSecondary = hexToColor((string)dict["Skin_" + i.ToString() + "pNodeColor2"]);
                skins[i].npc_NodeColor = hexToColor((string)dict["Skin_" + i.ToString() + "nNodeColor"]);
                skins[i].npc_NodeColorSecondary = hexToColor((string)dict["Skin_" + i.ToString() + "nNodeColor2"]);
                skins[i].action_NodeColor = hexToColor((string)dict["Skin_" + i.ToString() + "aNodeColor"]);
                skins[i].action_NodeColorSecondary = hexToColor((string)dict["Skin_" + i.ToString() + "aNodeColor2"]);
                skins[i].background_color = hexToColor((string)dict["Skin_" + i.ToString() + "back"]);
                skins[i].grid_color = hexToColor((string)dict["Skin_" + i.ToString() + "grid"]);
                skins[i].connectors_color = hexToColor((string)dict["Skin_" + i.ToString() + "connectors"]);
                skins[i].playerText = hexToColor((string)dict["Skin_" + i.ToString() + "pText"]);
                skins[i].npcText = hexToColor((string)dict["Skin_" + i.ToString() + "nText"]);
                skins[i].actionText = hexToColor((string)dict["Skin_" + i.ToString() + "aText"]);
                skins[i].playerText2 = hexToColor((string)dict["Skin_" + i.ToString() + "pText2"]);
                skins[i].npcText2 = hexToColor((string)dict["Skin_" + i.ToString() + "nText2"]);
                skins[i].actionText2 = hexToColor((string)dict["Skin_" + i.ToString() + "aText2"]);
            }
        }


    }


    //Loads from dialogues
    public void Load(bool clear)
    {
        localizationMenu = false;
        updateNodesRectsOnce = true;

        if (clear)
        {
            db.playerDiags = new List<VIDE_EditorDB.DialogueNode>();
            db.actionNodes = new List<VIDE_EditorDB.ActionNode>();
        }

        if (saveNames.Count < 1)
            return;

        if (db.currentDiag >= saveNames.Count)
        {
            Debug.LogError("Dialogue file not found! Loading default.");
            db.currentDiag = 0;
        }

        if (db.currentDiag < 0) db.currentDiag = 0;
        int currentName = -1;

        /* Get file location based on name */
        for (int i = 0; i < saveNames.Count; i++)
        {
            if (saveNamesFull[i].Contains(saveNames[db.currentDiag] + ".json"))
                currentName = i;
        }

        if (currentName == -1)
        {
            return;
        }

        //if (!File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Dialogues/" + saveNames[db.currentDiag] + ".json"))
        if (!File.Exists(Application.dataPath + "/../" + saveNamesFull[currentName]))
        {
            return;
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>("");
        AudioClip[] audios = Resources.LoadAll<AudioClip>("");
        List<string> spriteNames = new List<string>();
        List<string> audioNames = new List<string>();
        foreach (Sprite t in sprites)
            spriteNames.Add(t.name);
        foreach (AudioClip t in audios)
            audioNames.Add(t.name);

        Dictionary<string, object> dict = SerializeHelper.ReadFromFile(saveNamesFull[currentName]) as Dictionary<string, object>;

        int pDiags = (int)((long)dict["playerDiags"]);
        int nDiags = 0;
        if (dict.ContainsKey("npcDiags"))
            nDiags = (int)((long)dict["npcDiags"]);

        if (dict.ContainsKey("dID"))
            db.currentID = ((int)((long)dict["dID"]));

        int aDiags = 0;
        if (dict.ContainsKey("actionNodes")) aDiags = (int)((long)dict["actionNodes"]);

        db.startID = (int)((long)dict["startPoint"]);
        if (dict.ContainsKey("loadTag"))
            db.loadTag = (string)dict["loadTag"];

        if (dict.ContainsKey("previewPanning"))
            db.previewPanning = (bool)dict["previewPanning"];

        if (dict.ContainsKey("autosave"))
            db.autosave = (bool)dict["autosave"];

        if (dict.ContainsKey("locEdit"))
            db.locEdit = (bool)dict["locEdit"];

        VIDE_Localization.isEnabled = db.locEdit;

        if (dict.ContainsKey("showSettings"))
        {
            showSettings = (bool)dict["showSettings"];
            startDiag.height = 10;
        }

        //Create first...
        for (int i = 0; i < pDiags; i++)
        {
            string tagt = "";

            if (dict.ContainsKey("pd_pTag_" + i.ToString()))
                tagt = (string)dict["pd_pTag_" + i.ToString()];

            string k = "pd_rect_" + i.ToString();
            List<object> rect = (List<object>)(dict[k]);
            addSet(new Vector2((float)((long)rect[0]), (float)((long)rect[1])),
                (int)((long)dict["pd_comSize_" + i.ToString()]),
                (int)((long)dict["pd_ID_" + i.ToString()]),
                tagt,
                false
                );


            if (dict.ContainsKey("pd_isp_" + i.ToString()))
                db.playerDiags[db.playerDiags.Count - 1].isPlayer = (bool)dict["pd_isp_" + i.ToString()];
            else
                db.playerDiags[db.playerDiags.Count - 1].isPlayer = true;

            if (dict.ContainsKey("pd_sprite_" + i.ToString()))
            {
                string name = Path.GetFileNameWithoutExtension((string)dict["pd_sprite_" + i.ToString()]);
                if (spriteNames.Contains(name))
                    db.playerDiags[db.playerDiags.Count - 1].sprite = sprites[spriteNames.IndexOf(name)];
                else if (name != string.Empty)
                    Debug.LogError("'" + name + "' not found in any Resources folder!");
            }

            if (dict.ContainsKey("pd_expand_" + i.ToString()))
                db.playerDiags[db.playerDiags.Count - 1].expand = (bool)dict["pd_expand_" + i.ToString()];

            if (dict.ContainsKey("pd_vars" + i.ToString()))
            {
                for (int v = 0; v < (int)(long)dict["pd_vars" + i.ToString()]; v++)
                {
                    db.playerDiags[db.playerDiags.Count - 1].vars.Add((string)dict["pd_var_" + i.ToString() + "_" + v.ToString()]);
                    db.playerDiags[db.playerDiags.Count - 1].varKeys.Add((string)dict["pd_varKey_" + i.ToString() + "_" + v.ToString()]);
                }
            }

        }

        int npcIndexStart = db.playerDiags.Count;

        for (int i = 0; i < nDiags; i++)
        {
            string k = "nd_rect_" + i.ToString();
            List<object> rect = (List<object>)(dict[k]);

            string tagt = "";

            if (dict.ContainsKey("nd_tag_" + i.ToString()))
                tagt = (string)dict["nd_tag_" + i.ToString()];

            db.playerDiags.Add(new VIDE_EditorDB.DialogueNode());
            var npc = db.playerDiags[db.playerDiags.Count - 1];
            var v2 = new Vector2(((long)rect[0]), (long)(rect[1]));
            npc.rect = new Rect(v2.x, v2.y, 300, 50);
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
                npc.comment.Add(new VIDE_EditorDB.Comment());
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

            if (dict.ContainsKey("nd_expand_" + i.ToString()))
                npc.expand = (bool)dict["nd_expand_" + i.ToString()];

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
            string k = "ac_rect_" + i.ToString();
            List<object> rect = (List<object>)(dict[k]);
            float pFloat;
            var pfl = dict["ac_pFloat_" + i.ToString()];
            if (pfl.GetType() == typeof(System.Double))
                pFloat = System.Convert.ToSingle(pfl);
            else
                pFloat = (float)(long)pfl;


            db.actionNodes.Add(new VIDE_EditorDB.ActionNode(
                new Vector2((float)((long)rect[0]), (float)((long)rect[1])),
                (int)((long)dict["ac_ID_" + i.ToString()]),
                (string)dict["ac_meth_" + i.ToString()],
                (string)dict["ac_goName_" + i.ToString()],
                (bool)dict["ac_pause_" + i.ToString()],
                (bool)dict["ac_pBool_" + i.ToString()],
                (string)dict["ac_pString_" + i.ToString()],
                (int)((long)dict["ac_pInt_" + i.ToString()]),
                pFloat
                ));

            db.actionNodes[db.actionNodes.Count - 1].nameIndex = (int)((long)dict["ac_nIndex_" + i.ToString()]);

            if (dict.ContainsKey("ac_ovrStartNode_" + i.ToString()))
                db.actionNodes[db.actionNodes.Count - 1].ovrStartNode = (int)((long)dict["ac_ovrStartNode_" + i.ToString()]);

            if (dict.ContainsKey("ac_renameDialogue_" + i.ToString()))
                db.actionNodes[db.actionNodes.Count - 1].renameDialogue = (string)dict["ac_renameDialogue_" + i.ToString()];

            if (dict.ContainsKey("ac_more_" + i.ToString()))
                db.actionNodes[db.actionNodes.Count - 1].more = (bool)dict["ac_more_" + i.ToString()];

            if (dict.ContainsKey("ac_goto_" + i.ToString()))
                db.actionNodes[db.actionNodes.Count - 1].gotoNode = (int)((long)dict["ac_goto_" + i.ToString()]);



            List<string> opts = new List<string>();
            List<string> nameOpts = new List<string>();

            for (int ii = 0; ii < (int)((long)dict["ac_optsCount_" + i.ToString()]); ii++)
                opts.Add((string)dict["ac_opts_" + ii.ToString() + "_" + i.ToString()]);

            for (int ii = 0; ii < (int)((long)dict["ac_namesCount_" + i.ToString()]); ii++)
                nameOpts.Add((string)dict["ac_names_" + ii.ToString() + "_" + i.ToString()]);

            db.actionNodes[db.actionNodes.Count - 1].opts = opts.ToArray();
            db.actionNodes[db.actionNodes.Count - 1].nameOpts = nameOpts;

            int dc = (int)((long)dict["ac_methCount_" + i.ToString()]);

            for (int ii = 0; ii < dc; ii++)
            {
                db.actionNodes[db.actionNodes.Count - 1].methods.Add(
                    (string)dict["ac_meth_key_" + i.ToString() + "_" + ii.ToString()],
                    (string)dict["ac_meth_val_" + i.ToString() + "_" + ii.ToString()]
                    );
            }


        }

        //Connect now...
        for (int i = 0; i < db.playerDiags.Count - nDiags; i++)
        {
            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "text"))
                    db.playerDiags[i].comment[ii].text = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "text"];

                if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "visible"))
                    db.playerDiags[i].comment[ii].visible = (bool)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "visible"];

                if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "showmore"))
                    db.playerDiags[i].comment[ii].showmore = (bool)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "showmore"];

                if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"))
                {
                    string name = Path.GetFileNameWithoutExtension((string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"]);

                    if (spriteNames.Contains(name))
                        db.playerDiags[i].comment[ii].sprites = sprites[spriteNames.IndexOf(name)];
                    else if (name != "")
                        Debug.LogError("'" + name + "' not found in any Resources folder!");
                }

                if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"))
                {
                    string name = Path.GetFileNameWithoutExtension((string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"]);

                    if (audioNames.Contains(name))
                        db.playerDiags[i].comment[ii].audios = audios[audioNames.IndexOf(name)];
                    else if (name != "")
                        Debug.LogError("'" + name + "' not found in any Resources folder!");
                }

                if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "extraD"))
                    db.playerDiags[i].comment[ii].extraData = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "extraD"];

                int index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "iSet"]);

                if (index != -1)
                    db.playerDiags[i].comment[ii].inputSet = db.playerDiags[index];

                index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAns"]);

                if (index != -1)
                {
                    if (nDiags > 0)
                    {
                        db.playerDiags[i].comment[ii].outNode = db.playerDiags[(db.playerDiags.Count - nDiags) + index];
                    }
                    else
                    {
                        db.playerDiags[i].comment[ii].outNode = db.playerDiags[index];
                    }
                }

                index = -1;
                if (dict.ContainsKey("pd_" + i.ToString() + "_com_" + ii.ToString() + "oAct"))
                    index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAct"]);

                if (index != -1)
                    db.playerDiags[i].comment[ii].outAction = db.actionNodes[index];
            }
        }
        for (int i = npcIndexStart; i < db.playerDiags.Count; i++)
        {
            int x = i - npcIndexStart;
            int index = -1;
            if (dict.ContainsKey("nd_oSet_" + x.ToString()))
                index = (int)((long)dict["nd_oSet_" + x.ToString()]);

            if (index != -1)
                db.playerDiags[i].comment[0].outNode = db.playerDiags[index];

            if (dict.ContainsKey("nd_oAct_" + x.ToString()))
            {
                index = -1;
                index = (int)((long)dict["nd_oAct_" + x.ToString()]);
                if (index != -1)
                    db.playerDiags[i].comment[0].outAction = db.actionNodes[index];
            }
        }
        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            db.actionNodes[i].paramType = (int)((long)dict["ac_paramT_" + i.ToString()]);
            db.actionNodes[i].methodIndex = (int)((long)dict["ac_methIndex_" + i.ToString()]);

            int index = -1;
            index = (int)((long)dict["ac_oSet_" + i.ToString()]);

            if (index != -1)
                db.actionNodes[i].outPlayer = db.playerDiags[index];

            if (dict.ContainsKey("ac_oAct_" + i.ToString()))
            {
                index = -1;
                index = (int)((long)dict["ac_oAct_" + i.ToString()]);
                if (index != -1)
                    db.actionNodes[i].outAction = db.actionNodes[index];
            }
        }

        //Listen to localization events
        if (saveNames.Count > 0 && VIDE_Localization.isEnabled)
        {
            VIDE_Localization.LoadLanguages(saveNames[db.currentDiag], false);
            LoadLocalized();
        }

        db.pNode = null;
        db.aNode = null;
        db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
        repaintLines = true;
        UpdateTagList();
        Repaint();
    }

    //Refreshes file list
    public void loadFiles(int focused)
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        saveNames = new List<string>();
        saveNamesFull = new List<string>();
        db.currentDiag = focused;
        foreach (TextAsset f in files)
        {
            saveNames.Add(f.name);
            saveNamesFull.Add(AssetDatabase.GetAssetPath(f));
        }
        saveNames.Sort();
    }

    #endregion

    //TOOLBAR
    void DrawToolbar()
    {

        //Current Dialogue

        GUI.enabled = editEnabled;
        GUIStyle titleSt = new GUIStyle(GUI.skin.GetStyle("Label"));
        titleSt.fontStyle = FontStyle.Bold;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Editing: ", EditorStyles.label, GUILayout.Width(55));
        int t_file = db.fileIndex;
        if (GUILayout.Button(">", EditorStyles.toolbarButton))
        {
            assignMenu = false;
            localizationMenu = false;
            spyView = false;
            editingColors = false;
            searchingForDialogue = !searchingForDialogue;
            searchWord = "";
            Repaint();
            //return;
        }

        if (Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp)
        {
            if (searchingForDialogue)
            {
                searchingForDialogue = false;
                searchWord = "";
                Repaint();
            }
        }

        if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp)
        {
            if (searchingForDialogue && searchWord != "")
            {
                for (int i = 0; i < saveNames.Count; i++)
                {
                    if (saveNames[i].ToLower().Contains(searchWord.ToLower()))
                    {
                        Undo.RecordObject(db, "Loaded dialogue");
                        db.fileIndex = i;
                        if (playerReady)
                        {
                            Save();
                        }
                        searchingForDialogue = false;
                        spyView = false;
                        editingColors = false;
                        db.currentDiag = db.fileIndex;
                        saveEditorSettings(db.currentDiag);
                        Load(true);
                        CenterAll(false, db.startID, true);
                        Repaint();
                        break;
                        //return;
                    }
                }
            }
        }

        if (saveNames.Count > 0)
        {
            GUIStyle gs = new GUIStyle(EditorStyles.toolbarPopup);
            gs.fontStyle = FontStyle.Bold;

            if (searchingForDialogue)
            {
                GUI.SetNextControlName("filterSearch");
                searchWord = GUILayout.TextField(searchWord, GUILayout.Width(150));
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                int fileIndexTEMP = EditorGUILayout.Popup(db.fileIndex, saveNames.ToArray(), gs, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(db, "Changed Dialogue");
                    db.fileIndex = fileIndexTEMP;

                    if (t_file != db.fileIndex)
                    {
                        if (playerReady)
                        {
                            Save();
                        }
                        editingColors = false;
                        spyView = false;
                        db.currentDiag = db.fileIndex;
                        saveEditorSettings(db.currentDiag);
                        Load(true);
                        CenterAll(false, db.startID, true);
                        return;
                    }
                }
            }

        }

        if (searchingForDialogue) GUI.enabled = false;


        //Add new
        if (GUILayout.Button("Add new dialogue", EditorStyles.toolbarButton))
        {
            editEnabled = false;
            spyView = false;
            editingColors = false;
            showHelp = false;
            newFile = true;
            GUI.FocusWindow(99998);
        }

        //Delete
        if (saveNames.Count > 0)
        {
            if (GUILayout.Button("Delete current", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                spyView = false;
                editingColors = false;
                editEnabled = false;
                showHelp = false;
                deletePopup = true;
            }
            GUIStyle bb = new GUIStyle(GUI.skin.label);
            bb.fontStyle = FontStyle.Bold;
            bb.normal.textColor = Color.red;

            if (showError)
                GUI.enabled = false;

            if (GUILayout.Button("Assign", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                localizationMenu = false;
                searchingForDialogue = false;
                editingColors = false;
                spyView = false;
                showHelp = false;
                assignMenu = !assignMenu;
            }

            GUI.color = defaultColor;

            if (GUILayout.Button("Mini Map", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                localizationMenu = false;
                searchingForDialogue = false;
                assignMenu = false;
                showHelp = false;
                spyView = !spyView;
                Repaint();
            }


            if (needSave) GUI.color = Color.yellow;
            if (GUILayout.Button("SAVE", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Save();
                AssetDatabase.Refresh();

                needSave = false;
                newFileName = "My Dialogue";
                editEnabled = true;
                overwritePopup = false;
                newFile = false;
                errorMsg = "";
                saveEditorSettings(db.currentDiag);

            }
            GUI.color = defaultColor;

            GUILayout.Label("Autosave", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            bool auto = GUILayout.Toggle(db.autosave, "");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set Autosave");
                db.autosave = auto;
                needSave = true;
            }
            GUI.enabled = true;

            if (!hasID) { GUI.color = Color.red; }
            GUILayout.Label("Start Node ID: ", EditorStyles.miniLabel);

            EditorGUI.BeginChangeCheck();
            int sid = EditorGUILayout.IntField(db.startID, EditorStyles.toolbarTextField, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set Start ID");
                db.startID = sid;
                needSave = true;
            }

            GUI.color = defaultColor;
            /*GUILayout.Label("Load Tag: ", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            string lt = EditorGUILayout.TextField(db.loadTag, EditorStyles.toolbarTextField, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set Load Tag");
                db.loadTag = lt;
                needSave = true;

            }
            GUI.color = defaultColor;*/
        }

        GUILayout.EndHorizontal();
        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUIStyle s = new GUIStyle(EditorStyles.toolbarButton);
        s.alignment = TextAnchor.MiddleRight;
        s.fontStyle = FontStyle.Bold;
        if (GUILayout.Button(newsHeadline, s, GUILayout.MinWidth(30)))
        {
            if (newsHeadlineLink.Length > 0)
            {
                Application.OpenURL(newsHeadlineLink);
                EditorGUIUtility.ExitGUI();
            }
        }
        GUI.color = defaultColor;

        if (GUILayout.Button("Help", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            localizationMenu = false;
            assignMenu = false;
            searchingForDialogue = false;
            editingColors = false;
            spyView = false;
            showHelp = !showHelp;
        }

    }

    void DrawToolbar2()
    {
        //Current Dialogue
        if (saveNames.Count > 0)
            GUI.enabled = true;

        GUI.enabled = editEnabled;
        GUI.enabled = !searchingForDialogue;
        GUIStyle titleSt = new GUIStyle(GUI.skin.GetStyle("Label"));
        titleSt.fontStyle = FontStyle.Bold;

        if (saveNames.Count > 0)
        {

            GUILayout.Label("Add nodes: ", EditorStyles.label);
            GUILayout.BeginHorizontal();

            // ADD NEW BUTTONS
            Rect lr;

            if (dragNewNode == 1)
                GUILayout.Box("", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(20));
            else
                GUILayout.Box(newNodeIcon, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(20));
            lr = GUILayoutUtility.GetLastRect();
            if (editEnabled && lr.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
                dragNewNode = 1;
            }

            if (dragNewNode == 2)
                GUILayout.Box("", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(20));
            else
                GUILayout.Box(newNodeIcon3, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(20));
            lr = GUILayoutUtility.GetLastRect();
            if (editEnabled && lr.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
                dragNewNode = 2;
            }

            GUILayout.EndHorizontal();

            GUI.color = defaultColor;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Focus Next ", EditorStyles.toolbarButton))
            {
                CenterAll(true, -1, false);
                Repaint();
            }
            if (GUILayout.Button("Go to:", EditorStyles.toolbarButton))
            {
                CenterAll(false, gotofocus, false);
                Repaint();
            }
            gotofocus = EditorGUILayout.IntField(gotofocus, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            bool pp = GUILayout.Toggle(db.previewPanning, "Perf. panning");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Changed preview panning");
                db.previewPanning = pp;
                needSave = true;

            }

        }

        string loca = "Localization: None";
        if (VIDE_Localization.isEnabled)
            if (VIDE_Localization.currentLanguage != null)
                loca = "Localization: " + VIDE_Localization.currentLanguage.name;

        if (GUILayout.Button(loca, EditorStyles.toolbarButton))
        {
            assignMenu = false;
            searchingForDialogue = false;
            localizationMenu = !localizationMenu;

            Repaint();
        }

        GUILayout.FlexibleSpace();

        if (localizationMenu) GUI.enabled = false;

        if (GUILayout.Button("Imp. TXT", EditorStyles.toolbarButton)) 
        {
            ImportTxtFile();
            Repaint();
        }

        if (GUILayout.Button("Exp. TXT", EditorStyles.toolbarButton))
        {
            ExportTxtFile();
            Repaint();
        }

        GUI.enabled = true;

        EditorGUI.BeginChangeCheck();
        int fileIndexTEMP = EditorGUILayout.Popup(db.skinIndex, VIDE_Editor_Skin.GetNames(), GUILayout.Width(100));
        if (EditorGUI.EndChangeCheck())
        {
            //Undo.RecordObject(db, "Set skin");
            db.skinIndex = fileIndexTEMP;
            editingColors = true;
            GUIUtility.keyboardControl = 0;
        }

        GUI.enabled = true;


        //GUILayout.Label("VIDE 2.2", EditorStyles.miniLabel);
        if (GUILayout.Button(twitIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
        {
            Application.OpenURL("https://twitter.com/VIDEDialogues");
            EditorGUIUtility.ExitGUI();
        }

    }

    void SearchDialogue()
    {
        GUILayout.Space(10);
        searchScrollView = GUILayout.BeginScrollView(searchScrollView, GUILayout.Height(position.height - 50));
        for (int i = 0; i < saveNames.Count; i++)
        {
            if (saveNames[i].ToLower().Contains(searchWord.ToLower()))
                if (GUILayout.Button(saveNames[i], EditorStyles.toolbarButton))
                {
                    db.fileIndex = i;
                    if (playerReady)
                    {
                        Save();
                    }
                    searchingForDialogue = false;
                    db.currentDiag = db.fileIndex;
                    saveEditorSettings(db.currentDiag);
                    Load(true);
                    CenterAll(false, db.startID, true);
                    return;
                }
        }
        GUILayout.EndScrollView();


    }

    void AssignMenu()
    {
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
        {
            assignMenu = false;
            Repaint();
        }
        assignScroll = GUILayout.BeginScrollView(assignScroll, GUILayout.Height(position.height - 50));

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Select gameobject to assign current dialogue to.");
        GUILayout.Label("A VIDE_Assign component will be added to it if none is found.");

        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Box("No VIDE_Assign", EditorStyles.toolbarButton);
        Color c = Color.green;
        c.r += 0.7f;
        c.g += 0.7f;
        c.b += 0.7f;
        c.a = 1;
        GUI.color = c;
        GUILayout.Box("Has VIDE_Assign", EditorStyles.toolbarButton);

        c = Color.blue;
        c.r += 0.7f;
        c.g += 0.7f;
        c.b += 0.7f;
        c.a = 1;
        GUI.color = c;
        GUILayout.Box("Already assigned", EditorStyles.toolbarButton);


        GUI.color = defaultColor;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Label("Search: ");
        assignMenuFilter = GUILayout.TextField(assignMenuFilter, GUILayout.Width(200));

        GUILayout.Space(20);


        GameObject[] gos = Resources.FindObjectsOfTypeAll<GameObject>().OrderBy(go => go.name).ToArray();

        for (int i = 0; i < gos.Length; i++)
        {
            if (!gos[i].name.ToLower().Contains(assignMenuFilter.ToLower()))
                continue;

            if (gos[i].hideFlags != HideFlags.None)
                continue;

            if (gos[i].activeInHierarchy)
            {
                GUI.color = defaultColor;


                if (gos[i].GetComponent<VIDE_Assign>() != null)
                {
                    c = Color.green;
                    c.r += 0.7f;
                    c.g += 0.7f;
                    c.b += 0.7f;
                    c.a = 1;
                    GUI.color = c;

                    if (gos[i].GetComponent<VIDE_Assign>().assignedDialogue == saveNames[db.currentDiag])
                    {
                        c = Color.blue;
                        c.r += 0.7f;
                        c.g += 0.7f;
                        c.b += 0.7f;
                        c.a = 1;
                        GUI.color = c;
                    }
                }

                if (GUILayout.Button(gos[i].name, EditorStyles.toolbarButton))
                {
                    Undo.RecordObject(gos[i], "Assigned");
                    if (gos[i].GetComponent<VIDE_Assign>() == null)
                    {
                        gos[i].AddComponent<VIDE_Assign>();
                    }
                    Selection.activeGameObject = gos[i];
                    Repaint();
                    gos[i].GetComponent<VIDE_Assign>().assignedDialogue = saveNames[db.currentDiag];
                    gos[i].GetComponent<VIDE_Assign>().assignedIndex = db.currentDiag;
                    gos[i].GetComponent<VIDE_Assign>().assignedID = db.currentID;
                    Repaint();
                }
            }
        }
        GUILayout.Space(20);
        GUI.color = defaultColor;

        string show = "Show More";
        if (assignMenuShowMore) show = "Show Less"; else show = "Show More";

        if (GUILayout.Button(show, GUILayout.Height(30)))
        {
            assignMenuShowMore = !assignMenuShowMore;
        }

        if (assignMenuShowMore)
            for (int i = 0; i < gos.Length; i++)
            {
                if (!gos[i].name.ToLower().Contains(assignMenuFilter.ToLower()))
                    continue;

                if (gos[i].hideFlags != HideFlags.None)
                    continue;

                if (!gos[i].activeInHierarchy)
                {
                    GUI.color = defaultColor;


                    if (gos[i].GetComponent<VIDE_Assign>() != null)
                    {
                        c = Color.green;
                        c.r += 0.7f;
                        c.g += 0.7f;
                        c.b += 0.7f;
                        c.a = 1;
                        GUI.color = c;

                        if (gos[i].GetComponent<VIDE_Assign>().assignedDialogue == saveNames[db.currentDiag])
                        {
                            c = Color.blue;
                            c.r += 0.7f;
                            c.g += 0.7f;
                            c.b += 0.7f;
                            c.a = 1;
                            GUI.color = c;
                        }
                    }

                    if (GUILayout.Button(gos[i].name, EditorStyles.toolbarButton))
                    {

                        Undo.RecordObject(gos[i], "Assigned");
                        if (gos[i].GetComponent<VIDE_Assign>() == null)
                        {
                            gos[i].AddComponent<VIDE_Assign>();
                        }
                        Selection.activeGameObject = gos[i];
                        Repaint();
                        gos[i].GetComponent<VIDE_Assign>().assignedDialogue = saveNames[db.currentDiag];
                        gos[i].GetComponent<VIDE_Assign>().assignedIndex = db.currentDiag;
                        gos[i].GetComponent<VIDE_Assign>().assignedID = db.currentID;
                        Repaint();
                    }
                }
            }
        GUILayout.EndScrollView();

        GUI.color = defaultColor;

    }

    void LocalizationMenu()
    {
        if (deletingLanguage != -1)
        {
            return;
        }

        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
        {
            localizationMenu = false;
            Repaint();
        }

        GUILayout.Space(20);

        string txt = "Enable Localization";
        if (VIDE_Localization.enabledInGame)
        {
            txt = "Disable Localization";
            GUI.color = Color.green;
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(txt, GUILayout.ExpandWidth(true), GUILayout.Height(30), GUILayout.Width(position.width / 4)))
        {
            VIDE_Localization.enabledInGame = !VIDE_Localization.enabledInGame;

            if (VIDE_Localization.enabledInGame)
            {
                VIDE_Localization.LoadLanguages(saveNames[db.currentDiag], false);
            }

            VIDE_Localization.SaveSettings();
            Repaint();
        }
        GUI.color = defaultColor;
        string txt2 = "Enable Edit";
        if (VIDE_Localization.isEnabled)
        {
            txt2 = "Disable Edit";
            GUI.color = Color.green;
        }

        if (GUILayout.Button(txt2, GUILayout.ExpandWidth(true), GUILayout.Height(30), GUILayout.Width(position.width / 4)))
        {
            VIDE_Localization.isEnabled = !VIDE_Localization.isEnabled;

            if (VIDE_Localization.isEnabled)
            {
                VIDE_Localization.LoadLanguages(saveNames[db.currentDiag], false);
                LoadLocalized();
                VIDE_Localization.SaveSettings();
            }
            else
            {
                VIDE_Localization.currentLanguage = VIDE_Localization.defaultLanguage;
                VIDE_Localization.SaveSettings();
                LoadLocalized();
                Repaint();
            }

            Repaint();
        }
        GUI.color = defaultColor;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();



        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (!VIDE_Localization.enabledInGame)
        {
            EditorGUILayout.HelpBox("When disabled, only default language will be loaded while in-game.", MessageType.Info);
        }
        if (!VIDE_Localization.isEnabled)
        {
            EditorGUILayout.HelpBox("Enable Edit to localize. When disabled, no localization data will be saved or stored in memory.", MessageType.Info);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (!VIDE_Localization.isEnabled)
            return;


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("When disabling Edit, no saved localization data will be lost. Also, will reset nodes to default language.\n'Default' has effect during runtime. 'Current' is for Editor, the one you are currently localizing. System will autosave changes when switching 'Current'.", MessageType.Info);

        EditorGUILayout.HelpBox("Deleting a language is NOT undoable.", MessageType.Warning);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginArea(new Rect(0, 200, position.width / 2, position.height / 2), GUI.skin.box);

        if (VIDE_Localization.currentLanguage != null)
            GUILayout.Box("Current Language: " + VIDE_Localization.currentLanguage.name, GUILayout.ExpandWidth(true));
        languageScrollArea = GUILayout.BeginScrollView(languageScrollArea, GUILayout.Width(position.width / 2.02f), GUILayout.Height((position.height / 2) - 30));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Languages");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Add New"))
        {
            SaveToLanguage();
            VIDE_Localization.SaveSettings();
            LoadLocalized();
            selLang = VIDE_Localization.AddLang();
            Repaint();
        }
        GUILayout.Space(10);

        for (int i = 0; i < VIDE_Localization.languages.Count; i++)
        {
            GUILayout.BeginHorizontal();

            if (VIDE_Localization.languages[i] == VIDE_Localization.defaultLanguage) GUI.enabled = false;

            if (GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                deletingLanguage = i;
                return;

            }
            GUI.enabled = true;


            string langName = VIDE_Localization.languages[i].name;

            if (VIDE_Localization.languages[i].selected)
                GUI.color = new Color(0.7f, 0.7f, 0.95f, 1);
            else
                GUI.color = defaultColor;

            if (GUILayout.Button(langName, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true)))
            {
                if (VIDE_Localization.languages[i].selected)
                {
                    VIDE_Localization.SaveSettings();

                    VIDE_Localization.DeselectAll();
                    selLang = null;
                }
                else
                {
                    VIDE_Localization.SaveSettings();
                    VIDE_Localization.DeselectAll();
                    selLang = VIDE_Localization.languages[i];
                    VIDE_Localization.languages[i].selected = true;
                    Repaint();
                }
            }
            GUI.color = defaultColor;

            if (VIDE_Localization.defaultLanguage == VIDE_Localization.languages[i]) GUI.color = Color.green;

            if (GUILayout.Button("Default", EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                VIDE_Localization.defaultLanguage = VIDE_Localization.languages[i];
            }
            GUI.color = defaultColor;

            if (VIDE_Localization.currentLanguage == VIDE_Localization.languages[i]) GUI.color = new Color(0.7f, 0.7f, 0.95f, 1);

            if (GUILayout.Button("Current", EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                SaveToLanguage();
                VIDE_Localization.currentLanguage = VIDE_Localization.languages[i];
                VIDE_Localization.SaveSettings();
                LoadLocalized();
                Repaint();
            }
            GUI.color = defaultColor;

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUI.color = defaultColor;

        GUILayout.BeginArea(new Rect(position.width / 2, 200, position.width / 2, position.height / 2), GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Language Settings");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (selLang != null)
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Name: ");
            selLang.name = EditorGUILayout.TextField(selLang.name, GUILayout.Width(200));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUI.color = Color.cyan;

            if (!copyFromDefSure)
            {
                if (GUILayout.Button("Copy Localization From Default"))
                {
                    copyFromDefSure = true;
                }
            }
            else
            {
                GUI.color = Color.yellow;

                if (GUILayout.Button("Are you sure? Click again to confirm"))
                {
                    Undo.RecordObject(db, "Copied Localization");
                    LoadLocalizedFromDefault();
                    localizationMenu = false;
                    copyFromDefSure = false;
                    Repaint();
                }

                if (Event.current.type == EventType.MouseUp)
                {
                    copyFromDefSure = false;
                    Repaint();
                }
            }
            GUI.color = defaultColor;
            GUI.enabled = true;

        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(0, position.height - 20, position.width, 20), GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Version 0.1");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();


    }

    private static string UppercaseString(string inputString)
    {
        return inputString.ToUpper();
    }

    Rect mapBounds = new Rect();
    float mapDivider = 2;
    List<Rect> mapRects = new List<Rect>();
    Texture2D mapTex;
    Rect lastWindowSize = new Rect();

    void CallSpyMap()
    {
        if (db.selectedNodes.Count > 0)
        {
            if (db.selectedNodes[0].aNode != null)
            {
                db.aNode = db.selectedNodes[0].aNode;
                db.aNodeID = db.aNode.ID;
                db.pNode = null;
                db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();

            }
            if (db.selectedNodes.Count > 0 && db.selectedNodes[0].dNode != null)
            {
                db.pNode = db.selectedNodes[0].dNode;
                db.pNodeID = db.pNode.ID;
                db.aNode = null;
                db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
            }

        }
        mapDivider = 2;
        mapRects = new List<Rect>();
        foreach (VIDE_EditorDB.DialogueNode d in db.playerDiags)
            mapRects.Add(d.rect);
        foreach (VIDE_EditorDB.ActionNode d in db.actionNodes)
            mapRects.Add(d.rect);
        mapBounds = GetBoundaries(mapRects.ToArray());
        while (mapDivider < 10 && Mathf.Abs(mapBounds.width / mapDivider) > position.width - 32)
        {
            mapDivider += 0.01f;
        }

        while (mapDivider < 10 && Mathf.Abs((mapBounds.height - 450) / mapDivider) > position.height - 450)
        {
            mapDivider += 0.01f;
        }

        repaintLines = true;

    }

    Rect GetBoundaries(Rect[] rects)
    {
        float minX = 0;
        float maxX = 0;
        float minY = 0;
        float maxY = 0;

        for (int i = 0; i < rects.Length; i++)
        {
            List<float> vals = new List<float>();
            foreach (Rect r in rects)
                vals.Add(r.x);
            minX = Mathf.Min(vals.ToArray());
            vals = new List<float>();

            foreach (Rect r in rects)
                vals.Add(r.y);
            minY = Mathf.Min(vals.ToArray());
            vals = new List<float>();

            foreach (Rect r in rects)
                vals.Add(r.x + r.width);
            maxX = Mathf.Max(vals.ToArray());
            vals = new List<float>();

            foreach (Rect r in rects)
                vals.Add(r.y + r.height);
            maxY = Mathf.Max(vals.ToArray());
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    void RefreshMiniMap()
    {
        mapDivider = 2;
        mapRects = new List<Rect>();
        foreach (VIDE_EditorDB.DialogueNode d in db.playerDiags)
            mapRects.Add(d.rect);
        foreach (VIDE_EditorDB.ActionNode d in db.actionNodes)
            mapRects.Add(d.rect);
        mapBounds = GetBoundaries(mapRects.ToArray());
        while (mapDivider < 10 && Mathf.Abs(mapBounds.width / mapDivider) > position.width - 32)
        {
            mapDivider += 0.01f;
        }

        while (mapDivider < 10 && Mathf.Abs((mapBounds.height - 450) / mapDivider) > position.height - 450)
        {
            mapDivider += 0.01f;
        }
    }

    VIDE_EditorDB.DialogueNode GetDNodeWithID(int ID)
    {

        foreach (VIDE_EditorDB.DialogueNode d in db.playerDiags)
        {
            if (d.ID == ID)
                return d;
        }
        return null;
    }
    VIDE_EditorDB.ActionNode GetANodeWithID(int ID)
    {

        foreach (VIDE_EditorDB.ActionNode d in db.actionNodes)
        {
            if (d.ID == ID)
                return d;
        }
        return null;
    }


    void DrawSpyView()
    {

        //ONTOPOFEVERYTHINGGRID
        GUI.BeginGroup(new Rect(0, 37, position.width, position.height - 37));
        DrawGrid();
        GUI.EndGroup();

        //UNDO UPDATE 
        if (Event.current.commandName == "UndoRedoPerformed")
        {
            CallSpyMap();
            if (db.pNode != null)
                db.pNode = GetDNodeWithID(db.pNodeID);
            if (db.aNode != null)
                db.aNode = GetANodeWithID(db.aNodeID);
            Repaint();
            GUIUtility.keyboardControl = -1;
            return;
        }

        if (lastWindowSize != position)
        {
            RefreshMiniMap();
            lastWindowSize = position;
        }

        //TITLES
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = VIDE_Editor_Skin.GetColor(9, db.skinIndex);
        titleStyle.fontSize = 10;
        GUI.Label(new Rect(16, 50, 300, 64), "Back (Esc/Space)", titleStyle);
        titleStyle.fontSize = 12;
        GUI.Label(new Rect(16, position.height - 250, 300, 64), "Editing: ", titleStyle);

        GUI.Label(new Rect(68, position.height - 250, 300, 64), saveNames[db.currentDiag], titleStyle);

        //INPUT EVENTS

        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
        {
            spyView = false;
            Repaint();
            return;
        }

        if (Event.current.type == EventType.MouseUp)
        {
            dragNewNode = 0;
        }


        //MINIMAP
        Rect groupRect = new Rect(0, 0, mapBounds.width / mapDivider, mapBounds.height / mapDivider);

        float difference = position.width - mapBounds.width / mapDivider;
        float differenceY = (position.height - 260) - mapBounds.height / mapDivider;
        groupRect.x += difference / 2;
        groupRect.y += (differenceY / 2);

        GUIStyle buttStyle = new GUIStyle(GUI.skin.button);
        buttStyle.fontSize = Mathf.RoundToInt(50 / mapDivider);
        buttStyle.fontStyle = FontStyle.Normal;

        GUI.BeginGroup(new Rect(0, 37, position.width, position.height - 37));

        GUI.BeginGroup(groupRect);
        for (int i = 0; i < mapRects.Count; i++)
        {
            float xAvr = 100 - (100 * ((mapBounds.x + mapBounds.width) - mapRects[i].x)) / (mapBounds.width);
            float yAvr = 100 - (100 * ((mapBounds.y + mapBounds.height) - mapRects[i].y)) / (mapBounds.height);

            Rect newPos = new Rect(((xAvr * mapBounds.width / mapDivider) / 100), ((yAvr * mapBounds.height / mapDivider) / 100), mapRects[i].width / mapDivider, mapRects[i].height / mapDivider);

            int ID = -1;
            Color theCol = Color.white;
            if (i < db.playerDiags.Count)
            {
                ID = db.playerDiags[i].ID;
                if (db.playerDiags[i].isPlayer) theCol = VIDE_Editor_Skin.GetColor(0, db.skinIndex);
                else theCol = VIDE_Editor_Skin.GetColor(2, db.skinIndex);
                if (db.pNode != null)
                    if (db.playerDiags[i].ID == db.pNode.ID) theCol = new Color(theCol.r + 0.2f, theCol.g + 0.2f, theCol.b + 0.2f, 1);
                DrawMapDialogueLine(db.playerDiags[i]);
                GUI.color = theCol;
                if (GUI.Button(newPos, "", VIDE_Editor_Skin.instance.windowStyle))
                {
                    if (db.pNode != null)
                        if (db.playerDiags[i].ID == db.pNode.ID)
                        {
                            spyView = false;
                            CenterAll(false, db.pNode.ID, false);
                            return;
                        }
                    Undo.RecordObject(db, "Selected MiniMap node");
                    areYouSure = false;
                    db.pNode = db.playerDiags[i];
                    db.pNodeID = db.pNode.ID;
                    db.aNode = null;
                    GUIUtility.keyboardControl = 0;
                }
                GUI.color = Color.white;

                GUIStyle lbst = text1st;
                lbst.alignment = TextAnchor.UpperLeft;
                lbst.fontSize = Mathf.RoundToInt(24 / mapDivider);

                //TAG             
                lbst.normal.textColor = (db.playerDiags[i].isPlayer) ? VIDE_Editor_Skin.GetColor(14, db.skinIndex) : VIDE_Editor_Skin.GetColor(15, db.skinIndex);

                GUI.Label(new Rect(newPos.x + 74 / mapDivider, newPos.y + 12 / mapDivider, newPos.width, newPos.height), db.playerDiags[i].playerTag, lbst);

                //EVs
                GUI.Label(new Rect(newPos.x + 16 / mapDivider, newPos.y + newPos.height - (50 / mapDivider), newPos.width, newPos.height), "x" + db.playerDiags[i].varKeys.Count.ToString() + " Extra Vars", lbst);
                //ID
                lbst.alignment = TextAnchor.UpperRight;
                GUI.Label(new Rect((newPos.x + newPos.width) - (110 / mapDivider), newPos.y + newPos.height - (50 / mapDivider), (86 / mapDivider), newPos.height), "ID: " + db.playerDiags[i].ID.ToString(), lbst);
                //COMMENT LINES
                for (int c = 0; c < db.playerDiags[i].comment.Count; c++)
                {
                    if (c == 0 || c == 1)
                        GUI.DrawTexture(new Rect(newPos.x + 75 / mapDivider, 37 / mapDivider + newPos.y + (16 / mapDivider * c), newPos.width - 100 / mapDivider, 8 / mapDivider), mapTex);
                    else
                        GUI.DrawTexture(new Rect(newPos.x + 32 / mapDivider, 37 / mapDivider + newPos.y + (16 / mapDivider * c), newPos.width - 58 / mapDivider, 8 / mapDivider), mapTex);

                }
                //SPRITE
                if (db.playerDiags[i].sprite != null)
                    GUI.DrawTexture(new Rect(newPos.x + 8, newPos.y + 7, 50 / mapDivider, 50 / mapDivider), db.playerDiags[i].sprite.texture);
                else
                    GUI.DrawTexture(new Rect(newPos.x + 8, newPos.y + 7, 50 / mapDivider, 50 / mapDivider), mapTex);

            }
            else
            {
                ID = db.actionNodes[i - db.playerDiags.Count].ID;
                theCol = VIDE_Editor_Skin.GetColor(4, db.skinIndex);
                DrawMapActionLine(db.actionNodes[i - db.playerDiags.Count]);
                if (db.aNode != null)
                    if (db.actionNodes[i - db.playerDiags.Count].ID == db.aNode.ID) theCol = new Color(theCol.r + 0.2f, theCol.g + 0.2f, theCol.b + 0.2f, 1);
                GUI.color = theCol;
                if (GUI.Button(newPos, "", VIDE_Editor_Skin.instance.windowStyle))
                {
                    if (db.aNode != null)
                        if (db.actionNodes[i - db.playerDiags.Count].ID == db.aNode.ID)
                        {
                            spyView = false;
                            CenterAll(false, db.aNode.ID, false);
                            return;
                        }
                    db.pNode = null;
                    Undo.RecordObject(db, "Selected MiniMap action node");
                    areYouSure = false;
                    db.aNode = db.actionNodes[i - db.playerDiags.Count];
                    db.aNodeID = db.aNode.ID;
                    GUIUtility.keyboardControl = 0;
                }

                GUI.color = Color.white;
                GUIStyle lbsta = text1st;
                lbsta.alignment = TextAnchor.UpperRight;
                lbsta.fontSize = Mathf.RoundToInt(24 / mapDivider);
                lbsta.normal.textColor = VIDE_Editor_Skin.GetColor(16, db.skinIndex);
                //ID
                GUI.Label(new Rect((newPos.x + newPos.width) - (110 / mapDivider), newPos.y + newPos.height - (50 / mapDivider), (86 / mapDivider), newPos.height), "ID: " + ID.ToString(), lbsta);
            }



        }
        GUI.EndGroup();
        GUI.EndGroup();

        //EDIT AREA
        Rect editRect = new Rect(16, position.height - 232, position.width - 32, 216);

        Color bc = Color.grey;
        bc.a = 0.5f;
        GUI.color = bc;
        GUI.BeginGroup(editRect, GUI.skin.box);
        GUI.color = Color.white;

        if (db.pNode != null)
        {
            DrawMMEditBar(editRect);
        }
        else if (db.aNode != null)
        {
            DrawMMActionBar(editRect);
        }
        else
        {
            GUI.Label(new Rect(10, 10, 300, 64), "Please select a node.", titleStyle);

        }

        GUI.EndGroup();


    }

    void DrawMMActionBar(Rect editRect)
    {
        GUIStyle boxBackst = new GUIStyle(GUI.skin.box);
        boxBackst.normal.background = mapTex;
        text1st.fontSize = 14;
        text1st.normal.textColor = Color.white;
        text1st.alignment = TextAnchor.MiddleCenter;

        GUI.BeginGroup(new Rect(0, 8, editRect.width, 32), boxBackst);
        GUI.Label(new Rect(0, 0, 80, 32), "ID: " + db.aNode.ID, text1st);
        //FOCUS
        if (GUI.Button(new Rect(88, 4, 64, 24), "Focus", txtComst))
        {
            spyView = false;
            CenterAll(false, db.aNode.ID, false);
        }
        //DELETE
        string delText = "Del";
        if (areYouSureIndex == db.actionNodes.IndexOf(db.aNode))
            if (areYouSure) { delText = "Sure?"; GUI.color = new Color32(176, 128, 54, 255); }
        if (GUI.Button(new Rect(editRect.width - 60, 4, 52, 24), delText, txtComst))
        {
            if (areYouSureIndex != db.actionNodes.IndexOf(db.aNode)) areYouSure = false;
            if (!areYouSure)
            {
                areYouSure = true;
                areYouSureIndex = db.actionNodes.IndexOf(db.aNode);
            }
            else
            {
                areYouSure = false;
                areYouSureIndex = 0;
                removeAction(db.actionNodes[db.actionNodes.IndexOf(db.aNode)]);
                needSave = true;
                db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
                CallSpyMap();
                db.aNode = null;
                db.aNodeID = -1;
                Repaint();
                return;
            }
        }
        GUI.color = Color.white;
        if (Event.current.type == EventType.MouseDown)
        {
            areYouSure = false;
            Repaint();
        }

        GUI.EndGroup();
        DrawMMActionBarContent(editRect);

    }
    void DrawMMActionBarContent(Rect er)
    {

        GUIStyle boxBackst = new GUIStyle(GUI.skin.box);
        boxBackst.normal.background = mapTex;

        GUIStyle text1st = new GUIStyle(GUI.skin.label);
        text1st.fontSize = 14;
        text1st.normal.textColor = Color.white;
        text1st.alignment = TextAnchor.MiddleRight;

        txtComst.alignment = TextAnchor.MiddleCenter;

        GUI.BeginGroup(new Rect(0, 50, er.width / 2, er.height - 40));

        if (GUI.Button(new Rect(40, 0, er.width / 2 - 40, 32), "Reset and fetch", txtComst))
        {
            Undo.RecordObject(db, "Reset and fetch");

            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            db.aNode.nameOpts.Clear();

            int c = 0;
            db.aNode.nameOpts.Add("[No object]");

            foreach (GameObject g in objects)
            {
                if (g.activeInHierarchy && checkUseful(g))
                    db.aNode.nameOpts.Add(g.name);

                c++;
            }

            db.aNode.Clean();

            //Fill up methods dictionary
            var gos = Resources.FindObjectsOfTypeAll<GameObject>();
            db.aNode.methods = new Dictionary<string, string>();
            for (int i = 0; i < gos.Length; i++)
            {
                if (gos[i].activeInHierarchy && checkUseful(gos[i]))
                {
                    List<MethodInfo> methodz = GetMethods(gos[i]);

                    for (int ii = 0; ii < methodz.Count; ii++)
                    {
                        if (!db.aNode.methods.ContainsKey(gos[i].name + ii.ToString()))
                            db.aNode.methods.Add(gos[i].name + ii.ToString(), methodz[ii].Name);
                    }
                }
            }

            db.aNode.opts = new string[] { "[No method]" };

            needSave = true;
        }

        GUI.color = Color.white;

        if (!db.aNode.editorRefreshed && Event.current.type == EventType.Repaint)
        {
            db.aNode.editorRefreshed = true;

            if (db.aNode.nameIndex != 0)
            {
                db.aNode.gameObjectName = db.aNode.nameOpts[db.aNode.nameIndex];
            }
            else
            {
                db.aNode.gameObjectName = "[No object]";
                db.aNode.methodName = "[No method]";
                db.aNode.methodIndex = 0;
                db.aNode.paramType = -1;
            }

            if (db.aNode.methodIndex != 0)
            {
                db.aNode.methodName = db.aNode.opts[db.aNode.methodIndex];
            }
            else
            {
                db.aNode.methodName = "[No method]";
                db.aNode.methodIndex = 0;
                db.aNode.paramType = -1;
            }

            //Repaint();
            //return;
        }

        if (db.aNode.nameOpts.Count > 0)
        {
            EditorGUI.BeginChangeCheck();
            int idx = EditorGUI.Popup(new Rect(40, 40, er.width / 2 - 40, 24), db.aNode.nameIndex, db.aNode.nameOpts.ToArray(), txtComst);
            if (EditorGUI.EndChangeCheck()) //Pick name
            {
                Undo.RecordObject(db, "Changed name Index");
                db.aNode.nameIndex = idx;
                db.aNode.gameObjectName = db.aNode.nameOpts[db.aNode.nameIndex];
                db.aNode.methodName = "[No method]";
                db.aNode.methodIndex = 0;
                db.aNode.paramType = -1;

                List<string> opti = new List<string>();
                opti.Add("[No method]");

                for (int x = 0; x < 10000; x++)
                {
                    if (db.aNode.methods.ContainsKey(db.aNode.gameObjectName + x.ToString()))
                    {
                        opti.Add(db.aNode.methods[db.aNode.gameObjectName + x.ToString()]);
                    }
                    else
                    {
                        break;
                    }
                }
                db.aNode.opts = opti.ToArray();

                needSave = true;
            }
        }

        EditorGUI.BeginChangeCheck();
        int meth = EditorGUI.Popup(new Rect(40, 68, er.width / 2 - 40, 24), db.aNode.methodIndex, db.aNode.opts, txtComst);

        if (EditorGUI.EndChangeCheck()) //Pick method
        {
            Undo.RecordObject(db, "Changed method index");
            db.aNode.methodIndex = meth;
            db.aNode.methodName = db.aNode.opts[db.aNode.methodIndex];

            GameObject ob = GameObject.Find(db.aNode.gameObjectName);
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == db.aNode.gameObjectName);

            foreach (GameObject g in objects)
            {
                if (g.activeInHierarchy && checkUseful(g))
                {
                    ob = g;
                    break;
                }

            }

            List<MethodInfo> methods = GetMethods(ob);


            if (db.aNode.methodIndex > 0)
                db.aNode.paramType = checkParam(methods[db.aNode.methodIndex - 1]);
            else
                db.aNode.paramType = -1;

            needSave = true;

        }
        //PARAMS
        text1st.fontSize = 14;
        txtComst.alignment = TextAnchor.MiddleLeft;
        if (db.aNode.paramType > 0)
            GUI.Label(new Rect(40, 100, 100, 16), "Parameters: ", text1st);

        if (db.aNode.paramType == 1)
        {
            EditorGUI.BeginChangeCheck();
            bool parab = EditorGUI.Toggle(new Rect(150, 100, 100, 24), db.aNode.param_bool, txtComst);
            if (EditorGUI.EndChangeCheck()) //Pick method
            {
                Undo.RecordObject(db, "Changed param");
                db.aNode.param_bool = parab;
                needSave = true;
            }
        }

        if (db.aNode.paramType == 2)
        {
            EditorGUI.BeginChangeCheck();
            string ps = EditorGUI.TextField(new Rect(150, 100, 100, 24), db.aNode.param_string, txtComst);
            if (EditorGUI.EndChangeCheck()) //Pick method
            {
                Undo.RecordObject(db, "Changed param");
                db.aNode.param_string = ps;
                needSave = true;
            }
        }
        if (db.aNode.paramType == 3)
        {
            EditorGUI.BeginChangeCheck();
            int pi = EditorGUI.IntField(new Rect(150, 100, 100, 24), db.aNode.param_int, txtComst);
            if (EditorGUI.EndChangeCheck()) //Pick method
            {
                Undo.RecordObject(db, "Changed param");
                db.aNode.param_int = pi;
                needSave = true;
            }
        }
        if (db.aNode.paramType == 4)
        {
            EditorGUI.BeginChangeCheck();
            float pf = EditorGUI.FloatField(new Rect(150, 100, 100, 24), db.aNode.param_float, txtComst);
            if (EditorGUI.EndChangeCheck()) //Pick method
            {
                Undo.RecordObject(db, "Changed param");
                db.aNode.param_float = pf;
                needSave = true;
            }
        }
        //END
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(er.width / 2, 50, er.width / 2, er.height - 40));
        GUI.Label(new Rect(40, 0, 200, 32), "OvrStartNode", text1st);
        EditorGUI.BeginChangeCheck();
        int ovr = EditorGUI.IntField(new Rect(250, 0, er.width / 2 - 290, 32), db.aNode.ovrStartNode, txtComst);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(db, "Set Override Start Node");
            db.aNode.ovrStartNode = ovr;
            needSave = true;
        }
        GUI.Label(new Rect(40, 40, 200, 32), "RenameDialogue", text1st);
        EditorGUI.BeginChangeCheck();
        string ren = EditorGUI.TextField(new Rect(250, 40, er.width / 2 - 290, 32), db.aNode.renameDialogue, txtComst);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(db, "Set Rename Dialogue");
            db.aNode.renameDialogue = ren;
            needSave = true;
        }
        GUI.Label(new Rect(40, 80, 200, 32), "Go to Node", text1st);
        EditorGUI.BeginChangeCheck();
        int goton = EditorGUI.IntField(new Rect(250, 80, er.width / 2 - 290, 32), db.aNode.gotoNode, txtComst);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(db, "Set goto node");
            db.aNode.gotoNode = goton;
            needSave = true;
        }


        GUI.EndGroup();

    }

    void DrawMMEditBar(Rect editRect)
    {

        GUIStyle boxBackst = new GUIStyle(GUI.skin.box);
        boxBackst.normal.background = mapTex;
        GUIStyle text1st = new GUIStyle(GUI.skin.label);
        text1st.fontSize = 14;
        text1st.normal.textColor = Color.white;
        text1st.alignment = TextAnchor.MiddleCenter;

        GUI.BeginGroup(new Rect(0, 8, editRect.width, 32), boxBackst);
        GUI.Label(new Rect(0, 0, 80, 32), "ID: " + db.pNode.ID, text1st);
        string isp = "Is Player";
        if (!db.pNode.isPlayer) isp = "Is NPC";

        if (GUI.Button(new Rect(88, 4, 120, 24), isp))
        {
            Undo.RecordObject(db, "Set Node type");
            db.pNode.isPlayer = !db.pNode.isPlayer;

            if (!db.pNode.isPlayer)
            {
                for (int i = 0; i < db.pNode.comment.Count; i++)
                {
                    if (i == 0) continue;
                    db.pNode.comment[i].outNode = null;
                    db.pNode.comment[i].outAction = null;
                }
            }
        }
        GUI.color = Color.white;
        //TAG
        EditorGUI.BeginChangeCheck();
        GUI.Label(new Rect(216, 4, 40, 24), "TAG: ", text1st);
        text1st = new GUIStyle(GUI.skin.textField);
        text1st.normal.textColor = Color.black;
        text1st.alignment = TextAnchor.MiddleLeft;
        string pt = EditorGUI.TextField(new Rect(254, 4, 150, 24), db.pNode.playerTag, txtComst);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(db, "Set player Tag");
            db.pNode.playerTag = pt;
            needSave = true;
        }

        //FOCUS
        if (GUI.Button(new Rect(408, 4, 64, 24), "Focus", txtComst))
        {
            spyView = false;
            CenterAll(false, db.pNode.ID, false);
        }
        //DELETE
        string delText = "Del";
        if (areYouSureIndex == db.playerDiags.IndexOf(db.pNode))
            if (areYouSure) { delText = "Sure?"; GUI.color = new Color32(176, 128, 54, 255); }
        if (GUI.Button(new Rect(editRect.width - 60, 4, 52, 24), delText, txtComst))
        {
            if (areYouSureIndex != db.playerDiags.IndexOf(db.pNode)) areYouSure = false;
            if (!areYouSure)
            {
                areYouSure = true;
                areYouSureIndex = db.playerDiags.IndexOf(db.pNode);
            }
            else
            {
                areYouSure = false;
                areYouSureIndex = 0;
                removeSet(db.playerDiags[db.playerDiags.IndexOf(db.pNode)]);
                needSave = true;
                db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
                CallSpyMap();
                db.pNode = null;
                db.pNodeID = -1;
                Repaint();
                return;
            }
        }
        GUI.color = Color.white;

        if (Event.current.type == EventType.MouseDown)
        {
            areYouSure = false;
            Repaint();
        }

        GUI.EndGroup();

        DrawMMEditBar2(editRect);

    }

    Vector2 comScroll;
    Vector2 evScroll;

    void DrawMMEditBar2(Rect er)
    {
        GUI.color = Color.white;
        GUIStyle boxBackst = new GUIStyle(GUI.skin.box);
        boxBackst.normal.background = mapTex;
        GUIStyle text1st = new GUIStyle(GUI.skin.label);
        text1st.fontSize = 14;
        text1st.normal.textColor = Color.white;
        text1st.alignment = TextAnchor.MiddleCenter;

        GUI.BeginGroup(new Rect(0, 40, er.width / 2, 200));
        GUI.BeginGroup(new Rect(0, 0, er.width / 2, 32));
        if (GUI.Button(new Rect(4, 4, 136, 24), "Toggle contents", txtComst))
        {
            mmShowComments = !mmShowComments;
            needSave = true;
        }
        if (GUI.Button(new Rect(144, 4, 240, 24), "Add Comment", txtComst))
        {
            areYouSure = false;
            addComment(db.pNode);
            needSave = true;
        }
        EditorGUI.BeginChangeCheck();
        Sprite sp = (Sprite)EditorGUI.ObjectField(new Rect(392, 8, er.width / 2 - 392, 16), db.pNode.sprite, typeof(Sprite), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(db, "Set Sprite");
            db.pNode.sprite = sp;
            needSave = true;
        }
        GUI.EndGroup();

        if (db.pNode == null) return;

        comScroll = GUI.BeginScrollView(new Rect(0, 32, er.width / 2, 144), comScroll, new Rect(0, 0, er.width / 2, 32 * db.pNode.comment.Count), GUIStyle.none, GUIStyle.none);
        DrawMMComments(er);
        GUI.EndScrollView();
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(er.width / 2, 40, er.width / 2, 200));
        GUI.BeginGroup(new Rect(0, 0, er.width / 2, 32));
        if (GUI.Button(new Rect(4, 4, 240, 24), "Add Extra Variable", txtComst))
        {
            Undo.RecordObject(db, "Add Extra Variable");
            db.pNode.vars.Add(string.Empty);
            db.pNode.varKeys.Add("Key" + db.pNode.vars.Count.ToString());
            needSave = true;
        }
        GUI.EndGroup();
        evScroll = GUI.BeginScrollView(new Rect(0, 32, er.width / 2, 144), evScroll, new Rect(0, 0, er.width / 2, 32 * db.pNode.varKeys.Count), GUIStyle.none, GUIStyle.none);
        DrawMMEV(er);
        GUI.EndScrollView();

        GUI.EndGroup();

    }

    bool mmShowComments = true;

    void DrawMMEV(Rect er)
    {

        for (int i = 0; i < db.pNode.vars.Count; i++)
        {
            txtComst.alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(new Rect(4, (32 * i) + 4, 24, 24), "X", txtComst))
            {
                Undo.RecordObject(db, "Removed Extra Variable");
                db.pNode.vars.RemoveAt(i);
                db.pNode.varKeys.RemoveAt(i);
                needSave = true;
                break;
            }
            txtComst.alignment = TextAnchor.UpperLeft;

            EditorGUI.BeginChangeCheck();
            string key = EditorGUI.TextField(new Rect(44, (32 * i) + 4, 132, 24), db.pNode.varKeys[i], txtComst);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set key");
                db.pNode.varKeys[i] = key;
                needSave = true;
            }
            EditorGUI.BeginChangeCheck();
            string val = EditorGUI.TextField(new Rect(184, (32 * i) + 4, (er.width / 2) - 188, 24), db.pNode.vars[i], txtComst);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set val");
                db.pNode.vars[i] = val;
                needSave = true;
            }

            txtComst.alignment = TextAnchor.MiddleCenter;

        }
    }

    void DrawMMComments(Rect er)
    {
        if (db.pNode != null)
        {
            GUIStyle txtst = new GUIStyle(GUI.skin.label);
            txtst.normal.textColor = Color.white;
            txtst.alignment = TextAnchor.MiddleLeft;
            txtst.fontSize = 14;

            for (int i = 0; i < db.pNode.comment.Count; i++)
            {

                txtComst.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(4, i * 32, 32, 32), i.ToString() + ". ", txtst);
                if (db.pNode.comment.Count > 1)
                    if (GUI.Button(new Rect(44, (i * 32) + 4, 24, 24), "X", txtComst))
                    {
                        areYouSure = false;
                        removeComment(db.pNode.comment[i]);
                        needSave = true;
                        return;
                    }

                string isVis = "O";
                if (!db.pNode.comment[i].visible) isVis = "Ø";

                if (GUI.Button(new Rect(72 + 4, (i * 32) + 4, 24, 24), isVis, txtComst))
                {
                    Undo.RecordObject(db, "Set Comment visibility");
                    db.pNode.comment[i].visible = !db.pNode.comment[i].visible;
                    needSave = true;
                }

                if (GUI.Button(new Rect(108, i * 32, 32, 16), "▲", txtComst))
                {
                    if (i > 0)
                    {
                        Undo.RecordObject(db, "Arranged comment");
                        ArrangeComment(db.pNode, -1, i);
                        repaintLines = true;
                        Repaint();
                        needSave = true;
                    }
                }

                if (GUI.Button(new Rect(108, (i * 32) + 16, 32, 16), "▼", txtComst))
                {
                    if (i < db.pNode.comment.Count - 1)
                    {
                        Undo.RecordObject(db, "Arranged comment");
                        ArrangeComment(db.pNode, 1, i);
                        repaintLines = true;
                        Repaint();
                        needSave = true;
                    }

                }
                txtComst.alignment = TextAnchor.UpperLeft;

                if (mmShowComments)
                {
                    GUIStyle exD = new GUIStyle(GUI.skin.textField);
                    exD.wordWrap = false;
                    EditorGUI.BeginChangeCheck();
                    txtComst.fontSize = 12;
                    string testText = EditorGUI.TextArea(new Rect(144, (i * 32), (er.width / 2) - 162, 30), db.pNode.comment[i].text, txtComst);
                    txtComst.fontSize = 14;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(db, "Edited Player comment");
                        db.pNode.comment[i].text = testText;
                        needSave = true;
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    Sprite spr = (Sprite)EditorGUI.ObjectField(new Rect(148 + 4, (i * 32) + 8, 100, 16), db.pNode.comment[i].sprites, typeof(Sprite), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(db, "Set Comment Sprite");
                        db.pNode.comment[i].sprites = spr;
                        needSave = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    AudioClip aud = (AudioClip)EditorGUI.ObjectField(new Rect(256 + 4, (i * 32) + 8, 100, 16), db.pNode.comment[i].audios, typeof(AudioClip), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(db, "Set Comment Audio");
                        db.pNode.comment[i].audios = aud;
                        needSave = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    string exd = EditorGUI.TextArea(new Rect(364, (i * 32) + 4, (er.width / 2) - 396, 24), db.pNode.comment[i].extraData);
                    GUI.color = Color.white;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(db, "Edited Player extra data");
                        db.pNode.comment[i].extraData = exd;
                        needSave = true;
                    }
                }
                txtComst.alignment = TextAnchor.MiddleCenter;
            }
        }
    }

    void DrawMapDialogueLine(VIDE_EditorDB.DialogueNode node)
    {
        foreach (VIDE_EditorDB.Comment c in node.comment)
        {
            if (c.outNode != null)
            {
                Rect sPosStart = new Rect(c.outRect.x + node.rect.x + 35, c.outRect.y + node.rect.y + 10, 10, 10);
                DrawNodeLineMap(GetMapRect(c.outRect), GetMapRect(c.outNode.rect), GetMapRect(node.rect), GetMapRect(sPosStart));
            }

            if (c.outAction != null)
            {
                Rect sPosStart = new Rect(c.outRect.x + node.rect.x + 35, c.outRect.y + node.rect.y + 10, 10, 10);
                DrawNodeLineMap(GetMapRect(c.outRect), GetMapRect(c.outAction.rect), GetMapRect(node.rect), GetMapRect(sPosStart));

            }
        }
    }

    void DrawMapActionLine(VIDE_EditorDB.ActionNode node)
    {
        if (node.outAction != null)
        {
            Rect sPosStart = new Rect(node.rect.x + 190, node.rect.y + 30, 10, 10);
            DrawActionNodeLineMap(GetMapRect(node.rect), GetMapRect(node.outAction.rect), GetMapRect(sPosStart));
        }
        if (node.outPlayer != null)
        {
            Rect sPosStart = new Rect(node.rect.x + 190, node.rect.y + 30, 10, 10);
            DrawActionNodeLineMap(GetMapRect(node.rect), GetMapRect(node.outPlayer.rect), GetMapRect(sPosStart));
        }

    }

    Rect GetMapRect(Rect r)
    {
        float xAvr = 100 - (100 * ((mapBounds.x + mapBounds.width) - r.x)) / (mapBounds.width);
        float yAvr = 100 - (100 * ((mapBounds.y + mapBounds.height) - r.y)) / (mapBounds.height);
        Rect newPos = new Rect(((xAvr * mapBounds.width / mapDivider) / 100), ((yAvr * mapBounds.height / mapDivider) / 100), r.width / mapDivider, r.height / mapDivider);
        return newPos;
    }

    void DrawEditSkin(int id)
    {

        VIDE_Editor_Skin.Skin s = VIDE_Editor_Skin.instance.skins[db.skinIndex];
        /* Colors */

        GUILayout.BeginHorizontal();
        GUILayout.Label("Player Node", GUILayout.Width(80));
        EditorGUI.BeginChangeCheck();
        Color cc = EditorGUILayout.ColorField(s.player_NodeColor);
        if (EditorGUI.EndChangeCheck()) s.player_NodeColor = cc;
        s.player_NodeColorSecondary = EditorGUILayout.ColorField(s.player_NodeColorSecondary);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Player Text", GUILayout.Width(80));
        s.playerText2 = EditorGUILayout.ColorField(s.playerText2);
        s.playerText = EditorGUILayout.ColorField(s.playerText);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("NPC Node", GUILayout.Width(80));
        s.npc_NodeColor = EditorGUILayout.ColorField(s.npc_NodeColor);
        s.npc_NodeColorSecondary = EditorGUILayout.ColorField(s.npc_NodeColorSecondary);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("NPC Text", GUILayout.Width(80));
        s.npcText2 = EditorGUILayout.ColorField(s.npcText2);
        s.npcText = EditorGUILayout.ColorField(s.npcText);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Action Node", GUILayout.Width(80));
        s.action_NodeColor = EditorGUILayout.ColorField(s.action_NodeColor);
        s.action_NodeColorSecondary = EditorGUILayout.ColorField(s.action_NodeColorSecondary);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Action Text", GUILayout.Width(80));
        s.actionText2 = EditorGUILayout.ColorField(s.actionText2);
        s.actionText = EditorGUILayout.ColorField(s.actionText);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Background", GUILayout.Width(80));
        s.background_color = EditorGUILayout.ColorField(s.background_color);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Grid", GUILayout.Width(80));
        s.grid_color = EditorGUILayout.ColorField(s.grid_color);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Connectors", GUILayout.Width(80));
        s.connectors_color = EditorGUILayout.ColorField(s.connectors_color);
        GUILayout.EndHorizontal();


        gridTex.SetPixel(0, 0, VIDE_Editor_Skin.GetColor(7, db.skinIndex));
        gridTex.Apply();
        GUILayout.BeginHorizontal();

        /*if (GUILayout.Button("Def", GUILayout.Width(30)))
        {
            VIDE_Editor_Skin.SetDefault(db.skinIndex);
        }*/
        if (GUILayout.Button("Reset", GUILayout.Width(50)))
        {
            VIDE_Editor_Skin.Reset(db.skinIndex);
        }
        if (GUILayout.Button("Close", GUILayout.ExpandWidth(true)))
        {
            editingColors = false;
            saveEditorSettings(db.currentDiag);
            Repaint();
        }

        GUILayout.EndHorizontal();


    }

    bool showNaviTab = true;

    void ShowHelp()
    {
        if (Event.current.keyCode == KeyCode.Escape)
        {
            showHelp = false;
            Repaint();
        }
        GUILayout.BeginArea(new Rect(position.width / 10, position.height / 10, (position.width / 10) * 8, (position.height / 10) * 8), GUI.skin.box);
        GUIStyle font = new GUIStyle(GUI.skin.GetStyle("label"));
        font.fontSize = 18;
        GUIStyle links = new GUIStyle(GUI.skin.GetStyle("button"));
        links.normal.textColor = Color.blue;
        links.fontSize = 14;
        links.fontStyle = FontStyle.Bold;

        GUILayout.BeginHorizontal(GUI.skin.box);
        if (GUILayout.Button("Navigation"))
            showNaviTab = true;

        if (GUILayout.Button("Links"))
            showNaviTab = false;

        GUILayout.EndHorizontal();

        GUILayout.Space(24);

        if (showNaviTab)
        {
            GUILayout.Label("Panning: Click and drag on empty area", font);
            GUILayout.Label("Clone node: Right click and drag from a node", font);
            GUILayout.Label("New linked node: Click and drag from connection button to empty area", font);
            GUILayout.Space(20);
            GUILayout.Label("Right click: Drop new Dialogue node (Left click to cancel)", font);
            GUILayout.Label("Return: Dialogue quick search", font);
            GUILayout.Label("Space: Mini Map", font);
            GUILayout.Label("Esc: Exit menus", font);
        } else
        {



            if (GUILayout.Button("Store Page", links, GUILayout.Height(40))){
                Application.OpenURL("https://assetstore.unity.com/packages/tools/ai/vide-dialogues-pro-69932");
            }
            if (GUILayout.Button("Documentation", links, GUILayout.Height(40))){
                Application.OpenURL("https://videdialogues.wordpress.com/about/");
            }
            if (GUILayout.Button("FAQ", links, GUILayout.Height(40)))
            {
                Application.OpenURL("https://videdialogues.wordpress.com/faq/");
            }
            if (GUILayout.Button("Scripting Tutorial", links, GUILayout.Height(40)))
            {
                Application.OpenURL("https://videdialogues.wordpress.com/tutorial/");
            }
            if (GUILayout.Button("Contact", links, GUILayout.Height(40)))
            {
                Application.OpenURL("https://videdialogues.wordpress.com/contact/");
            }
            if (GUILayout.Button("VIDE Dialogues Twitter", links, GUILayout.Height(40)))
            {
                Application.OpenURL("https://twitter.com/VIDEDialogues/");
            }
        }

        GUILayout.EndArea();
        GUILayout.Label("VIDE Dialogues 2.2.2");
    }

    //Here's where we actually draw everything
    GUISkin actionNodeSkin;
    void OnGUI()
    {
        Event e = Event.current;


        //Set colors we'll be using later
        colors = new Color32[]{new Color32(255,255,255,255),
            new Color32(180,180,180,255),
            new Color32(142,172,180,255),
            new Color32(84,110,137,255),
            new Color32(198,143,137,255)
        };

        defaultColor = GUI.color;

        if (searchingForDialogue)
        {
            GUI.FocusControl("filterSearch");
        }

        if (e.keyCode == KeyCode.Space && e.type == EventType.KeyUp && GUIUtility.keyboardControl < 1)
        {
            spyView = !spyView;
            if (spyView) CallSpyMap();
            Repaint();

        }

        if (e.keyCode == KeyCode.Return && e.type == EventType.KeyUp && GUIUtility.keyboardControl == 0)
        {
            if (!searchingForDialogue && !spyView)
            {
                searchingForDialogue = true;
                spyView = false;
                searchWord = "";
                Repaint();
                return;
            }
        }

        GUIStyle sty = new GUIStyle(EditorStyles.toolbar);
        sty.fixedHeight = 18;
        GUILayout.BeginHorizontal(sty);
        DrawToolbar();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(sty);
        DrawToolbar2();
        GUILayout.EndHorizontal();


        if (searchingForDialogue)
        {
            SearchDialogue();
            return;
        }

        if (assignMenu)
        {
            AssignMenu();
            return;
        }

        if (showHelp)
        {
            ShowHelp();
            return;
        }

        if (localizationMenu)
        {
            LocalizationMenu();
            if (deletingLanguage != -1)
            {
                BeginWindows();

                fWin = new Rect(position.width / 4, position.height / 4, position.width / 2, 0);
                fWin = GUILayout.Window(99995, fWin, DrawDeleteLang, "Are you sure?");
                GUI.FocusWindow(99995);

                EndWindows();
            }
            return;
        }

        if (spyView)
        {
            DrawSpyView();
            if (editingColors)
            {
                BeginWindows();
                Rect r = new Rect(position.width - 216, 48, 200, 150);
                r = GUILayout.Window(100000, r, DrawEditSkin, "Skins", GUI.skin.GetStyle("Window"));
                GUI.BringWindowToFront(100000);
                EndWindows();
            }
            return;
        }

        scrollArea = GUI.BeginScrollView(new Rect(0, 37, position.width, position.height - 37), scrollArea, canvas, GUIStyle.none, GUIStyle.none);

        DrawGrid();

        defaultColor = GUI.color;

        //handle input events
        if (editEnabled)
        {

            if (!dragnWindows)
            {
                if (e.type == EventType.MouseUp && GUIUtility.hotControl == 0 && e.button == 1)
                {
                    startDiag.x = e.mousePosition.x - 150;
                    startDiag.y = e.mousePosition.y - 25;
                    GUIUtility.keyboardControl = 0;
                    Repaint();
                }
            }


            if (e.type == EventType.MouseDown)
            {
                if (holdingBall) //dropball
                {
                    if (e.button == 0)
                    {
                        holdingBall = false;
                        ballsGravity[ballsGravity.Count - 1] = new Vector2(Random.Range(-300, 300), Random.Range(200, 600) * -1);
                    }

                }

                if (GUIUtility.hotControl != 0) GUIUtility.hotControl = 0;
                if (lerpFocusTime)
                    lerpFocusTime = false;

                foreach (VIDE_EditorDB.DialogueNode d in db.playerDiags)
                {
                    if (d.rect.Contains(e.mousePosition))
                    {
                        insideNode = true; break;

                    }
                }
                foreach (VIDE_EditorDB.ActionNode d in db.actionNodes)
                {
                    if (d.rect.Contains(e.mousePosition))
                    {
                        insideNode = true; break;
                    }
                }
                willDeselect = true;


            }

            if (position.Contains(GUIUtility.GUIToScreenPoint(e.mousePosition)))
            {
                if (e.type == EventType.MouseDrag && e.button == 0 && dragNewNode == 0) //Pan
                {
                    if (GUIUtility.hotControl == 0 && !insideNode && !editingColors)
                    {
                        dragnWindows = true;
                        if (Mathf.Abs(e.delta.x) < 200 && Mathf.Abs(e.delta.y) < 200)
                        {
                            willDeselect = false;
                            scrollArea = new Vector2(scrollArea.x -= e.delta.x, scrollArea.y -= e.delta.y);
                            Repaint();
                        }
                    }
                }

                if (e.type == EventType.MouseDown && e.button == 1 && GUIUtility.hotControl == 0) 
                {
                    if (!holdingBall)
                    {
                        dragNewNode = 0;
                        holdingBall = true;
                        balls.Add(e.mousePosition);
                        ballsGravity.Add(Vector2.zero);
                    }
                }
            }
            else
            {
                if (dragnWindows) // Stop dragging windows
                {
                    dragnWindows = false;
                    Repaint();
                    repaintLines = true;
                }
            }


            if (e.type == EventType.MouseUp)
            {             
                if (draggingLine) //Connect node detection
                {
                    TryConnectToDialogueNode(e.mousePosition, draggedCom);
                    TryConnectAction(e.mousePosition, draggedAction);
                    needSave = true;
                    Repaint();
                    GUIUtility.hotControl = 0;
                    repaintLines = true;
                }
                if (dragnWindows) // Stop dragging windows
                {
                    dragnWindows = false;
                    Repaint();
                    repaintLines = true;
                }

                if (!insideNode && willDeselect)
                {
                    db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
                    willDeselect = false;
                    Repaint();
                }

                insideNode = false;
                draggingLine = false;
            }

        }
        //Draw connection line
        if (editEnabled)
        {
            if (draggingLine)
            {
                DrawNodeLine3(dragStart, e.mousePosition);
                Repaint();
            }
        }

        //Draw all connected lines
        if (e.type == EventType.Repaint)
        {
            if (dragnWindows)
            {
                if (!db.previewPanning)
                    DrawLines();
            }
            else
            {
                DrawLines();
            }
        }


        //Here we'll draw all of the windows
        BeginWindows();

        int setID = 0;
        int ansID = 0;
        int acID = 0;
        GUI.enabled = editEnabled;

        GUIStyle st = new GUIStyle(VIDE_Editor_Skin.instance.windowStyle);
        st.fontStyle = FontStyle.Bold;
        st.fontSize = 12;
        st.richText = true;
        st.wordWrap = true;
        st.clipping = TextClipping.Clip;

        if (selectNodeDelayed != -1)
        {
            db.selectedNodes.Add(new VIDE_EditorDB.NodeSelection(db.playerDiags[selectNodeDelayed]));
            selectNodeDelayed = -1;
        }
        if (selectANodeDelayed != -1)
        {
            db.selectedNodes.Add(new VIDE_EditorDB.NodeSelection(db.actionNodes[selectANodeDelayed]));
            selectANodeDelayed = -1;
        }

        if (!newFile && !deletePopup && !overwritePopup)
        {
            if (db.playerDiags.Count > 0)
            {
                for (; setID < db.playerDiags.Count; setID++)
                {
                    if (!updateNodesRectsOnce)
                        if (!CheckInsideWindow(db.playerDiags[setID].rect)) continue;

                    if (db.playerDiags[setID].isPlayer)
                        GUI.color = VIDE_Editor_Skin.GetColor(0, db.skinIndex);
                    else
                        GUI.color = VIDE_Editor_Skin.GetColor(2, db.skinIndex);

                    foreach (VIDE_EditorDB.NodeSelection ns in db.selectedNodes)
                    {
                        if (ns.dNode == db.playerDiags[setID])
                            GUI.color = new Color(GUI.color.r + 0.1f, GUI.color.g + 0.1f, GUI.color.b + 0.1f, 1);
                    }


                    if (!dragnWindows)
                    {
                        db.playerDiags[setID].rect = GUILayout.Window(setID, db.playerDiags[setID].rect, DrawNodeWindow, "", st, GUILayout.Width(50), GUILayout.Height(40));

                    }
                    else
                    {
                        if (db.previewPanning)
                        {
                            db.playerDiags[setID].rect = GUILayout.Window(setID, db.playerDiags[setID].rect, DrawEmptyWindow, "", st, GUILayout.Height(40));
                        }
                        else
                        {
                            db.playerDiags[setID].rect = GUILayout.Window(setID, db.playerDiags[setID].rect, DrawNodeWindow, "", st, GUILayout.Height(40));
                        }
                    }
                }
            }
            GUI.color = defaultColor;
            if (db.actionNodes.Count > 0)
            {

                for (; acID < db.actionNodes.Count; acID++)
                {
                    if (!updateNodesRectsOnce)
                        if (!CheckInsideWindow(db.actionNodes[acID].rect)) continue;


                    GUI.color = VIDE_Editor_Skin.GetColor(4, db.skinIndex);

                    foreach (VIDE_EditorDB.NodeSelection ns in db.selectedNodes)
                    {
                        if (ns.aNode == db.actionNodes[acID])
                            GUI.color = new Color(GUI.color.r + 0.1f, GUI.color.g + 0.1f, GUI.color.b + 0.1f, 1);
                    }

                    if (!dragnWindows)
                    {
                        db.actionNodes[acID].rect = GUILayout.Window(acID + setID + ansID, db.actionNodes[acID].rect, DrawActionWindow, "", st, GUILayout.Height(40), GUILayout.Width(200));
                    }
                    else
                    {
                        if (db.previewPanning)
                        {
                            db.actionNodes[acID].rect = GUILayout.Window(acID + setID + ansID, db.actionNodes[acID].rect, DrawEmptyWindow, "", st, GUILayout.Height(40), GUILayout.Width(200));
                        }
                        else
                        {
                            db.actionNodes[acID].rect = GUILayout.Window(acID + setID + ansID, db.actionNodes[acID].rect, DrawActionWindow, "", st, GUILayout.Height(40), GUILayout.Width(200));
                        }

                    }
                }

            }
        }

        if (draggingNode)
        {
            if (position.Contains(GUIUtility.GUIToScreenPoint(e.mousePosition)))
            {
                if (Mathf.Abs(e.delta.x) < 200 && Mathf.Abs(e.delta.y) < 200)
                {
                    Undo.RecordObject(db, "dragged");
                    DragOtherNodes(e.delta);
                }
            }
            else
            {
                draggingNode = false;
            }

        }

        if (e.type == EventType.MouseUp)
        {
            if (draggingNode)
            {
                draggingNode = false;
            }
        }

        //Snap diags 
        foreach (VIDE_EditorDB.DialogueNode n in db.playerDiags)
        {
            Rect rawrect = n.rect;
            rawrect.x = Mathf.Floor(rawrect.x / gridSize) * gridSize;
            rawrect.y = Mathf.Floor(rawrect.y / gridSize) * gridSize;
            n.rect = new Rect(rawrect.x, rawrect.y, n.rect.width, n.rect.height);
        }
        foreach (VIDE_EditorDB.ActionNode n in db.actionNodes)
        {
            Rect rawrect = n.rect;
            rawrect.x = Mathf.Floor(rawrect.x / gridSize) * gridSize;
            rawrect.y = Mathf.Floor(rawrect.y / gridSize) * gridSize;
            n.rect = new Rect(rawrect.x, rawrect.y, n.rect.width, n.rect.height);
        }


        //Here we check for errors in the node stfructure

        playerReady = true;
        hasID = false;
        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (db.startID == db.playerDiags[i].ID)
            {
                hasID = true;
            }
        }
        for (int i = 0; i < db.actionNodes.Count; i++)
        {
            if (db.startID == db.actionNodes[i].ID)
            {
                hasID = true;
            }
        }

        if (e.type == EventType.Layout)
        {
            showError = false;
            if (!playerReady)
                showError = true;
        }
        GUI.color = colors[0];
        GUI.SetNextControlName("startD");

        GUI.enabled = true;
        if (newFile)
        {
            fWin = new Rect(canvas.x + scrollArea.x + position.width / 4, canvas.y + scrollArea.y + position.height / 4, position.width / 2, 0);
            fWin = GUILayout.Window(99998, fWin, DrawNewFileWindow, "New Dialogue:");
            GUI.BringWindowToFront(99998);
        }
        if (overwritePopup)
        {
            fWin = new Rect(canvas.x + scrollArea.x + position.width / 4, canvas.y + scrollArea.y + position.height / 4, position.width / 2, 0);
            fWin = GUILayout.Window(99997, fWin, DrawOverwriteWindow, "Overwrite?");
            GUI.BringWindowToFront(99997);
        }
        if (deletePopup)
        {
            fWin = new Rect(canvas.x + scrollArea.x + position.width / 4, canvas.y + scrollArea.y + position.height / 4, position.width / 2, 0);
            fWin = GUILayout.Window(99996, fWin, DrawDeleteWindow, "Are you sure?");
            GUI.BringWindowToFront(99996);
        }
        if (editingColors)
        {
            Rect r = new Rect(canvas.x + scrollArea.x + position.width - 216, canvas.y + scrollArea.y + 8, 200, 150);
            r = GUILayout.Window(100000, r, DrawEditSkin, "Skins", GUI.skin.GetStyle("Window"));
            GUI.BringWindowToFront(100000);
        }
        EndWindows();


        if (e.button == 0 && e.type == EventType.MouseDown)
        {
            areYouSure = false;
            if (!editingColors)
            {
                //GUIUtility.keyboardControl = 0;
            }
            Repaint();
        }

        GUI.EndScrollView();


        if (editEnabled)
            if (e.type == EventType.MouseUp)
            {
                if (dragNewNode > 0) // Stop dragging windows
                {
                    addNewNode(e.mousePosition, dragNewNode);
                    dragNewNode = 0;
                    Repaint();
                }
                if (holdingBall) // DropNewD
                {
                    addNewNode(e.mousePosition, 1);
                    dragNewNode = 0;
                    balls.RemoveAt(balls.Count - 1);
                    ballsGravity.RemoveAt(ballsGravity.Count - 1);
                    holdingBall = false;
                    Repaint();
                }
            }

        //Draws dragged node
        if (dragNewNode > 0)
        {
            dragNewNodeRect.x = e.mousePosition.x - 50;
            dragNewNodeRect.y = e.mousePosition.y - 20;
            dragNewNodeRect.width = 100;
            dragNewNodeRect.height = 40;
            if (dragNewNode == 1)
                GUI.DrawTexture(dragNewNodeRect, newNodeIcon, ScaleMode.StretchToFill);
            if (dragNewNode == 2)
                GUI.DrawTexture(dragNewNodeRect, newNodeIcon3, ScaleMode.StretchToFill);
            Repaint();
        }

        if (updateNodesRectsOnce && Event.current.type == EventType.Repaint)
        {
            updateNodesRectsOnce = false;
        }

        //Ballstuff
        if (holdingBall)
        {
            if (balls[balls.Count - 1].y > position.height)
            {
                holdingBall = false;
                ballsGravity[ballsGravity.Count - 1] = new Vector2(Random.Range(-100, 100), Random.Range(200, 500)*-1);
            } else
            {
                balls[balls.Count - 1] = e.mousePosition;
            }
            Repaint();

        }

        for (int i = 0; i < balls.Count; i++)
        {
            float rspeed = ballsGravity[i].y / 4 * Mathf.Sign(ballsGravity[i].x);
            if (i == balls.Count - 1 && holdingBall) rspeed = 0;

            Matrix4x4 matrixBackup = GUI.matrix;

            GUIUtility.RotateAroundPivot(rspeed, new Vector2(balls[i].x, balls[i].y));
            GUI.DrawTexture(new Rect(balls[i].x - 40, balls[i].y - 15, 80, 30), newNodeIcon, ScaleMode.StretchToFill, true);

            GUI.matrix = matrixBackup;

            //ballsGravity[i] += new Vector2(0, 2f);
            //balls[i] += ballsGravity[i] * deltatime;

            if (balls[i].y > position.height + 50)
            {
                balls.RemoveAt(i);
                ballsGravity.RemoveAt(i);
            }

            Repaint();
        }

        if (Event.current.commandName == "UndoRedoPerformed")
            Repaint();

    }


    Texture2D gridTex;

    void DrawGrid()
    {

        Color dCol = Handles.color;
        Color gridColor = VIDE_Editor_Skin.GetColor(8, db.skinIndex);
        GUI.DrawTexture(canvas, gridTex, ScaleMode.StretchToFill);

        int wlines = Mathf.RoundToInt(canvas.width / gridSize);
        int hlines = Mathf.RoundToInt(canvas.height / gridSize);
        Handles.color = gridColor;

        if (db.previewPanning) return;


        for (int i = 0; i < wlines; i++)
        {
            if (i % 4 == 0)
                Handles.DrawLine(new Vector3(canvas.x, canvas.y + i * gridSize, 0), new Vector3(canvas.x + canvas.width, canvas.y + i * gridSize, 0));
        }
        for (int i = 0; i < hlines; i++)
        {
            if (i % 4 == 0)
                Handles.DrawLine(new Vector3(canvas.x + i * gridSize, canvas.y, 0), new Vector3(canvas.x + i * gridSize, canvas.height, 0));
        }
        Handles.color = dCol;
    }

    bool CheckInsideWindow(Rect rr)
    {
        Rect viewport = canvas;
        viewport.x += scrollArea.x;
        viewport.y += scrollArea.y;
        viewport.width = position.width;
        viewport.height = position.height;

        Rect r = rr;


        if (viewport.Contains(new Vector2(r.x, r.y)))
        {
            return true;
        }
        if (viewport.Contains(new Vector2(r.x + r.width, r.y + r.height)))
        {
            return true;
        }
        if (viewport.Contains(new Vector2(r.x + r.width, r.y)))
        {
            return true;
        }
        if (viewport.Contains(new Vector2(r.x, r.y + r.height)))
        {
            return true;
        }
        return false;
    }

    void DrawEmptyWindow(int id)
    {
        GUI.color = Color.clear;
        GUILayout.Box("", GUILayout.Width(200), GUILayout.Height(50));
        GUI.color = Color.white;

    }

    void DrawNodeWindow(int id)
    {
        GUI.skin = VIDE_Editor_Skin.instance.ActionSkin;

        if (id >= db.playerDiags.Count) return;

        if (db.playerDiags[id].isPlayer)
        {
            GUI.backgroundColor = VIDE_Editor_Skin.GetColor(1, db.skinIndex);
            GUI.contentColor = VIDE_Editor_Skin.GetColor(11, db.skinIndex);
        }
        else
        {
            GUI.backgroundColor = VIDE_Editor_Skin.GetColor(3, db.skinIndex);
            GUI.contentColor = VIDE_Editor_Skin.GetColor(12, db.skinIndex);
        }
        GUIStyle stf = new GUIStyle(EditorStyles.textArea);
        stf.wordWrap = true;
        stf.normal.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
        stf.focused.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
        stf.active.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
        Color last = GUI.backgroundColor;
        GUIStyle exD = new GUIStyle(EditorStyles.textArea);
        exD.normal.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
        exD.active.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
        exD.focused.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
        exD.wordWrap = false;

        if (id >= db.playerDiags.Count)
            return;

        Event e = Event.current;

        GUI.enabled = editEnabled;
        bool dontDrag = false;
        if (e.type == EventType.MouseUp)
        {
            draggingLine = false;
            dontDrag = true;

        }

        GUI.color = new Color(0, 0, 0, 0.2f);
        GUILayout.BeginVertical(GUI.skin.box);
        GUI.color = defaultColor;

        Color lastc = GUI.contentColor;
        if (db.playerDiags[id].isPlayer)
            GUI.contentColor = VIDE_Editor_Skin.GetColor(14, db.skinIndex);
        else
            GUI.contentColor = VIDE_Editor_Skin.GetColor(15, db.skinIndex);


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUILayout.Label("Dialogue Node - ID: " + db.playerDiags[id].ID.ToString());
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.contentColor = lastc;

        GUILayout.BeginHorizontal();

        string delText = "Delete Node";
        if (areYouSureIndex == id)
            if (areYouSure) { delText = "Sure?"; }
        if (GUILayout.Button(delText, GUILayout.Width(80)))
        {
            if (areYouSureIndex != id) areYouSure = false;
            if (!areYouSure)
            {
                areYouSure = true;
                areYouSureIndex = id;
            }
            else
            {
                areYouSure = false;
                areYouSureIndex = 0;
                removeSet(db.playerDiags[id]);
                needSave = true;
                return;
            }
        }
        if (e.type == EventType.MouseDown)
        {
            areYouSure = false;
            Repaint();
        }

        GUI.color = defaultColor;
        if (GUILayout.Button("Add comment", GUILayout.Width(140)))
        {
            areYouSure = false;
            addComment(db.playerDiags[id]);
            needSave = true;
        }

        string isp = "Is Player";
        if (!db.playerDiags[id].isPlayer) isp = "Is NPC";

        if (GUILayout.Button(isp))
        {
            Undo.RecordObject(db, "Set Node type");
            db.playerDiags[id].isPlayer = !db.playerDiags[id].isPlayer;
            if (!db.playerDiags[id].isPlayer)
            {
                for (int i = 0; i < db.playerDiags[id].comment.Count; i++)
                {
                    if (i == 0) continue;
                    db.playerDiags[id].comment[i].outNode = null;
                    db.playerDiags[id].comment[i].outAction = null;

                }
            }
        }
        GUILayout.EndHorizontal();
        GUI.color = defaultColor;

        GUILayout.EndVertical();

        GUILayout.Space(5);


        for (int i = 0; i < db.playerDiags[id].comment.Count; i++)
        {
            if (db.playerDiags[id].comment.Count > 0)
            {
                GUILayout.BeginHorizontal();
                lastc = GUI.contentColor;
                if (db.playerDiags[id].isPlayer)
                    GUI.contentColor = VIDE_Editor_Skin.GetColor(14, db.skinIndex);
                else
                    GUI.contentColor = VIDE_Editor_Skin.GetColor(15, db.skinIndex);
                GUILayout.Label((i).ToString() + ". ", GUILayout.Width(20));
                GUI.contentColor = lastc;
                if (i == 0) GUILayout.Space(24);
                if (i != 0)
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        areYouSure = false;
                        removeComment(db.playerDiags[id].comment[i]);
                        needSave = true;
                        return;
                    }

                EditorGUI.BeginChangeCheck();
                last = GUI.backgroundColor;
                lastc = GUI.contentColor;
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.white;
                string testText = EditorGUILayout.TextArea(db.playerDiags[id].comment[i].text, stf, GUILayout.Width(200));
                GUI.contentColor = lastc;
                GUI.backgroundColor = last;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(db, "Edited Player comment");
                    db.playerDiags[id].comment[i].text = testText;
                    needSave = true;
                }

                GUI.color = new Color(0, 0, 0, 0.2f);
                GUILayout.BeginVertical();
                GUI.color = defaultColor;

                if (GUILayout.Button("", GUILayout.Height(10), GUILayout.Width(16), GUILayout.ExpandHeight(true)))
                {
                    if (i != 0)
                    {
                        Undo.RecordObject(db, "Arranged commen");
                        ArrangeComment(db.playerDiags[id], -1, i);
                        repaintLines = true;
                        Repaint();
                        needSave = true;
                    }
                }

                if (GUILayout.Button("", GUILayout.Height(10), GUILayout.Width(16), GUILayout.ExpandHeight(true)))
                {
                    if (i != db.playerDiags[id].comment.Count - 1)
                    {
                        Undo.RecordObject(db, "Arranged commen");
                        ArrangeComment(db.playerDiags[id], 1, i);
                        repaintLines = true;
                        Repaint();
                        needSave = true;
                    }

                }

                GUILayout.EndVertical();


                string showmore = "+";
                if (db.playerDiags[id].comment[i].showmore) showmore = "-";

                if (GUILayout.Button(showmore, GUILayout.Width(20)))
                {
                    Undo.RecordObject(db, "Show Audio and Sprites");
                    db.playerDiags[id].comment[i].showmore = !db.playerDiags[id].comment[i].showmore;
                    Repaint();
                    needSave = true;
                }

                bool drawConnectors = true;

                if (!db.playerDiags[id].isPlayer)
                {
                    if (i > 0)
                        drawConnectors = false;
                    else
                        drawConnectors = true;
                }

                if (drawConnectors)
                {
                    if (db.playerDiags[id].comment[i].outNode == null && db.playerDiags[id].comment[i].outAction == null)
                    {
                        Rect lr;

                        if (GUILayout.RepeatButton("O", GUILayout.Width(30)))
                        {
                            areYouSure = false;
                            lr = GUILayoutUtility.GetLastRect();
                            lr = new Rect(lr.x + db.playerDiags[id].rect.x + 30, lr.y + db.playerDiags[id].rect.y + 7, 0, 0);
                            if (!draggingLine && !dontDrag)
                            {
                                draggedCom = db.playerDiags[id].comment[i];
                                draggedAction = null;
                                dragStart = new Vector2(lr.x, lr.y);
                                draggingLine = true;
                                needSave = true;
                            }
                        }
                        GUI.color = defaultColor;
                    }
                    else
                    {

                        last = GUI.backgroundColor;
                        GUI.backgroundColor = Color.white;
                        if (GUILayout.Button("<color='black'>x</color>", GUILayout.Width(30)))
                        {
                            areYouSure = false;
                            breakConnection(0, db.playerDiags[id].comment[i], null);
                            needSave = true;
                        }
                        if (e.type == EventType.Repaint)
                        {
                            db.playerDiags[id].comment[i].outRect = GUILayoutUtility.GetLastRect();
                        }
                        GUI.backgroundColor = last;
                        GUI.color = defaultColor;

                    }
                }
                GUILayout.FlexibleSpace();


                GUILayout.EndHorizontal();

                if (db.playerDiags[id].comment[i].showmore)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(24);
                    string visText = "O";

                    if (!db.playerDiags[id].comment[i].visible)
                        visText = "Ø";

                    if (GUILayout.Button(visText, GUILayout.Width(20)))
                    {
                        Undo.RecordObject(db, "Set Comment visibility");
                        db.playerDiags[id].comment[i].visible = !db.playerDiags[id].comment[i].visible;
                        needSave = true;
                    }
                    GUI.color = defaultColor;

                    last = GUI.backgroundColor;
                    GUI.backgroundColor = Color.white;
                    lastc = GUI.contentColor;
                    GUI.contentColor = Color.white;

                    EditorGUI.BeginChangeCheck();
                    Sprite spr = (Sprite)EditorGUILayout.ObjectField(db.playerDiags[id].comment[i].sprites, typeof(Sprite), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(db, "Set Comment Sprite");
                        db.playerDiags[id].comment[i].sprites = spr;
                        needSave = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    AudioClip aud = (AudioClip)EditorGUILayout.ObjectField(db.playerDiags[id].comment[i].audios, typeof(AudioClip), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(db, "Set Comment Audio");
                        db.playerDiags[id].comment[i].audios = aud;
                        needSave = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    string exd = EditorGUILayout.TextArea(db.playerDiags[id].comment[i].extraData, exD, GUILayout.Width(70));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(db, "Edited Player extra data");
                        db.playerDiags[id].comment[i].extraData = exd;
                        needSave = true;
                    }
                    GUI.backgroundColor = last;
                    GUI.contentColor = lastc;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }

            }
        }
        GUILayout.Space(5);

        GUI.color = new Color(0, 0, 0, 0.2f);
        GUILayout.BeginVertical(GUI.skin.box);
        GUI.color = defaultColor;

        GUILayout.BeginHorizontal();
        lastc = GUI.contentColor;
        if (db.playerDiags[id].isPlayer)
            GUI.contentColor = VIDE_Editor_Skin.GetColor(14, db.skinIndex);
        else
            GUI.contentColor = VIDE_Editor_Skin.GetColor(15, db.skinIndex);
        GUILayout.Label("Tag: ", GUILayout.Width(30));
        GUI.contentColor = lastc;

        EditorGUI.BeginChangeCheck();
        last = GUI.backgroundColor;
        lastc = GUI.contentColor;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        string pt = EditorGUILayout.TextField(db.playerDiags[id].playerTag, stf, GUILayout.Width(80));
        GUI.backgroundColor = last;
        GUI.contentColor = lastc;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(db, "Set player Tag");
            db.playerDiags[id].playerTag = pt;
            needSave = true;
        }


        //TagShortcut

        EditorGUI.BeginChangeCheck();
            Color pp = GUI.contentColor;
            GUI.contentColor = Color.white;
            int tidx = EditorGUILayout.Popup(-1, existingTags.ToArray(), GUILayout.Width(16));
            GUI.contentColor = pp;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set player Tag");
                string newtag = "";

                if (existingTags[tidx] != "[Empty]")
                newtag = existingTags[tidx];

                db.playerDiags[id].playerTag = newtag;
                needSave = true;

                UpdateTagList();
            }
       

        GUI.color = Color.white;

        GUILayout.FlexibleSpace();
        string exText = (db.playerDiags[id].expand) ? "-" : "+";
        if (GUILayout.Button(exText, GUILayout.Width(60)))
        {
            Undo.RecordObject(db, "Set Expand");
            db.playerDiags[id].expand = !db.playerDiags[id].expand;
            needSave = true;
        }

        GUILayout.EndHorizontal();
        GUI.color = defaultColor;

        /* Expand stuff */

        if (db.playerDiags[id].expand)
        {
            GUILayout.Space(5);
            GUIStyle obst = new GUIStyle(EditorStyles.objectField);
            obst.normal.textColor = GUI.contentColor;

            EditorGUI.BeginChangeCheck();

            last = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            lastc = GUI.contentColor;
            GUI.contentColor = Color.white;
            GUISkin pskin = GUI.skin;
            GUI.skin = null;
            Sprite sp = (Sprite)EditorGUILayout.ObjectField("Node Sprite: ", db.playerDiags[id].sprite, typeof(Sprite), false);
            GUI.skin = pskin;
            GUI.backgroundColor = last;
            GUI.contentColor = lastc;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set Sprite");
                db.playerDiags[id].sprite = sp;
                needSave = true;
            }

            GUILayout.BeginHorizontal();
            lastc = GUI.contentColor;
            if (db.playerDiags[id].isPlayer)
                GUI.contentColor = VIDE_Editor_Skin.GetColor(14, db.skinIndex);
            else
                GUI.contentColor = VIDE_Editor_Skin.GetColor(15, db.skinIndex);
            GUILayout.Label("Extra Variables: ");
            GUI.contentColor = lastc;
            if (GUILayout.Button("Add"))
            {
                Undo.RecordObject(db, "Add Extra Variable");
                db.playerDiags[id].vars.Add(string.Empty);
                db.playerDiags[id].varKeys.Add("Key" + db.playerDiags[id].vars.Count.ToString());
                needSave = true;
            }

            GUILayout.EndHorizontal();

            for (int i = 0; i < db.playerDiags[id].vars.Count; i++)
            {
                GUILayout.BeginHorizontal();
                lastc = GUI.contentColor;
                if (db.playerDiags[id].isPlayer)
                    GUI.contentColor = VIDE_Editor_Skin.GetColor(14, db.skinIndex);
                else
                    GUI.contentColor = VIDE_Editor_Skin.GetColor(15, db.skinIndex);
                GUILayout.Label(i.ToString() + ". ", GUILayout.Width(20));
                GUI.contentColor = lastc;
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    Undo.RecordObject(db, "Removed Extra Variable");
                    db.playerDiags[id].vars.RemoveAt(i);
                    db.playerDiags[id].varKeys.RemoveAt(i);
                    needSave = true;
                    break;
                }

                EditorGUI.BeginChangeCheck();
                last = GUI.backgroundColor;
                lastc = GUI.contentColor;
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.white;
                string key = EditorGUILayout.TextField(db.playerDiags[id].varKeys[i], stf, GUILayout.Width(80));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(db, "Set key");
                    db.playerDiags[id].varKeys[i] = key;
                    needSave = true;

                }

                EditorGUI.BeginChangeCheck();

                string val = EditorGUILayout.TextField(db.playerDiags[id].vars[i], stf);
                GUI.backgroundColor = last;
                GUI.contentColor = lastc;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(db, "Set value");
                    db.playerDiags[id].vars[i] = val;
                    needSave = true;

                }

                GUILayout.EndHorizontal();

            }

        }

        GUILayout.EndVertical();

        if (e.type == EventType.MouseDrag && e.button == 0)
        {

            needSave = true;
            bool hasNode = false;
            foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                if (s.dNode == db.playerDiags[id]) { hasNode = true; break; }

            if (hasNode && !draggingNode && GUIUtility.hotControl == 0)
            {
                draggingNode = true;
            }
        }
        if (!draggingNode)
        {
            db.playerDiags[id].rectRaw = db.playerDiags[id].rect;
        }

        if (e.button == 0 && e.type == EventType.MouseUp)
        {
            //Select node
            if (!draggingNode)
            {
                if (e.shift)
                {
                    bool hasNode = false;
                    foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                        if (s.dNode == db.playerDiags[id]) { hasNode = true; break; }
                    if (!hasNode)
                        selectNodeDelayed = id;
                }
                else
                {
                    db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
                    bool hasNode = false;
                    foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                        if (s.dNode == db.playerDiags[id]) { hasNode = true; break; }
                    if (!hasNode)
                    {
                        selectNodeDelayed = id;
                    }
                    UpdateTagList();
                }
                Repaint();
            }
            else
            {
                draggingNode = false;
            }

        }


        if (e.commandName == "UndoRedoPerformed")
            Repaint();

        if (!lerpFocusTime && e.button == 0)
        {
            bool hasNode2 = false;
            foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                if (s.dNode == db.playerDiags[id]) { hasNode2 = true; break; }

            if (position.Contains(GUIUtility.GUIToScreenPoint(e.mousePosition)) && !hasNode2)
            {
                if (e.type == EventType.MouseDrag)
                    Undo.RecordObject(db, "dragg");

                GUI.DragWindow();

            }
        }

        if (!lerpFocusTime && e.button == 1 && e.type == EventType.MouseDown)
        {
            dragNewNode = 1;
            copiedNode = db.playerDiags[id];

            holdingBall = false;
            balls.RemoveAt(balls.Count - 1);
            ballsGravity.RemoveAt(ballsGravity.Count - 1);
        }

    }

    void DragOtherNodes(Vector2 mPos)
    {
        foreach (VIDE_EditorDB.NodeSelection n in db.selectedNodes)
        {
            if (n.dNode != null)
            {
                Rect r = n.dNode.rectRaw;

                r.x += mPos.x / 2;
                r.y += mPos.y / 2;
                n.dNode.rectRaw = r;

                Rect rawrect = n.dNode.rectRaw;
                rawrect.x = Mathf.Floor(rawrect.x / gridSize) * gridSize;
                rawrect.y = Mathf.Floor(rawrect.y / gridSize) * gridSize;
                n.dNode.rect = new Rect(rawrect.x, rawrect.y, n.dNode.rect.width, n.dNode.rect.height);
            }
            else
            {
                Rect r = n.aNode.rectRaw;

                r.x += mPos.x / 2;
                r.y += mPos.y / 2;
                n.aNode.rectRaw = r;

                Rect rawrect = n.aNode.rectRaw;
                rawrect.x = Mathf.Floor(rawrect.x / gridSize) * gridSize;
                rawrect.y = Mathf.Floor(rawrect.y / gridSize) * gridSize;
                n.aNode.rect = new Rect(rawrect.x, rawrect.y, n.aNode.rect.width, n.aNode.rect.height);
            }

        }
        Repaint();
    }

    void DrawActionWindow(int id)
    {
        GUI.skin = VIDE_Editor_Skin.instance.ActionSkin;

        GUI.enabled = editEnabled;
        bool dontDrag = false;

        GUI.backgroundColor = VIDE_Editor_Skin.GetColor(5, db.skinIndex);
        GUI.contentColor = VIDE_Editor_Skin.GetColor(13, db.skinIndex);

        int aID = id - (db.playerDiags.Count);
        if (aID < 0)
            aID = 0;

        if (aID >= db.actionNodes.Count)
            return;

        if (Event.current.type == EventType.MouseUp)
        {
            draggingLine = false;
            dontDrag = true;
        }

        GUI.color = new Color(0, 0, 0, 0.2f);
        GUILayout.BeginVertical(GUI.skin.box);
        GUI.color = Color.white;

        Color last = GUI.contentColor;
        GUI.contentColor = VIDE_Editor_Skin.GetColor(16, db.skinIndex);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUILayout.Label("Action Node - ID: " + db.actionNodes[aID].ID.ToString());
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.contentColor = last;


        GUILayout.BeginHorizontal();

        string txt = "+";
        if (db.actionNodes[aID].more) txt = "-";

        if (GUILayout.Button(txt, GUILayout.Width(30)))
        {
            Undo.RecordObject(db, "Set Expand");
            db.actionNodes[aID].more = !db.actionNodes[aID].more;
        }

        string delText = "Delete Node";
        if (areYouSureIndex == id)
            if (areYouSure) { delText = "Sure?"; }

        GUI.color = Color.white;
        if (GUILayout.Button(delText))
        {
            if (areYouSureIndex != id) areYouSure = false;
            if (!areYouSure)
            {
                areYouSure = true;
                areYouSureIndex = id;
            }
            else
            {
                areYouSure = false;
                areYouSureIndex = 0;
                removeAction(db.actionNodes[aID]);
                needSave = true;
                return;
            }
        }
        if (Event.current.type == EventType.MouseDown)
        {
            areYouSure = false;
            Repaint();
        }

        GUI.color = defaultColor;

        GUIStyle stf = new GUIStyle(GUI.skin.textField);
        stf.wordWrap = true;

        if (db.actionNodes[aID].outPlayer == null && db.actionNodes[aID].outAction == null)
        {
            Rect lr;
            //GUI.color = VIDE_Editor_Skin.GetColor(5, db.skinIndex);
            if (GUILayout.RepeatButton("O", GUILayout.Width(30)))
            {
                areYouSure = false;
                lr = GUILayoutUtility.GetLastRect();
                lr = new Rect(lr.x + db.actionNodes[aID].rect.x + 30, lr.y + db.actionNodes[aID].rect.y + 7, 0, 0);
                if (!draggingLine && !dontDrag)
                {
                    draggedCom = null;
                    draggedAction = db.actionNodes[aID];
                    dragStart = new Vector2(lr.x, lr.y);
                    draggingLine = true;
                    needSave = true;
                }
            }
            GUI.color = Color.white;

        }
        else
        {
            if (GUILayout.Button("x", GUILayout.Width(30)))
            {
                areYouSure = false;
                breakConnection(1, null, db.actionNodes[aID]);
                needSave = true;
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(5);

        if (db.actionNodes[aID].more)
        {

            last = GUI.contentColor;
            GUIStyle intStyle = new GUIStyle(EditorStyles.textField);
            intStyle.normal.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
            intStyle.focused.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
            intStyle.active.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
            GUI.contentColor = VIDE_Editor_Skin.GetColor(16, db.skinIndex);

            GUI.color = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Label("OvrStartNode");
            GUI.contentColor = last;
            EditorGUI.BeginChangeCheck();
            last = GUI.backgroundColor;
            Color lastc = GUI.contentColor;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;

            int ovr = EditorGUILayout.IntField(db.actionNodes[aID].ovrStartNode, intStyle);
            GUI.backgroundColor = last;
            GUI.contentColor = lastc;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set Override Start Node");
                db.actionNodes[aID].ovrStartNode = ovr;
                needSave = true;
            }
            last = GUI.contentColor;
            GUI.contentColor = VIDE_Editor_Skin.GetColor(16, db.skinIndex);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("RenameDialogue");
            GUI.contentColor = last;

            EditorGUI.BeginChangeCheck();
            last = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            lastc = GUI.contentColor;
            GUI.contentColor = Color.white;
            string ren = EditorGUILayout.TextField(db.actionNodes[aID].renameDialogue, intStyle);
            GUI.backgroundColor = last;
            GUI.contentColor = lastc;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set Rename Dialogue");
                db.actionNodes[aID].renameDialogue = ren;
                needSave = true;
            }
            last = GUI.contentColor;
            GUI.contentColor = VIDE_Editor_Skin.GetColor(16, db.skinIndex);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Go to Node");
            GUI.contentColor = last;
            EditorGUI.BeginChangeCheck();
            last = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            lastc = GUI.contentColor;
            GUI.contentColor = Color.white;
            int goton = EditorGUILayout.IntField(db.actionNodes[aID].gotoNode, intStyle);
            GUI.contentColor = lastc;
            GUI.backgroundColor = last;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(db, "Set goto node");
                db.actionNodes[aID].gotoNode = goton;
                needSave = true;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginVertical();

            GUI.color = defaultColor;


            if (GUILayout.Button("Reset and fetch"))
            {
                Undo.RecordObject(db, "Reset and fetch");

                var objects = Resources.FindObjectsOfTypeAll<GameObject>();
                db.actionNodes[aID].nameOpts.Clear();

                int c = 0;
                db.actionNodes[aID].nameOpts.Add("[No object]");

                foreach (GameObject g in objects)
                {
                    if (g.activeInHierarchy && checkUseful(g))
                        db.actionNodes[aID].nameOpts.Add(g.name);

                    c++;
                }

                db.actionNodes[aID].Clean();

                //Fill up methods dictionary
                var gos = Resources.FindObjectsOfTypeAll<GameObject>();
                db.actionNodes[aID].methods = new Dictionary<string, string>();
                for (int i = 0; i < gos.Length; i++)
                {
                    if (gos[i].activeInHierarchy && checkUseful(gos[i]))
                    {
                        List<MethodInfo> methodz = GetMethods(gos[i]);

                        for (int ii = 0; ii < methodz.Count; ii++)
                        {
                            if (!db.actionNodes[aID].methods.ContainsKey(gos[i].name + ii.ToString()))
                                db.actionNodes[aID].methods.Add(gos[i].name + ii.ToString(), methodz[ii].Name);
                        }
                    }
                }

                db.actionNodes[aID].opts = new string[] { "[No method]" };

                needSave = true;
            }

            GUI.color = Color.white;

            if (!db.actionNodes[aID].editorRefreshed && Event.current.type == EventType.Repaint)
            {
                db.actionNodes[aID].editorRefreshed = true;

                if (db.actionNodes[aID].nameIndex != 0)
                {
                    db.actionNodes[aID].gameObjectName = db.actionNodes[aID].nameOpts[db.actionNodes[aID].nameIndex];
                }
                else
                {
                    db.actionNodes[aID].gameObjectName = "[No object]";
                    db.actionNodes[aID].methodName = "[No method]";
                    db.actionNodes[aID].methodIndex = 0;
                    db.actionNodes[aID].paramType = -1;
                }

                if (db.actionNodes[aID].methodIndex != 0)
                {
                    db.actionNodes[aID].methodName = db.actionNodes[aID].opts[db.actionNodes[aID].methodIndex];
                }
                else
                {
                    db.actionNodes[aID].methodName = "[No method]";
                    db.actionNodes[aID].methodIndex = 0;
                    db.actionNodes[aID].paramType = -1;
                }

                Repaint();
                return;
            }

            if (db.actionNodes[aID].nameOpts.Count > 0)
            {
                EditorGUI.BeginChangeCheck();
                Color pp = GUI.contentColor;
                Color ptc = EditorStyles.popup.normal.textColor;
                EditorStyles.popup.normal.textColor = VIDE_Editor_Skin.GetColor(13, db.skinIndex);
                GUI.contentColor = Color.white;
                int idx = EditorGUILayout.Popup(db.actionNodes[aID].nameIndex, db.actionNodes[aID].nameOpts.ToArray());
                GUI.contentColor = pp;
                EditorStyles.popup.normal.textColor = ptc;
                if (EditorGUI.EndChangeCheck()) //Pick name
                {
                    Undo.RecordObject(db, "Changed name Index");
                    db.actionNodes[aID].nameIndex = idx;
                    db.actionNodes[aID].gameObjectName = db.actionNodes[aID].nameOpts[db.actionNodes[aID].nameIndex];
                    db.actionNodes[aID].methodName = "[No method]";
                    db.actionNodes[aID].methodIndex = 0;
                    db.actionNodes[aID].paramType = -1;

                    List<string> opti = new List<string>();
                    opti.Add("[No method]");

                    for (int x = 0; x < 10000; x++)
                    {
                        if (db.actionNodes[aID].methods.ContainsKey(db.actionNodes[aID].gameObjectName + x.ToString()))
                        {
                            opti.Add(db.actionNodes[aID].methods[db.actionNodes[aID].gameObjectName + x.ToString()]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    db.actionNodes[aID].opts = opti.ToArray();

                    needSave = true;
                }
            }

            EditorGUI.BeginChangeCheck();
            Color pp2 = GUI.contentColor;
            GUI.contentColor = Color.white;
            Color ptc2 = EditorStyles.popup.normal.textColor;
            EditorStyles.popup.normal.textColor = VIDE_Editor_Skin.GetColor(13, db.skinIndex);
            int meth = EditorGUILayout.Popup(db.actionNodes[aID].methodIndex, db.actionNodes[aID].opts);
            GUI.contentColor = pp2;
            EditorStyles.popup.normal.textColor = ptc2;

            if (EditorGUI.EndChangeCheck()) //Pick method
            {
                Undo.RecordObject(db, "Changed method index");
                db.actionNodes[aID].methodIndex = meth;
                db.actionNodes[aID].methodName = db.actionNodes[aID].opts[db.actionNodes[aID].methodIndex];

                GameObject ob = GameObject.Find(db.actionNodes[aID].gameObjectName);
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == db.actionNodes[aID].gameObjectName);

                foreach (GameObject g in objects)
                {
                    if (g.activeInHierarchy && checkUseful(g))
                    {
                        ob = g;
                        break;
                    }

                }

                List<MethodInfo> methods = GetMethods(ob);


                if (db.actionNodes[aID].methodIndex > 0)
                    db.actionNodes[aID].paramType = checkParam(methods[db.actionNodes[aID].methodIndex - 1]);
                else
                    db.actionNodes[aID].paramType = -1;

                needSave = true;

            }

            GUI.color = Color.white;
            GUILayout.BeginHorizontal();


            if (db.actionNodes[aID].paramType > 0)
                GUILayout.Label("Param: ", GUILayout.Width(60));

            GUIStyle intStyle = new GUIStyle(EditorStyles.textField);
            intStyle.normal.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
            intStyle.focused.textColor = (Application.HasProLicense()) ? Color.white : Color.black;
            intStyle.active.textColor = (Application.HasProLicense()) ? Color.white : Color.black;


            if (db.actionNodes[aID].paramType == 1)
            {
                EditorGUI.BeginChangeCheck();
                bool parab = EditorGUILayout.Toggle(db.actionNodes[aID].param_bool, GUILayout.Width(50));
                if (EditorGUI.EndChangeCheck()) //Pick method
                {
                    Undo.RecordObject(db, "Changed param");
                    db.actionNodes[aID].param_bool = parab;
                    needSave = true;
                }
            }

            if (db.actionNodes[aID].paramType == 2)
            {
                EditorGUI.BeginChangeCheck();
                string ps = EditorGUILayout.TextField(db.actionNodes[aID].param_string, GUILayout.Width(100));
                if (EditorGUI.EndChangeCheck()) //Pick method
                {
                    Undo.RecordObject(db, "Changed param");
                    db.actionNodes[aID].param_string = ps;
                    needSave = true;
                }
            }
            if (db.actionNodes[aID].paramType == 3)
            {
                EditorGUI.BeginChangeCheck();
     

                int pi = EditorGUILayout.IntField(db.actionNodes[aID].param_int, intStyle, GUILayout.Width(100));
                if (EditorGUI.EndChangeCheck()) //Pick method
                {
                    Undo.RecordObject(db, "Changed param");
                    db.actionNodes[aID].param_int = pi;
                    needSave = true;
                }
            }
            if (db.actionNodes[aID].paramType == 4)
            {
                EditorGUI.BeginChangeCheck();
                float pf = EditorGUILayout.FloatField(db.actionNodes[aID].param_float, intStyle, GUILayout.Width(100));
                if (EditorGUI.EndChangeCheck()) //Pick method
                {
                    Undo.RecordObject(db, "Changed param");
                    db.actionNodes[aID].param_float = pf;
                    needSave = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }

        GUILayout.Space(5);

        GUI.color = new Color(0, 0, 0, 0.2f);
        GUILayout.BeginVertical(GUI.skin.box);
        GUI.color = defaultColor;

        if (db.actionNodes[aID].pauseHere) GUI.color = Color.green; else GUI.color = Color.white;
        if (GUILayout.Button("Pause Here: " + db.actionNodes[aID].pauseHere.ToString()))
        {
            Undo.RecordObject(db, "Changed pause here");
            db.actionNodes[aID].pauseHere = !db.actionNodes[aID].pauseHere;
            needSave = true;
        }
        GUI.color = Color.white;

        GUILayout.EndVertical();

        if (!draggingNode)
        {
            db.actionNodes[aID].rectRaw = db.actionNodes[aID].rect;
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            needSave = true;
            bool hasNode = false;
            foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                if (s.aNode == db.actionNodes[aID]) { hasNode = true; break; }

            if (hasNode && !draggingNode && GUIUtility.hotControl == 0)
            {
                draggingNode = true;
            }
        }


        if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
        {
            //Select node
            if (!draggingNode)
            {
                if (Event.current.shift)
                {
                    bool hasNode = false;
                    foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                        if (s.aNode == db.actionNodes[aID]) { hasNode = true; break; }
                    if (!hasNode)
                        selectANodeDelayed = aID;
                }
                else
                {
                    db.selectedNodes = new List<VIDE_EditorDB.NodeSelection>();
                    bool hasNode = false;
                    foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                        if (s.aNode == db.actionNodes[aID]) { hasNode = true; break; }
                    if (!hasNode)
                        selectANodeDelayed = aID;
                }
                Repaint();
            }
            else
            {
                draggingNode = false;
            }
        }


        if (Event.current.commandName == "UndoRedoPerformed")
            Repaint();


        if (!lerpFocusTime && Event.current.button == 0)
        {
            bool hasNode2 = false;
            foreach (VIDE_EditorDB.NodeSelection s in db.selectedNodes)
                if (s.aNode == db.actionNodes[aID]) { hasNode2 = true; break; }

            if (position.Contains(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)) && !hasNode2)
            {
                if (Event.current.type == EventType.MouseDrag)
                    Undo.RecordObject(db, "dragg");

                GUI.DragWindow();
            }
        }


        if (!lerpFocusTime && Event.current.button == 1 && Event.current.type == EventType.MouseDown)
        {
            dragNewNode = 2;
            copiedNode = db.actionNodes[aID];
            holdingBall = false;
            balls.RemoveAt(balls.Count - 1);
            ballsGravity.RemoveAt(ballsGravity.Count - 1);
        }

    }


    bool notInBlackList(MonoBehaviour mb)
    {
        for (int i = 0; i < namespaceBlackList.Length; i++)
        {
            if (mb.GetType().Namespace != null && mb.GetType().Namespace.Contains(namespaceBlackList[i]))
                return false;

            if (mb.GetType().Name.Contains(namespaceBlackList[i]))
                return false;
        }
        return true;
    }

    bool checkUseful(GameObject g)
    {
        bool useful = false;
        var methods = new List<MethodInfo>();
        var mbs = g.GetComponents<MonoBehaviour>();

        var publicFlags = bf.Instance | bf.Public | bf.DeclaredOnly | bf.IgnoreReturn;

        foreach (MonoBehaviour mb in mbs)
        {
            if (mb != null)
                if (notInBlackList(mb))
                {
                    methods.AddRange(mb.GetType().GetMethods(publicFlags));
                }
        }

        string[] ops = GetOptions(methods);

        if (ops.Length > 1)
            useful = true;
        else
            useful = false;

        if (mbs.Length < 1)
            useful = false;

        return useful;
    }

    int checkParam(MethodInfo m)
    {
        ParameterInfo[] ps = m.GetParameters();

        if (ps.Length == 1)
        {
            if (ps[0].ParameterType == typeof(System.Boolean))
            {
                return 1;
            }
            if (ps[0].ParameterType == typeof(System.String))
            {
                return 2;
            }
            if (ps[0].ParameterType == typeof(System.Int32))
            {
                return 3;
            }
            if (ps[0].ParameterType == typeof(System.Single))
            {
                return 4;
            }
            return -1;
        }

        if (ps.Length > 1)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    string[] GetOptions(List<MethodInfo> ms)
    {
        List<string> str = new List<string>();

        str.Add("[No object]");

        foreach (MethodInfo m in ms)
        {
            ParameterInfo[] ps = m.GetParameters();

            if (ps.Length < 2 && m.ReturnType == typeof(void))
            {
                if (checkParam(m) > -1)
                    str.Add(m.Name);
            }
        }
        return str.ToArray();
    }

    List<MethodInfo> GetMethods(GameObject obj)
    {
        var methods = new List<MethodInfo>();
        var methodsFiltered = new List<MethodInfo>();

        if (obj == null) { return methods; }

        var mbs = obj.GetComponents<MonoBehaviour>();

        var publicFlags = bf.Instance | bf.Public | bf.DeclaredOnly | bf.IgnoreReturn;

        foreach (MonoBehaviour mb in mbs)
        {
            if (notInBlackList(mb))
            {
                methods.AddRange(mb.GetType().GetMethods(publicFlags));
            }
        }

        foreach (MethodInfo m in methods)
        {
            if (checkParam(m) > -1)
            {
                methodsFiltered.Add(m);
            }
        }

        return methodsFiltered;
    }

    void DrawNewFileWindow(int id)
    {
        GUI.FocusControl("createFile");
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.alignment = TextAnchor.UpperCenter;
        st.fontSize = 16;
        st.fontStyle = FontStyle.Bold;
        GUILayout.Label("Please name your new dialogue:", st);
        GUIStyle stf = new GUIStyle(GUI.skin.textField);
        stf.fontSize = 14;
        stf.alignment = TextAnchor.MiddleCenter;
        GUI.SetNextControlName("createFile");
        newFileName = GUILayout.TextField(newFileName, stf, GUILayout.Height(40));
        newFileName = Regex.Replace(newFileName, @"[^a-zA-Z0-9_$&#]", "");
        GUI.color = Color.green;
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (tryCreate(newFileName))
            {
                db.fileIndex = db.currentDiag;
                newFileName = "My Dialogue";
                scrollArea = new Vector2(4000, 4000);
                editEnabled = true;
                newFile = false;
                errorMsg = "";
                needSave = true;
                Load(true);
                Repaint();
                saveEditorSettings(db.currentDiag);
                Save();
                AssetDatabase.Refresh();
            }
            else
            {
                errorMsg = "File already exists!";
            }
        }
        if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp)
        {
            if (tryCreate(newFileName))
            {
                db.fileIndex = db.currentDiag;
                newFileName = "My Dialogue";
                editEnabled = true;
                newFile = false;
                errorMsg = "";
                needSave = true;
                Load(true);
                Repaint();
                saveEditorSettings(db.currentDiag);
                Save();
                AssetDatabase.Refresh();
                return;
            }
            else
            {
                errorMsg = "File already exists!";
            }
        }
        GUI.color = defaultColor;
        if (GUILayout.Button("Cancel", GUILayout.Height(20)) || Event.current.keyCode == KeyCode.Escape)
        {
            newFileName = "My Dialogue";
            editEnabled = true;
            newFile = false;
            errorMsg = "";
            Repaint();
        }
        st.normal.textColor = Color.red;
        GUILayout.Label(errorMsg, st);
    }

    void DrawOverwriteWindow(int id)
    {
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.alignment = TextAnchor.UpperCenter;
        st.fontSize = 16;
        st.fontStyle = FontStyle.Bold;

        if (saveNames.Count > 0)
        {
            if (File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Dialogues/" + saveNames[db.currentDiag] + ".json"))
            {
                GUILayout.Label('"' + saveNames[db.currentDiag] + '"' + " already exists! Overwrite?", st);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Yes!", GUILayout.Height(30), GUILayout.Width(position.width / 4)))
                {
                    Save();
                    needSave = false;
                    newFileName = "My Dialogue";
                    editEnabled = true;
                    overwritePopup = false;
                    newFile = false;
                    errorMsg = "";
                    saveEditorSettings(db.currentDiag);
                    return;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("No", GUILayout.Height(20), GUILayout.Width(position.width / 6)))
                {
                    newFileName = "My Dialogue";
                    editEnabled = true;
                    overwritePopup = false;
                    newFile = false;
                    errorMsg = "";
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }
        GUILayout.Label("Save as new...", st);
        GUIStyle stf = new GUIStyle(GUI.skin.textField);
        stf.fontSize = 14;
        stf.alignment = TextAnchor.MiddleCenter;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        newFileName = GUILayout.TextField(newFileName, stf, GUILayout.Height(40), GUILayout.Width(position.width / 4));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        newFileName = Regex.Replace(newFileName, @"[^a-zA-Z0-9_$&#]", "");
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Height(20), GUILayout.Width(position.width / 4)))
        {
            if (tryCreate(newFileName))
            {
                db.fileIndex = db.currentDiag;
                Load(false);
                newFileName = "My Dialogue";
                editEnabled = true;
                newFile = false;
                overwritePopup = false;
                errorMsg = "";
                needSave = true;
            }
            else
            {
                errorMsg = "File already exists!";
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Cancel", GUILayout.Height(20), GUILayout.Width(position.width / 6)))
        {
            newFileName = "My Dialogue";
            editEnabled = true;
            overwritePopup = false;
            newFile = false;
            errorMsg = "";
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        st.normal.textColor = Color.red;
        if (errorMsg != "")
            GUILayout.Label(errorMsg, st);
    }

    void DrawDeleteWindow(int id)
    {
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.alignment = TextAnchor.UpperCenter;
        st.fontSize = 16;
        st.fontStyle = FontStyle.Bold;
        GUILayout.Label("Are you sure you want to delete " + "'" + saveNames[db.fileIndex] + "'?", st);
        GUILayout.Label("LOC file will also get deleted.", st);
        GUILayout.Label("A VIDE_Assign might still have this dialogue assigned to it", st);

        if (GUILayout.Button("Yes", GUILayout.Height(30)) || Event.current.keyCode == KeyCode.Return)
        {
            DeleteDiag();
            db.fileIndex = 0;
            editEnabled = true;
            deletePopup = false;
            newFile = false;
            saveEditorSettings(db.currentDiag);
            CenterAll(false, db.startID, true);
            Repaint();
            return;
        }
        if (GUILayout.Button("No", GUILayout.Height(20)) || Event.current.keyCode == KeyCode.Escape)
        {
            editEnabled = true;
            deletePopup = false;
            newFile = false;
            Repaint();
            return;
        }
    }

    void DrawDeleteLang(int id)
    {
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.alignment = TextAnchor.UpperCenter;
        st.fontSize = 16;
        st.fontStyle = FontStyle.Bold;
        string lName = VIDE_Localization.languages[deletingLanguage].name;
        GUILayout.Label("Deleting '" + lName + "' will result in dataloss once you save the dialogue(s).", st);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("Warning: System autosaves when clicking the 'Current' button.", MessageType.Warning);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Yes", GUILayout.Height(20)) || Event.current.keyCode == KeyCode.Return)
        {
            VIDE_Localization.languages.RemoveAt(deletingLanguage);
            VIDE_Localization.currentLanguage = VIDE_Localization.defaultLanguage;
            VIDE_Localization.SaveSettings();
            LoadLocalized();
            deletingLanguage = -1;
        }
        if (GUILayout.Button("No", GUILayout.Height(40)) || Event.current.keyCode == KeyCode.Escape)
        {
            deletingLanguage = -1;
        }
    }

    void DrawLines()
    {
        Handles.color = colors[3];
        if (editEnabled)
        {
            if (draggingLine)
            {
                DrawNodeLine3(dragStart, Event.current.mousePosition);
                Repaint();
            }
            for (int i = 0; i < db.playerDiags.Count; i++)
            {
                for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
                {
                    if (db.playerDiags[i].comment[ii].outNode != null)
                    {
                        DrawNodeLine(db.playerDiags[i].comment[ii].outRect,
                        db.playerDiags[i].comment[ii].outNode.rect, db.playerDiags[i].rect);
                    }

                    if (db.playerDiags[i].comment[ii].outAction != null)
                    {
                        DrawNodeLine(db.playerDiags[i].comment[ii].outRect,
                        db.playerDiags[i].comment[ii].outAction.rect, db.playerDiags[i].rect);
                    }
                }


            }
            for (int i = 0; i < db.actionNodes.Count; i++)
            {
                if (db.actionNodes[i].outPlayer != null)
                {
                    DrawActionNodeLine(db.actionNodes[i].rect,
                    db.actionNodes[i].outPlayer.rect);
                }

                if (db.actionNodes[i].outAction != null)
                {
                    DrawActionNodeLine(db.actionNodes[i].rect,
                    db.actionNodes[i].outAction.rect);
                }
            }
        }
        repaintLines = false;

    }

    Vector3 Bezier3(Vector3 s, Vector3 st, Vector3 et, Vector3 e, float t)
    {
        return (((-s + 3 * (st - et) + e) * t + (3 * (s + et) - 6 * st)) * t + 3 * (st - s)) * t + s;
    }

    //Player Node
    void DrawNodeLine(Rect start, Rect end, Rect sPos)
    {
        Color nc = VIDE_Editor_Skin.GetColor(9, db.skinIndex);

        Vector3 startPos = new Vector3(start.x + sPos.x + 35, start.y + sPos.y + 10, 0);
        Vector3 endPos = new Vector3(end.x + 4, end.y + (end.height / 2), 0);

        float ab = Vector2.Distance(startPos, endPos);

        Vector3 startTan = startPos + Vector3.right * (ab / 3);
        Vector3 endTan = endPos + Vector3.left * (ab / 3);

        Handles.DrawBezier(startPos, endPos, startTan, endTan, nc, null, 3);


        //Draw arrow
        DrawArrow(startPos, startTan, endTan, endPos, sPos, start, true);

        if (repaintLines)
        {
            Repaint();
        }
    }

    void DrawNodeLineMap(Rect start, Rect end, Rect sPos, Rect pStart)
    {
        Color nc = VIDE_Editor_Skin.GetColor(9, db.skinIndex);

        Vector3 startPos = new Vector3(pStart.x, pStart.y, 0);
        Vector3 endPos = new Vector3(end.x + 8 / mapDivider, end.y + (end.height / 2), 0);

        float ab = Vector2.Distance(startPos, endPos);

        Vector3 startTan = startPos + Vector3.right * ((ab) / mapDivider);
        Vector3 endTan = endPos + Vector3.left * ((ab) / mapDivider);

        Handles.DrawBezier(startPos, endPos, startTan, endTan, nc, null, 1.5f);

        //Draw arrow
        DrawArrow(startPos, startTan, endTan, endPos, sPos, start, true);

        if (repaintLines)
        {
            Repaint();
        }
    }

    void DrawArrow(Vector3 startPos, Vector3 startTan, Vector3 endTan, Vector3 endPos, Rect sPos, Rect start, bool hasYpos)
    {
        Handles.BeginGUI();
        float ab = Vector2.Distance(startPos, endPos);
        if (ab < 25) return;

        float dist = 0.4f;

        Vector2 cen = Bezier3(startPos, startTan, endTan, endPos, dist);

        cen = Bezier3(startPos, startTan, endTan, endPos, dist);

        float rot = AngleBetweenVector2(cen, Bezier3(startPos, startTan, endTan, endPos, dist + 0.05f));

        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(rot + 90, new Vector2(cen.x, cen.y));
        GUI.color = VIDE_Editor_Skin.GetColor(9, db.skinIndex);
        if (spyView)
            GUI.DrawTexture(new Rect(cen.x - 20f / mapDivider, cen.y - 20f / mapDivider, 40 / mapDivider, 40 / mapDivider), lineIcon, ScaleMode.StretchToFill);
        else
            GUI.DrawTexture(new Rect(cen.x - 10, cen.y - 10, 20, 20), lineIcon, ScaleMode.StretchToFill);

        GUI.color = Color.white;
        GUI.matrix = matrixBackup;

        Handles.EndGUI();
    }

    //Action Node line
    void DrawActionNodeLine(Rect start, Rect end)
    {
        Color nc2 = VIDE_Editor_Skin.GetColor(9, db.skinIndex);

        Vector3 startPos = new Vector3(start.x + 190, start.y + 30, 0);
        Vector3 endPos = new Vector3(end.x + 4, end.y + (end.height / 2), 0);

        float ab = Vector2.Distance(startPos, endPos);

        Vector3 startTan = startPos + Vector3.right * (ab / 3);
        Vector3 endTan = endPos + Vector3.left * (ab / 3);

        Handles.DrawBezier(startPos, endPos, startTan, endTan, nc2, null, 3);

        DrawArrow(startPos, startTan, endTan, endPos, new Rect(0, 0, 0, 0), start, false);

        if (repaintLines)
        {
            Repaint();
        }
    }

    //Action Node line
    void DrawActionNodeLineMap(Rect start, Rect end, Rect sStart)
    {
        Color nc2 = VIDE_Editor_Skin.GetColor(9, db.skinIndex);

        Vector3 startPos = new Vector3(sStart.x, sStart.y, 0);
        Vector3 endPos = new Vector3(end.x + 8 / mapDivider, end.y + (end.height / 2), 0);

        float ab = Vector2.Distance(startPos, endPos);

        Vector3 startTan = startPos + Vector3.right * ((ab) / mapDivider);
        Vector3 endTan = endPos + Vector3.left * ((ab) / mapDivider);

        Handles.DrawBezier(startPos, endPos, startTan, endTan, nc2, null, 1.5f);

        DrawArrow(startPos, startTan, endTan, endPos, new Rect(0, 0, 0, 0), start, false);

        if (repaintLines)
        {
            Repaint();
        }
    }


    //Connection line
    void DrawNodeLine3(Vector2 start, Vector2 end)
    {
        Vector3 startPos = new Vector3(start.x, start.y, 0);
        Vector3 endPos = new Vector3(end.x + 4, end.y, 0);

        float ab = Vector2.Distance(startPos, endPos);

        Vector3 startTan = startPos + Vector3.right * (ab / 3);
        Vector3 endTan = endPos + Vector3.left * (ab / 3);

        Handles.DrawBezier(startPos, endPos, startTan, endTan, colors[0], null, 5);

        DrawArrow(startPos, startTan, endTan, endPos, new Rect(0, 0, 0, 0), new Rect(start.x, start.y, 0, 0), false);
    }

    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    //Clean the database
    void ClearAll()
    {
        db.playerDiags = new List<VIDE_EditorDB.DialogueNode>();
        db.actionNodes = new List<VIDE_EditorDB.ActionNode>();
    }

    void addNewNode(Vector2 pos, int type)
    {
        if (pos.y < 40) return;

        Undo.RecordObject(db, "Added Node");

        Rect viewport = canvas;
        viewport.x += scrollArea.x;
        viewport.y += scrollArea.y;
        viewport.width = position.width;
        viewport.height = position.height;

        pos.x += canvas.x + scrollArea.x;
        pos.y += canvas.y + scrollArea.y;


        switch (type)
        {
            case 1:
                db.playerDiags.Add(new VIDE_EditorDB.DialogueNode(pos, setUniqueID()));
                if (copiedNode != null)
                {
                    db.CopyLastDialogueNode(copiedNode);
                    copiedNode = null;
                }

                if (VIDE_Localization.isEnabled)
                {
                    for (int i = 0; i < VIDE_Localization.languages.Count; i++)
                    {
                        if (VIDE_Localization.languages[i].playerDiags != null)
                        {
                            VIDE_Localization.languages[i].playerDiags.Add(new VIDE_EditorDB.DialogueNode(pos, 0));
                        }
                    }
                }
                break;
            case 2:
                db.actionNodes.Add(new VIDE_EditorDB.ActionNode(pos, setUniqueID()));
                if (copiedNode != null)
                {
                    db.CopyLastActionNode(copiedNode);
                    copiedNode = null;
                }

                break;
        }
        needSave = true;

        dragNewNode = 0;
        Repaint();
    }

    void UpdateTagList()
    {
        List<string> uniqueTags = new List<string>();
        uniqueTags.Add("[Empty]");

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (!uniqueTags.Contains(db.playerDiags[i].playerTag))
                uniqueTags.Add(db.playerDiags[i].playerTag);
        }
        existingTags = uniqueTags;
    }

    void ExportTxtFile()
    {
        string fname = saveNames[db.currentDiag];
        if (VIDE_Localization.isEnabled) fname += "_" + VIDE_Localization.currentLanguage.name;

        StreamWriter sw = new StreamWriter(VIDE_EditorDB.videRoot + "/Export/" + fname + ".txt");

        sw.WriteLine("Edit the dialogue on the go.");
        sw.WriteLine("You can edit and add comments, edit the tags, and set node type.");
        sw.WriteLine("Do not attempt to add more nodes: the number of nodes must match when importing the text file.");
        sw.WriteLine("Do not modify the IDs.");
        sw.WriteLine("Do not add or remove empty lines.");
        sw.WriteLine("Do not restructure the data.");
        sw.WriteLine();
        sw.WriteLine("Dialogue name: " + saveNames[db.currentDiag]);
        sw.WriteLine("---------------------------------");

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            sw.WriteLine("#NODEID#");
            sw.WriteLine(db.playerDiags[i].ID.ToString());
            sw.WriteLine("#TAG#");
            sw.WriteLine(db.playerDiags[i].playerTag.ToString());
            sw.WriteLine("#ISPLAYER#");
            sw.WriteLine(db.playerDiags[i].isPlayer.ToString());
            sw.WriteLine("---");
            for (int c = 0; c < db.playerDiags[i].comment.Count; c++)
            {
                sw.WriteLine("#COMMENT#");
                sw.WriteLine(db.playerDiags[i].comment[c].text);
            }

            sw.WriteLine("---------------------------------");
        }

        sw.WriteLine("#END#");

        sw.Close();

    }

    void ImportTxtFile()
    {
        Undo.RecordObject(db, "Imported from txt file");

        string fname = saveNames[db.currentDiag];
        if (VIDE_Localization.isEnabled) fname += "_" + VIDE_Localization.currentLanguage.name;


        if (File.Exists(VIDE_EditorDB.videRoot + "/Export/" + fname + ".txt"))
        {
            StreamReader sr = new StreamReader(VIDE_EditorDB.videRoot + "/Export/" + fname + ".txt");

            string line;
            int index = 0;

            while ((line = sr.ReadLine()) != "#END#")
            {
                if (line == null) continue; 

                int nodeID = -1;
                string tag = "";
                bool isPlayer = false;
                List<string> comments = new List<string>();

                if (line != null && line.Contains("#NODEID#"))
                {
                    line = sr.ReadLine();
                    int.TryParse(line, out nodeID);
                } else
                {
                    continue;
                }
                line = sr.ReadLine();
                if (line != null && line.Contains("#TAG#"))
                {
                    line = sr.ReadLine();
                    tag = line;
                }
                line = sr.ReadLine();
                if (line != null && line.Contains("#ISPLAYER#"))
                {
                    line = sr.ReadLine().ToLower();
                    if (line == "true") isPlayer = true; else isPlayer = false;
                }
                line = sr.ReadLine();
                line = sr.ReadLine();
                while (line.Contains("#COMMENT#"))
                {
                    line = sr.ReadLine();
                    comments.Add(line);
                    line = sr.ReadLine();
                }

                if (nodeID != -1)
                {
                    if (db.playerDiags[index].ID != nodeID)
                    {
                        Debug.LogWarning("Node ID mismatch!");
                    } else
                    {
                        db.playerDiags[index].playerTag = tag;
                        db.playerDiags[index].isPlayer = isPlayer;

                        int removeFrom = 0;
                        for (int i = 0; i < comments.Count; i++)
                        {
                            if (i >= db.playerDiags[index].comment.Count)
                            {
                                db.playerDiags[index].comment.Add(new VIDE_EditorDB.Comment(db.playerDiags[index]));
                            }

                            db.playerDiags[index].comment[i].text = comments[i];
                            removeFrom++;
                        }
                        if (removeFrom < db.playerDiags[index].comment.Count)
                        {
                            db.playerDiags[index].comment.RemoveRange(removeFrom, db.playerDiags[index].comment.Count - removeFrom);
                        }
                    }

                    index++;
                }

            }
            sr.Close();
            needSave = true;

        } else
        {
            Debug.LogWarning("'" + fname + "' not found!");
        }

    }


}