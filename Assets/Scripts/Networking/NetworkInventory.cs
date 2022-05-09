using System;
using System.Collections;
using System.Collections.Generic;
using Headhunters.Networking.Interactions;
using Mirror;
using UnityEngine;

/// <summary>
/// Provides functionality for equipping instantiated gameObjects.
///
/// Access local inventory with NetworkInventory.LocalInventory.
/// 
/// Players can only carry a single item at a time (for now).
/// </summary>
public class NetworkInventory : NetworkBehaviour
{
    public static NetworkInventory LocalInventory;
    
    //public Transform localCarryPoint;

    [SerializeField]
    private KeyCode dropItemKey = KeyCode.C;

    [SyncVar]
    public NetworkCarriableBase sync_currentCarriableInHand;

    private Dictionary<uint, CarryPoint> carryPoints;

    private void Start()
    {
        carryPoints = new Dictionary<uint, CarryPoint>();
        foreach (CarryPoint carryPoint in GetComponentsInChildren<CarryPoint>())
        {
            carryPoints.Add(carryPoint.ID, carryPoint);
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (LocalInventory != null)
        {
            Debug.LogWarning("LocalInventory set multiple times!");
            return;
        }

        LocalInventory = this;
    }

    public static NetworkInventory GetInventoryOfConnection(NetworkConnectionToClient target)
    {
        Player player = Player.GetPlayerOfConnection(target);

        if (player == null)
        {
            return null;
        }

        return player.inventory;
    }

    public static bool LocalInventoryContainsAnyOfMaterialTag(List<string> materialTags)
    {
        return
            LocalInventory.sync_currentCarriableInHand != null && 
            materialTags.Contains(LocalInventory.sync_currentCarriableInHand.materialTag);
    }

    [Command(requiresAuthority = true)]
    public void Cmd_TryPickupItem(NetworkCarriableBase item, NetworkConnectionToClient requester = null)
    {
        // Check if the connection is valid.
        if (requester == null)
        {
            Debug.LogWarning("Received TryPickupItem Command from invalid NetworkConnection!");
            return;
        }
        
        // Check if the item is already being carried.
        if (item.sync_isCarriedCurrently)
        {
            Debug.LogWarning("De-sync / possible cheater detected: Connection " + requester.connectionId + " tried to carry without permission!");
            
            return;
        }

        // Check if we can get the player component of the requester.
        // This should never fail, unless the requesting player is dead.
        Player requestingPlayer = Player.GetPlayerOfConnection(requester);

        if (requestingPlayer == null)
            return;

        // Return early if item cannot be picked up when in headhunter state.
        if (!item.canBeCarriedWhileHeadhunter && requestingPlayer.sync_currentHeadhunterState == Headhunter.HeadhunterState.Headhunter)
        {
            return;
        }

        // Drop the currently carried item.
        if (sync_currentCarriableInHand != null)
        {
            Server_DropCarriedItem();
        }
        
        Server_PickupItem(item, requester);
    }

    [Command(requiresAuthority = true)]
    public void Cmd_TryDropCarriedItem(NetworkConnectionToClient requester = null)
    {
        // Check if the connection is valid.
        if (requester == null)
        {
            Debug.LogWarning("Received TryDropCarriedItem Command from invalid NetworkConnection!");
            return;
        }
        
        // If not carrying an item currently, return.
        NetworkCarriableBase carriedItem = sync_currentCarriableInHand;
        if(carriedItem == null) return;

        // Check that the item is being carried.
        if (!carriedItem.sync_isCarriedCurrently)
        {
            Debug.LogWarning("De-sync / possible cheater detected: Connection " + requester.connectionId + " tried to drop item that isn't carried!");
                
            return;
        }

        Server_DropCarriedItem();
    }

    [Command(requiresAuthority = true)]
    public void Cmd_TryUseCarriedItem(NetworkConnectionToClient requester = null)
    {
        // Check if the connection is valid.
        if (requester == null)
        {
            Debug.LogWarning("Received TryDropCarriedItem Command from invalid NetworkConnection!");
            return;
        }
        
        // If not carrying an item currently, return.
        NetworkCarriableBase carriedItem = sync_currentCarriableInHand;
        if(carriedItem == null) return;

        // Check that the item is being carried.
        if (!carriedItem.sync_isCarriedCurrently)
        {
            Debug.LogWarning("De-sync / possible cheater detected: Connection " + requester.connectionId + " tried to drop item that isn't carried!");
                
            return;
        }

        // Check that the external use condition is met
        if (!carriedItem.Server_ExternalUseCondition())
        {
            return;
        }
        
        Server_UseCarriedItem();
    }

    [Server]
    public void Server_DestroyCarriedItem()
    {
        if (sync_currentCarriableInHand == null)
        {
            return;
        }
        
        sync_currentCarriableInHand.Server_Destroy();
        sync_currentCarriableInHand = null;
    }

    [Server]
    private void Server_PickupItem(NetworkCarriableBase item, NetworkConnectionToClient requester)
    {
        // Assign obj authority to requester
        item.netIdentity.RemoveClientAuthority();
        item.netIdentity.AssignClientAuthority(requester);

        // Set SyncVars
        sync_currentCarriableInHand = item;
        item.sync_isCarriedCurrently = true;
        
        // Call functions on the carriable
        item.Server_AfterCarryStart();
        item.Rpc_Owner_AfterCarryStart(requester);
        item.Rpc_Client_AfterCarryStart();
    }

    [Server]
    public void Server_DropCarriedItem()
    {
        NetworkCarriableBase carriedItem = sync_currentCarriableInHand;
        if(carriedItem == null) return;
        
        // Get the owner/current carrier of the item
        NetworkConnectionToClient owner = carriedItem.netIdentity.connectionToClient;
        
        // Remove obj authority
        carriedItem.netIdentity.RemoveClientAuthority();
        
        // Set SyncVars
        carriedItem.sync_isCarriedCurrently = false;
        sync_currentCarriableInHand = null;
        
        // Call functions on the carriable
        carriedItem.Server_AfterCarryStop();
        carriedItem.Rpc_Owner_AfterCarryStopped(owner);
        carriedItem.Rpc_Client_AfterCarryStopped();
    }

    [Server]
    public void Server_UseCarriedItem()
    {
        NetworkCarriableBase carriedItem = sync_currentCarriableInHand;
        if(carriedItem == null) return;
        
        // Get the owner/current carrier of the item
        NetworkConnectionToClient owner = carriedItem.netIdentity.connectionToClient;
        
        carriedItem.Server_BeforeUsed();
        
        // Remove obj authority
        if(carriedItem.removeAuthorityOnUse)
            carriedItem.netIdentity.RemoveClientAuthority();
        
        // Set SyncVars
        carriedItem.sync_isCarriedCurrently = false;
        sync_currentCarriableInHand = null;
        
        // Call functions on the carriable
        carriedItem.Server_AfterUsed();
        carriedItem.Rpc_Owner_AfterUsed(owner);
        carriedItem.Rpc_Client_AfterUsed();
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(dropItemKey))
            {
                if(sync_currentCarriableInHand != null)
                    Cmd_TryDropCarriedItem();
            }
        }
    }

    /*
    //WARN: NetworkCarriableBase?
    public NetworkCarriable currentlyCarriedItem;

    //TODO: Function that takes a netID as input, converts that into a NetworkCarriable, if null, error/return. Mirrors that same netID to clients to assign.
    [Server]
    public void Server_EquipCarriable(uint carriableNetID, NetworkConnectionToClient requester = null)
    {
        
    }

    [ClientRpc]
    private void Rpc_OnStartedCarrying(uint carriableNetID)
    {
        if (isLocalPlayer)
        {
            
        }

        if (NetworkClient.spawned.TryGetValue(carriableNetID, out NetworkIdentity networkIdentity))
        {
            NetworkCarriable carriable = networkIdentity.GetComponent<NetworkCarriable>();

            if (carriable == null)
            {
                Debug.LogError("Remote client tried to carry non existent carriable.");
            }
        }
        else
        {
            Debug.LogError("Remote client tried to carry non existent carriable.");
        }
    }*/
}
