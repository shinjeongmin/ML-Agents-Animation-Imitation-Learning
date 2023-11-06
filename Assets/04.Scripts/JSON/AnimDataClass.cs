using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformList
{
    public List<Quaternion> rotationList;

    public TransformList() { }
    public TransformList(List<Quaternion> rotLst) {
        rotationList = rotLst;
    }

    /// <summary>
    /// Test Data Set
    /// </summary>
    public TransformList(bool isSet) {
        if (isSet)
        {
            rotationList.Add(Quaternion.identity);
        }
    }
}

[System.Serializable]
public class AnimDataClass
{
    public int clipIndex;
    public string clipName;
    public List<TransformList> transformList = new List<TransformList>();
    public float forwardVelocity;

    public AnimDataClass() { }

    /// <summary>
    /// Test Data Set
    /// </summary>
    public AnimDataClass(bool isSet)
    {
        if (isSet)
        {
            clipIndex = 1;
            clipName = "walking";
            transformList.Add(new TransformList(true));
            forwardVelocity = 0;
        }
    }

    public void Print()
    {
        Debug.Log("clipIndex = " + clipIndex);
        Debug.Log("clipName = " + clipName);
    }
}

[System.Serializable]
public class AnimDataListClass
{
    public List<string> animClipName = new List<string>();
    public List<AnimDataClass> animData = new List<AnimDataClass>();
    public AnimDataListClass() { }
    public void AddData(AnimDataClass adc)
    {
        adc.clipIndex = animData.Count;
        animClipName.Add(adc.clipName);
        animData.Add(adc);
    }
}
