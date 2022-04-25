using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// This class animates the player on remote clients.
/// Local client sends their input values to the server, which mirrors them to other clients.
/// Upon receiving the input packets, other clients apply them to their animators.
/// </summary>
public class NetworkMovementAnimator : NetworkBehaviour
{
    [SerializeField]
    private Animator targetAnimator;
    
    [SerializeField]
    private float blendSmoothing = 0.075f;

    private MovementController movementController;

    private static readonly int VelocityX = Animator.StringToHash("Velocity_x");
    private static readonly int VelocityY = Animator.StringToHash("Velocity_z");

    private void Awake()
    {
        movementController = GetComponentInChildren<MovementController>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        
        Cmd_SendVelocity(movementController.InputVelocity);
    }
    
    [Command(channel = Channels.Unreliable)]
    private void Cmd_SendVelocity(Vector2 velocity)
    {
        Rpc_ReceiveVelocity(velocity);
    }

    [ClientRpc(includeOwner = false)]
    private void Rpc_ReceiveVelocity(Vector2 velocity)
    {
        OnVelocityReceived(velocity);
    }

    private void OnVelocityReceived(Vector2 velocity)
    {
        //TODO: Set the animation speed based on the velocity
        targetAnimator.SetFloat(VelocityX, velocity.x, blendSmoothing, Time.deltaTime);
        targetAnimator.SetFloat(VelocityY, velocity.y, blendSmoothing, Time.deltaTime);
    }
}
