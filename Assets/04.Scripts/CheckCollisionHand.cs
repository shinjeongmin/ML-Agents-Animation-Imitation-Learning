using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckCollisionHand : MonoBehaviour
{
    public UnityEvent rewardAction;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "target")
        {
            Debug.Log("손이 닿음");
            rewardAction.Invoke();
        }
    }
}
