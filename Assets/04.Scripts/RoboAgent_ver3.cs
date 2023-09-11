using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using System.IO;

public class RoboAgent_ver3 : Agent
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

    private float moveVelocity = 0.01f;
    [Header("제공하는 parameter 및 조건 - 이 값들은 필수로 입력하시오")]
    public float minVelocity = 0.01f;
    public float limitVelocity = 0.1f;
    public float limitAngle = 60f;

    // animation data storage
    public AnimDataListClass animDataList = new AnimDataListClass();
    private int clipCount = 0;

    [Header("Write text save path and name")]
    public string textSavePath;
    public string textFileName;
    // text content buffer
    public string textContent;

    [Header("Realtime debug data")]
    public Vector3 lastCubePos; // 큐브 고정 시간을 알기 위한 position
    public Vector3 lastCubeRot; // 큐브 고정 시간을 알기 위한 rotation
    public float fixedTime;
    public int currentFrame = 0;

    [Header("Collision reward components : 해당 스크립트에 Agent 보상 이벤트 넣어주기")]
    public CheckCollisionHand leftHand;
    public CheckCollisionHand rightHand;

    private void Start()
    {
        if (LoadAnimationDataFromText()) Debug.Log("Load animation success!");
        else Debug.LogError("Load animation fail!");
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
            )
        {
            target.localPosition = targetStartTrans.position;
            target.localRotation = targetStartTrans.rotation;
            target.GetComponent<Rigidbody>().velocity = Vector3.zero;
            fixedTime = 0;

            // body bones position and rotation initialize
            for(int i = 0; i < 55; i++)
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
        List<float> action = new List<float>();
        for (int i = 0; i < animationCount; i++) action.Add(actions.ContinuousActions[index++]);
        float actionSum = 0;
        for (int i = 0; i < animationCount; i++) actionSum += action[i];

        // action normalization 분모가 0이 되어 발산하는 경우는 animation frame도 넘기지 않고 넘어감.
        for(int i = 0; i < animationCount; i++)
        {
            if (float.IsNaN(action[i] / actionSum))
            {
                SetReward(-0.0001f);
                EndEpisode();
                return;
            }
        }

        // action값을 넣을 때 기본 애니메이션들에 weight를 주어 반영
        // 아직 animClip A와 animClip B의 합에 대해서 정규화 하지는 않았음. clamp처리 및 더하기만 함.
        for (int i = 0;i < 55; i++){
            List<Quaternion> quat = new List<Quaternion>();
            for (int j = 0; j < animationCount; j++)
                quat.Add(animDataList.animData[j].transformList[currentFrame].rotationList[i]);

            Quaternion signalQuat = Quaternion.identity;
            signalQuat
                = new Quaternion(
                    combineActionQuaternionEle(action, quat, 'x', animationCount, actionSum),
                    combineActionQuaternionEle(action, quat, 'y', animationCount, actionSum),
                    combineActionQuaternionEle(action, quat, 'z', animationCount, actionSum),
                    combineActionQuaternionEle(action, quat, 'w', animationCount, actionSum)
                )
            ;
            // each action 2

            if (animator.GetBoneTransform((HumanBodyBones)i) != null)
            {
                animator.GetBoneTransform((HumanBodyBones)i).transform.localRotation = signalQuat;
                //if ((HumanBodyBones)i == HumanBodyBones.Hips)
                //    Debug.Log("Root rotation : " + signalQuat);
            }
        }
        moveVelocity = Mathf.Clamp(actions.ContinuousActions[index++], minVelocity, limitVelocity); // 1
        // action : (2) * 55 + 1
        currentFrame++;
        currentFrame %= 30;

        // 손과 타켓의 거리가 가까울 수록 보상
        if(Vector3.Distance(leftHand.transform.localPosition, target.localPosition) < 1f)
        {
            Debug.Log($"거리보상 : {Vector3.Distance(leftHand.transform.localPosition, target.localPosition)}");
            SetReward(Vector3.Distance(leftHand.transform.localPosition, target.localPosition));
        }

        // 모자가 떨어진 경우
        if (target.transform.localPosition.y < .5f) EndEpisode();
        // 너무 많이 걸어간 경우 : z거리 7
        else if (transform.localPosition.z > 7f) EndEpisode();
        else
        {
            SetReward(0.1f);
        }
    }

    private float combineActionQuaternionEle(List<float> action, List<Quaternion> quat, char select,
        int animCnt, float actionSum)
    {
        float sum = 0;
        switch (select)
        {
            case 'x':
                for(int i = 0; i < animCnt; i++) sum += (Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].x);
                break;
            case 'y':
                for (int i = 0; i < animCnt; i++) sum += (Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].y);
                break;
            case 'z':
                for (int i = 0; i < animCnt; i++) sum += (Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].z);
                break;
            case 'w':
                for (int i = 0; i < animCnt; i++) sum += (Mathf.Clamp(action[i] / (actionSum), 0f, 1f) * quat[i].w);
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
}
