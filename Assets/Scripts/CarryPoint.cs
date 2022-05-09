using System;
using System.Collections;
using System.Collections.Generic;
using Headhunters.Networking.Interactions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Smooth;
using UnityEngine;

/// <summary>
/// Represents an in-game inventory slot.
///
/// Automatically disables assigned Carriable's SmoothSync components.
/// Automatically syncs assigned Carriable's position and rotation with this object.
///
/// Every CarryPoint should have a different ID!
/// </summary>
public class CarryPoint : MonoBehaviour
{
    public CarryLocation location;

    public uint ID;

    public bool HasCarriable;

    [ReadOnly]
    [OdinSerialize]
    private NetworkCarriableBase currentlyCarriedItem;

    private SmoothSyncMirror[] smooths;

    /// <summary>
    /// Assigns a carriable to this CarryPoint.
    /// </summary>
    /// <param name="carriable"></param>
    public void AssignCarriable(NetworkCarriableBase carriable)
    {
        currentlyCarriedItem = carriable;
        carriable.currentlyAssignedCarryPoint = this;

        smooths = carriable.GetComponents<SmoothSyncMirror>();
        
        if(smooths == null) return;

        foreach (SmoothSyncMirror smooth in smooths)
        {
            smooth.enabled = false;
        }

        HasCarriable = true;
    }

    /// <summary>
    /// Un-Assigns a carriable from this CarryPoint.
    /// </summary>
    public void UnAssignCarriable()
    {
        if (smooths != null)
        {
            foreach (SmoothSyncMirror smooth in smooths)
            {
                smooth.enabled = false;
            }
        }

        currentlyCarriedItem.currentlyAssignedCarryPoint = null;
        currentlyCarriedItem = null;
        HasCarriable = false;
    }

    private void Update()
    {
        if(currentlyCarriedItem == null) return;
        
        currentlyCarriedItem.transform.position = transform.position;
        currentlyCarriedItem.transform.rotation = transform.rotation;
    }
}