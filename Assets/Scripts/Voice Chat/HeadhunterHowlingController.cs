using System;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class HeadhunterHowlingController : NetworkBehaviour
{
    [SerializeField]
    private AudioSource howlSource;

    [SerializeField]
    private AudioClip[] howlClips;

    [SerializeField]
    private float minTimeBetweenClips = 0.5f;
    
    [SerializeField]
    private float maxTimeBetweenClips = 4f;

    private bool s_isHeadhunterSpeaking;

    private float s_timeSinceLastClip;
    
    private float s_currentClipLength;

    private bool s_wasHeadhunterSpeakingLastFrame;

    private void OnEnable()
    {
        VoiceChatController.Singleton.OnLocalPlayerSpeakChanged += OnLocalPlayerSpeakChanged;
    }

    private void Start()
    {
        RandomizeClipLength();
    }

    private void OnDisable()
    {
        if(VoiceChatController.Singleton != null)
            VoiceChatController.Singleton.OnLocalPlayerSpeakChanged -= OnLocalPlayerSpeakChanged;
    }

    private void OnLocalPlayerSpeakChanged(bool isSpeaking)
    {
        // Only growl/howl if in headhunter state
        if(Player.LocalPlayer.CurrentlyInHeadhunterState)
            Command_SetHowling(isSpeaking);
    }
    
    [Client]
    [Command]
    private void Command_SetHowling(bool isHowling)
    {
        // Called by owner client, ran on the server.

        s_isHeadhunterSpeaking = isHowling;
    }

    private void FixedUpdate()
    {
        // Only execute the code below server-side
        if(!isServer) return;
        
        //  IF: Headhunter is speaking in voiceChat
        //      IF: It's been long enough so that we can play a new sound, or the headhunter just started speaking
        //          Select and play a random clip on all survivor clients.
        //      ELSE:
        //          Increment time when the last clip was played
        //  ELSE:
        //      Reset time when the last clip was played
        if (s_isHeadhunterSpeaking)
        {
            if (s_timeSinceLastClip > s_currentClipLength || !s_wasHeadhunterSpeakingLastFrame && s_isHeadhunterSpeaking)
            {
                int clipIndex = Random.Range(0, howlClips.Length);
        
                Rpc_PlayHowlOnSurvivors(clipIndex);
                
                s_timeSinceLastClip = 0;
                RandomizeClipLength();
            }
            else
            {
                s_timeSinceLastClip += Time.fixedDeltaTime;
            }
        }

        s_wasHeadhunterSpeakingLastFrame = s_isHeadhunterSpeaking;
    }

    [Server]
    [ClientRpc]
    private void Rpc_PlayHowlOnSurvivors(int clipIndex)
    {
        // Called by the server, ran on all clients.
        
        // Only play the howls on survivor clients
        if(Player.LocalPlayer.IsHeadhunter) return;
        
        howlSource.PlayOneShot(howlClips[clipIndex]);
    }

    private void RandomizeClipLength()
    {
        s_currentClipLength = Random.Range(minTimeBetweenClips, maxTimeBetweenClips);
    }
}