
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public class Corpse : NetworkedConsumable, IInteractable
{
    [SerializeField]
    [Tooltip("Time after the corpse rots, and cannot be eaten anymore.")]
    private float rotTime = 60;
    
    [SerializeField]
    [Tooltip("Time after the corpse de-spawns from the world.")]
    private float despawnTime = 600;

    [SerializeField]
    private string freshModelRoot = "Fresh";

    [SerializeField]
    private string rottenModelRoot = "Rotten";

    private float timeAlive;

    [SyncVar(hook = nameof(SetRotten))]
    private bool sync_isRotten;
    
    private Transform freshModel;
    private Transform rottenModel;

    private void Start()
    {
        Transform modelRoot = transform.Find("Model");
        freshModel = modelRoot.Find(freshModelRoot);
        rottenModel = modelRoot.Find(rottenModelRoot);
    }

    private void FixedUpdate()
    {
        if(!isServer) return;
        
        if (timeAlive > despawnTime)
        {
            NetDestroy();
        }
        
        if (!sync_isRotten && timeAlive > rotTime)
        {
            sync_isRotten = true;
            
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
        return CanBeInteractedWith() ? "Eat" : "That's rotten!";
    }

    public void Interact()
    {
        Consume();
    }

    public bool CanBeInteractedWith()
    {
        return Player.LocalPlayer.IsHeadhunter && !sync_isRotten;
    }
    
    protected override void OnConsumeStart(float time)
    {
        //TODO: Play animations
    }

    protected override void OnConsumeEnd()
    {
        
    }
    
    private void NetDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }
}
