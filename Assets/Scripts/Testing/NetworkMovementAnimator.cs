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

    [SerializeField]
    private bool rawInput;

    private static readonly int InputX = Animator.StringToHash("Input_x");
    private static readonly int InputY = Animator.StringToHash("Input_y");

    private void Update()
    {
        if (!isLocalPlayer) return;
        
        Vector2 input = rawInput ?
            new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) :
            new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        Cmd_SendInput(input);
    }
    
    [Command(channel = Channels.Unreliable)]
    private void Cmd_SendInput(Vector2 input)
    {
        Rpc_ReceiveInput(input);
    }

    [ClientRpc(includeOwner = false)]
    private void Rpc_ReceiveInput(Vector2 input)
    {
        OnInputReceived(input);
    }

    private void OnInputReceived(Vector2 input)
    {
        //TODO: Set the animation speed based on the velocity
        targetAnimator.SetFloat(InputX, input.x, blendSmoothing, Time.deltaTime);
        targetAnimator.SetFloat(InputY, input.y, blendSmoothing, Time.deltaTime);
    }
}
