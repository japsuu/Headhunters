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
    [SerializeField]
    private Transform headTransform;

    private CameraMouseLook mouseLook;

    private Vector3 headInitialOffset;

    private float latestHeadXAngle;

    private void Awake()
    {
        mouseLook = GetComponentInChildren<CameraMouseLook>();
    }

    private void Start()
    {
        headInitialOffset = headTransform.localEulerAngles;
    }

    private void Update()
    {
        if(isLocalPlayer)
            Cmd_SendHeadXAngle(mouseLook.CurrentXAngle);
    }

    private void LateUpdate()
    {
        if(isLocalPlayer) return;
        
        Vector3 localEulerAngles = headTransform.localEulerAngles;
        
        Quaternion latestHeadRotation = Quaternion.Euler(new Vector3(
            headInitialOffset.x + latestHeadXAngle, 
            localEulerAngles.y, 
            localEulerAngles.z));
        
        ApplyHeadRotation(latestHeadRotation);
    }

    [Command(channel = Channels.Unreliable)]
    private void Cmd_SendHeadXAngle(float xAngle)
    {
        Rpc_ReceiveHeadXAngle(xAngle);
    }

    [ClientRpc(includeOwner = false)]
    private void Rpc_ReceiveHeadXAngle(float xAngle)
    {
        latestHeadXAngle = xAngle;
    }

    private void ApplyHeadRotation(Quaternion rot)
    {
        headTransform.localRotation = rot;
    }
}
