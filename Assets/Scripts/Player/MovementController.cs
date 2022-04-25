using System.Collections;
using System.Collections.Generic;
using CMF;
using UnityEngine;

public class MovementController : AdvancedWalkerController
{
    [SerializeField]
    public float walkSpeed = 2f;
    
    [SerializeField]
    private float sprintSpeed = 4f;
    
    [SerializeField]
    private float sideStrafeSprintSpeed = 3f;
    
    [SerializeField]
    [Range(0.01f, 1f)]
    private float sideStrafeFactor = 0.75f;

    public bool IsWalking { get; private set; }
    public bool IsSprinting { get; private set; }
    public Vector2 InputVelocity { get; private set; }
    
    private InputController inputController;
    
    protected override void Setup()
    {
        inputController = (InputController) characterInput;
        baseWalkSpeed = walkSpeed;
    }

    protected override Vector3 CalculateMovementDirection()
    {
        //If no character input script is attached to this object, return;
        if(characterInput == null)
            return Vector3.zero;

        Vector3 worldSpaceVelocity = Vector3.zero;

        //If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
        if(cameraTransform == null)
        {
            worldSpaceVelocity += tr.right * (characterInput.GetHorizontalMovementInput() * sideStrafeFactor);
            worldSpaceVelocity += tr.forward * characterInput.GetVerticalMovementInput();
        }
        else
        {
            //If a camera transform has been assigned, use the assigned transform's axes for movement direction;
            //Project movement direction so movement stays parallel to the ground;
            worldSpaceVelocity += Vector3.ProjectOnPlane(
                cameraTransform.right,
                tr.up).normalized * (characterInput.GetHorizontalMovementInput() * sideStrafeFactor);
            
            worldSpaceVelocity += Vector3.ProjectOnPlane(
                cameraTransform.forward,
                tr.up).normalized * characterInput.GetVerticalMovementInput();
        }

        //If necessary, clamp movement vector to magnitude of 1f;
        if(worldSpaceVelocity.magnitude > 1f)
            worldSpaceVelocity.Normalize();

        return worldSpaceVelocity;
    }

    protected override Vector3 CalculateMovementVelocity()
    {
        //Calculate (normalized) movement direction;
        Vector3 worldSpaceVelocity = CalculateMovementDirection();

        IsWalking = new Vector2(worldSpaceVelocity.x, worldSpaceVelocity.z).magnitude > 0.1f;
        IsSprinting = IsWalking && inputController.IsSprinting();

        InputVelocity = new Vector2(
            inputController.GetHorizontalMovementInput(),
            inputController.GetVerticalMovementInput());

        float multiplier;

        //Multiply (normalized) velocity with correct movement speed:
        
        if (inputController.IsSprinting())  // IF sprint key held
        {
            if (InputVelocity.y > 0)        // IF moving forward
            {
                multiplier = sprintSpeed;   // Move at sprint speed
            }
            else                            // IF not moving forward
            {
                multiplier = sideStrafeSprintSpeed; // Move at sideStrafeSprint speed
            }
        }
        else                                // IF not sprinting
        {
            multiplier = walkSpeed;         // Move at walk speed
            
            if (InputVelocity.y < 0)        // IF moving backwards
            {
                multiplier *= sideStrafeFactor;     // Move at walkSpeed * sideStrafeFactor speed
            }
        }
        
        worldSpaceVelocity *= multiplier;
        InputVelocity *= multiplier;

        return worldSpaceVelocity;
    }
}
