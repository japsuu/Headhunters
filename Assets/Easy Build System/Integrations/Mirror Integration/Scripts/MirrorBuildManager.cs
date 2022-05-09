#if MIRRORNETWORK
using Mirror;
using UnityEngine;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage;

public class MirrorBuildManager : MonoBehaviour
{
    #region Fields

    private BuildStorage Storage { get; set; }

    #endregion

    #region Methods

    private void Awake()
    {
        Storage = FindObjectOfType<BuildStorage>();

        if (Storage != null)
        {
            Storage.LoadPrefabs = false;
            Storage.SavePrefabs = false;
        }

        BuildEvent.Instance.OnStorageLoadingResult.AddListener((PieceBehaviour[] pieces) => {

            if (pieces == null) return;

            if (NetworkServer.active)
            {
                foreach (PieceBehaviour Piece in pieces)
                {
                    Piece.transform.parent = null;
                    NetworkServer.Spawn(Piece.gameObject);
                }
            }

        });
    }

    private void Start()
    {
        foreach (PieceBehaviour Piece in BuildManager.Instance.Pieces)
            if (Piece != null)
                NetworkClient.RegisterPrefab(Piece.gameObject);
    }

    private void OnApplicationQuit()
    {
        if (Storage != null)
            Storage.SaveStorageFile();
    }

    private void FixedUpdate()
    {
        if (NetworkServer.active)
        {
            if (Storage != null)
            {
                if (!Storage.LoadedFile)
                {
                    Storage.LoadStorageFile();
                }
            }
        }
    }

    #endregion
}
#endif