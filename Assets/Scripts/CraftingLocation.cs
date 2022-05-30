using System;
using System.Collections;
using System.Collections.Generic;
using Headhunters.Networking.Interactions;
using UnityEngine;

public class CraftingLocation : MonoBehaviour, IInteractable
{
    [SerializeField]
    private List<CraftingRecipe> craftableRecipes;

    private List<NetworkCarriableBase> placedItems;
    private CraftingRecipe currentlyCraftableRecipe;

    private void Awake()
    {
        placedItems = new List<NetworkCarriableBase>();
    }

    private void CalculateCurrentlyCraftableRecipe()
    {
        
    }

    public void PlaceItem(NetworkCarriableBase item)
    {
        placedItems.Add(item);

        CalculateCurrentlyCraftableRecipe();
    }

    public string GetInteractText()
    {
        if (currentlyCraftableRecipe != null)
        {
            return $"Craft: {currentlyCraftableRecipe.Result.uiName}";
        }

        NetworkCarriableBase handItem = NetworkInventory.LocalInventory.GetItemCurrentlyInHand(false);
        return handItem == null ? "" : $"Place: {handItem.uiName}";
    }

    public void Interact()
    {
        
    }

    public bool CanBeInteractedWith()
    {
        return currentlyCraftableRecipe != null || NetworkInventory.LocalInventory.GetItemCurrentlyInHand(false) != null;
    }
}
