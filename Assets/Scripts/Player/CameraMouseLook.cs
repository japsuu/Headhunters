using System;
using System.Collections;
using System.Collections.Generic;
using CMF;
using UnityEngine;

public class CameraMouseLook : CameraController
{
    
    [SerializeField]
    private Transform bodyRootTransform;

    [SerializeField]
    private float lookAtTargetDistance = 10;

    public Vector3 LookAtTarget { get; private set; }

    //public float CurrentXAngle => currentXAngle;

    protected override void UpdateRotation()
    {
        tr.localRotation = Quaternion.Euler(new Vector3(0, currentYAngle, 0));

        //Save 'facingDirection' and 'upwardsDirection' for later;
        facingDirection = tr.forward;
        upwardsDirection = tr.up;

        tr.localRotation = Quaternion.Euler(new Vector3(currentXAngle, 0, 0));
        
        bodyRootTransform.rotation = Quaternion.Euler(new Vector3(bodyRootTransform.eulerAngles.x, currentYAngle, bodyRootTransform.eulerAngles.z));

        LookAtTarget = transform.position + transform.forward * lookAtTargetDistance;
    }
}
