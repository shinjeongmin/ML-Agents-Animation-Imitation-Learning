using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AnimationJointTextExporter : MonoBehaviour
{
    // 사용할 animation controller를 넣고 animation joint text exporter를 작동 시킨다.
    public Animator animator;
    public AnimDataListClass animDataList = new AnimDataListClass();

    [Header("Test")]
    public string textContent;
    [Header("Apply animation key frame to model")]
    public GameObject targetModel;

    [Space]
    [Header("Write text save path and name")]
    public string textSavePath;
    public string textFileName;

    private void Start()
    {
        if (false == File.Exists(textSavePath + textFileName + ".txt"))
        {
            var file = File.CreateText(textSavePath + textFileName + ".txt");
            file.Close();
        }
        else
        {
            Debug.LogError("File not created as already exist");
        }

        // Test Debug
        //AnimDataListClass adc = new AnimDataListClass();
        //adc.AddData(new AnimDataClass(true));
        //Debug.Log(JsonUtility.ToJson(adc));
    }

    /// <summary>
    /// 애니메이터의 controller를 변경하고나서 animation clip 정보를 key:value json 형태로 저장
    /// </summary>
    public void AddAnimationClipData()
    {
        StartCoroutine(SaveAnimKeyFrameToText());
    }

    public void ExportAnimationKeyFrameToText()
    {
        StreamWriter sw = new StreamWriter(textSavePath + textFileName + ".txt");

        // textContent는 추후 animation각 key frame의
        // joint tranform 정보를 저장하는 형태가 되어야함.
        sw.WriteLine(textContent);
        sw.Flush();
        sw.Close();
    }

    // 각 프레임별로 bone transform들의 position, rotation 데이터를 list로 저장
    public IEnumerator SaveAnimKeyFrameToText()
    {
        AnimDataClass animData = new AnimDataClass();

        animator.speed = 0;

        // animation clip is 30 frame
        for (int i = 0; i <= 30; i++)
        {
            List<Vector3> transformPosition = new List<Vector3>();
            List<Quaternion> transformRotation = new List<Quaternion>();

            // animator set current frame
            animator.CrossFade(
                animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                0,
                0,
                i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);

            // Test
            Debug.Log(i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);

            // apply transform into anim data
            for(int j = 0; j < 55; j++) // bone transform is 55 in total (0~54)
            {
                Transform boneTransform = animator.GetBoneTransform((HumanBodyBones) j);

                // add current bone index data in current frame
                if (boneTransform)
                {
                    transformPosition.Add(boneTransform.localPosition);
                    transformRotation.Add(boneTransform.localRotation);
                }
                else // if index's bone is not exists set zero vector or quaternion
                {
                    transformPosition.Add(Vector3.zero);
                    transformRotation.Add(Quaternion.identity);
                }
            }

            animData.transformList.Add(new TransformList(transformPosition, transformRotation));

            yield return new WaitForEndOfFrame();
        }

        // frame 0 is equal frame 30. so 30 frame put into 0 frame.
        animData.transformList[0] = animData.transformList[animData.transformList.Count - 1];

        animData.clipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        animDataList.AddData(animData);

        Debug.Log($"Complete get animation {animData.clipName} data");
        // pass to text file content
        textContent = (JsonUtility.ToJson(animDataList));
        yield return null;
    }
}
