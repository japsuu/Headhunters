using System.Collections;
using System.Collections.Generic;
using Headhunters.Networking.Interactions;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/New Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public List<string> RequiredMaterials;

    public NetworkCarriableBase Result;
}
