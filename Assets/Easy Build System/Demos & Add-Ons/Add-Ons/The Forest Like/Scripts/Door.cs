using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Transform doorPivot;
    
    [SerializeField]
    private float openSpeed = 2f;

    [SerializeField]
    private float openYDegrees = 70f;

    [SerializeField]
    private bool startOpen;
    
    [ReadOnly]
    [SerializeField]
    public bool isOpen;
    
    [ReadOnly]
    [SerializeField]
    private bool isChangingState;

    private void Start()
    {
        if (startOpen)
        {
            isOpen = true;
            doorPivot.transform.rotation = Quaternion.Euler(0, openYDegrees, 0);
        }
    }

    public string GetInteractText()
    {
        if (isChangingState) return "";
        
        return isOpen ? "Close" : "Open";
    }

    public void Interact()
    {
        if(isChangingState) return;

        StartCoroutine(MoveDoor());
    }

    public bool CanBeInteractedWith()
    {
        return !isChangingState;
    }

    private IEnumerator MoveDoor()
    {
        isChangingState = true;
        
        isOpen = !isOpen;

        float degrees = isOpen ? openYDegrees : -openYDegrees;
        
        doorPivot.DORotate(new Vector3(0, degrees, 0), openSpeed, RotateMode.LocalAxisAdd);
        
        yield return new WaitForSecondsRealtime(openSpeed);

        isChangingState = false;
    }
}