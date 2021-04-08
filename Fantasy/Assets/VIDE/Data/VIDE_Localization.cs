using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON_VIDE;
using System.IO;

public class VIDE_Localization
{
    /*
     * Do not modify or use this class
     */

    public static bool isEnabled = false;
    public static bool enabledInGame;
    public static VLanguage currentLanguage;
    public static VLanguage defaultLanguage;
    public static List<VLanguage> languages = new List<VLanguage>();

    public delegate void OnSaveLoad(bool save, bool all);

    [System.Serializable]
    public class VLanguage 
    {
        public bool enabled;
        public bool selected;
        public string name = "New Language";
        public Sprite flag;

        public List<VIDE_EditorDB.DialogueNode> playerDiags;

        public VLanguage(List<VIDE_EditorDB.DialogueNode> diag)
        {
            playerDiags = diag;
        }

        public VLanguage()
        {
            name = "New Language";
        }
    }

    public static void DeselectAll()
    {
        foreach (VLanguage v in languages)
            v.selected = false;
    }

    public class SerializeHelper
    {
        static string fileDataPath = Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/";
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

    public static VLanguage AddLang()
    {
        languages.Add(new VIDE_Localization.VLanguage(defaultLanguage.playerDiags));
        currentLanguage = languages[languages.Count - 1];
        SaveSettings();
        return currentLanguage;
    }

    static VLanguage SetDefault()
    {
        languages = new List<VLanguage>();
        languages.Add(new VLanguage());
        languages[0].enabled = true;
        languages[0].selected = true;
        languages[0].name = "English";
        currentLanguage = languages[0];
        defaultLanguage = languages[0];
        isEnabled = true;
        SaveSettings();
        return defaultLanguage;
    }

    public static VLanguage LoadSettings()
    {
        Dictionary<string, object> dict;

        if (!Application.isPlaying)
        {
            if (!File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/LocalizationSettings.json"))
            {
                if (File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/demo_loc.json"))
                {
                    dict = SerializeHelper.ReadFromFile("demo_loc" + ".json") as Dictionary<string, object>;
                }
                else
                {
                    return SetDefault();
                }
            }
            else
            {
                dict = SerializeHelper.ReadFromFile("LocalizationSettings" + ".json") as Dictionary<string, object>;
            }
        }
        else
        {
            string jsonString = "";

            if (GameObject.Find("CanvasLocDemo") != null)
                if (Resources.Load<TextAsset>("demo_loc") != null)
                    jsonString = Resources.Load<TextAsset>("demo_loc").text;
                else Debug.LogError("No demo_loc.json found in Resources!");

            if (jsonString == "")
                if (Resources.Load<TextAsset>("LocalizationSettings") != null)
                {
                    jsonString = Resources.Load<TextAsset>("LocalizationSettings").text;
                }
                else
                {
                    return null;
                }
            dict = MiniJSON_VIDE.DiagJson.Deserialize(jsonString) as Dictionary<string, object>;
        }

        enabledInGame = (bool)dict["enabledInGame"];
        languages = new List<VLanguage>();
        int langs = (int)((long)dict["langs"]);
        for (int i = 0; i < langs; i++)
        {
            languages.Add(new VLanguage());
            languages[i].enabled = (bool)dict["langEnabled_" + i.ToString()];
            languages[i].name = (string)dict["lang_Name" + i.ToString()];
        }
        if ((int)((long)dict["current"]) != -1)
            currentLanguage = languages[(int)((long)dict["current"])];
        if ((int)((long)dict["default"]) != -1)
            defaultLanguage = languages[(int)((long)dict["default"])];

        if (defaultLanguage != null)
            defaultLanguage.selected = true;

        return defaultLanguage;
    }


    public static void SaveSettings()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("enabledInGame", enabledInGame);

        if (currentLanguage != null)
            dict.Add("current", languages.IndexOf(currentLanguage));
        else
            dict.Add("current", -1);

        if (defaultLanguage != null)
            dict.Add("default", languages.IndexOf(defaultLanguage));
        else
            dict.Add("default", -1);

        dict.Add("langs", languages.Count);
        for (int i = 0; i < languages.Count; i++)
        {
            dict.Add("langEnabled_" + i.ToString(), languages[i].enabled);
            dict.Add("lang_Name" + i.ToString(), languages[i].name);
        }
        SerializeHelper.WriteToFile(dict as Dictionary<string, object>, "LocalizationSettings" + ".json");

    }

    public static void LoadLanguages(string dName, bool onlyLoadDef)
    {
        Dictionary<string, object> dict;

        if (!Application.isPlaying)
        {
            if (!File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/" + "LOC_" + dName + ".json"))
            {
                for (int i = 0; i < languages.Count; i++)
                {
                    languages[i].playerDiags = null;
                }
                return;
            }
            else
            {
                string fileDataPath = Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/";
                string jsonString = File.ReadAllText(fileDataPath + "LOC_" + dName + ".json");
                dict = DiagJson.Deserialize(jsonString) as Dictionary<string, object>;
            }
        }
        else
        {
            if (Resources.Load<TextAsset>("Localized/" + "LOC_" + dName) == null)
            {
                foreach (VLanguage l in languages)
                    l.playerDiags = null;

                return;
            }

            string jstr = Resources.Load<TextAsset>("Localized/" + "LOC_" + dName).text;
            dict = MiniJSON_VIDE.DiagJson.Deserialize(jstr) as Dictionary<string, object>;
        }


        for (int d = 0; d < languages.Count; d++)
        {
            if (onlyLoadDef)
                if (languages[d] != defaultLanguage) continue;

            string lang = languages[d].name + "_";
            Sprite[] sprites = Resources.LoadAll<Sprite>("");
            AudioClip[] audios = Resources.LoadAll<AudioClip>("");
            List<string> spriteNames = new List<string>();
            List<string> audioNames = new List<string>();
            foreach (Sprite t in sprites)
                spriteNames.Add(t.name);
            foreach (AudioClip t in audios)
                audioNames.Add(t.name);

            if (!dict.ContainsKey(lang + "playerDiags")) continue;

            int pDiags = (int)((long)dict[lang + "playerDiags"]);

            if (pDiags > 0) languages[d].playerDiags = new List<VIDE_EditorDB.DialogueNode>();

            for (int i = 0; i < pDiags; i++)
            {
                languages[d].playerDiags.Add(new VIDE_EditorDB.DialogueNode());
                VIDE_EditorDB.DialogueNode c = languages[d].playerDiags[i];

                if (!dict.ContainsKey(lang + "pd_pTag_" + i.ToString())) continue;

                c.playerTag = (string)dict[lang + "pd_pTag_" + i.ToString()];
                int cSize = (int)((long)dict[lang + "pd_comSize_" + i.ToString()]);

                string name = Path.GetFileNameWithoutExtension((string)dict[lang + "pd_sprite_" + i.ToString()]);
                if (spriteNames.Contains(name))
                    c.sprite = sprites[spriteNames.IndexOf(name)];
                else if (name != string.Empty)
                    Debug.LogError("'" + name + "' not found in any Resources folder!");

                for (int ii = 0; ii < cSize; ii++)
                {
                    c.comment.Add(new VIDE_EditorDB.Comment());

                    c.comment[ii].text = (string)dict[lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "text"];

                    string namec = Path.GetFileNameWithoutExtension((string)dict[lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"]);

                    if (spriteNames.Contains(namec))
                        c.comment[ii].sprites = sprites[spriteNames.IndexOf(namec)];
                    else if (namec != "")
                        Debug.LogError("'" + namec + "' not found in any Resources folder!");

                    namec = Path.GetFileNameWithoutExtension((string)dict[lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"]);

                    if (audioNames.Contains(namec))
                        c.comment[ii].audios = audios[audioNames.IndexOf(namec)];
                    else if (namec != "")
                        Debug.LogError("'" + namec + "' not found in any Resources folder!");
                }
            }
        }
    }

    public static List<VLanguage> PreloadLanguages(string dName)
    {
        Dictionary<string, object> dict;
        List<VLanguage> langs = new List<VLanguage>();

        if (!Application.isPlaying)
        {
            if (!File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/LocalizationSettings.json"))
            {
                if (File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/demo_loc.json"))
                {
                    dict = SerializeHelper.ReadFromFile("demo_loc" + ".json") as Dictionary<string, object>;
                }
                else
                {
                    Debug.LogError("No localization settings found");
                    return null;
                }
            }
            else
            {
                dict = SerializeHelper.ReadFromFile("LocalizationSettings" + ".json") as Dictionary<string, object>;
            }
        } else
        {
            return null;
        }

        int langCount = (int)((long)dict["langs"]);
        for (int i = 0; i < langCount; i++)
        {
            langs.Add(new VLanguage());
            langs[i].enabled = (bool)dict["langEnabled_" + i.ToString()];
            langs[i].name = (string)dict["lang_Name" + i.ToString()];
        }

        if (!Application.isPlaying)
        {
            if (!File.Exists(Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/" + "LOC_" + dName + ".json"))
            {
                //Debug.LogWarning("No localization file found");
                return null;
            }
            else
            {
                string fileDataPath = Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/";
                string jsonString = File.ReadAllText(fileDataPath + "LOC_" + dName + ".json");
                dict = DiagJson.Deserialize(jsonString) as Dictionary<string, object>;
            }
        }

        for (int d = 0; d < langs.Count; d++)
        {
            string lang = langs[d].name + "_";
            Sprite[] sprites = Resources.LoadAll<Sprite>("");
            AudioClip[] audios = Resources.LoadAll<AudioClip>("");
            List<string> spriteNames = new List<string>();
            List<string> audioNames = new List<string>();
            foreach (Sprite t in sprites)
                spriteNames.Add(t.name);
            foreach (AudioClip t in audios)
                audioNames.Add(t.name);

            if (!dict.ContainsKey(lang + "playerDiags")) continue;

            int pDiags = (int)((long)dict[lang + "playerDiags"]);

            if (pDiags > 0) langs[d].playerDiags = new List<VIDE_EditorDB.DialogueNode>();

            for (int i = 0; i < pDiags; i++)
            {
                langs[d].playerDiags.Add(new VIDE_EditorDB.DialogueNode());
                VIDE_EditorDB.DialogueNode c = langs[d].playerDiags[i];

                if (!dict.ContainsKey(lang + "pd_pTag_" + i.ToString())) continue;

                c.playerTag = (string)dict[lang + "pd_pTag_" + i.ToString()];
                int cSize = (int)((long)dict[lang + "pd_comSize_" + i.ToString()]);

                string name = Path.GetFileNameWithoutExtension((string)dict[lang + "pd_sprite_" + i.ToString()]);
                if (spriteNames.Contains(name))
                    c.sprite = sprites[spriteNames.IndexOf(name)];
                else if (name != string.Empty)
                    Debug.LogError("'" + name + "' not found in any Resources folder!");

                for (int ii = 0; ii < cSize; ii++)
                {
                    c.comment.Add(new VIDE_EditorDB.Comment());

                    c.comment[ii].text = (string)dict[lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "text"];

                    string namec = Path.GetFileNameWithoutExtension((string)dict[lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite"]);

                    if (spriteNames.Contains(namec))
                        c.comment[ii].sprites = sprites[spriteNames.IndexOf(namec)];
                    else if (namec != "")
                        Debug.LogError("'" + namec + "' not found in any Resources folder!");

                    namec = Path.GetFileNameWithoutExtension((string)dict[lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "audio"]);

                    if (audioNames.Contains(namec))
                        c.comment[ii].audios = audios[audioNames.IndexOf(namec)];
                    else if (namec != "")
                        Debug.LogError("'" + namec + "' not found in any Resources folder!");
                }
            }
        }

        return langs;
    }


    public static void SaveLanguages(string currentDiag)
    {
#if UNITY_EDITOR
        Dictionary<string, object> dict = new Dictionary<string, object>();
        List<string> sameNames = new List<string>();

        for (int d = 0; d < languages.Count; d++)
        {
            string lang = languages[d].name + "_";
            if (!sameNames.Contains(lang)) sameNames.Add(lang); else continue;


            if (languages[d].playerDiags != null)
                dict.Add(lang + "playerDiags", languages[d].playerDiags.Count);

            if (languages[d].playerDiags != null)
                for (int i = 0; i < languages[d].playerDiags.Count; i++)
                {
                    dict.Add(lang + "pd_pTag_" + i.ToString(), languages[d].playerDiags[i].playerTag);
                    dict.Add(lang + "pd_comSize_" + i.ToString(), languages[d].playerDiags[i].comment.Count);

                    if (languages[d].playerDiags[i].sprite != null)
                        dict.Add(lang + "pd_sprite_" + i.ToString(), UnityEditor.AssetDatabase.GetAssetPath(languages[d].playerDiags[i].sprite));
                    else
                        dict.Add(lang + "pd_sprite_" + i.ToString(), string.Empty);

                    for (int ii = 0; ii < languages[d].playerDiags[i].comment.Count; ii++)
                    {
                        dict.Add(lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "text", languages[d].playerDiags[i].comment[ii].text);

                        if (languages[d].playerDiags[i].comment[ii].audios != null)
                            dict.Add(lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "audio", UnityEditor.AssetDatabase.GetAssetPath(languages[d].playerDiags[i].comment[ii].audios));
                        else
                            dict.Add(lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "audio", string.Empty);

                        if (languages[d].playerDiags[i].comment[ii].sprites != null)
                            dict.Add(lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite", UnityEditor.AssetDatabase.GetAssetPath(languages[d].playerDiags[i].comment[ii].sprites));
                        else
                            dict.Add(lang + "pd_" + i.ToString() + "_com_" + ii.ToString() + "sprite", string.Empty);

                    }
                }

        }


        string fileDataPath = Application.dataPath + "/../" + VIDE_EditorDB.videRoot + "/Resources/Localized/";

        string outString = DiagJson.Serialize(dict as Dictionary<string, object>);
        File.WriteAllText(fileDataPath + "LOC_" + currentDiag + ".json", outString);
#endif
    }

}
