using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AnimationJointTextExporter : MonoBehaviour
{
    // 사용할 animation controller를 넣고 animation joint text exporter를 작동 시킨다.
    public Animator animator;
    public Transform rootTransform;
    public AnimDataListClass animDataList = new AnimDataListClass();

    [Header("Test")]
    public string textContent;

    [Space]
    [Header("Write text save path and name")]
    public string textSavePath;
    public string textFileName;

    /// <summary>
    /// 애니메이터의 controller를 변경하고나서 animation clip 정보를 key:value json 형태로 저장
    /// </summary>
    public void AddAnimationClipData()
    {
        // Exception: check root transform assigned
        if (!rootTransform)
            Debug.LogError("Root Transform을 넣어주세요");
        StartCoroutine(SaveAnimKeyFrameToText());
    }

    public void ExportAnimationKeyFrameToText()
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

        // get first frame's forward position
        float startPosZ = 0;
        // get last frame's forward position
        float endPosZ = 0;

        // animation clip is 30 frame
        for (int i = 0; i <= 30; i++)
        {
            List<Quaternion> transformRotation = new List<Quaternion>();

            // animator set current frame
            animator.CrossFade(
                animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                0,
                0,
                i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);

            // Test
            Debug.Log(i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);

            // apply human body bone transform into anim data
            for (int j = 0; j < 55; j++) // bone transform is 55 in total (0~54)
            {
                Transform boneTransform = animator.GetBoneTransform((HumanBodyBones) j);

                // add current bone index data in current frame
                if (boneTransform)
                {
                    transformRotation.Add(boneTransform.localRotation);
                }
                else // if index's bone is not exists set zero vector or quaternion
                {
                    transformRotation.Add(Quaternion.identity);
                }
            }

            animData.transformList.Add(new TransformList(transformRotation));

            // first and last frame root position save
            // i == 1 : 0번째 인덱스를 적용하고 프레임을 넘겨야 animator에서 실행하므로 index 1에서 가져와야 함
            if(i == 1) startPosZ = rootTransform.localPosition.z;
            // 마지막 프레임은 적용하면 가장 처음으로 초기화되므로 마지막에서 1앞에있는 index로 가져옴
            if(i == 30) endPosZ = rootTransform.localPosition.z;

            yield return new WaitForEndOfFrame();
        }

        // set forward velocity (with 30 frame tranformed)
        float forwardVelocity = Mathf.Abs(endPosZ - startPosZ) / 30; // fixed 30 frame
        animData.forwardVelocity = forwardVelocity;

        // frame 0 is equal frame 30. so 30 frame put into 0 frame.
        animData.transformList[0] = animData.transformList[animData.transformList.Count - 1];

        animData.clipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        animDataList.AddData(animData);

        Debug.Log($"forward velocity : {forwardVelocity}");
        Debug.Log($"Complete get animation {animData.clipName} data");
        // pass to text file content
        textContent = (JsonUtility.ToJson(animDataList));
        yield return null;
    }
}
