using System;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
/*
[RequireComponent(typeof(Rigidbody))]
public class CarriableLog : NetworkBehaviour, IInteractable, ICarriable
{
    [SerializeField]
    private float dropForce = 100f;
    
    private Rigidbody rb;
    private Collider[] colliders;

    [ReadOnly]
    [SerializeField]
    [SyncVar]
    private bool sync_currentlyBeingCarried;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public GameObject GetCarriableObject()
    {
        return gameObject;
    }

    public GameObject GetSpawnableObject()
    {
        return gameObject;
    }

    [Client]
    public void OnPickedUp()
    {
        rb.isKinematic = true;
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    [Client]
    public void OnDropped()
    {
        if(hasAuthority) Cmd_OnDropped();
        
    }

    [Command]
    private void Cmd_OnDropped()
    {
        netIdentity.RemoveClientAuthority();
        
        Rpc_OnDropped();
    }

    [ClientRpc]
    private void Rpc_OnDropped()
    {
        rb.isKinematic = false;
        sync_currentlyBeingCarried = false;
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
        
        rb.AddForce(transform.forward * dropForce, ForceMode.Impulse);
        rb.AddTorque(new Vector3(50, 0, 0), ForceMode.Impulse);
    }

    
    [Client]
    [Command(requiresAuthority = false)]
    private void Cmd_ClientRequestPickUp(NetworkConnectionToClient requester = null)
    {
        //TODO: Check if requester is in headhunter state -> reject if is.
        
        netIdentity.RemoveClientAuthority();
        netIdentity.AssignClientAuthority(requester);
        
        sync_currentlyBeingCarried = true;
        
        Target_PickUp(requester);
        Rpc_OnPickedUp();
    }

    [TargetRpc]
    private void Target_PickUp(NetworkConnection target)
    {
        // Called on the client which requested pickup
        
        //CarriableController.Singleton.LocalClient_OnCarryStart(this);
    }

    [ClientRpc(includeOwner = false)]
    private void Rpc_OnPickedUp()
    {
        // Called on every other client than the pickup requester.
        
        //CarriableController.Singleton.RemoteClient_PickUpCarriable(this);
    }

    public string GetInteractText()
    {
        return "Carry";
    }

    public void Interact()
    {
        Cmd_ClientRequestPickUp();
    }

    public bool CanBeInteractedWith()
    {
        return !sync_currentlyBeingCarried;
    }
}*/