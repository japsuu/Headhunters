using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class FootIKSolver : MonoBehaviour
{
    [SerializeField]
    private Transform rootTransform;
    
    [SerializeField]
    private LayerMask terrainLayer;
    
    [SerializeField]
    private Transform raycastOrigin;
    
    [SerializeField]
    private FootIKSolver otherFoot;
    
    [SerializeField]
    private float stepSpeedFactor = 1;
    
    [SerializeField]
    private float stepDistanceFactor = 1;
    
    [SerializeField]
    private float stepLengthFactor = 1;
    
    [SerializeField]
    private float stepHeightFactor = 1;
    
    [SerializeField]
    private Vector3 footOffset;

    [SerializeField]
    private bool debugMode = true;

    [ReadOnly]
    [SerializeField]
    private Vector3 velocity;

    //[ReadOnly]
    //[SerializeField]
    //public bool isGrounded;

    private Vector3 oldRootTransformPos;
    private Vector3 oldPosition;
    private Vector3 currentPosition;
    private Vector3 newPosition;
    private Vector3 oldNormal;
    private Vector3 currentNormal;
    private Vector3 newNormal;
    private float lerp;
    private float footSpacing;

    private void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;
        lerp = 1;
    }
    
    private void Update()
    {
        transform.position = currentPosition;
        transform.up = currentNormal;

        //isGrounded = Physics.CheckSphere(transform.position, 0.06f, terrainLayer);
        
        //if(!otherFoot.isGrounded) return;

        velocity = (rootTransform.position - oldRootTransformPos) / Time.deltaTime;
        float stepSpeed = velocity.magnitude * stepSpeedFactor;
        float stepDistance = velocity.magnitude * stepDistanceFactor;
        float stepLength = velocity.magnitude * stepLengthFactor;
        float stepHeight = velocity.magnitude * stepHeightFactor;
        if (stepSpeed == 0) stepSpeed = 3;
        if (stepDistance == 0) stepDistance = 0.4f;
        if (stepLength == 0) stepLength = 0.4f;
        if (stepHeight == 0) stepHeight = 0.4f;

        Ray ray = new Ray(raycastOrigin.position + (raycastOrigin.right * footSpacing), Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit info, 5, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
            {
                lerp = 0;
                int direction = raycastOrigin.InverseTransformPoint(info.point).z > raycastOrigin.InverseTransformPoint(newPosition).z ? 1 : -1;
                newPosition = info.point + raycastOrigin.forward * (stepLength * direction) + footOffset;
                newNormal = info.normal;
            }
        }

        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp += Time.deltaTime * stepSpeed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;
        }

        oldRootTransformPos = rootTransform.position;
    }
    
    private bool IsMoving()
    {
        return lerp < 1;
    }

    private void OnDrawGizmos()
    {
        if(!debugMode) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(oldPosition, 0.06f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(newPosition, 0.06f);
    }
}
