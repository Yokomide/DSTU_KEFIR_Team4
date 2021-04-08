using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using VIDE_Data;

[CustomEditor(typeof(VD))]
public class VDE : Editor
{
    Vector2 scrollPos = new Vector2();

    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    public override void OnInspectorGUI()
    {

        GUIStyle b = new GUIStyle(GUI.skin.GetStyle("Label"));
        b.fontStyle = FontStyle.Bold;

        if (EditorApplication.isPlaying)
        {

            if (VD.isActive)
            {
                GUILayout.Box("Active: " + VD.saved[VD.currentDiag].name, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Box("No dialogue Active", GUILayout.ExpandWidth(true));
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.GetStyle("Box"), GUILayout.ExpandWidth(true), GUILayout.Height(400));
            for (int i = 0; i < VD.saved.Count; i++)
            {
                if (!VD.saved[i].loaded)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(i.ToString() + ". " + VD.saved[i].name + ": NOT LOADED");
                    if (VD.isActive) GUI.enabled = false;
                    if (GUILayout.Button("Load!")) VD.LoadDialogues(VD.saved[i].name);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();

                }
                else
                {
                    EditorGUILayout.LabelField(i.ToString() + ". " + VD.saved[i].name + ": LOADED", b);
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (VD.isActive) GUI.enabled = false;

            if (GUILayout.Button("Load All"))
            {
                VD.LoadDialogues();
            }
            if (GUILayout.Button("Unload All"))
            {
                VD.UnloadDialogues();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

        } else
        {
            GUILayout.Box("Enter PlayMode to display loaded/unloaded information", GUILayout.MaxWidth(300));
        }


    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}
