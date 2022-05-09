#if MIRRORNETWORK
using Mirror;
using UnityEngine;

public class Demo_ExamplePlayer : NetworkBehaviour
{
    public Camera Camera;

    private void Start()
    {
        Camera.gameObject.SetActive(isLocalPlayer);
    }
}
#endif