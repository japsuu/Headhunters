using System.Collections;
using System.Collections.Generic;
using CMF;
using UnityEngine;

public class MovementController : AdvancedWalkerController
{
    [SerializeField]
    private float sprintSpeed = 10f;

    //TODO: Implement
    public bool IsWalking;
    public bool IsSprinting;
    
    private InputController inputController;
    
    protected override void Setup()
    {
        inputController = (InputController) characterInput;
    }

    protected override Vector3 CalculateMovementVelocity()
    {
        //Calculate (normalized) movement direction;
        Vector3 velocity = CalculateMovementDirection();

        //Multiply (normalized) velocity with correct movement speed;
        if (inputController.IsSprinting())
        {
            velocity *= sprintSpeed;
        }
        else
        {
            velocity *= walkSpeed;
        }

        return velocity;
    }
}
