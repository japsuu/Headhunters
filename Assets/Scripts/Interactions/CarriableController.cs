using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Headhunters.Networking.Interactions;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// WARN: Not server authoritative!!!
/// TODO: Make server authoritative
/// </summary>
public class CarriableController : SingletonBehaviour<CarriableController>
{
    /*
    [SerializeField]
    private KeyCode dropCarriableKey = KeyCode.C;

    /// <summary>
    /// Contains the local player's carryPoints.
    /// </summary>
    [ReadOnly]
    [SerializeField]
    private Transform[] carryPoints;
    /// <summary>
    /// Contains the items currently carried. Capacity = carryPoints size.
    /// </summary>
    private NetworkCarriableBase[] carrySlots;

    private void Update()
    {
        if(carryPoints == null) return;
        
        if (Input.GetKeyDown(dropCarriableKey))
        {
            DropLastCarriable();
        }
    }

    public void LocalClient_RegisterCarryPoints(Transform[] points)
    {
        carryPoints = points;
        carrySlots = new NetworkCarriableBase[carryPoints.Length];
    }
    
    public void LocalClient_OnCarryStart(NetworkCarriableBase carriable)
    {
        // Check if there is a slot available.
        // If is -> assign carryable to it. If not -> return.
        int availableSlot = GetFirstAvailableSlot();
        if(availableSlot == -1) return;
        
        carrySlots[availableSlot] = carriable;
        carriable.SetCarryPoint(carryPoints[availableSlot]);
        
        carriable.gameObject.transform.SetParent(null);
    }

    public void TryPickUp(NetworkCarriableBase target)
    {
        if(CanPickUp(target))
            target.RequestCarry();
    }

    private bool CanPickUp(NetworkCarriableBase carriable)
    {
        if (!CanCarryOfMaterial(carriable.materialTag)) return false;
        
        return GetFirstAvailableSlot() != -1;
    }

    private int GetFirstAvailableSlot()
    {
        int availableSlot = -1;
        for (int i = 0; i < carrySlots.Length; i++)
        {
            if (carrySlots[i] == null)
            {
                availableSlot = i;
                break;
            }
        }

        return availableSlot;
    }

    // Checks if client can carry a carriable of a specific ID.
    private bool CanCarryOfMaterial(string material)
    {
        foreach (NetworkCarriableBase slot in carrySlots)
        {
            if(slot == null) continue;
            
            if (slot.materialTag != material) return false;
        }

        return true;
    }

    public void RemoteClient_PickUpCarriable(NetworkCarriableBase carriable)
    {
        carriable.gameObject.transform.SetParent(null);
    }
    
    private void DropCarriable(int index)
    {
        NetworkCarriableBase carriable = carrySlots[index];

        carriable.localClientCarryPointIndex = index;
        
        carriable.RequestDrop();
    }

    private void DestroyCarriable(int index)
    {
        NetworkCarriableBase carriable = carrySlots[index];

        carriable.localClientCarryPointIndex = index;
        
        carriable.RequestDestroy();
    }

    public void LocalClient_OnCarryStop(NetworkCarriableBase target)
    {
        target.gameObject.transform.SetParent(null);

        carrySlots[target.localClientCarryPointIndex] = null;
    }

    public void LocalClient_OnCarriableDestroyed(NetworkCarriableBase target)
    {
        carrySlots[target.localClientCarryPointIndex] = null;
    }

    private void DropLastCarriable()
    {
        for (int i = carrySlots.Length - 1; i >= 0; i--)
        {
            if (carrySlots[i] == null) continue;
            
            DropCarriable(i);
                
            return;
        }
    }

    public string GetTagAndDestroyLastCarriable()
    {
        for (int i = carrySlots.Length - 1; i >= 0; i--)
        {
            if (carrySlots[i] == null) continue;

            string lastTag = carrySlots[i].materialTag;
            
            DestroyCarriable(i);
                
            return lastTag;
        }

        return null;
    }

    public bool IsMaterialCarriedCurrently(string materialTag)
    {
        for (int i = carrySlots.Length - 1; i >= 0; i--)
        {
            if (carrySlots[i] == null) continue;

            if (carrySlots[i].materialTag == materialTag)
                return true;
        }

        return false;
    }

    public bool IsAnyMaterialCarriedCurrently(IEnumerable<string> materialTags)
    {
        return materialTags.Any(IsMaterialCarriedCurrently);
    }

    public string GetLastCarriableMaterialTag()
    {
        for (int i = carrySlots.Length - 1; i >= 0; i--)
        {
            if (carrySlots[i] == null) continue;
                
            return carrySlots[i].materialTag;
        }

        return null;
    }*/
}
