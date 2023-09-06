using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoboAgent_ver2))]
public class RoboAgent_ver2EventBtn : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RoboAgent_ver2 generator = (RoboAgent_ver2)target;
        if (GUILayout.Button("Load Animation Data From Text"))
        {
            generator.LoadAnimationDataFromText();
        }
    }
}
