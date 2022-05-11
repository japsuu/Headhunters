using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZCameraShake;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class PostProcessingController : SingletonBehaviour<PostProcessingController>
{
    [SerializeField]
    private Volume survivorVolume;
    
    [SerializeField]
    private Volume headhunterVolume;

    [SerializeField]
    private Color survivorAmbientLightColor;

    [SerializeField]
    private Color headhunterAmbientLightColor;

    private Vignette headhunterVignette;
    private Vignette survivorVignette;
    private ColorAdjustments headhunterColorAdjustments;
    private ColorAdjustments survivorColorAdjustments;

    private void Awake()
    {
        RenderSettings.ambientLight = survivorAmbientLightColor;
        
        if (survivorVolume.profile.TryGet(typeof(Vignette), out Vignette sVignette))
        {
            survivorVignette = sVignette;
        }

        if (headhunterVolume.profile.TryGet(typeof(Vignette), out Vignette hVignette))
        {
            headhunterVignette = hVignette;
        }

        if (survivorVolume.profile.TryGet(typeof(ColorAdjustments), out ColorAdjustments sColorAdj))
        {
            survivorColorAdjustments = sColorAdj;
        }

        if (headhunterVolume.profile.TryGet(typeof(ColorAdjustments), out ColorAdjustments hColorAdj))
        {
            headhunterColorAdjustments = hColorAdj;
        }
    }
    
    private void OnEnable()
    {
        EventManager.ClientEvents.OnLocalPlayerDamaged.AddListener(OnPlayerDamaged);
    }

    private void OnDisable()
    {
        EventManager.ClientEvents.OnLocalPlayerDamaged.RemoveListener(OnPlayerDamaged);
    }

    private void OnPlayerDamaged(PlayerDamageSource damageSource, float damageAmount)
    {
        CameraShaker.Instance.ShakeOnce(4f, damageAmount / 5, .1f, .3f);
        //CameraShaker.Instance.ShakeOnce(4f, 4f, .1f, .3f);
    }

    public void OnHeadhunterSwitchState(Headhunter.HeadhunterState targetState, float transitionTime)
    {
        StartCoroutine(I_HeadhunterSwitchEffects(targetState, transitionTime));
    }

    private IEnumerator I_HeadhunterSwitchEffects(Headhunter.HeadhunterState targetState, float transitionTime)
    {
        float transitionTimeHalved = transitionTime / 2;
        
        if (targetState == Headhunter.HeadhunterState.Headhunter)
        {
            // Increase vignette and darken the screen
            StartStateTransition(survivorVignette, survivorColorAdjustments, transitionTimeHalved);
        
            yield return new WaitForSecondsRealtime(transitionTimeHalved);

            // Switch the active post processing profiles
            survivorVolume.weight = 0;
            headhunterVolume.weight = 1;
            
            survivorVignette.intensity.value = 0;
            headhunterVignette.intensity.value = 1;
            survivorColorAdjustments.postExposure.value = 0;
            headhunterColorAdjustments.postExposure.value = -10;
        
            // Set the ambient light based on targetState.
            RenderSettings.ambientLight = headhunterAmbientLightColor;
            
            // Decrease vignette and lighten the screen
            EndStateTransition(headhunterVignette, headhunterColorAdjustments, transitionTimeHalved);
        }
        else
        {
            // Increase vignette and darken the screen
            StartStateTransition(headhunterVignette, headhunterColorAdjustments, transitionTimeHalved);
        
            yield return new WaitForSecondsRealtime(transitionTimeHalved);

            // Switch the active post processing profiles
            survivorVolume.weight = 1;
            headhunterVolume.weight = 0;
            
            survivorVignette.intensity.value = 1;
            headhunterVignette.intensity.value = 0;
            survivorColorAdjustments.postExposure.value = -10;
            headhunterColorAdjustments.postExposure.value = 0;
        
            // Set the ambient light based on targetState.
            RenderSettings.ambientLight = survivorAmbientLightColor;
            
            // Decrease vignette and lighten the screen
            EndStateTransition(survivorVignette, survivorColorAdjustments, transitionTimeHalved);
        }
    }

    private static void StartStateTransition(Vignette targetVignette, ColorAdjustments targetColorAdj, float transitionTimeHalved)
    {
        DOTween.To(() => targetVignette.intensity.value, x => targetVignette.intensity.value = x, 1, transitionTimeHalved);
        DOTween.To(() => targetColorAdj.postExposure.value, x => targetColorAdj.postExposure.value = x, -10, transitionTimeHalved);
    }

    private static void EndStateTransition(Vignette targetVignette, ColorAdjustments targetColorAdj, float transitionTimeHalved)
    {
        DOTween.To(() => targetVignette.intensity.value, x => targetVignette.intensity.value = x, 0, transitionTimeHalved);
        DOTween.To(() => targetColorAdj.postExposure.value, x => targetColorAdj.postExposure.value = x, 0, transitionTimeHalved);
    }
}
