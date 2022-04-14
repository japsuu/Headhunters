using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWater : NetworkedConsumable, IInteractable
{
    public string GetInteractText()
    {
        return "Drink";
    }

    public void Interact()
    {
        Consume();
    }

    public bool CanBeInteractedWith()
    {
        return true;
    }
}
