using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class HeadIKApplier : MonoBehaviour
{
    [ReadOnly]
    public bool UseIK = true;

    [ReadOnly]
    public Vector3 LatestLookAtTarget;

    private Animator targetAnimator;
    
    private void Reset()
    {
        targetAnimator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (UseIK)
        {
            targetAnimator.SetLookAtWeight(1);
            
            targetAnimator.SetLookAtPosition(LatestLookAtTarget);
        }
        else
        {
            targetAnimator.SetLookAtWeight(0);
        }
    }
}
