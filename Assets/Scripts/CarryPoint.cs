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
    public NetworkCarriableBase CurrentlyCarriedItem { get; private set; }

    private SmoothSyncMirror[] smooths;

    /// <summary>
    /// Assigns a carriable to this CarryPoint.
    /// </summary>
    /// <param name="carriable"></param>
    public void AssignCarriable(NetworkCarriableBase carriable)
    {
        CurrentlyCarriedItem = carriable;
        carriable.CurrentlyAssignedCarryPoint = this;

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
                smooth.enabled = true;
                smooth.clearBuffer();
            }
        }

        CurrentlyCarriedItem.CurrentlyAssignedCarryPoint = null;
        CurrentlyCarriedItem = null;
        HasCarriable = false;
    }

    private void Update()
    {
        if(CurrentlyCarriedItem == null) return;
        
        CurrentlyCarriedItem.transform.position = transform.position;
        CurrentlyCarriedItem.transform.rotation = transform.rotation;
    }

    private void Reset()
    {
        List<CarryPoint> otherPoints = new();
        otherPoints.AddRange(transform.root.GetComponentsInChildren<CarryPoint>());

        uint currentHighestID = 0;

        foreach (CarryPoint point in otherPoints)
        {
            if (point.ID > currentHighestID) currentHighestID = point.ID;
        }

        ID = currentHighestID + 1;
    }
}