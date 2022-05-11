using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using Headhunters.Networking.Interactions;
using Mirror;
using UnityEngine;

public class UpgradeableStructure : NetworkBehaviour, IInteractable
{
    private List<string> upgradeMaterialTags;
    
    private AddonTheForestLike forestAddon;
    private PieceBehaviour pieceBehaviour;
    private Dictionary<string, int> distinctUpgradeMaterials;
    private string currentUpgradeText = "";

    private void Awake()
    {
        forestAddon = GetComponent<AddonTheForestLike>();
        pieceBehaviour = GetComponent<PieceBehaviour>();

        if (forestAddon == null)
        {
            forestAddon = GetComponentInParent<AddonTheForestLike>();
        }

        if (pieceBehaviour == null)
        {
            pieceBehaviour = GetComponentInParent<PieceBehaviour>();
        }

        if (forestAddon == null)
        {
            forestAddon = GetComponentInChildren<AddonTheForestLike>();
        }

        if (pieceBehaviour == null)
        {
            pieceBehaviour = GetComponentInChildren<PieceBehaviour>();
        }

        upgradeMaterialTags = new List<string>();
        distinctUpgradeMaterials = new Dictionary<string, int>();
        
        foreach (Transform childElement in forestAddon.Elements)
        {
            if (childElement.CompareTag("Untagged"))
            {
                Debug.LogWarning("Missing material tag on " + gameObject.name);
                
                continue;
            }
            
            upgradeMaterialTags.Add(childElement.tag);
        }

        for (int i = 0; i < upgradeMaterialTags.Count; i++)
        {
            if (distinctUpgradeMaterials.ContainsKey(upgradeMaterialTags[i]))
            {
                distinctUpgradeMaterials[upgradeMaterialTags[i]] += 1;
            }
            else
            {
                distinctUpgradeMaterials.Add(upgradeMaterialTags[i], 1);
            }
        }
    }

    public string GetInteractText()
    {
        if (forestAddon.IsCompleted())
        {
            return "";
        }
        
        // If carrying the wrong material
        if (!NetworkInventory.LocalInventory.InventoryContainsAnyOfMaterialTags(upgradeMaterialTags))
        {
            //TODO: Write progression per material
            return $"Progression: {forestAddon.GetCurrentProgression()}/{forestAddon.Elements.Length}.\nUpgrade with {currentUpgradeText}.";
        }
        
        return $"Progression: {forestAddon.GetCurrentProgression()}/{forestAddon.Elements.Length}.\nPlace";
    }

    public void Interact()
    {
        TryUpgrade();
    }

    public bool CanBeInteractedWith()
    {
        if (forestAddon == null)
            return false;

        if (forestAddon.IsCompleted())
            return false;

        //if (!CarriableController.Singleton.IsAnyMaterialCarriedCurrently(upgradeMaterialTags))
        if (!NetworkInventory.LocalInventory.InventoryContainsAnyOfMaterialTags(upgradeMaterialTags))
            return false;

        return true;
    }

    private void OnUpgraded(string materialUpgradedWith)
    {
        distinctUpgradeMaterials[materialUpgradedWith] -= 1;
        
        currentUpgradeText = "";
        for (int i = 0; i < distinctUpgradeMaterials.Count; i++)
        {
            if (i != 0)
            {
                currentUpgradeText += ", ";
            }
            
            (string material, int amount) = distinctUpgradeMaterials.ElementAt(i);
            currentUpgradeText += $"{material} x {amount}";
        }
    }

    private void TryUpgrade()
    {
        if (pieceBehaviour.CurrentState != StateType.Queue)
            return;
        
        Cmd_RequestUpgrade();

        /*string materialTag = NetworkInventory.LocalInventory.sync_currentCarriableInHand.materialTag;

        if (upgradeMaterialTags.Contains(materialTag))
        {
            Cmd_RequestUpgrade(materialTag);
        }*/
        
        /*if (CarriableController.Singleton.IsAnyMaterialCarriedCurrently(upgradeMaterialTags))
        {
            Cmd_RequestUpgrade(CarriableController.Singleton.GetLastCarriableMaterialTag());
            
            // WARN: ???
            //for (int m = 0; m < PickableController.Instance.TempElements.Count; m++)
            //    if (GetComponent<AddonTheForestLike>().Elements.FirstOrDefault(x => !x.gameObject.activeSelf && x.tag == PickableController.Instance.TempElements[m]) == null)
            //        return;
        }*/
    }

    [Command(requiresAuthority = false)]
    private void Cmd_RequestUpgrade(NetworkConnectionToClient requester = null)
    {
        // Check if the connection is valid.
        if (requester == null)
        {
            Debug.LogWarning("Received RequestUpgrade Command from invalid NetworkConnection!");
            return;
        }

        NetworkInventory inventory = NetworkInventory.GetInventoryOfConnection(requester);

        // Check if the player is actually carrying the item used to upgrade
        if (inventory.InventoryContainsAnyOfMaterialTags(upgradeMaterialTags, out NetworkCarriableBase material))
        {
            inventory.Server_DestroyCarriedItem(material);
        
            Rpc_ClientOnUpgrade(material.materialTag);
        }
    }

    [ClientRpc]
    private void Rpc_ClientOnUpgrade(string material)
    {
        forestAddon.Upgrade(material);
        
        OnUpgraded(material);
    }
}
