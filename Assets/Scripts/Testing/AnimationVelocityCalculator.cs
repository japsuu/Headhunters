using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class AnimationVelocityCalculator : MonoBehaviour
{
    [Serializable]
    public class ClipTarget
    {
        [ReadOnly]
        public string Name;

        [ReadOnly]
        public string MovementDirection;

        [ReadOnly]
        public float AverageSpeed;
        
        public Vector2 BlendTreePosition;

        public float NeededClipSpeed;

        public ClipTarget(string name, string movementDirection, float averageSpeed)
        {
            Name = name;
            MovementDirection = movementDirection;
            AverageSpeed = averageSpeed;
        }

        public void CalculateNeededClipSpeed()
        {
            NeededClipSpeed = BlendTreePosition.magnitude / AverageSpeed;
        }
    }
    
    [SerializeField]
    private Animator targetAnimator;

    [Button("Read Clips", ButtonSizes.Large)]
    private void ReadStateInfo()
    {
        targetVelocities = new ClipTarget[targetAnimator.runtimeAnimatorController.animationClips.Length];

        for (int i = 0; i < targetAnimator.runtimeAnimatorController.animationClips.Length; i++)
        {
            AnimationClip clip = targetAnimator.runtimeAnimatorController.animationClips[i];
            
            float avgSpeed = clip.averageSpeed.x;
            string direction = avgSpeed < 0 ? "Left" : "Right";

            if (Mathf.Abs(avgSpeed) < Mathf.Abs(clip.averageSpeed.y))
            {
                avgSpeed = clip.averageSpeed.y;
                direction = avgSpeed < 0 ? "Downwards" : "Upwards";
            }

            if (Mathf.Abs(avgSpeed) < Mathf.Abs(clip.averageSpeed.z))
            {
                avgSpeed = clip.averageSpeed.z;
                direction = avgSpeed < 0 ? "Backwards" : "Forwards";
            }
            
            targetVelocities[i] = new ClipTarget(clip.name, direction, Mathf.Abs(avgSpeed));
        }
    }

    [SerializeField]
    private ClipTarget[] targetVelocities;

    [Button("Calculate")]
    private void CalculateClipSpeeds()
    {
        foreach (ClipTarget target in targetVelocities)
        {
            target.CalculateNeededClipSpeed();
        }
    }
    
    private void Reset()
    {
        targetAnimator = GetComponent<Animator>();
        targetVelocities = null;
    }
    
    /*[SerializeField]
    private Rigidbody targetRb;
    [SerializeField]
    private float animatorSpeed = 1;

    [SerializeField]
    private int sampleCount = 20;

    [ReadOnly]
    [SerializeField]
    private float currentVelocity;

    [ReadOnly]
    [SerializeField]
    private float averageVelocity;

    [ReadOnly]
    [SerializeField]
    private float lowestVelocity;

    [ReadOnly]
    [SerializeField]
    private float highestVelocity;

    private const int BufferFrames = 5;
    private int physicsFrameCount = 0;
    private Queue<float> samples;

    private void Reset()
    {
        targetRb = GetComponent<Rigidbody>();
        targetAnimator = GetComponent<Animator>();
    }

    private void Awake()
    {
        ResetData();
    }

    private void ResetData()
    {
        samples = new Queue<float>();
        lowestVelocity = float.MaxValue;
        highestVelocity = float.MinValue;
        currentVelocity = 0;
        averageVelocity = 0;
        physicsFrameCount = 0;
    }

    private void FixedUpdate()
    {
        physicsFrameCount++;
        
        if(physicsFrameCount < BufferFrames) return;

        currentVelocity = targetRb.velocity.z;
        
        samples.Enqueue(currentVelocity);

        if (currentVelocity > highestVelocity)
            highestVelocity = currentVelocity;

        if (currentVelocity < lowestVelocity)
            lowestVelocity = currentVelocity;

        //averageVelocity = (highestVelocity + lowestVelocity) / 2;
        
        float sampleTotals = samples.Sum();
        averageVelocity = sampleTotals / samples.Count;

        if (samples.Count == sampleCount)
        {
            samples.Dequeue();
        }
    }

    private void OnValidate()
    {
        if(targetAnimator == null) return;
        
        targetAnimator.speed = animatorSpeed;
        ResetData();
    }*/
}
