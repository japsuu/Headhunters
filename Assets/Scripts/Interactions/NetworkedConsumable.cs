using System.Collections;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class NetworkedConsumable : NetworkBehaviour
{
    [Header("Consume settings")]
    
    [SerializeField]
    private bool destroyAfterConsumed = true;
    [SerializeField]
    private bool disableInputWhileConsumed = true;
    [SerializeField]
    private bool canBeUsedByMultipleSimultaneously;
    [SerializeField]
    private bool onlyHeadhuntersCanConsume;
    [SerializeField]
    private bool onlySurvivorsReceiveEffects;
    [SerializeField]
    private bool willRenew;
    [SerializeField]
    private float renewTime = 6f;   //WARN: Debug values, change before build!!!
    [SerializeField]
    private float consumeTime = 4f;

    [Header("Effects")]
    
    [SerializeField]
    private float healthEffect;
    [SerializeField]
    private float saturationEffect;
    [SerializeField]
    private float hydrationEffect;

    [SyncVar]
    private bool sync_isCurrentlyBeingConsumed;

    [SyncVar]
    public bool sync_needsRenew;

    [SerializeField]
    private bool localClientRequested;  //WARN: Could be gotten rid of by using a separate TargetRPC targeting the requesting player, which is meant just to disable/enable input.

    private bool CanBeConsumedCurrentlyBy(Player requestingPlayer)
    {
        if (sync_isCurrentlyBeingConsumed && !canBeUsedByMultipleSimultaneously ||
            requestingPlayer == null ||
            willRenew && sync_needsRenew ||
            onlyHeadhuntersCanConsume && !requestingPlayer.sync_isHeadhunter)
        {
            return false;
        }

        return true;
    }

    protected void Consume()
    {
        Command_RequestConsumeAction();

        localClientRequested = true;
    }

    [Command(requiresAuthority = false)]
    private void Command_RequestConsumeAction(NetworkConnectionToClient requester = null)
    {
        // Executed on the server

        if (requester == null)
        {
            Debug.LogWarning("Null connection tried to request consume on object", this);
            return;
        }

        Player requestingPlayer = requester.identity.GetComponent<Player>();
        
        // Determine if the item can be consumed right now, if it can, allow the client to consume it.
        if (!CanBeConsumedCurrentlyBy(requestingPlayer))
        {
            Rpc_RequestDenied(requester);
            
            return;
        }
        
        if(!canBeUsedByMultipleSimultaneously)
            sync_isCurrentlyBeingConsumed = true;
        
        StartCoroutine(I_Server_Consume(requestingPlayer));
    }

    [Server]
    private IEnumerator RenewAfter(float seconds)
    {
        sync_needsRenew = true;

        yield return new WaitForSecondsRealtime(seconds);

        sync_needsRenew = false;
        
        Rpc_OnRenew();
    }

    [Server]
    private IEnumerator I_Server_Consume(Player requestingPlayer)
    {
        Rpc_ConsumeStart();

        yield return new WaitForSecondsRealtime(consumeTime);

        // Only give the wanted effects to players who should get them
        if (!onlySurvivorsReceiveEffects || onlySurvivorsReceiveEffects && !requestingPlayer.sync_isHeadhunter)
        {
            if(healthEffect > 0)
                requestingPlayer.Server_Heal(Player.HealSource.Consumable, healthEffect);
            else
                requestingPlayer.Server_Damage(Player.DamageSource.Consumable, healthEffect);
        
            requestingPlayer.Server_Eat(saturationEffect);
            requestingPlayer.Server_Drink(hydrationEffect);
        }


        if(!canBeUsedByMultipleSimultaneously)
            sync_isCurrentlyBeingConsumed = false;
        
        if (willRenew)
        {
            StartCoroutine(RenewAfter(renewTime));
        }
        
        Rpc_ConsumeEnd();
    }

    [TargetRpc]
    private void Rpc_RequestDenied(NetworkConnection target)
    {
        // Called on the requesting client when the request is denied
        
        Debug.Log("Consumable interaction denied by the server");
        
        localClientRequested = false;
    }
    
    [ClientRpc]
    private void Rpc_ConsumeStart()
    {
        // Called on all clients. Use localClientRequested to determine if it was the localClient which requested.
        
        if (localClientRequested && disableInputWhileConsumed)
        {
            Player.LocalPlayer.DisableInput();
        }
        
        OnConsumeStart(consumeTime);
    }
    
    [ClientRpc]
    private void Rpc_ConsumeEnd()
    {
        // Called on all clients. Use localClientRequested to determine if it was the localClient which requested.
        
        if (localClientRequested && disableInputWhileConsumed)
        {
            Player.LocalPlayer.EnableInput();
        }

        localClientRequested = false;
        
        OnConsumeEnd();
        
        if (destroyAfterConsumed)
        {
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void Rpc_OnRenew()
    {
        // Called on all clients
        
        OnRenew();
    }

    /// <summary>
    /// Called when Consume() is called.
    ///
    /// Can be used for animations.
    /// </summary>
    /// <param name="time">How long the consume process will take.</param>
    protected virtual void OnConsumeStart(float time) { }
    
    /// <summary>
    /// Called when the object has been Consumed.
    ///
    /// Can be used for cleanup.
    /// </summary>
    protected virtual void OnConsumeEnd() { }
    
    /// <summary>
    /// Called when the object has been Consumed.
    ///
    /// Can be used for cleanup.
    /// </summary>
    protected virtual void OnRenew() { }
}