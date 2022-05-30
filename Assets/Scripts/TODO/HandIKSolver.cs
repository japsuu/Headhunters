using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIKSolver : MonoBehaviour
{
    [SerializeField]
    private Transform handIKTarget;
    
    [SerializeField]
    private Transform spearIKTarget;

    [SerializeField]
    private Transform handRestPosition;

    [SerializeField]
    private float maxIKDistance = 8f;

    private Vector3 ikTargetPos;
    private Quaternion ikTargetRot;

    private void Update()
    {
        if (Vector3.Distance(transform.position, spearIKTarget.position) > maxIKDistance)
        {
            ikTargetPos = Vector3.Lerp(ikTargetPos, handRestPosition.position, Time.deltaTime * 3);
            ikTargetRot = Quaternion.Slerp(ikTargetRot, handRestPosition.rotation, Time.deltaTime * 3);
        }
        else
        {
            ikTargetPos = spearIKTarget.transform.position;
            ikTargetRot = spearIKTarget.transform.rotation;
        }
        
        handIKTarget.position = ikTargetPos;
        handIKTarget.rotation = ikTargetRot;
    }
}
