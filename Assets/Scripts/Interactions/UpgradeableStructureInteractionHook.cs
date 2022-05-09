using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UpgradeableStructureInteractionHook : MonoBehaviour, IInteractable
{
    [SerializeField]
    private UpgradeableStructure target;

    public string GetInteractText()
    {
        return target.GetInteractText();
    }

    public void Interact()
    {
        target.Interact();
    }

    public bool CanBeInteractedWith()
    {
        return target.CanBeInteractedWith();
    }
}
