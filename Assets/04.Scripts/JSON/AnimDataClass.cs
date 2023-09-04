using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformList
{
    public List<Vector3> positionList;
    public List<Quaternion> rotationList;

    public TransformList() { }
    public TransformList(List<Vector3> posLst, List<Quaternion> rotLst) {
        positionList = posLst;
        rotationList = rotLst;
    }

    /// <summary>
    /// Test Data Set
    /// </summary>
    public TransformList(bool isSet) {
        if (isSet)
        {
            positionList.Add(Vector3.zero);
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
    public List<AnimDataClass> animData = new List<AnimDataClass>();
    public AnimDataListClass() { }
    public void AddData(AnimDataClass adc)
    {
        adc.clipIndex = animData.Count;
        animData.Add(adc);
    }
}
