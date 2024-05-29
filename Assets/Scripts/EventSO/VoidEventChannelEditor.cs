using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(VoidEventChannelSO))]

public class VoidEventChannelSOEditor : Editor
{
    [Multiline]
    public string Description = "Custom";


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        VoidEventChannelSO eventChannel = (VoidEventChannelSO)target;

        Description = GUILayout.TextArea(Description, 400);

        if (GUILayout.Button("Raise Event"))
        {
            eventChannel.RaiseEvent();
        }
    }
}

#endif