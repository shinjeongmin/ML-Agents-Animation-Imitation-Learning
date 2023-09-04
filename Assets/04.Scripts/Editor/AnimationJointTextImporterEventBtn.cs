using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationJointTextImporter))]
public class AnimationJointTextImporterEventBtn : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AnimationJointTextImporter generator = (AnimationJointTextImporter)target;
        if (GUILayout.Button("Load Animation Data From Text"))
        {
            generator.LoadAnimationDataFromText();
        }
        if (GUILayout.Button("Apply Current Frame Animation Data To Model"))
        {
            generator.ApplyCurrentFrameAnimationDataToModel();
        }
        if (GUILayout.Button("Apply Next Frame Animation Data To Model"))
        {
            generator.ApplyNextFrameAnimationDataToModel();
        }
    }
}
