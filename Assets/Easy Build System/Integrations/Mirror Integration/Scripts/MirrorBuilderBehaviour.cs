#if MIRRORNETWORK
using Mirror;
using UnityEngine;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;

public class MirrorBuilderBehaviour : BuilderBehaviour
{
    #region Fields

    private MirrorBuilderBehaviourReflector Reflector;

    #endregion

    #region Methods

    public override void Awake()
    {
        base.Awake();
        Reflector = GetComponentInParent<MirrorBuilderBehaviourReflector>();
    }

    public override void PlacePrefab(GroupBehaviour group = null)
    {
        AllowPlacement = CheckPlacementConditions();

        if (!AllowPlacement)
        {
            return;
        }

        if (CurrentEditionPreview != null)
        {
            Destroy(CurrentEditionPreview.gameObject);
        }

        Reflector.CmdPlace(SelectedPiece.Id, CurrentPreview.transform.position,
            CurrentPreview.transform.eulerAngles,
            CurrentPreview.transform.localScale);

        if (Source != null)
        {
            if (PlacementClips.Length != 0)
            {
                Source.PlayOneShot(PlacementClips[UnityEngine.Random.Range(0, PlacementClips.Length)]);
            }
        }

        if (!CurrentPreview.KeepLastRotation)
            CurrentPreviewRotationOffset = Vector3.zero;

        CurrentSocket = null;
        LastSocket = null;
        AllowPlacement = false;
        HasSocket = false;

        if (CurrentPreview != null)
        {
            Destroy(CurrentPreview.gameObject);
        }
    }

    public override void DestroyPrefab()
    {
        AllowDestruction = CheckDestructionConditions();

        if (!AllowDestruction)
        {
            return;
        }

        Reflector.CmdDestroy(CurrentRemovePreview.gameObject.GetComponent<NetworkIdentity>());

        if (Source != null)
        {
            if (DestructionClips.Length != 0)
            {
                Source.PlayOneShot(DestructionClips[UnityEngine.Random.Range(0, DestructionClips.Length)]);
            }
        }

        CurrentSocket = null;
        LastSocket = null;
        AllowDestruction = false;
        HasSocket = false;
    }

    #endregion
}
#endif