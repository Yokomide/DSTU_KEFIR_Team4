using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VIDE_Editor_Skin : MonoBehaviour
{

    public GUIStyle mm_box_default;
    public GUIStyle mm_labels;
    public GUISkin ActionSkin;
    public GUIStyle windowStyle;

    public List<Skin> skins = new List<Skin>();
    public static VIDE_Editor_Skin instance;

    [System.Serializable]
    public class Skin
    {
        public string name;
        public Color player_NodeColor;
        public Color player_NodeColorSecondary;
        public Color npc_NodeColor;
        public Color npc_NodeColorSecondary;
        public Color action_NodeColor;
        public Color action_NodeColorSecondary;
        public Color background_color;
        public Color grid_color;
        public Color connectors_color;
        public Color playerText;
        public Color npcText;
        public Color actionText;
        public Color playerText2;
        public Color npcText2;
        public Color actionText2;

        public Color def_player_NodeColor;
        public Color def_player_NodeColorSecondary;
        public Color def_npc_NodeColor;
        public Color def_npc_NodeColorSecondary;
        public Color def_action_NodeColor;
        public Color def_action_NodeColorSecondary;
        public Color def_background_color;
        public Color def_grid_color;
        public Color def_connectors_color;
        public Color def_playerText;
        public Color def_npcText;
        public Color def_actionText;
        public Color def_playerText2;
        public Color def_npcText2;
        public Color def_actionText2;
    }

    public static string[] GetNames()
    {
        List<string> names = new List<string>();
        foreach (Skin s in instance.GetComponent<VIDE_Editor_Skin>().skins)
        {
            names.Add(s.name);
        }
        return names.ToArray();
    }

    public static void SetDefault(int index)
    {
        instance.skins[index].def_player_NodeColor = instance.skins[index].player_NodeColor;
        instance.skins[index].def_player_NodeColorSecondary = instance.skins[index].player_NodeColorSecondary;
        instance.skins[index].def_npc_NodeColor = instance.skins[index].npc_NodeColor;
        instance.skins[index].def_npc_NodeColorSecondary = instance.skins[index].npc_NodeColorSecondary;
        instance.skins[index].def_action_NodeColor = instance.skins[index].action_NodeColor;
        instance.skins[index].def_action_NodeColorSecondary = instance.skins[index].action_NodeColorSecondary;
        instance.skins[index].def_background_color = instance.skins[index].background_color;
        instance.skins[index].def_grid_color = instance.skins[index].grid_color;
        instance.skins[index].def_connectors_color = instance.skins[index].connectors_color;
        instance.skins[index].def_playerText = instance.skins[index].playerText;
        instance.skins[index].def_npcText = instance.skins[index].npcText;
        instance.skins[index].def_actionText = instance.skins[index].actionText;
        instance.skins[index].def_playerText2 = instance.skins[index].playerText2;
        instance.skins[index].def_npcText2 = instance.skins[index].npcText2;
        instance.skins[index].def_actionText2 = instance.skins[index].actionText2;
    }

    public static void Reset(int index)
    {
        instance.skins[index].player_NodeColor = instance.skins[index].def_player_NodeColor;
        instance.skins[index].player_NodeColorSecondary = instance.skins[index].def_player_NodeColorSecondary;
        instance.skins[index].npc_NodeColor = instance.skins[index].def_npc_NodeColor;
        instance.skins[index].npc_NodeColorSecondary = instance.skins[index].def_npc_NodeColorSecondary;
        instance.skins[index].action_NodeColor = instance.skins[index].def_action_NodeColor;
        instance.skins[index].action_NodeColorSecondary = instance.skins[index].def_action_NodeColorSecondary;
        instance.skins[index].background_color = instance.skins[index].def_background_color;
        instance.skins[index].grid_color = instance.skins[index].def_grid_color;
        instance.skins[index].connectors_color = instance.skins[index].def_connectors_color;
        instance.skins[index].playerText = instance.skins[index].def_playerText;
        instance.skins[index].npcText = instance.skins[index].def_npcText;
        instance.skins[index].actionText = instance.skins[index].def_actionText;
        instance.skins[index].playerText2 = instance.skins[index].def_playerText2;
        instance.skins[index].npcText2 = instance.skins[index].def_npcText2;
        instance.skins[index].actionText2 = instance.skins[index].def_actionText2;
    }

    public static Color GetColor(int type, int index)
    {
        switch (type)
        {
            case 0:
                return instance.skins[index].player_NodeColor;
            case 1:
                return instance.skins[index].player_NodeColorSecondary;
            case 2:
                return instance.skins[index].npc_NodeColor;
            case 3:
                return instance.skins[index].npc_NodeColorSecondary;
            case 4:
                return instance.skins[index].action_NodeColor;
            case 5:
                return instance.skins[index].action_NodeColorSecondary;
            case 7:
                return instance.skins[index].background_color;
            case 8:
                return instance.skins[index].grid_color;
            case 9:
                return instance.skins[index].connectors_color;
            case 11:
                return instance.skins[index].playerText;
            case 12:
                return instance.skins[index].npcText;
            case 13:
                return instance.skins[index].actionText;
            case 14:
                return instance.skins[index].playerText2;
            case 15:
                return instance.skins[index].npcText2;
            case 16:
                return instance.skins[index].actionText2;
        }

        return Color.black;
    }

}
