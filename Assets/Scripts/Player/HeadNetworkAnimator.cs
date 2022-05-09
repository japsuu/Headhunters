using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Animates the player's head X-axis rotation on remote clients, based on input by the local client.
/// </summary>
public class HeadNetworkAnimator : NetworkBehaviour
{
    //[SerializeField]
    //private Transform headTransform;

    [SerializeField]
    private bool useIK = true;
    
    private CameraMouseLook mouseLook;
    private HeadIKApplier headIK;
    
    //private Vector3 headInitialOffset;

    //private float latestHeadXAngle;

    private void Awake()
    {
        mouseLook = GetComponentInChildren<CameraMouseLook>();
        headIK = GetComponentInChildren<HeadIKApplier>();

        if (headIK == null)
        {
            Debug.LogWarning("No HeadIKApplier found. Disabling head IK.");
            useIK = false;
        }
    }

    //private void Start()
    //{
    //    headInitialOffset = headTransform.localEulerAngles;
    //}

    private void Update()
    {
        if(isLocalPlayer)
            Cmd_SendLookAtTarget(mouseLook.LookAtTarget);
    }

    /*private void LateUpdate()
    {
        if(isLocalPlayer) return;
        
        Vector3 localEulerAngles = headTransform.localEulerAngles;
        
        Quaternion latestHeadRotation = Quaternion.Euler(new Vector3(
            headInitialOffset.x + latestHeadXAngle, 
            localEulerAngles.y, 
            localEulerAngles.z));
        
        ApplyHeadRotation(latestHeadRotation);
    }*/

    [Command(channel = Channels.Unreliable)]
    private void Cmd_SendLookAtTarget(Vector3 target)
    {
        Rpc_ReceiveLookAtTarget(target);
    }

    [ClientRpc(includeOwner = false)]
    private void Rpc_ReceiveLookAtTarget(Vector3 target)
    {
        // Only apply IK to remote/other players
        if(isLocalPlayer) return;
        
        headIK.UseIK = useIK;
        headIK.LatestLookAtTarget = target;
    }

    //private void ApplyHeadRotation(Quaternion rot)
    //{
    //    headTransform.localRotation = rot;
    //}
}
