using System;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Headhunters.Networking.Interactions
{
    public abstract class NetworkCarriableBase : NetworkBehaviour, IInteractable
    {
        [Tooltip("Used by the upgrade system to determine if material can be used to upgrade a building")]
        public string materialTag;
        
        [SerializeField]
        [Tooltip("Name of this item in the carry UI")]
        private string uiName;

        [SerializeField]
        public bool canBeCarriedWhileHeadhunter;

        [SerializeField]
        public bool removeAuthorityOnUse = true;

        [SerializeField]
        public bool removeFromInventoryOnUse = true;

        [SyncVar]
        [ReadOnly]
        [SerializeField]
        public bool sync_isCarriedCurrently;

        [SerializeField]
        private ItemPickupPreference PickupPreference;
        public CarryLocation CarryLocation;
        public CarryPoint CurrentlyAssignedCarryPoint;
        public bool PreferPickupToMainHand => CarryLocation.HasFlag(CarryLocation.MainHand) && PickupPreference == ItemPickupPreference.MainHand;
        public bool PreferPickupToOffHand => CarryLocation.HasFlag(CarryLocation.OffHand) && PickupPreference == ItemPickupPreference.Offhand;

        #region ABSTRACT METHODS

        /// <summary>
        /// Called on server when the object is picked up.
        /// </summary>
        public virtual void Server_AfterCarryStart(){}

        /// <summary>
        /// Called on server when the object is dropped.
        /// </summary>
        public virtual void Server_AfterCarryStop(){}
        
        /// <summary>
        /// Called on server before the object is used.
        /// </summary>
        public virtual void Server_BeforeUsed(){}

        /// <summary>
        /// Called on server when the object is used.
        /// </summary>
        public virtual void Server_AfterUsed(){}
        
        /// <summary>
        /// Called on owner client when the object is picked up.
        /// </summary>
        public virtual void Owner_AfterCarryStart(){}
        
        /// <summary>
        /// Called on owner client when the object is dropped.
        /// </summary>
        public virtual void Owner_AfterCarryStop(){}

        /// <summary>
        /// Called on owner client when the object is destroyed.
        /// </summary>
        public virtual void Owner_AfterDestroyed(){}

        /// <summary>
        /// Called on owner client when the object is used.
        /// </summary>
        public virtual void Owner_AfterUsed(){}

        /// <summary>
        /// Called on all clients when the object is picked up.
        /// </summary>
        public virtual void Client_AfterCarryStart(uint carryPointID, uint carryingPlayerNetID)
        {
            NetworkInventory.GetInventoryOfNetID(carryingPlayerNetID).GetCarryPointByID(carryPointID).AssignCarriable(this);
        }

        /// <summary>
        /// Called on all clients when the object is dropped.
        /// </summary>
        public virtual void Client_AfterCarryStop()
        {
            CurrentlyAssignedCarryPoint.UnAssignCarriable();
        }

        /// <summary>
        /// Called on all clients when the object is destroyed.
        /// </summary>
        public virtual void Client_AfterDestroyed()
        {
            CurrentlyAssignedCarryPoint.UnAssignCarriable();
        }

        /// <summary>
        /// Called on all clients when the object is used.
        /// </summary>
        public virtual void Client_AfterUsed()
        {
            if (removeFromInventoryOnUse)
            {
                CurrentlyAssignedCarryPoint.UnAssignCarriable();
            }
        }

        /// <summary>
        /// Inheritable serverside condition that defines if the carriable can be used currently.
        /// </summary>
        /// <returns></returns>
        [Server]
        public virtual bool Server_ExternalUseCondition()
        {
            return true;
        }

        #endregion

        #region I_INTERACTABLE IMPLEMENTATION

        public virtual string GetInteractText()
        {
            return "Carry: " + uiName;
        }

        public virtual void Interact()
        {
            NetworkInventory.LocalInventory.Cmd_TryPickupItem(this);
        }

        public virtual bool CanBeInteractedWith()
        {
            // False if carried right now
            if (sync_isCarriedCurrently) return false;
            
            // False if player is currently headhunter and cannot be carried while in headhunter state
            if (Player.LocalPlayer.CurrentlyInHeadhunterState && !canBeCarriedWhileHeadhunter) return false;

            return true;
        }

        #endregion

        #region REMOTE ACTIONS

        [TargetRpc]
        public void Rpc_Owner_AfterCarryStart(NetworkConnection target)
        {
            Owner_AfterCarryStart();
        }
        
        [ClientRpc]
        public void Rpc_Client_AfterCarryStart(uint carryPointID, uint carryingPlayerNetID)
        {
            Client_AfterCarryStart(carryPointID, carryingPlayerNetID);
        }

        [TargetRpc]
        public void Rpc_Owner_AfterCarryStopped(NetworkConnection target)
        {
            Owner_AfterCarryStop();
        }
        
        [ClientRpc]
        public void Rpc_Client_AfterCarryStopped()
        {
            Client_AfterCarryStop();
        }

        [ClientRpc]
        private void Rpc_ClientOnDestroyed()
        {
            Client_AfterDestroyed();
        }

        [TargetRpc]
        private void Rpc_OwnerOnDestroyed(NetworkConnection target)
        {
            Owner_AfterDestroyed();
        }

        [ClientRpc]
        public void Rpc_Client_AfterUsed()
        {
            Client_AfterUsed();
        }

        [TargetRpc]
        public void Rpc_Owner_AfterUsed(NetworkConnection target)
        {
            Owner_AfterUsed();
        }

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Destroys this object on all clients.
        /// </summary>
        [Server]
        public void Server_Destroy()
        {
            Rpc_ClientOnDestroyed();
            Rpc_OwnerOnDestroyed(netIdentity.connectionToClient);
            
            //NetworkServer.Destroy(gameObject);
        }

        #endregion

        /*protected virtual void Update()
        {
            if (!hasAuthority || !sync_isCarriedCurrently) return;
            
            ApplyPositionAndRotation();
        }

        [Client]
        protected virtual void ApplyPositionAndRotation()
        {
            transform.position = Vector3.Lerp(transform.position, NetworkInventory.LocalInventory.localCarryPoint.position, Time.deltaTime * 15f);
            transform.rotation = Quaternion.Lerp(transform.rotation, NetworkInventory.LocalInventory.localCarryPoint.rotation, Time.deltaTime * 15f);
        }*/
    }
}