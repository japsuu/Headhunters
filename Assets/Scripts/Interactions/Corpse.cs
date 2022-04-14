
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public class Corpse : NetworkBehaviour, IInteractable
{
    [SerializeField]
    [Tooltip("Time after the corpse rots, and cannot be eaten anymore.")]
    private float rotTime = 60;
    
    [SerializeField]
    [Tooltip("Time after the corpse de-spawns from the world.")]
    private float despawnTime = 600;

    private float timeAlive;

    [SyncVar(hook = nameof(SetRotten))]
    private bool isRotten;
    
    private Transform freshModel;
    private Transform rottenModel;

    private void Start()
    {
        Transform modelRoot = transform.Find("Model");
        freshModel = modelRoot.Find("Fresh");
        rottenModel = modelRoot.Find("Rotten");
    }

    private void FixedUpdate()
    {
        if(!isServer) return;
        
        if (timeAlive > despawnTime)
        {
            NetDestroy();
        }
        
        if (!isRotten && timeAlive > rotTime)
        {
            isRotten = true;
            
            freshModel.gameObject.SetActive(false);
            rottenModel.gameObject.SetActive(true);
        }
        
        timeAlive += Time.fixedDeltaTime;
    }

    private void SetRotten(bool oldVal, bool newVal)
    {
        freshModel.gameObject.SetActive(false);
        rottenModel.gameObject.SetActive(true);
    }

    public string GetInteractText()
    {
        return CanBeInteractedWith() ? "Consume" : "I'm going to eat that!";
    }

    public void Interact()
    {
        Command_ConsumeCorpse();

        StartCoroutine(Headhunter.LocalHeadhunter.OnCorpseConsumed(this));
    }

    public bool CanBeInteractedWith()
    {
        return Player.LocalPlayer.sync_isHeadhunter && !isRotten;
    }
    
    
    [Command(requiresAuthority = false)]
    private void Command_ConsumeCorpse()
    {
        Rpc_HeadhunterConsumeCorpse();
    }
    
    [ClientRpc]
    private void Rpc_HeadhunterConsumeCorpse()
    {
        //TODO: Play animation
    }

    [Command(requiresAuthority = false)]
    public void OnConsumed()
    {
        NetDestroy();
    }

    private void NetDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }
}
