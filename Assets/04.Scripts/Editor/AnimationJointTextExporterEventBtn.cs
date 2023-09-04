using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationJointTextExporter))]
public class AnimationJointTextExporterEventBtn : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AnimationJointTextExporter generator = (AnimationJointTextExporter)target;
        if (GUILayout.Button("Add Animation Clip Data"))
        {
            generator.AddAnimationClipData();
        }
        if (GUILayout.Button("Export Animation Key Frame To Text"))
        {
            generator.ExportAnimationKeyFrameToText();
            
        }
    }
}
