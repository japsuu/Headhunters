using System;
using Dissonance;
using UnityEngine;

public class VoiceChatController : SingletonBehaviour<VoiceChatController>
{
    [SerializeField]
    private string headhunterVoiceToken = "Headhunter";
    
    [SerializeField]
    private string survivorVoiceToken = "Survivor";
    
    [SerializeField]
    private DissonanceComms voiceComms;
    
    [SerializeField]
    private VoiceBroadcastTrigger headhunterBroadcastTrigger;

    public Action<bool> OnLocalPlayerSpeakChanged;

    private bool wasSpeakingLastFrame;

    private void Update()
    {
        if (wasSpeakingLastFrame != headhunterBroadcastTrigger.IsTransmitting)
        {
            OnLocalPlayerSpeakChanged?.Invoke(headhunterBroadcastTrigger.IsTransmitting);
        }

        wasSpeakingLastFrame = headhunterBroadcastTrigger.IsTransmitting;
    }

    public void OnLocalPlayerSpawned(bool isHeadhunter)
    {
        voiceComms.AddToken(isHeadhunter ? headhunterVoiceToken : survivorVoiceToken);
    }
}
