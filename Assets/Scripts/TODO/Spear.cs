using System;
using System.Collections;
using System.Collections.Generic;
using Headhunters.Networking.Interactions;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Spear : NetworkCarriableObject
{
    public static readonly UnityEvent<Collision> OnSpearHitSomething = new();

    [SerializeField]
    private float baseThrowForce = 30f;
    
    [SerializeField]
    private float maxThrowForce = 80f;

    [SerializeField]
    [Range(0f, 40f)]
    private float fovDecreaseWhenAimed = 20f;

    [SerializeField]
    private float aimTime = 3f;

    [SerializeField]
    private float parabolaAngleFactor = 5;

    [SerializeField]
    private bool isAiming;

    private bool Local_IsHeld => hasAuthority && sync_isCarriedCurrently;
    
    private float timeAimedFor;
    private const float FovResetTime = 1f;
    private int server_throwingPlayerID;

    protected override void Update()
    {
        base.Update();

        if (Local_IsHeld)
        {
            // Start aiming with LMB down
            if (Input.GetMouseButtonDown(0))
            {
                Cmd_OnStartAim();
                CameraEffects.Singleton.SetTargetFovModifier(fovDecreaseWhenAimed, aimTime);
            }

            // Cancel aiming with RMB down
            if (Input.GetMouseButtonDown(1))
            {
                Cmd_OnStopAim();
                CameraEffects.Singleton.SetTargetFovModifier(0, FovResetTime);
            }

            // Throw with LMB up
            if (isAiming && Input.GetMouseButtonUp(0))
            {
                NetworkInventory.LocalInventory.Cmd_TryUseCarriedItem();
                
                CameraEffects.Singleton.SetTargetFovModifier(0, FovResetTime);
            }
        }

        if (isServer)
        {
            if (isAiming)
            {
                timeAimedFor += Time.deltaTime;

                if (timeAimedFor > aimTime)
                    timeAimedFor = aimTime;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isAiming && isServer)
        {
            if(mainRb.velocity.magnitude > 3f)
            {
                mainRb.rotation = Quaternion.LookRotation(Vector3.Slerp(transform.forward, mainRb.velocity, Time.deltaTime * parabolaAngleFactor));
            }
        }
    }

    //TODO: Parent the model to the collision target. When unparented, teleport the root object to the model (?).
    private void OnCollisionEnter(Collision other)
    {
        Player hitPlayer = other.gameObject.GetComponent<Player>();

        if (hitPlayer == null) return;
        
        if (isServer)
        {
            
            if (hitPlayer.netIdentity.connectionToClient.connectionId == server_throwingPlayerID) return;

            hitPlayer.Server_Damage(Player.DamageSource.Player, mainRb.velocity.magnitude);

            mainRb.velocity /= 4;
        }
        
        OnSpearHitSomething.Invoke(other);
    }

    public override void Client_AfterUsed()
    {
        EnablePhysics();
    }

    public override void Server_BeforeUsed()
    {
        server_throwingPlayerID = netIdentity.connectionToClient.connectionId;
    }

    public override void Server_AfterUsed()
    {
        EnablePhysics();
        
        Vector3 throwForce = transform.forward * Mathf.Max(baseThrowForce, timeAimedFor / aimTime * maxThrowForce);
        mainRb.AddForce(throwForce, ForceMode.Impulse);
        
        isAiming = false;
        timeAimedFor = 0;
    }

    [Command(requiresAuthority = true)]
    private void Cmd_OnStartAim()
    {
        isAiming = true;
    }

    [Command(requiresAuthority = true)]
    private void Cmd_OnStopAim()
    {
        isAiming = false;
        timeAimedFor = 0;
    }

    public override bool Server_ExternalUseCondition()
    {
        return true;
    }

    protected override void ApplyPositionAndRotation()
    {
        transform.position = Vector3.Lerp(transform.position, Player.LocalPlayer.spearHoldTransform.position, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, Player.LocalPlayer.spearHoldTransform.rotation, Time.deltaTime * 15f);
    }
}
