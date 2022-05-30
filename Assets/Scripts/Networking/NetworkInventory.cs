using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private KeyCode dropItemKey = KeyCode.C;

    public readonly SyncList<NetworkCarriableBase> CurrentlyCarriedItems = new();
    private Dictionary<uint, CarryPoint> carryPoints;
    private CarryPoint mainHandPoint;
    private CarryPoint offHandPoint;

    public CarryPoint GetCarryPointByID(uint carryPointID)
    {
        return carryPoints[carryPointID];
    }

    private void Start()
    {
        carryPoints = new Dictionary<uint, CarryPoint>();
        foreach (CarryPoint carryPoint in GetComponentsInChildren<CarryPoint>())
        {
            carryPoints.Add(carryPoint.ID, carryPoint);

            if (carryPoint.location == CarryLocation.MainHand)
            {
                if (mainHandPoint != null)
                    Debug.LogWarning("Multiple MainHands set!");

                mainHandPoint = carryPoint;
            }
            if (carryPoint.location == CarryLocation.OffHand)
            {
                if (offHandPoint != null)
                    Debug.LogWarning("Multiple OffHands set!");

                offHandPoint = carryPoint;
            }
        }

        if (carryPoints.Count < 1)
        {
            Debug.LogWarning($"Could not find any CarryPoints for {gameObject.name}");
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

    public static NetworkInventory GetInventoryOfConnection(NetworkConnection target)
    {
        Player player = Player.Server_GetPlayerOfConnection(target);

        if (player == null)
        {
            return null;
        }

        return player.Inventory;
    }

    public static NetworkInventory GetInventoryOfNetID(uint netID)
    {
        return NetworkClient.spawned[netID].gameObject.GetComponent<Player>().Inventory;
    }

    public bool InventoryContainsAnyOfMaterialTags(List<string> materialTags)
    {
        foreach (NetworkCarriableBase item in CurrentlyCarriedItems)
        {
            if (materialTags.Contains(item.materialTag))
                return true;
        }

        return false;
    }

    public bool InventoryContainsAnyOfMaterialTags(List<string> materialTags, out NetworkCarriableBase firstMatch)
    {
        foreach (NetworkCarriableBase item in CurrentlyCarriedItems)
        {
            if (materialTags.Contains(item.materialTag))
            {
                firstMatch = item;
                return true;
            }
        }

        firstMatch = null;
        return false;
    }

    public NetworkCarriableBase GetItemCurrentlyInHand(bool includeOffhand)
    {
        // If not carrying an item currently, return.
        NetworkCarriableBase carriedItem = mainHandPoint.CurrentlyCarriedItem;
        if(carriedItem == null)
        {
            if (!includeOffhand) return null;
            
            carriedItem = offHandPoint.CurrentlyCarriedItem;
            
            if(carriedItem == null)
                return null;
        }

        return carriedItem;
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
        Player requestingPlayer = Player.Server_GetPlayerOfConnection(requester);

        if (requestingPlayer == null)
            return;

        // Return early if item cannot be picked up when in headhunter state.
        if (!item.canBeCarriedWhileHeadhunter && requestingPlayer.CurrentlyInHeadhunterState)
        {
            return;
        }

        if (mainHandPoint == null)
        {
            Debug.LogWarning("MainHand CarryPoint missing!");
            return;
        }

        if (offHandPoint == null)
        {
            Debug.LogWarning("OffHand CarryPoint missing!");
            return;
        }

        CarryPoint offHandCarryPoint = null;
        CarryPoint mainHandCarryPoint = null;
        CarryPoint firstAvailableInventoryCarryPoint = null;
        
        // If item can be carried in OffHand, check for a free hand.
        if(item.CarryLocation.HasFlag(CarryLocation.OffHand) && !offHandPoint.HasCarriable)
        {
            offHandCarryPoint = offHandPoint;
        }
        
        // If item can be carried in MainHand, check for a free hand.
        if(item.CarryLocation.HasFlag(CarryLocation.MainHand) && !mainHandPoint.HasCarriable)
        {
            mainHandCarryPoint = mainHandPoint;
        }

        foreach (CarryPoint carryPoint in carryPoints.Values)
        {
            if (carryPoint.HasCarriable) continue;
            if (item.PreferPickupToMainHand && carryPoint.location == CarryLocation.MainHand) continue;
            if (item.PreferPickupToOffHand && carryPoint.location == CarryLocation.OffHand) continue;
            if (!item.CarryLocation.HasFlag(carryPoint.location)) continue;
            
            firstAvailableInventoryCarryPoint = carryPoint;
            break;
        }

        if (item.PreferPickupToMainHand && mainHandCarryPoint != null)
        {
            Server_PickupItem(item, mainHandCarryPoint, requester);
            return;
        }
        if (item.PreferPickupToOffHand && offHandCarryPoint != null)
        {
            Server_PickupItem(item, offHandCarryPoint, requester);
            return;
        }
        if(firstAvailableInventoryCarryPoint != null)
        {
            Server_PickupItem(item, firstAvailableInventoryCarryPoint, requester);
            return;
        }
    }

    [Command(requiresAuthority = true)]
    public void Cmd_TryDropHandItem(NetworkConnectionToClient requester = null)
    {
        // Check if the connection is valid.
        if (requester == null)
        {
            Debug.LogWarning("Received TryDropCarriedItem Command from invalid NetworkConnection!");
            return;
        }
        
        // If not carrying an item currently, return.
        NetworkCarriableBase carriedItem = mainHandPoint.CurrentlyCarriedItem;
        if(carriedItem == null)
        {
            carriedItem = offHandPoint.CurrentlyCarriedItem;
            
            if(carriedItem == null)
                return;
        }

        // Check that the item is being carried.
        if (!carriedItem.sync_isCarriedCurrently)
        {
            Debug.LogWarning("De-sync / possible cheater detected: Connection " + requester.connectionId + " tried to drop item that isn't carried!");
                
            return;
        }

        Server_DropItem(carriedItem);
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
        NetworkCarriableBase carriedItem = mainHandPoint.CurrentlyCarriedItem;
        if(carriedItem == null)
        {
            carriedItem = offHandPoint.CurrentlyCarriedItem;
            
            if(carriedItem == null)
            {
                Debug.Log("Not carrying anything");
                return;
            }
        }

        // Check that the item is being carried.
        if (!carriedItem.sync_isCarriedCurrently)
        {
            Debug.LogWarning($"De-sync / possible cheater detected: Connection {requester.connectionId} tried to drop item ({carriedItem.name}) that isn't currently carried!");
                
            return;
        }

        // Check that the external use condition is met
        if (!carriedItem.Server_ExternalUseCondition())
        {
            return;
        }
        
        Server_UseCarriedItem(carriedItem);
    }

    [Server]
    public void Server_DestroyCarriedItem(NetworkCarriableBase item)
    {
        /*if (sync_currentCarriableInHand == null)
        {
            return;
        }
        
        sync_currentCarriableInHand.Server_Destroy();
        sync_currentCarriableInHand = null;*/

        CurrentlyCarriedItems.Remove(item);
        item.Server_Destroy();
    }

    [Server]
    private void Server_PickupItem(NetworkCarriableBase item, CarryPoint carryPoint, NetworkConnectionToClient requester)
    {
        // Assign obj authority to requester
        item.netIdentity.RemoveClientAuthority();
        item.netIdentity.AssignClientAuthority(requester);

        // Set SyncVars
        //if(carryPoint.location == CarryLocation.Hand)
        //    sync_currentCarriableInHand = item;
        item.sync_isCarriedCurrently = true;
        CurrentlyCarriedItems.Add(item);
        
        // Call functions on the carriable
        item.Server_AfterCarryStart();
        item.Rpc_Owner_AfterCarryStart(requester);
        item.Rpc_Client_AfterCarryStart(carryPoint.ID, requester.identity.netId);
    }

    [Server]
    public void Server_DropItem(NetworkCarriableBase carriedItem)
    {
        if(carriedItem == null) return;
        
        // Get the owner/current carrier of the item
        NetworkConnectionToClient owner = carriedItem.netIdentity.connectionToClient;
        
        // Remove obj authority
        carriedItem.netIdentity.RemoveClientAuthority();
        
        // Set SyncVars
        carriedItem.sync_isCarriedCurrently = false;
        //sync_currentCarriableInHand = null;
        CurrentlyCarriedItems.Remove(carriedItem);
        
        // Call functions on the carriable
        carriedItem.Server_AfterCarryStop();
        carriedItem.Rpc_Owner_AfterCarryStopped(owner);
        carriedItem.Rpc_Client_AfterCarryStopped();
    }

    [Server]
    public void Server_DropAllItems()
    {
        foreach (NetworkCarriableBase carriedItem in CurrentlyCarriedItems)
        {
            if(carriedItem == null) return;
        
            // Get the owner/current carrier of the item
            NetworkConnectionToClient owner = carriedItem.netIdentity.connectionToClient;
        
            // Remove obj authority
            carriedItem.netIdentity.RemoveClientAuthority();
        
            // Set SyncVars
            carriedItem.sync_isCarriedCurrently = false;
            CurrentlyCarriedItems.Remove(carriedItem);
        
            // Call functions on the carriable
            carriedItem.Server_AfterCarryStop();
            carriedItem.Rpc_Owner_AfterCarryStopped(owner);
            carriedItem.Rpc_Client_AfterCarryStopped();
        }
    }

    [Server]
    public void Server_UseCarriedItem(NetworkCarriableBase carriedItem)
    {
        if(carriedItem == null) return;
        
        // Get the owner/current carrier of the item
        NetworkConnectionToClient owner = carriedItem.netIdentity.connectionToClient;
        
        carriedItem.Server_BeforeUsed();
        
        // Remove obj authority
        if(carriedItem.removeAuthorityOnUse)
        {
            carriedItem.netIdentity.RemoveClientAuthority();
        }

        if (carriedItem.removeFromInventoryOnUse)
        {
            carriedItem.sync_isCarriedCurrently = false;
            
            CurrentlyCarriedItems.Remove(carriedItem);
        }
        
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
                if(mainHandPoint.CurrentlyCarriedItem != null || offHandPoint.CurrentlyCarriedItem != null)
                    Cmd_TryDropHandItem();
            }
        }
    }
}
