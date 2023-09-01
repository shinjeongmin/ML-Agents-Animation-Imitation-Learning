using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class RoboAgent : Agent
{
    [Header("Target To Push up")]
    public Transform targetCube;
    private Transform targetCubeStartTrans;

    [Header("Body Parts")]
    public Transform head;
    public Transform neck;
    public Transform rib;
    public Transform left_upperarm;
    public Transform start_left_upperarm;

    public Transform left_forearm;
    public Transform left_wrist;
    public Transform right_upperarm;
    public Transform start_right_upperarm;

    public Transform right_forearm;
    public Transform right_wrist;

    // 초기 위치 저장 dictionary
    private Dictionary<string, Transform> jointItems = new Dictionary<string, Transform>();

    private void Start()
    {
    }

    public override void Initialize()
    {
        // save start cube transform
        targetCubeStartTrans = new GameObject().transform;
        targetCubeStartTrans.position = targetCube.localPosition;
        targetCubeStartTrans.rotation = targetCube.localRotation;

        // save start joint transform
        start_left_upperarm = new GameObject().transform;
        start_left_upperarm.localRotation = left_upperarm.localRotation;
        start_right_upperarm = new GameObject().transform;
        start_right_upperarm.localRotation = right_upperarm.localRotation;

        jointItems.Add("head", head);
        jointItems.Add("neck", neck);
        jointItems.Add("rib", rib);
        jointItems.Add("left_upperarm", left_upperarm);
        jointItems.Add("left_forearm", left_forearm);
        jointItems.Add("left_wrist", left_wrist);
        jointItems.Add("right_upperarm", right_upperarm);
        jointItems.Add("right_forearm", right_forearm);
        jointItems.Add("right_wrist", right_wrist);
    }

    public override void OnEpisodeBegin()
    {
        // 큐브가 흉부 밑으로 내려간 경우 원래 위치로 옮겨놓기
        if(targetCube.transform.localPosition.y < .5f
            || fixedTime >= 2f)
        {
            targetCube.localPosition = targetCubeStartTrans.position;
            targetCube.localRotation = targetCubeStartTrans.rotation;
            fixedTime = 0;

            left_upperarm.localRotation = start_left_upperarm.localRotation;
            right_upperarm.localRotation = start_right_upperarm.localRotation;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(targetCube.localPosition);
        sensor.AddObservation(targetCube.localRotation.x);
        sensor.AddObservation(targetCube.localRotation.y);
        sensor.AddObservation(targetCube.localRotation.z);
        sensor.AddObservation(jointItems["left_upperarm"].transform.localRotation.x);
        sensor.AddObservation(jointItems["left_upperarm"].transform.localRotation.y);
        sensor.AddObservation(jointItems["left_upperarm"].transform.localRotation.z);
        sensor.AddObservation(jointItems["right_upperarm"].transform.localRotation.x);
        sensor.AddObservation(jointItems["right_upperarm"].transform.localRotation.y);
        sensor.AddObservation(jointItems["right_upperarm"].transform.localRotation.z);
    }

    [Header("Joint Angle")]
    public float angle = 1;
    // delta
    public Vector3 lastCubePos;
    public Vector3 lastCubeRot;
    public float fixedTime;

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 leftUpperArmcontrolSignal = Vector3.zero;
        Vector3 rightUpperArmcontrolSignal = Vector3.zero;
        // left upper arm
        leftUpperArmcontrolSignal.x = actionBuffers.ContinuousActions[0];
        leftUpperArmcontrolSignal.y = actionBuffers.ContinuousActions[1];
        leftUpperArmcontrolSignal.z = actionBuffers.ContinuousActions[2];
        // right upper arm
        rightUpperArmcontrolSignal.x = actionBuffers.ContinuousActions[3];
        rightUpperArmcontrolSignal.y = actionBuffers.ContinuousActions[4];
        rightUpperArmcontrolSignal.z = actionBuffers.ContinuousActions[5];

        jointItems["left_upperarm"].transform.localRotation
            = Quaternion.Euler(jointItems["left_upperarm"].transform.localRotation.eulerAngles + leftUpperArmcontrolSignal * angle);
        jointItems["right_upperarm"].transform.localRotation
            = Quaternion.Euler(jointItems["right_upperarm"].transform.localRotation.eulerAngles + rightUpperArmcontrolSignal * angle);

        // 판이 떨어진 경우
        if (targetCube.transform.localPosition.y < .5f)
        {
            EndEpisode();
        }
        // 2초 이상 판이 고정되는 경우
        else if (fixedTime >= 2f)
        {
            EndEpisode();
        }
        else
        {
            SetReward(0.1f);
        }
    }

    private void FixedUpdate()
    {
        // 판이 멈춰서 고정된 경우 처리
        if (lastCubePos == targetCube.transform.localPosition
            && lastCubeRot == targetCube.transform.localRotation.eulerAngles)
        {
            fixedTime += Time.deltaTime;
        }
        lastCubePos = targetCube.transform.localPosition;
        lastCubeRot = targetCube.transform.localRotation.eulerAngles;

        //AddReward(0.001f);
    }
}
