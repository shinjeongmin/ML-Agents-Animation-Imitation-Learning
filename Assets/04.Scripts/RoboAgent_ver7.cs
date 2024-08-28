using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using System.IO;

/// <summary>
/// ver 5에서는 다리 애니메이션을 임의로 하나 지정해서 적용하기만 하였다.
/// 이번 버전 6에서는 다리 애니메이션도 블렌딩하되, 일정 주기동안 한 애니메이션을 적용하다가 임의로 다른 애니메이션으로 변경한다.
/// </summary>
public class RoboAgent_ver7 : Agent
{
    [Header("Model Animator")]
    public Animator animator;
    // start avatar bone transform
    // 시작하는 위치의 뼈대 transform 데이터
    private List<Transform> startAvatarBoneTransformList = new List<Transform>();
    private List<GameObject> startAvatarBoneTransformObjectList = new List<GameObject>();

    [Header("Target To Push up")]
    public Transform target;
    private Transform targetStartTrans;
    private float targetRadius;

    private float moveVelocity = 0.01f;

    // animation data storage
    public AnimDataListClass animDataList = new AnimDataListClass();
    private int clipCount = 0;

    [Header("Write text save path and name")]
    public string textSavePath;
    public string textFileName;
    // text content buffer
    public string textContent;

    [Header("Realtime debug data")]
    public Vector3 lastCubePos; // 타겟 고정 시간을 알기 위한 position
    public Vector3 lastCubeRot; // 타겟 고정 시간을 알기 위한 rotation
    public float fixedTime;
    public int currentFrame = 0;
    #region frame cnt 프레임을 나누는 과정에 필요한 변수
    public int frameCnt = 0;
    const int nextFrameNeedCnt = 5;
    public void nextFrameSign(){
        frameCnt++;
        if(frameCnt >= nextFrameNeedCnt)
        {
            frameCnt = 0;
            currentFrame++;
            currentFrame %= 30;
        }
    }
    #endregion
    private bool initEpisode = false;

    [Header("Collision reward components : 해당 스크립트에 Agent 보상 이벤트 넣어주기")]
    public CheckCollisionHand leftHand;
    public CheckCollisionHand rightHand;

    // clamping 을 위한 변수
    private float clampingAngle = .5f; // 순간적으로 움직이는 한계치 각도를 clamping에 설정할 값
    private List<Quaternion> quatLastFrame = new List<Quaternion>(); // 직전 프레임에서의 각 human body bone 별 쿼터니언 각도
    private List<bool> isClampArriveBones = new List<bool>(); // 각 human body bone 별 clamp 목적각에 도달하였는지 체크

    private void Start()
    {
        if (LoadAnimationDataFromText()) Debug.Log("Load animation success!");
        else Debug.LogError("Load animation fail!");

        Vector3 parentScale = target.parent.lossyScale;
        targetRadius = (target.GetComponent<BoxCollider>().size.x * parentScale.x) * target.lossyScale.x;
    }

    public override void Initialize()
    {
        // save start target transform
        targetStartTrans = new GameObject().transform;
        targetStartTrans.position = target.localPosition;
        targetStartTrans.rotation = target.localRotation;

        // save start joint transform gameobject
        for(int i = 0; i < 55; i++)
        {
            GameObject gameObject = new GameObject();
            startAvatarBoneTransformObjectList.Add(gameObject);
            startAvatarBoneTransformList.Add(gameObject.transform);

            // humanbody bone에 transform이 mapping되지 않은 경우 넘기기
            if (animator.GetBoneTransform((HumanBodyBones)i) == null) continue;
            startAvatarBoneTransformList[i].localPosition = animator.GetBoneTransform((HumanBodyBones) i).localPosition;
            startAvatarBoneTransformList[i].localRotation = animator.GetBoneTransform((HumanBodyBones) i).localRotation;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (
            target.transform.localPosition.y < .5f // 타겟이 흉부 밑으로 내려간 경우 원래 위치로 옮겨놓기
            || transform.localPosition.z > 7f // ground 밖으로 이동시 초기화
            || initEpisode)
        {
            target.localPosition = targetStartTrans.position;
            target.localRotation = targetStartTrans.rotation;
            target.GetComponent<Rigidbody>().velocity = Vector3.zero;
            fixedTime = 0;
            initEpisode = false;

            // body bones position and rotation initialize
            for (int i = 0; i < 55; i++)
            {
                if (animator.GetBoneTransform((HumanBodyBones)i) == null) continue;
                animator.GetBoneTransform((HumanBodyBones)i).localPosition = startAvatarBoneTransformList[i].localPosition;
                animator.GetBoneTransform((HumanBodyBones)i).localRotation = startAvatarBoneTransformList[i].localRotation;
            }

            // frame initialize
            currentFrame = 0;

            // move forward initialized
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition); // 3
        sensor.AddObservation(target.localRotation); // 4

        // total observation : 3+4
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int index = 0;
        int animationCount = animDataList.animData.Count;
        // action list
        List<float> action = new List<float>();
        float actionSum = 0;
        List<float> actionForHead = new List<float>();
        float actionForHeadSum = 0;
        List<float> actionForHand = new List<float>();
        float actionForHandSum = 0;
        for (int i = 0; i < animationCount; i++) {
            action.Add(actions.ContinuousActions[index++]);
            actionForHead.Add(actions.ContinuousActions[index++]);
            actionForHand.Add(actions.ContinuousActions[index++]);
        }
        for (int i = 0; i < animationCount; i++) {
            actionSum += action[i];
            actionForHeadSum += actionForHead[i];
            actionForHandSum += actionForHand[i];
        }

        // action 종류 중 하나의 normalization 분모가 0이 되어 발산하는 경우 에피소드를 초기화
        for(int i = 0; i < animationCount; i++)
        {
            if (float.IsNaN(action[i] / actionSum)
                || float.IsNaN(actionForHead[i] / actionForHeadSum)
                || float.IsNaN(actionForHand[i] / actionForHandSum))
            {
                initEpisode = true;
                SetReward(-0.01f);
                EndEpisode();
                return;
            }
        }

        // action값을 넣을 때 기본 애니메이션들에 weight를 주어 반영
        // 아직 animClip A와 animClip B의 합에 대해서 정규화 하지는 않았음. clamp처리 및 더하기만 함.
        for (int i = 0;i < 55; i++){
            List<Quaternion> quatAllAnim = new List<Quaternion>();
            for (int j = 0; j < animationCount; j++)
                quatAllAnim.Add(animDataList.animData[j].transformList[currentFrame].rotationList[i]);

            // 파트별로 별도의 animation weight 비율 적용
            Quaternion signalQuat = Quaternion.identity;
            if ((HumanBodyBones)i == HumanBodyBones.Neck
                || (HumanBodyBones)i == HumanBodyBones.Head)
            {
                signalQuat
                = new Quaternion(
                    combineActionQuaternionEle(actionForHead, quatAllAnim, 'x', animationCount, actionForHeadSum),
                    combineActionQuaternionEle(actionForHead, quatAllAnim, 'y', animationCount, actionForHeadSum),
                    combineActionQuaternionEle(actionForHead, quatAllAnim, 'z', animationCount, actionForHeadSum),
                    combineActionQuaternionEle(actionForHead, quatAllAnim, 'w', animationCount, actionForHeadSum)
                )
                ;
            }
            else if ((HumanBodyBones)i == HumanBodyBones.LeftHand
                || (HumanBodyBones)i == HumanBodyBones.RightHand)
            {
                signalQuat
                = new Quaternion(
                    combineActionQuaternionEle(actionForHand, quatAllAnim, 'x', animationCount, actionForHandSum, actions.ContinuousActions[index++]),
                    combineActionQuaternionEle(actionForHand, quatAllAnim, 'y', animationCount, actionForHandSum, actions.ContinuousActions[index++]),
                    combineActionQuaternionEle(actionForHand, quatAllAnim, 'z', animationCount, actionForHandSum, actions.ContinuousActions[index++]),
                    combineActionQuaternionEle(actionForHand, quatAllAnim, 'w', animationCount, actionForHandSum, actions.ContinuousActions[index++])
                )
                ;
            }
            else
            {
                signalQuat
                = new Quaternion(
                    combineActionQuaternionEle(action, quatAllAnim, 'x', animationCount, actionSum),
                    combineActionQuaternionEle(action, quatAllAnim, 'y', animationCount, actionSum),
                    combineActionQuaternionEle(action, quatAllAnim, 'z', animationCount, actionSum),
                    combineActionQuaternionEle(action, quatAllAnim, 'w', animationCount, actionSum)
                )
                ;
            }
            // (animation data count) * 3 + (LeftHand, RightHand additional action = 8)

            // action weight를 받아 새로 시도하는 quat값과 last quat값 사이 각을 clamping
            float betweenAngle;
            if (quatLastFrame.Count < 55) // 맨 처음 동작의 경우 clamping하지 않음
            {
                quatLastFrame.Add(signalQuat);
                isClampArriveBones.Add(false);
            }
            else // quatLastFrame.Count 가 55
            {
                betweenAngle = Vector3.Angle(quatLastFrame[i].eulerAngles, signalQuat.eulerAngles);
                if(betweenAngle > clampingAngle)
                {
                    float angleDivide = betweenAngle / clampingAngle;
                    //Debug.Log($"{(HumanBodyBones)i} 등분 : {angleDivide}");
                    signalQuat = Quaternion.LerpUnclamped(quatLastFrame[i], signalQuat, 1 / angleDivide);
                }
            }

            if (animator.GetBoneTransform((HumanBodyBones)i) == null) { }
            else if ((HumanBodyBones)i == HumanBodyBones.Hips
                || (HumanBodyBones)i == HumanBodyBones.LeftUpperLeg
                || (HumanBodyBones)i == HumanBodyBones.LeftLowerLeg
                || (HumanBodyBones)i == HumanBodyBones.LeftFoot
                || (HumanBodyBones)i == HumanBodyBones.LeftToes
                || (HumanBodyBones)i == HumanBodyBones.RightUpperLeg
                || (HumanBodyBones)i == HumanBodyBones.RightLowerLeg
                || (HumanBodyBones)i == HumanBodyBones.RightFoot
                || (HumanBodyBones)i == HumanBodyBones.RightToes)
            { // 다리 모델에는 action을 적용하지 않고 특정 index의 애니메이션만 적용한다.
                animator.GetBoneTransform((HumanBodyBones)i).transform.localRotation = GetInterpolatedFrameAnimation(
                    /* animData */ 5, currentFrame, /* human body bone index */ i);
            }
            else
            {
                animator.GetBoneTransform((HumanBodyBones)i).transform.localRotation = signalQuat;
                quatLastFrame[i] = signalQuat; // 현재 동작 각도 저장
            }
        }
        nextFrameSign();

        // 손이 머리위치에 가까울 수록 보상
        float disRight = Mathf.Abs(rightHand.transform.position.y - animator.GetBoneTransform(HumanBodyBones.Head).position.y);
        float disLeft = Mathf.Abs(leftHand.transform.position.y - animator.GetBoneTransform(HumanBodyBones.Head).position.y);
        if (disRight < .1f && disLeft < .1f)
        {
            //Debug.Log($"손과 머리 거리 : {disRight} / {disLeft}");
            SetReward(0.1f);
        }
        else SetReward(-0.01f);

        // 양손이 서로 가까울 수록 연속적 보상
        float disHand = Vector3.Distance(rightHand.transform.position, leftHand.transform.position);
        float disHandReward=0;
        float goalDisHand = 0.5f;
        if(disHand < goalDisHand)
        {
            //Debug.Log($"양손 {goalDisHand}보다 가까움");
            disHandReward = 1 + Mathf.Pow(1 - disHand, 2) * 1;
        }
        else
        {
            //Debug.Log($"양손 {goalDisHand}보다 멂");
            disHandReward = -Mathf.Sqrt(disHand) * 0.01f;
        }
        //Debug.Log($"양손 거리 보상 {disHandReward}");
        SetReward(disHandReward);

        // 머리 - 손 삼각형에 안에 들어오면 보상을 주고, 그 외 위치에 있으면 보상을 깎도록
        if (CheckWithinTriangleRange(animator.GetBoneTransform(HumanBodyBones.Head).position, rightHand.transform.position, leftHand.transform.position, target.position))
        {
            //Debug.Log("삼각형 안으로 들어왔다.");
            // 손과 타켓의 거리가 가까울 수록 보상
            if (Vector3.Distance(leftHand.transform.position, target.position) - targetRadius < 1f)
            {
                float distance = Vector3.Distance(leftHand.transform.position, target.position) - targetRadius;
                float reward = Mathf.Pow(1 - distance, 5) * 5;
                //Debug.Log($"거리 : {distance}, 거리보상 : {reward}");
                SetReward(reward);
            }
        }
        else
        {
            SetReward(-0.01f);
        }

        // 고개를 펼 수록 보상을, 숙일 수록 벌점을 준다 (숙이면 대략 300도, 고개를 쭉 피면 355도 정도)
        // ((z - 300) / 55)^2 - 0.5
        float nodReward = Mathf.Pow((NormalizeAngle(animator.GetBoneTransform(HumanBodyBones.Neck).transform.localRotation.eulerAngles.z) - 300) / 55, 2) - 0.5f;
        SetReward(nodReward);
        //Debug.Log($"고개 : {animator.GetBoneTransform(HumanBodyBones.Neck).transform.localRotation.eulerAngles.z}");
        //Debug.Log($"고개 점수 : {nodReward}");

        #region hand palm is facing target center

        float reward_FacingOnBound;
        float reward_FacingOutBound;
        float rewardHand;

        // 손바닥이 공의 중심을 향할 수록 보상을 준다
        Vector3 leftHandLineOrigin = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
        Vector3 leftHandLineDir = leftHand.transform.position - leftHand.transform.up;
        float disLeftHandLine2Target = GetDistancePointAndLine(leftHandLineOrigin, leftHandLineDir, target.position);
        bool isLeftHandFacingTarget = Vector3.Dot(leftHandLineDir - leftHandLineOrigin, target.position - leftHandLineOrigin) > 0;

        reward_FacingOnBound = Mathf.Sqrt(targetRadius - disLeftHandLine2Target);
        reward_FacingOutBound = (targetRadius - disLeftHandLine2Target) * .1f;
        //Debug.Log($"왼손 각도 거리 {disLeftHandLine2Target}");
        //Debug.Log($"왼손 손바닥 방향 : {isLeftHandFacingTarget}");

        // 거리가 구의 반지름보다 작을수록 보상, 클수록 벌점
        if (isLeftHandFacingTarget)
        {
            if (disLeftHandLine2Target < targetRadius)
                rewardHand = reward_FacingOnBound;
            else
                rewardHand = reward_FacingOutBound;
        }
        else rewardHand = -0.5f;
        //Debug.Log($"왼손 점수 {rewardHand}");
        SetReward(rewardHand);

        Vector3 rightHandLineOrigin = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        Vector3 rightHandLineDir = rightHand.transform.position + rightHand.transform.up;
        bool isRightHandFacingTarget = Vector3.Dot(rightHandLineDir - rightHandLineOrigin, target.position - rightHandLineOrigin) > 0;
        float disRightHandLine2Target = GetDistancePointAndLine(rightHandLineOrigin, rightHandLineDir, target.position);

        reward_FacingOnBound = Mathf.Sqrt(targetRadius - disRightHandLine2Target);
        reward_FacingOutBound = (targetRadius - disRightHandLine2Target) * .1f;
        //Debug.Log($"오른손 각도 거리 {disRightHandLine2Target}");
        //Debug.Log($"오른손 손바닥 방향 : {isRightHandFacingTarget}");

        if (isRightHandFacingTarget)
        {
            if (disRightHandLine2Target < targetRadius)
                rewardHand = reward_FacingOnBound;
            else
                rewardHand = reward_FacingOutBound;
        }
        else rewardHand = -0.5f;
        //Debug.Log($"오른손 점수 {rewardHand}");
        SetReward(rewardHand);

        #endregion

        // 타겟이 떨어진 경우
        if (target.transform.localPosition.y < .5f)
        {
            //// (속도 조정 보상) 타겟보다 앞으로 갔으면 감점
            //if (transform.position.z > target.position.z)
            //{
            //    Debug.Log("공보다 앞으로 감");
            //    SetReward(-3f);
            //}
            EndEpisode();
        }
        // 너무 많이 걸어간 경우 : z거리 7
        else if (transform.localPosition.z > 7f) EndEpisode();
        else
        {
            SetReward(0.1f);
        }
    }

    private float combineActionQuaternionEle(List<float> action, List<Quaternion> quat, char select,
        int animCnt, float actionSum, float additionalSupplyAction = 0)
    {
        float sum = 0;
        float additionalActionMinVal = -0.25f;
        float additionalActionMaxVal = 0.25f;
        switch (select)
        {
            case 'x':
                for(int i = 0; i < animCnt; i++)
                    sum += ((Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].x)
                        + Mathf.Clamp(additionalSupplyAction, additionalActionMinVal, additionalActionMaxVal)
                    );
                break;
            case 'y':
                for (int i = 0; i < animCnt; i++)
                    sum += ((Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].y)
                        + Mathf.Clamp(additionalSupplyAction, additionalActionMinVal, additionalActionMaxVal)
                    );
                break;
            case 'z':
                for (int i = 0; i < animCnt; i++)
                    sum += ((Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].z)
                        + Mathf.Clamp(additionalSupplyAction, additionalActionMinVal, additionalActionMaxVal)
                    );
                break;
            case 'w':
                for (int i = 0; i < animCnt; i++)
                    sum += ((Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].w)
                        + Mathf.Clamp(additionalSupplyAction, additionalActionMinVal, additionalActionMaxVal)
                    );
                break;
        }
        return sum;
    }

    private void FixedUpdate()
    {
        // 판이 멈춰서 고정된 경우 처리
        if (lastCubePos == target.transform.localPosition
            && lastCubeRot == target.transform.localRotation.eulerAngles)
        {
            fixedTime += Time.deltaTime;
        }
        lastCubePos = target.transform.localPosition;
        lastCubeRot = target.transform.localRotation.eulerAngles;

        // 전진 처리
        transform.Translate(Vector3.forward * moveVelocity);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(leftHand.transform.position, target.position, Color.red);
        Debug.DrawLine(rightHand.transform.position, target.position, Color.red);

        // debug sphere
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(animator.GetBoneTransform(HumanBodyBones.LeftHand).position, .05f);
        Gizmos.DrawWireSphere(animator.GetBoneTransform(HumanBodyBones.RightHand).position, .05f);
        Gizmos.DrawWireSphere(animator.GetBoneTransform(HumanBodyBones.Head).position, .1f);

        // debug head - hand triangle
        Debug.DrawLine(leftHand.transform.position, animator.GetBoneTransform(HumanBodyBones.Head).position, Color.magenta);
        Debug.DrawLine(animator.GetBoneTransform(HumanBodyBones.Head).position, rightHand.transform.position, Color.magenta);
        Debug.DrawLine(rightHand.transform.position, leftHand.transform.position, Color.magenta);

        // debug hand palm vector
        Debug.DrawLine(leftHand.transform.position, leftHand.transform.position - leftHand.transform.up.normalized * 0.3f, Color.black);
        Debug.DrawLine(rightHand.transform.position, rightHand.transform.position + rightHand.transform.up.normalized * 0.3f, Color.black);
        // debug hand up vector
        Debug.DrawLine(leftHand.transform.position, leftHand.transform.position - leftHand.transform.right.normalized * 0.3f, Color.cyan);
        Debug.DrawLine(rightHand.transform.position, rightHand.transform.position + rightHand.transform.right.normalized * 0.3f, Color.cyan);

        // debug neck vector
        Debug.DrawLine(animator.GetBoneTransform(HumanBodyBones.Neck).position,
            animator.GetBoneTransform(HumanBodyBones.Neck).position - animator.GetBoneTransform(HumanBodyBones.Neck).right * 0.3f, Color.black);
    }

    public bool LoadAnimationDataFromText()
    {
        // load text file content
        if (File.Exists(textSavePath + textFileName + ".txt"))
        {
            Debug.Log(textSavePath + textFileName + ".txt");
            StreamReader reader = new StreamReader(textSavePath + textFileName + ".txt");
            textContent = reader.ReadToEnd();
            reader.Close();
        }
        else
        {
            Debug.LogError("File can't read as not exist");
            return false;
        }

        // parse text to json
        animDataList = JsonUtility.FromJson<AnimDataListClass>(textContent);
        clipCount = animDataList.animData.Count;
        foreach (var _animDataUnit in animDataList.animData)
        {
            Debug.Log(_animDataUnit.clipIndex + " : " + _animDataUnit.clipName);
        }

        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //float angle = 10f;
            //Debug.Log($"회전각 : {angle}");
            //animator.GetBoneTransform(HumanBodyBones.LeftHand).rotation *= Quaternion.AngleAxis(angle, animator.GetBoneTransform(HumanBodyBones.LeftHand).right);
            //animator.GetBoneTransform(HumanBodyBones.RightHand).rotation *= Quaternion.AngleAxis(angle, animator.GetBoneTransform(HumanBodyBones.RightHand).right);
        }
    }

    // 각도를 0에서 360도로 정규화합니다.
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
        {
            angle += 360f;
        }
        return angle;
    }

    private bool CheckWithinTriangleRange(Vector3 A, Vector3 B, Vector3 C, Vector3 target)
    {
        Vector3 vecAB = B - A;
        Vector3 vecBC = C - B;
        Vector3 vecCD = target - C;

        Vector3 N = Vector3.Cross(vecAB, vecBC);

        float h = Vector3.Dot(vecCD, N) / N.magnitude;

        float dist_AD = Vector3.Distance(target, A);
        float dist_BD = Vector3.Distance(target, B);
        float dist_CD = Vector3.Distance(target, C);


        // D가 삼각형 ABC 내에 있는지를 확인합니다.
        if (h <= 0 && dist_AD <= vecAB.magnitude && dist_BD <= vecBC.magnitude && dist_CD <= vecCD.magnitude) return true;
        else return false;
    }

    float GetDistancePointAndLine(Vector3 A, Vector3 B, Vector3 point)
    {
        Vector3 AB = B - A;
        return (Vector3.Cross(point - A, AB)).magnitude / AB.magnitude;
    }

    private Quaternion GetInterpolatedFrameAnimation(int _animDataIdx, float _frame, int _humanBodyBoneIdx)
    {
        if(0 > _frame || 30 <= _frame)
        {
            Debug.LogError("Wrong frame range");
            return new Quaternion(0,0,0,0);
        }

        return Quaternion.LerpUnclamped(
            animDataList.animData[_animDataIdx].transformList[(int)_frame].rotationList[_humanBodyBoneIdx],
            animDataList.animData[_animDataIdx].transformList[(int)_frame + 1].rotationList[_humanBodyBoneIdx],
            (float)frameCnt / nextFrameNeedCnt);
    }
}
