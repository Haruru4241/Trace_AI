using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(AI))]
public class AI_Editor : Editor
{
    private AI ai;

    void OnEnable()
    {
        ai = (AI)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Layer Penalties", EditorStyles.boldLabel);

        if (ai.layerPenaltiesArray == null)
        {
            ai.layerPenaltiesArray = new List<AI.LayerPenalty>();
        }

        // Display the list of LayerPenalty
        for (int i = 0; i < ai.layerPenaltiesArray.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Layer", GUILayout.Width(50)); // Adjust label width
            ai.layerPenaltiesArray[i].layer = LayerMaskField(ai.layerPenaltiesArray[i].layer, GUILayout.Width(100)); // Adjust field width

            GUILayout.Label("Penalty", GUILayout.Width(50)); // Adjust label width
            ai.layerPenaltiesArray[i].penalty = EditorGUILayout.IntField(ai.layerPenaltiesArray[i].penalty, GUILayout.Width(50)); // Adjust field width

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                ai.layerPenaltiesArray.RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        // Button to add a new LayerPenalty
        if (GUILayout.Button("Add Layer Penalty"))
        {
            ai.layerPenaltiesArray.Add(new AI.LayerPenalty());
        }

        // Apply the changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private LayerMask LayerMaskField(LayerMask selected, params GUILayoutOption[] options)
    {
        var layers = InternalEditorUtility.layers;
        int mask = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if (((1 << LayerMask.NameToLayer(layers[i])) & selected.value) > 0)
            {
                mask |= (1 << i);
            }
        }
        mask = EditorGUILayout.MaskField(mask, layers, options);
        int newMask = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if ((mask & (1 << i)) > 0)
            {
                newMask |= (1 << LayerMask.NameToLayer(layers[i]));
            }
        }
        selected.value = newMask;
        return selected;
    }
}
