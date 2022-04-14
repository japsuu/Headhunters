using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Headhunter : NetworkBehaviour
{
    public static Headhunter LocalHeadhunter;
    
    [SerializeField]
    private Color headhunterStateColor = Color.red;

    [SerializeField]
    private KeyCode transitionKey = KeyCode.G;

    /// <summary>
    /// The time it takes for a transition from one state to another to complete.
    /// </summary>
    [SerializeField]
    private float transitionTime = 4f;
    
    /// <summary>
    /// Minimum delay between transitions.
    /// </summary>
    [SerializeField]
    private float transitionMinimumDelay = 8f;

    private Material material;

    private Player player;

    private bool canTransition;
    
    private static readonly int TransitionProgressProperty = Shader.PropertyToID("_TransitionProgress");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public enum HeadhunterState
    {
        Survivor,
        Headhunter
    }

    private void Awake()
    {
        material = GetComponentInChildren<MeshRenderer>().material;

        player = GetComponent<Player>();
    }

    public override void OnStartLocalPlayer()
    {
        if(LocalHeadhunter != null) Debug.LogError("Multiple local headhunters set?");
            
        LocalHeadhunter = this;
    }

    public IEnumerator OnCorpseConsumed(Corpse target)
    {
        player.DisableInput();
        
        yield return new WaitForSecondsRealtime(4);
        
        target.OnConsumed();
        
        player.EnableInput();
    }

    public IEnumerator EnableTransitionsAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        canTransition = true;
    }

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        
        StartCoroutine(EnableTransitionsAfter(transitionMinimumDelay));
    }

    private void Update()
    {
        if(!isLocalPlayer) return;
        
        GetInput();
    }

    private void GetInput()
    {
        // Disable input if we are in the middle of a transition
        if(!player.InputEnabled) return;
        
        // Transition mechanic
        if (Input.GetKeyDown(transitionKey) && canTransition)
        {
            HeadhunterState targetState;
            switch (player.sync_currentHeadhunterState)
            {
                case HeadhunterState.Headhunter:
                    targetState = HeadhunterState.Survivor;
                    break;
                case HeadhunterState.Survivor:
                    targetState = HeadhunterState.Headhunter;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(player.sync_currentHeadhunterState));
            }
            
            Debug.Log($"CurrentState= {player.sync_currentHeadhunterState}, Target= {targetState}");

            if (targetState != player.sync_currentHeadhunterState)
            {
                Debug.Log("Command called");
                Command_HeadhunterTransitionTo(targetState);
            }
        }
    }
    
    [Command]
    private void Command_HeadhunterTransitionTo(HeadhunterState targetState)
    {
        Debug.Log("Command_HeadhunterTransitionTo");
        Rpc_HeadhunterTransitionTo(targetState);
    }
    
    [ClientRpc]
    private void Rpc_HeadhunterTransitionTo(HeadhunterState targetState)
    {
        Debug.Log("Rpc_HeadhunterTransitionTo");
        StartCoroutine(ChangeHeadhunterStateTo(targetState));
    }

    private IEnumerator ChangeHeadhunterStateTo(HeadhunterState targetState)
    {
        Debug.Log("ChangeHeadhunterStateTo");
        canTransition = false;
        player.sync_currentHeadhunterState = targetState;
        
        player.DisableInput();

        float transitionTimeHalved = transitionTime / 2;

        float transitionLeft = transitionTimeHalved;
        
        // Fade out
        while (transitionLeft > 0)
        {
            transitionLeft -= Time.deltaTime;
            
            material.SetFloat(TransitionProgressProperty, Mathf.Clamp01(1 - transitionLeft / transitionTimeHalved));

            yield return null;
        }
        
        //TODO: Switch the model
        switch (targetState)
        {
            case HeadhunterState.Headhunter:
                material.SetColor(BaseColor, headhunterStateColor);
                break;
            case HeadhunterState.Survivor:
                material.SetColor(BaseColor, Color.white);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetState), targetState, null);
        }

        transitionLeft = transitionTimeHalved;
        
        // Fade in
        while (transitionLeft > 0)
        {
            transitionLeft -= Time.deltaTime;

            material.SetFloat(TransitionProgressProperty, Mathf.Clamp01(transitionLeft / transitionTimeHalved));

            yield return null;
        }
        
        player.EnableInput();

        if(isLocalPlayer)
            StartCoroutine(EnableTransitionsAfter(transitionMinimumDelay));
    }
}
