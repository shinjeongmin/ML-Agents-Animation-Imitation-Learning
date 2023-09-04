using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class RoboAgent_ver1 : Agent
{
    [Header("Target To Push up")]
    public Transform targetCube;
    private Transform targetCubeStartTrans;

    [Header("Body Parts")]
    public Transform head;
    public Transform neck;
    public Transform rib;
    public Transform left_upperarm;
    public Transform left_forearm;
    public Transform left_wrist;
    public Transform right_upperarm;
    public Transform right_forearm;
    public Transform right_wrist;

    [Header("Start Transform")]
    public Transform start_left_upperarm;
    public Transform start_left_forearm;
    public Transform start_left_wrist;
    public Transform start_right_upperarm;
    public Transform start_right_forearm;
    public Transform start_right_wrist;

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
        start_left_forearm = new GameObject().transform;
        start_left_wrist = new GameObject().transform;
        start_left_upperarm.localRotation = left_upperarm.localRotation;
        start_left_forearm.localRotation = left_forearm.localRotation;
        start_left_wrist.localRotation = left_wrist.localRotation;
        start_right_upperarm = new GameObject().transform;
        start_right_forearm = new GameObject().transform;
        start_right_wrist = new GameObject().transform;
        start_right_upperarm.localRotation = right_upperarm.localRotation;
        start_right_forearm.localRotation = right_forearm.localRotation;
        start_right_wrist.localRotation = right_wrist.localRotation;

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
            left_forearm.localRotation = start_left_forearm.localRotation;
            left_wrist.localRotation = start_left_wrist.localRotation;
            right_upperarm.localRotation = start_right_upperarm.localRotation;
            right_forearm.localRotation = start_right_forearm.localRotation;
            right_wrist.localRotation = start_right_wrist.localRotation;
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
        sensor.AddObservation(jointItems["left_forearm"].transform.localRotation.x);
        sensor.AddObservation(jointItems["left_forearm"].transform.localRotation.y);
        sensor.AddObservation(jointItems["left_forearm"].transform.localRotation.z);
        sensor.AddObservation(jointItems["left_wrist"].transform.localRotation.x);
        sensor.AddObservation(jointItems["left_wrist"].transform.localRotation.y);
        sensor.AddObservation(jointItems["left_wrist"].transform.localRotation.z);
        sensor.AddObservation(jointItems["right_upperarm"].transform.localRotation.x);
        sensor.AddObservation(jointItems["right_upperarm"].transform.localRotation.y);
        sensor.AddObservation(jointItems["right_upperarm"].transform.localRotation.z);
        sensor.AddObservation(jointItems["right_forearm"].transform.localRotation.x);
        sensor.AddObservation(jointItems["right_forearm"].transform.localRotation.y);
        sensor.AddObservation(jointItems["right_forearm"].transform.localRotation.z);
        sensor.AddObservation(jointItems["right_wrist"].transform.localRotation.x);
        sensor.AddObservation(jointItems["right_wrist"].transform.localRotation.y);
        sensor.AddObservation(jointItems["right_wrist"].transform.localRotation.z);
    }

    [Header("Joint Angle")]
    public float angle = 1;
    // delta
    public Vector3 lastCubePos;
    public Vector3 lastCubeRot;
    public float fixedTime;

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // left upper arm
        Vector3 leftUpperArmcontrolSignal = Vector3.zero;
        leftUpperArmcontrolSignal.x = actionBuffers.ContinuousActions[0];
        leftUpperArmcontrolSignal.y = actionBuffers.ContinuousActions[1];
        leftUpperArmcontrolSignal.z = actionBuffers.ContinuousActions[2];

        // left fore arm
        Vector3 leftForeArmcontrolSignal = Vector3.zero;
        leftForeArmcontrolSignal.x = actionBuffers.ContinuousActions[3];
        leftForeArmcontrolSignal.y = actionBuffers.ContinuousActions[4];
        leftForeArmcontrolSignal.z = actionBuffers.ContinuousActions[5];

        // left wrist
        Vector3 leftWristcontrolSignal = Vector3.zero;
        leftWristcontrolSignal.x = actionBuffers.ContinuousActions[6];
        leftWristcontrolSignal.y = actionBuffers.ContinuousActions[7];
        leftWristcontrolSignal.z = actionBuffers.ContinuousActions[8];

        // right upper arm
        Vector3 rightUpperArmcontrolSignal = Vector3.zero;
        rightUpperArmcontrolSignal.x = actionBuffers.ContinuousActions[9];
        rightUpperArmcontrolSignal.y = actionBuffers.ContinuousActions[10];
        rightUpperArmcontrolSignal.z = actionBuffers.ContinuousActions[11];

        // right fore arm
        Vector3 rightForeArmcontrolSignal = Vector3.zero;
        rightForeArmcontrolSignal.x = actionBuffers.ContinuousActions[12];
        rightForeArmcontrolSignal.y = actionBuffers.ContinuousActions[13];
        rightForeArmcontrolSignal.z = actionBuffers.ContinuousActions[14];

        // right wrist
        Vector3 rightWristcontrolSignal = Vector3.zero;
        rightWristcontrolSignal.x = actionBuffers.ContinuousActions[15];
        rightWristcontrolSignal.y = actionBuffers.ContinuousActions[16];
        rightWristcontrolSignal.z = actionBuffers.ContinuousActions[17];

        jointItems["left_upperarm"].transform.localRotation
            = Quaternion.Euler(jointItems["left_upperarm"].transform.localRotation.eulerAngles + leftUpperArmcontrolSignal * angle);
        jointItems["left_forearm"].transform.localRotation
            = Quaternion.Euler(jointItems["left_forearm"].transform.localRotation.eulerAngles + leftForeArmcontrolSignal * angle);
        jointItems["left_wrist"].transform.localRotation
            = Quaternion.Euler(jointItems["left_wrist"].transform.localRotation.eulerAngles + leftWristcontrolSignal * angle);
        jointItems["right_upperarm"].transform.localRotation
            = Quaternion.Euler(jointItems["right_upperarm"].transform.localRotation.eulerAngles + rightUpperArmcontrolSignal * angle);
        jointItems["right_forearm"].transform.localRotation
            = Quaternion.Euler(jointItems["right_forearm"].transform.localRotation.eulerAngles + rightForeArmcontrolSignal * angle);
        jointItems["right_wrist"].transform.localRotation
            = Quaternion.Euler(jointItems["right_wrist"].transform.localRotation.eulerAngles + rightWristcontrolSignal * angle);

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
