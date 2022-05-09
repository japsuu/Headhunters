using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    public bool InteractionEnabled { get; private set; }

    private IInteractable lastFrameInteractable;
    private Ray debugInteractionRay;

    private void Awake()
    {
        InteractionEnabled = true;
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        InteractionEnabled = isEnabled;

        lastFrameInteractable = null;
    }

    private void Update()
    {
        if(!InteractionEnabled || Player.LocalPlayer == null)
        {
            IngameUIManager.Singleton.SetInteractTextVisible(false);
            return;
        }

        InteractionUpdate();
    }

    private void LateUpdate()
    {
        if(Player.LocalPlayer == null) return;
        
        
    }

    private void InteractionUpdate()
    {
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
                //WARN: TODO: Make sure the interact text can not leave the screen rect!!!
                IngameUIManager.Singleton.SetInteractTextPosition(Player.LocalPlayer.InteractionCam, hit.collider.bounds.center);
                
                bool canInteract = interactable.CanBeInteractedWith();
                if (canInteract)
                {
                    if (Input.GetKeyDown(interactKey))
                    {
                        interactable.Interact();
                    }
                }

                /* If we are targeting a different entity from last frame
                if (interactable != lastFrameInteractable)
                {
                    
                }*/
                string interactText = interactable.GetInteractText();

                // If can be interacted with, add the key prompt
                string text = canInteract ? interactText + $" [{interactKey.ToString()}]" : interactText;
                    
                IngameUIManager.Singleton.SetInteractText(text);
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
