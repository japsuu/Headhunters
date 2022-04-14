using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestBush : NetworkedConsumable, IInteractable
{
    public UnityEvent OnBerriesEaten;
    public UnityEvent OnBerriesRespawn;
    
    public string GetInteractText()
    {
        return "Eat berries";
    }

    public void Interact()
    {
        Consume();
    }

    public bool CanBeInteractedWith()
    {
        return !sync_needsRenew;
    }

    protected override void OnConsumeEnd()
    {
        OnBerriesEaten?.Invoke();
    }

    protected override void OnRenew()
    {
        OnBerriesRespawn?.Invoke();
    }
}
