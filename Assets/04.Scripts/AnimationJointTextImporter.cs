using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnimationJointTextImporter : MonoBehaviour
{
    public Animator animator;
    public AnimDataListClass animDataList = new AnimDataListClass();

    [Header("Write text save path and name")]
    public string textSavePath;
    public string textFileName;
    // text content buffer
    public string textContent;

    [Space]
    [Header("Clip Count")]
    public int clipCount = 0;
    public int curClipCount = 0;

    [Header("Animation clip frame offset")]
    [Range(0, 30)]
    public int frameOffset;

    private void Start()
    {
        animDataList = null;
        textContent = null;
    }

    public void LoadAnimationDataFromText()
    {
        // load text file content
        if(File.Exists(textSavePath + textFileName + ".txt"))
        {
            Debug.Log(textSavePath + textFileName + ".txt");
            StreamReader reader = new StreamReader(textSavePath + textFileName + ".txt");
            textContent = reader.ReadToEnd();
            reader.Close();
        }
        else
        {
            Debug.LogError("File can't read as not exist");
            return;
        }

        // parse text to json
        animDataList = JsonUtility.FromJson<AnimDataListClass>(textContent);
        clipCount = animDataList.animData.Count;
        foreach (var _animDataUnit in animDataList.animData)
        {
            Debug.Log(_animDataUnit.clipIndex + " : " + _animDataUnit.clipName);
        }
    }

    public void ApplyCurrentFrameAnimationDataToModel()
    {
        // parent root transform apply
        animator.transform.position = animDataList.animData[curClipCount].transformList[frame].positionList[0];
        animator.transform.rotation = animDataList.animData[curClipCount].transformList[frame].rotationList[0];

        // each human body pose parts
        for (int i = 1; i<animDataList.animData[curClipCount].transformList[frameOffset].positionList.Count; i++)
        {
            if (animator.GetBoneTransform((HumanBodyBones)i) == null) continue;
            // position
            animator.GetBoneTransform((HumanBodyBones)i).localPosition
                = animDataList.animData[curClipCount].transformList[frameOffset].positionList[i];
            // rotation
            animator.GetBoneTransform((HumanBodyBones) i).localRotation
                = animDataList.animData[curClipCount].transformList[frameOffset].rotationList[i];
        }
    }

    private int frame = 0;
    public void ApplyNextFrameAnimationDataToModel()
    {
        Debug.Log("Current Frame : " + frame);

        // each human body pose parts
        for (int i = 0; i < animDataList.animData[curClipCount].transformList[frame].positionList.Count; i++)
        {
            if (animator.GetBoneTransform((HumanBodyBones)i) == null) continue;
            // position
            animator.GetBoneTransform((HumanBodyBones)i).localPosition
                = animDataList.animData[curClipCount].transformList[frame].positionList[i];
            // rotation
            animator.GetBoneTransform((HumanBodyBones)i).localRotation
                = animDataList.animData[curClipCount].transformList[frame].rotationList[i];
        }

        frame++;
        frame %= 31;
    }
}
