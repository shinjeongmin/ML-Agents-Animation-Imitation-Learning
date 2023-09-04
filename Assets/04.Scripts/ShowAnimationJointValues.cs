using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShowAnimationJointValues : MonoBehaviour
{
    public Animator animator;
    public HumanBodyBones bones;

    [Header("Apply Model")]
    public GameObject anotherRobot;
    Animator anotherRobotAnimator;

    // view
    public float viewNormalizedTime;
    [Range(0,30)]
    public int frame;
    public List<Vector3> viewBonePosition;
    public List<float> viewMuscle;

    [SerializeField]
    List<Transform> boneTransforms = new List<Transform>(55);
    [SerializeField]
    List<Vector3> bonePositions = new List<Vector3>(55);
    [SerializeField]
    List<Quaternion> boneRotations = new List<Quaternion>(55);

    AnimationClip clip;
    HumanPoseHandler humanPoseHandler;
    HumanPose humanPose;

    void Start()
    {
        clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        anotherRobotAnimator = anotherRobot.GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
        {
            //animator.CrossFade(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0f, 0, offset);
            //animator.StopPlayback();
        }

        // normalized Time view
        viewNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            animator.speed = 0;

            for (int i = 0; i < 31; i++)
            {
                //animator.CrossFade(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0, i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
                //ApplyAnotherRobotTransform();
                //viewBonePosition.Add(anotherRobotAnimator.GetBoneTransform((HumanBodyBones)1).localRotation.eulerAngles);

                //animator.CrossFade(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0, i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
                //ApplyAnotherRobotHandlerMuscle();
            }

            StartCoroutine(GetEachFrameBonePosition());
            StartCoroutine(GetEachFrameMuscle());
        }
    }

    IEnumerator GetEachFrameBonePosition()
    {
        for (int i = 0; i < 31; i++)
        {
            animator.CrossFade(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0, i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
            ApplyAnotherRobotTransform();
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    IEnumerator GetEachFrameMuscle()
    {
        for (int i = 0; i < 31; i++)
        {
            animator.CrossFade(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0, i / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
            ApplyAnotherRobotHandlerMuscle();
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    public void ApplyAnotherRobotTransform()
    {
        for (int i = 0; i < 55; i++)
        {
            // human body bones뽑기
            boneTransforms[i] = (animator.GetBoneTransform((HumanBodyBones)i));

            if (animator.GetBoneTransform((HumanBodyBones)i))
            {
                bonePositions[i] = (animator.GetBoneTransform((HumanBodyBones)i).localPosition);
                boneRotations[i] = (animator.GetBoneTransform((HumanBodyBones)i).localRotation);
            }
            else
            {
                bonePositions[i] = (Vector3.zero);
                boneRotations[i] = (Quaternion.identity);
            }

            // 다른 로봇에 position, rotation 값 적용하기
            if (animator.GetBoneTransform((HumanBodyBones)i))
            {
                anotherRobotAnimator.GetBoneTransform((HumanBodyBones)i).localPosition = bonePositions[i];
                anotherRobotAnimator.GetBoneTransform((HumanBodyBones)i).localRotation = boneRotations[i];
            }
        }

        // view
        viewBonePosition.Add(anotherRobotAnimator.GetBoneTransform((HumanBodyBones)1).localRotation.eulerAngles);
    }

    public void ApplyAnotherRobotHandlerMuscle()
    {
        humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        humanPose = new HumanPose();
        humanPoseHandler.GetHumanPose(ref humanPose);

        // 다른 로봇에 muscle 값 가져온 human pose 적용.
        HumanPoseHandler anothRobtHumanPoseHandler = new HumanPoseHandler(anotherRobotAnimator.avatar, anotherRobot.transform);
        anothRobtHumanPoseHandler.SetHumanPose(ref humanPose);

        // 데이터에 각 시간별로 human pose 데이터 저장
        anothRobtHumanPoseHandler.GetHumanPose(ref humanPose);

        // view
        viewMuscle.Add(humanPose.muscles[0]);
    }

}
