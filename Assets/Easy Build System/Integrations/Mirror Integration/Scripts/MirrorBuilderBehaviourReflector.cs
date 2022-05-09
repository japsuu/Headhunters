#if MIRRORNETWORK
using Mirror;
using UnityEngine;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;

public class MirrorBuilderBehaviourReflector : NetworkBehaviour
{
    #region Methods

    [Command]
    public void CmdPlace(string pieceId, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (BuildManager.Instance.GetPieceById(pieceId) != null)
            NetworkServer.Spawn(BuildManager.Instance.PlacePrefab(BuildManager.Instance.GetPieceById(pieceId), position, rotation, scale, null, null, false).gameObject);
    }

    [Command]
    public void CmdDestroy(NetworkIdentity identity)
    {
        NetworkServer.Destroy(identity.gameObject);
    }

    #endregion
}
#endif