using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraEffects : SingletonBehaviour<CameraEffects>
{
    private Camera cam;
    
    [ReadOnly]
    [SerializeField]
    private float defaultFOV;
    
    [ReadOnly]
    [SerializeField]
    private float targetFOV;
    
    [ReadOnly]
    [SerializeField]
    private float targetFOVSetTime;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        defaultFOV = cam.fieldOfView;
    }

    private void Update()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * targetFOVSetTime);
    }

    public void SetTargetFovModifier(float targetFov, float targetTime)
    {
        targetFOV = defaultFOV - targetFov;
        targetFOVSetTime = targetTime;

        //float fov = cameraDefaultFov + timeAimedFor / aimTime * fovIncreaseWhenAimed;
    }
}
