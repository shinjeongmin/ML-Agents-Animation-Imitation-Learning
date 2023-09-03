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
    [Range(0, 1)]
    public float offset;
    public float muscleValue0;
    public float muscleValue1;
    public float muscleValue2;
    public float viewNormalizedTime;

    AnimationClip clip;
    HumanPoseHandler humanPoseHandler;
    HumanPose humanPose;

    void Start()
    {
        clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        anotherRobotAnimator = anotherRobot.GetComponent<Animator>();

        // binding으로 curve에 접근해서 normalizedTime에 해당하는 curve
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            //Debug.Log(binding.path + "/" + binding.propertyName + ", Keys: " + curve.keys.Length);
            //Debug.Log(curve.Evaluate(animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
        {
            animator.CrossFade(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0f, 0, offset);
            animator.StopPlayback();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            // normalized Time view
            viewNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                //Debug.Log(binding.path + "/" + binding.propertyName + ", Keys: " + curve.keys.Length);
                //Debug.Log(binding.propertyName + " : " + curve.Evaluate(animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
            }

            humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            humanPose = new HumanPose();
            humanPoseHandler.GetHumanPose(ref humanPose);

            muscleValue0 = humanPose.muscles[0];
            muscleValue1 = humanPose.muscles[1];
            muscleValue2 = humanPose.muscles[2];

            // 다른 로봇에 muscle 값 가져온 human pose 적용.
            HumanPoseHandler anothRobtHumanPoseHandler = new HumanPoseHandler(anotherRobotAnimator.avatar, anotherRobot.transform);
            anothRobtHumanPoseHandler.SetHumanPose(ref humanPose);
        }
    }

    public void OnGUI()
    {
    }
}
