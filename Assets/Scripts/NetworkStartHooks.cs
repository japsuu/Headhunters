using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class NetworkStartHooks : NetworkBehaviour
{
    public UnityEvent OnStartLocal;
    public UnityEvent OnStartRemote;
    
    public override void OnStartClient()
    {
        if (netIdentity.isLocalPlayer)
        {
            OnStartLocal?.Invoke();
        }
        else
        {
            OnStartRemote?.Invoke();
        }
    }
}
