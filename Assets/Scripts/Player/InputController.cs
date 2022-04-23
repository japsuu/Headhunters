using System.Collections;
using System.Collections.Generic;
using CMF;
using UnityEngine;

public class InputController : CharacterInput
{
    [SerializeField]
    private string horizontalInputAxis = "Horizontal";
    
    [SerializeField]
    private string verticalInputAxis = "Vertical";
    
    [SerializeField]
    private KeyCode sprintModifierKey = KeyCode.LeftShift;
    
    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;

    //If this is enabled, Unity's internal input smoothing is bypassed;
    public bool useRawInput = true;

    public bool isInputEnabled = true;

    public override float GetHorizontalMovementInput()
    {
        if (!isInputEnabled) return 0;

        return useRawInput ? Input.GetAxisRaw(horizontalInputAxis) : Input.GetAxis(horizontalInputAxis);
    }

    public override float GetVerticalMovementInput()
    {
        if (!isInputEnabled) return 0;

        return useRawInput ? Input.GetAxisRaw(verticalInputAxis) : Input.GetAxis(verticalInputAxis);
    }

    public override bool IsJumpKeyPressed()
    {
        return isInputEnabled && Input.GetKey(jumpKey);
    }

    public bool IsSprinting()
    {
        return isInputEnabled && Input.GetKey(sprintModifierKey);
    }
}
