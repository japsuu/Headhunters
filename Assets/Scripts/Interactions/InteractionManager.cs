using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractionManager : SingletonBehaviour<InteractionManager>
{
    [SerializeField]
    private KeyCode interactKey = KeyCode.F;

    [SerializeField]
    private LayerMask interactLayer;

    public float interactDistance = 3;

    [ReadOnly]
    public bool interactionEnabled = true;

    private IInteractable lastFrameInteractable;
    private bool lastFrameCouldInteract;
    private Ray debugInteractionRay;

    private void Update()
    {
        if(!interactionEnabled || Player.LocalPlayer == null)
        {
            IngameUIManager.Singleton.SetInteractText("");
            return;
        }

        Transform playerCameraTransform = Player.LocalPlayer.InteractionCam.transform;
        Ray camForward = new Ray(playerCameraTransform.position, playerCameraTransform.forward);
        debugInteractionRay = camForward;
        IInteractable interactable = null;

        //  IF: Raycast hits a collider.
        //      IF: Collider is interactable.
        //          IF: Interactable can be interacted with.
        //              IF: Interaction key pressed.
        //                  Interact();
        
        if (Physics.Raycast(camForward, out RaycastHit hit, interactDistance, interactLayer))
        {
            interactable = hit.collider.gameObject.GetComponent<IInteractable>();

            if (interactable != null)
            {
                bool canInteract = interactable.CanBeInteractedWith();
                if (canInteract)
                {
                    if (Input.GetKeyDown(interactKey))
                    {
                        interactable.Interact();
                    }
                }
                
                // Get and display the UI text if the interactable is different from last frame.
                if (lastFrameInteractable != interactable || canInteract != lastFrameCouldInteract)
                {
                    string interactText = interactable.GetInteractText();

                    string text = canInteract ? interactText + $" [{interactKey.ToString()}]" : interactText;
                    
                    if(!string.IsNullOrEmpty(interactText))
                        IngameUIManager.Singleton.SetInteractText(text);
                }

                lastFrameCouldInteract = canInteract;
            }
            else
            {
                // Currently targeting an object that is not an interactable
                IngameUIManager.Singleton.SetInteractText("");
            }
        }
        else
        {
            // Currently not targeting an object
            IngameUIManager.Singleton.SetInteractText("");
        }

        lastFrameInteractable = interactable;
    }

    private void OnDrawGizmos()
    {
        if (debugInteractionRay.origin != Vector3.zero)
        {
            Gizmos.DrawRay(debugInteractionRay);
        }
    }
}
