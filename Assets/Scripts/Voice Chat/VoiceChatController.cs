using System;
using System.Collections;
using System.Collections.Generic;
using Dissonance;
using UnityEngine;

public class VoiceChatController : SingletonBehaviour<VoiceChatController>
{
    [SerializeField]
    private string headhunterVoiceToken = "Headhunter";
    
    [SerializeField]
    private string survivorVoiceToken = "Survivor";
    
    private DissonanceComms voiceComms;

    private void Awake()
    {
        voiceComms = FindObjectOfType<DissonanceComms>();

        if (voiceComms == null)
        {
            Debug.LogError("Cannot find DissonanceComms in the scene!");
        }
    }

    public void OnLocalPlayerSpawned(bool isHeadhunter)
    {
        voiceComms.AddToken(isHeadhunter ? headhunterVoiceToken : survivorVoiceToken);
    }
}
