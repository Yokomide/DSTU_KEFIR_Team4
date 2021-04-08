using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using MiniJSON_VIDE;

[CanEditMultipleObjects]
[CustomEditor(typeof(VIDE_Assign))]
public class VIDE_AssignC : Editor
{
    /*
     * Custom Inspector for the VIDE_Assign component
     */

    VIDE_Assign d;
    bool loadup = false;

    static string path = "";
    List<string> fullPaths = new List<string>();

    bool searching = false;
    string diagSearch = "";
    List<string> results = new List<string>();

    private void openVIDE_Editor(string idx)
    {
        if (d != null)
            loadFiles();

        VIDE_Editor editor = EditorWindow.GetWindow<VIDE_Editor>();
        editor.Init(idx, true);
    }

    void OnEnable()
    {
        loadup = true;

        path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        path = Directory.GetParent(path).ToString();
        path = Directory.GetParent(path).ToString();
        path = Directory.GetParent(path).ToString();

        loadFiles();
    }

    bool HasUniqueID(int id, string[] saveNames, int currentDiag)
    {
        //Retrieve all IDs
        foreach (string s in saveNames)
        {
            if (s == saveNames[currentDiag]) continue;

            if (File.Exists(Application.dataPath + "/../" + s))
            {
                Dictionary<string, object> dict = SerializeHelper.ReadFromFile(s) as Dictionary<string, object>;
                if (dict.ContainsKey("dID"))
                    if (id == ((int)((long)dict["dID"])))
                        return false;
            }
        }
        return true;
    }

    int AssignDialogueID(string[] saveNames)
    {
        List<int> ids = new List<int>();
        int newID = UnityEngine.Random.Range(0, 99999);

        //Retrieve all IDs
        foreach (string s in saveNames)
        {
            if (File.Exists(Application.dataPath + "/../" + s))
            {
                Dictionary<string, object> dict = SerializeHelper.ReadFromFile(s) as Dictionary<string, object>;
                if (dict.ContainsKey("dID"))
                    ids.Add((int)((long)dict["dID"]));
            }
        }

        //Make sure ID is unique
        while (ids.Contains(newID))
        {
            newID = UnityEngine.Random.Range(0, 99999);
        }

        return newID;
    }

    public class SerializeHelper
    {
        static string fileDataPath = Application.dataPath + "/../";

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
    }

    public override void OnInspectorGUI()
    {
        d = (VIDE_Assign)target;
        Color defColor = GUI.color;
        GUI.color = Color.yellow;

        if (loadup)
        {
            loadFiles();
            loadup = false;
        }

        if (searching)
        {
            ShowSearch();
            return;
        }

        //Create a button to open up the VIDE Editor and load the currently assigned dialogue
        if (GUILayout.Button("Open VIDE Editor"))
        {
            openVIDE_Editor(d.assignedDialogue);
        }

        GUI.color = defColor;

        //Refresh dialogue list
        if (Event.current.type == EventType.MouseDown)
        {
            if (d != null)
                loadFiles();
        }

        GUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent("Assigned dialogue", "Which dialogue is this NPC going to own?"));
        if (d.diags.Count > 0)
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(d, "Changed dialogue index");
            d.assignedIndex = EditorGUILayout.Popup(d.assignedIndex, d.diags.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                PreloadDialogue(false);
                int theID = 0;
                int currentName = -1;

                /* Get file location based on name */
                for (int i = 0; i < d.diags.Count; i++)
                {
                    if (fullPaths[i].Contains(d.diags[d.assignedIndex] + ".json"))
                        currentName = i;
                }

                if (currentName == -1)
                {
                    return;
                }

                if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
                {
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        theID = ((int)((long)dict["dID"]));

                    }
                    else Debug.LogError("Could not read dialogue ID!");
                }

                if (!HasUniqueID(theID, fullPaths.ToArray(), currentName))
                {
                    theID = AssignDialogueID(fullPaths.ToArray());
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        dict["dID"] = theID;
                    }
                    SerializeHelper.WriteToFile(dict as Dictionary<string, object>, fullPaths[currentName]);
                }

                d.assignedID = theID;
                d.assignedDialogue = d.diags[d.assignedIndex];


                foreach (var transform in Selection.transforms)
                {
                    VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                    scr.assignedIndex = d.assignedIndex;
                    scr.assignedDialogue = d.assignedDialogue;
                    scr.assignedID = d.assignedID;
                }

            }
        }
        else
        {
            GUILayout.Label("No saved Dialogues!");
        }

        if (GUILayout.Button("Search", EditorStyles.miniButton))
        {
            searching = true;
            diagSearch = "";
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent("Alias", "Custom alias for this dialogue"));

        Undo.RecordObject(d, "Changed custom name");
        EditorGUI.BeginChangeCheck();
        d.alias = EditorGUILayout.TextField(d.alias);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.alias = d.alias;
            }
        }

        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Override Start node", "Dialogue will instead begin on the node with this ID"));
        Undo.RecordObject(d, "Changed override start node");
        EditorGUI.BeginChangeCheck();
        d.overrideStartNode = EditorGUILayout.IntField(d.overrideStartNode);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.overrideStartNode = d.overrideStartNode;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        d.defaultPlayerSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Default Player Sprite", "Default player sprite for this component"), d.defaultPlayerSprite, typeof(Sprite), false);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.defaultPlayerSprite = d.defaultPlayerSprite;
            }
        }

        EditorGUI.BeginChangeCheck();
        d.defaultNPCSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Default NPC Sprite", "Default NPC sprite for this component"), d.defaultNPCSprite, typeof(Sprite), false);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.defaultNPCSprite = d.defaultNPCSprite;
            }
        }
        GUILayout.Label(new GUIContent("Interaction Count: " + d.interactionCount.ToString(), "How many times have we interacted with this NPC?"));

        GUILayout.BeginVertical(GUI.skin.box);

        if (!d.preload)
        {
            if (d.assignedDialogue == "" || d.assignedIndex == -1) GUI.enabled = false;
            if (GUILayout.Button("Preload dialogue"))
            {
                PreloadDialogue(true);
            }
            GUI.enabled = true;
            EditorGUILayout.HelpBox("The dialogue will be preloaded for all VAs and won't require loading from json, eliminating loading times.\nMake sure you preload again if you make changes to the dialogue!", MessageType.Info);
        } else
        {
            GUI.color = Color.green;
            if (GUILayout.Button("Unload"))
            {
                PreloadDialogue(false);
            }
            GUI.color = Color.white;

            string helptext = "Dialogue preloaded.";
            if (d.playerDiags != null) helptext += "\nDialogue Nodes: " + d.playerDiags.Count.ToString(); else helptext += "\nDialogue Nodes: 0";
            if (d.actionNodes != null) helptext += "\nAction Nodes: " + d.actionNodes.Count.ToString(); else helptext += "\nAction Nodes: 0";
            if (d.langs != null) helptext += "\nLanguages: " + d.langs.Count.ToString(); else helptext += "\nLanguages: 0";
            EditorGUILayout.HelpBox(helptext, MessageType.Info);
            if (d.notuptodate)
            {
                EditorGUILayout.HelpBox("You've made changes to the dialogue. Make sure you preload again.", MessageType.Warning);
            }
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();

        if (!Application.isPlaying || d.assignedIndex == -1 || d.targetManager == null) GUI.enabled = false;
        if (GUILayout.Button(new GUIContent("Test Interact", "Select dialogue, target gameobject, and enter play mode.")))
        {
            d.targetManager.SendMessage("Interact", d, SendMessageOptions.RequireReceiver);
        }
        GUI.enabled = true;

        d.targetManager = (GameObject)EditorGUILayout.ObjectField(d.targetManager, typeof(GameObject), true);

        GUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("You can select a gameobject containing a UI Manager and press 'Test Interact' during PlayMode to test this dialogue without requiring a dialogue trigger." +
            "\nUI Manager must contain an 'Interact' method like in Template_UIManager.cs", MessageType.Info);
        GUILayout.EndVertical();
    }

    Vector2 scrollpos = new Vector2();
    public void ShowSearch()
    {
        GUI.color = Color.white;
        GUILayout.BeginHorizontal(GUI.skin.box);
        if (GUILayout.Button("Back"))
        {
            searching = false;
        }
        diagSearch = EditorGUILayout.TextField(diagSearch);
        GUILayout.EndHorizontal();
        results.Clear();
        for (int i = 0; i < d.diags.Count; i++)
        {
            if (d.diags[i].ToLower().Contains(diagSearch.ToLower()))
            {
                results.Add(d.diags[i]);
            }
        }
        scrollpos = GUILayout.BeginScrollView(scrollpos, GUI.skin.box, GUILayout.Height(200));
        for (int i = 0; i < results.Count; i++)
        {
            if (GUILayout.Button(results[i], EditorStyles.miniButton))
            {
                PreloadDialogue(false);
                int theID = 0;
                int currentName = -1;

                d.assignedIndex = d.diags.IndexOf(results[i]);

                /* Get file location based on name */
                for (int i2 = 0; i2 < d.diags.Count; i2++)
                {
                    if (fullPaths[i2].Contains(d.diags[d.assignedIndex] + ".json"))
                        currentName = i2;
                }

                if (currentName == -1)
                {
                    return;
                }

                if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
                {
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        theID = ((int)((long)dict["dID"]));

                    }
                    else Debug.LogError("Could not read dialogue ID!");
                }

                if (!HasUniqueID(theID, fullPaths.ToArray(), currentName))
                {
                    theID = AssignDialogueID(fullPaths.ToArray());
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        dict["dID"] = theID;
                    }
                    SerializeHelper.WriteToFile(dict as Dictionary<string, object>, fullPaths[currentName]);
                }

                d.assignedID = theID;
                d.assignedDialogue = d.diags[d.assignedIndex];


                foreach (var transform in Selection.transforms)
                {
                    VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                    scr.assignedIndex = d.assignedIndex;
                    scr.assignedDialogue = d.assignedDialogue;
                    scr.assignedID = d.assignedID;
                }
                searching = false;
            }
        }

        GUILayout.EndScrollView();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    //Refresh dialogue list
    public void OnFocus()
    {
        if (d != null)
            loadFiles();
    }

    //Refresh dialogue list
    public void loadFiles()
    {
        AssetDatabase.Refresh();
        d = (VIDE_Assign)target;

        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        d.diags = new List<string>();
        fullPaths = new List<string>();

        if (files.Length < 1) return;

        foreach (TextAsset f in files)
        {
            d.diags.Add(f.name);
            fullPaths.Add(AssetDatabase.GetAssetPath(f));
        }

        d.diags.Sort();

        //Lets make sure we still have the right file
        IDCheck();
        Repaint();

    }

    void IDCheck()
    {
        int theID = 0;
        List<int> theIDs = new List<int>();
        if (d.assignedIndex == -1)
        {
            if (d.assignedDialogue != "" && d.diags.Contains(d.assignedDialogue))
            {
                d.assignedIndex = d.diags.IndexOf(d.assignedDialogue);
            }
            else
            {
                return;
            }
        }

        if (d.assignedIndex >= d.diags.Count)
        {
            for (int i = 0; i < d.diags.Count; i++)
            {
                    if (d.diags[i] == d.assignedDialogue)
                {
                    d.assignedIndex = i;
                }
            }
        }

        int currentName = -1;

        /* Get file location based on name */
        for (int i = 0; i < d.diags.Count; i++)
        {
            if (fullPaths[i].Contains(d.diags[d.assignedIndex] + ".json"))
                currentName = i;
        }

        if (currentName == -1)
        {
            return;
        }

        if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
        {
            Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
            if (dict.ContainsKey("dID"))
            {
                theID = ((int)((long)dict["dID"]));
            }
            else { Debug.LogError("Could not read dialogue ID!"); return; }
        }

        if (theID != d.assignedID)
        {

            foreach (string s in d.diags)
            {
                for (int i = 0; i < d.diags.Count; i++)
                {
                    if (fullPaths[i].Contains(d.diags[d.diags.IndexOf(s)] + ".json"))
                        currentName = i;
                }

                if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
                {
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                        theIDs.Add((int)((long)dict["dID"]));
                }
            }
            var theRealID_Index = theIDs.IndexOf(d.assignedID);

            d.assignedIndex = theRealID_Index;

            if (d.assignedIndex != -1)
                d.assignedDialogue = d.diags[d.assignedIndex];
        }
    }

    
    void PreloadDialogue(bool preload)
    {
        if (preload)
        {
            IDCheck();

            VIDE_Data.Diags diag = VIDE_Data.VD.PreloadLoad(d.assignedDialogue);

            d.playerDiags = diag.playerNodes;
            d.actionNodes = diag.actionNodes;
            d.loadtag = diag.loadTag;
            d.startp = diag.start;
            d.preload = true;

            VIDE_EditorDB.videRoot = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
            VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
            VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();

            d.langs = VIDE_Localization.PreloadLanguages(d.assignedDialogue);
            d.notuptodate = false;

        }
        else
        {
            d.playerDiags = new List<VIDE_Data.DialogueNode>();
            d.actionNodes = new List<VIDE_Data.ActionNode>();
            d.langs = new List<VIDE_Localization.VLanguage>() ;
            d.loadtag = "";
            d.startp = 0;
            d.preload = false;
            d.notuptodate = false;
        }
       
    }




}
