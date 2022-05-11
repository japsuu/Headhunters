using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class VitalsSystem : NetworkBehaviour
{
    [SyncVar]
    public float sync_maxHealth;
    [SyncVar]
    public float sync_currentHealth;
    
    [SyncVar]
    public float sync_maxSaturation;
    [SyncVar]
    public float sync_currentSaturation;
    
    [SyncVar]
    public float sync_maxHydration;
    [SyncVar]
    public float sync_currentHydration;
    
    public PlayerDamageSource LatestDamageSource { get; private set; }
    public PlayerHealSource LatestHealSource { get; private set; }
    
    private Player player;

    #region LIFECYCLE FUNCTIONS

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public override void OnStartServer()
    {
        Server_InitializeVitals();
    }
    
    private void FixedUpdate()
    {
        // Update the vitals data on the server.
        if (isServer)
        {
            Server_UpdateVitals();
        }
    }

    #endregion


    #region PUBLIC FUNCTIONS

    [Server]
    public void Server_Damage(PlayerDamageSource source, float amount)
    {
        if(amount == 0) return;
        
        // Only accept positive values
        if (amount < 0) amount = Mathf.Abs(amount);

        LatestDamageSource = source;

        sync_currentHealth -= Mathf.Max(0, amount);
        
        Target_OnDamaged(source, amount);
    }
    
    [Server]
    public void Server_Heal(PlayerHealSource source, float amount)
    {
        // Only accept positive values
        if (amount < 0) amount = Mathf.Abs(amount);

        LatestHealSource = source;
        
        sync_currentHealth += Mathf.Min(sync_maxHealth, amount);
        
        Target_OnHealed(source, amount);
    }
    
    [Server]
    public void Server_Eat(float amount)
    {
        sync_currentSaturation = Mathf.Max(0, Mathf.Min(sync_maxSaturation, sync_currentSaturation + amount));
    }
    
    [Server]
    public void Server_Drink(float amount)
    {
        sync_currentHydration = Mathf.Max(0, Mathf.Min(sync_maxHydration, sync_currentHydration + amount));
    }

    #endregion


    #region PRIVATE FUNCTIONS

    [TargetRpc]
    private void Target_OnHealed(PlayerHealSource source, float amount)
    {
        LatestHealSource = source;
    }

    [TargetRpc]
    private void Target_OnDamaged(PlayerDamageSource source, float amount)
    {
        LatestDamageSource = source;
        
        EventManager.ClientEvents.OnLocalPlayerDamaged.Invoke(source, amount);
    }
    
    [Server]
    private void Server_InitializeVitals()
    {
        sync_maxHealth = player.IsHeadhunter ? Constants.HeadhunterMaxHealth : Constants.SurvivorMaxHealth;
        sync_currentHealth = sync_maxHealth;
        
        sync_maxHydration = player.IsHeadhunter ? Constants.HeadhunterMAXHydration : Constants.SurvivorMAXHydration;
        sync_currentHydration = sync_maxHydration;
        
        sync_maxSaturation = player.IsHeadhunter ? Constants.HeadhunterMAXSaturation : Constants.SurvivorMAXSaturation;
        sync_currentSaturation = sync_maxSaturation;
    }
    
    [Server]
    private void Server_UpdateVitals()
    {
        // Decrease saturation and hydration
        float saturationCost = Server_GetThisFrameSaturationCost();
        float hydrationCost = Server_GetThisFrameHydrationCost();

        sync_currentSaturation = Mathf.Max(0, sync_currentSaturation - saturationCost);
        sync_currentHydration = Mathf.Max(0, sync_currentHydration - hydrationCost);

        // Dying of hunger
        if (sync_currentSaturation <= 0)
        {
            Server_Damage(PlayerDamageSource.Hunger, Constants.HungryHealthDepletion * Time.fixedDeltaTime);
        }

        // Dying of thirst
        if (sync_currentHydration <= 0)
        {
            Server_Damage(PlayerDamageSource.Thirst, Constants.ThirstyHealthDepletion * Time.fixedDeltaTime);
        }

        // Healing naturally
        if (sync_currentSaturation > Constants.SaturationRequiredToHeal &&
            sync_currentHydration > Constants.HydrationRequiredToHeal &&
            sync_currentHealth < sync_maxHealth)
        {
            Server_Heal(PlayerHealSource.Regeneration, Constants.HealRate * Time.fixedDeltaTime);
        }

        // If health is below 0, we kill the player. No need for any checks since this is all called on the server.
        if (sync_currentHealth <= 0)
        {
            player.Server_Kill();
        }
    }
    
    [Server]
    private float Server_GetThisFrameSaturationCost()
    {
        return player.CurrentlyInHeadhunterState ?
            (player.IsMoving ?
                Constants.HeadhunterSaturationRunningDepletion :
                Constants.HeadhunterSaturationBaseDepletion) * Time.fixedDeltaTime
            : (player.IsMoving ?
                Constants.SurvivorSaturationRunningDepletion :
                Constants.SurvivorSaturationBaseDepletion) * Time.fixedDeltaTime;
    }

    [Server]
    private float Server_GetThisFrameHydrationCost()
    {
        return player.CurrentlyInHeadhunterState ?
            (player.IsMoving ?
                Constants.HeadhunterHydrationRunningDepletion :
                Constants.HeadhunterHydrationBaseDepletion) * Time.fixedDeltaTime :
            (player.IsMoving ?
                Constants.SurvivorHydrationRunningDepletion :
                Constants.SurvivorHydrationBaseDepletion) * Time.fixedDeltaTime;
    }

    #endregion
}
