
using System;
using CMF;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour, IInteractable
{
    /*
     
    [Command]
    public void Command_ClientToServer()
    {
        // Executed on the server
        
        Rpc_ServerToClient();
    }
    
    [ClientRpc]
    private void Rpc_ServerToClient()
    {
        // Executed on all clients
    }
    
    */
    
    /// <summary>
    /// The local player instance.
    /// </summary>
    public static Player LocalPlayer;
    
    [AssetsOnly]
    [Tooltip("Spawned when a player is killed")]
    public GameObject s_corpsePrefab;

    /// <summary>
    /// Child Transform of this player to spawn the corpse to.
    ///
    /// Only used on server.
    /// </summary>
    public Transform s_corpseSpawnpoint;

    /// <summary>
    /// Time it takes to respawn after dying.
    ///
    /// Only used on server.
    /// </summary>
    [SerializeField]
    private float s_afterDeathRespawnTime = 15;
    
    /// <summary>
    /// Damage inflicted from player to headhunter on hit.
    ///
    /// Only used on server.
    /// </summary>
    [SerializeField]
    private float s_playerAttackDamage = 20f;
    
    /// <summary>
    /// Damage inflicted from headhunter to player on hit.
    ///
    /// Only used on server.
    /// </summary>
    [SerializeField]
    private float s_headhunterAttackDamage = 40f;
    
    public bool InputEnabled { get; private set; }

    /// <summary>
    /// Whether this player is a headhunter.
    ///
    /// SyncVar.
    /// </summary>
    [SyncVar]
    public bool sync_isHeadhunter;
    
    /// <summary>
    /// Username of the player.
    ///
    /// SyncVar.
    /// </summary>
    [SyncVar]
    public string sync_username;

    /// <summary>
    /// Current Headhunter state of the player, only set if the player is a headhunter.
    ///
    /// SyncVar.
    /// </summary>
    [SyncVar]
    public Headhunter.HeadhunterState sync_currentHeadhunterState = Headhunter.HeadhunterState.Survivor; //WARN: Use a hook to change the state of the player?

    /// <summary>
    /// True, if the player is both a headhunter, and currently in the headhunter state.
    ///
    /// Network safe.
    /// </summary>
    private bool CurrentlyInHeadhunterState => sync_isHeadhunter && sync_currentHeadhunterState == Headhunter.HeadhunterState.Headhunter;

    private bool IsMoving => rb.velocity.magnitude > 2;
    
    public enum DamageSource
    {
        Headhunter,
        Player,
        Hunger,
        Thirst,
        Server,
        Unknown
    }
    
    public enum HealSource
    {
        Regeneration,
        Server,
        Unknown
    }
    
    private CharacterKeyboardInput movementInput;
    private Rigidbody rb;
    private CameraMouseInput lookInput;

    public Camera InteractionCam { get; private set; }
    private DamageSource latestDamageSource;
    private HealSource latestHealSource;

    private readonly string[] headhunterInteractStrings =
    {
        "Slash",
        "Tear",
        "Bite",
        "Shred",
        "Taste"
    };
    
    // Survivor
    private const float SurvivorMaxHealth = 100;
    private const float SurvivorMAXHydration = 100;
    private const float SurvivorMAXSaturation = 100;
    private const float SurvivorHydrationBaseDepletion = 0.3f;
    private const float SurvivorSaturationBaseDepletion = 0.25f;
    private const float SurvivorHydrationRunningDepletion = 0.45f;
    private const float SurvivorSaturationRunningDepletion = 0.3f;
    
    // Headhunter
    private const float HeadhunterMaxHealth = 60;
    private const float HeadhunterMAXHydration = 100;
    private const float HeadhunterMAXSaturation = 120;
    private const float HeadhunterHydrationBaseDepletion = 0.4f;
    private const float HeadhunterSaturationBaseDepletion = 0.3f;
    private const float HeadhunterHydrationRunningDepletion = 0.7f;
    private const float HeadhunterSaturationRunningDepletion = 0.5f;
    
    // Shared
    private const float HungryHealthDepletion = 3f;
    private const float ThirstyHealthDepletion = 2f;
    private const float HydrationRequiredToHeal = 25f;
    private const float SaturationRequiredToHeal = 35f;
    private const float HealRate = 0.5f;
    
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
    
    /*private void UpdateMaxHealth(float oldVal, float newVal)
    {
        if(!isLocalPlayer)
        {
            Debug.Log("Return: not local player");
            return;
        }
        
        IngameUIManager.Singleton.UpdateMaxHealth(newVal);
    }

    private void UpdateMaxHydration(float oldVal, float newVal)
    {
        if(!isLocalPlayer) return;

        IngameUIManager.Singleton.UpdateMaxHydration(newVal);
    }

    private void UpdateMaxSaturation(float oldVal, float newVal)
    {
        if(!isLocalPlayer) return;

        IngameUIManager.Singleton.UpdateMaxSaturation(newVal);
    }*/

    private void Awake()
    {
        InputEnabled = true;
        
        movementInput = GetComponent<CharacterKeyboardInput>();
        rb = GetComponentInChildren<Rigidbody>();
        lookInput = GetComponentInChildren<CameraMouseInput>();
        InteractionCam = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        // Set the layer to "Interactable" if remote player.
        if(isLocalPlayer) return;

        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public override void OnStartLocalPlayer()
    {
        // Set the local player instance
        if(LocalPlayer != null)
            Debug.LogError("Multiple local players set?");
        
        LocalPlayer = this;
        Debug.Log("Local player set");
        
        // Notify the player of their role with an UI popup
        IngameUIManager.Singleton.OnPlayerSpawn();
    }

    public override void OnStartServer()
    {
        // Load the data of the owner client and assign it to the SyncVars
        PlayerData data = (PlayerData) connectionToClient.authenticationData;
        
        sync_isHeadhunter = data.IsHeadhunter;
        sync_username = data.Name;
        
        // Initialize the vitals data
        Server_InitializeVitals();
        
        Debug.Log("Init userData for " + data.Name);
    }

    /// <summary>
    /// Initializes the vitals related syncVars.
    /// </summary>
    private void Server_InitializeVitals()
    {
        sync_maxHealth = sync_isHeadhunter ? HeadhunterMaxHealth : SurvivorMaxHealth;
        sync_currentHealth = sync_maxHealth;
        
        sync_maxHydration = sync_isHeadhunter ? HeadhunterMAXHydration : SurvivorMAXHydration;
        sync_currentHydration = sync_maxHydration;
        
        sync_maxSaturation = sync_isHeadhunter ? HeadhunterMAXSaturation : SurvivorMAXSaturation;
        sync_currentSaturation = sync_maxSaturation;
    }

    private void Update()
    {
        // Get input on the owner client
        if (isLocalPlayer)
        {
            if(Input.GetKeyDown(KeyCode.H))
                Command_RequestHunger();
            
            if(Input.GetKeyDown(KeyCode.T))
                Command_RequestThirst();
        }
    }

    private void FixedUpdate()
    {
        // Update the vitals data on the server.
        if (isServer)
        {
            Server_UpdateVitals();
        }
    }

    /// <summary>
    /// Resets the calling client's hunger to 0.
    /// </summary>
    [Command]
    private void Command_RequestHunger()
    {
        sync_currentSaturation = 0;
    }

    /// <summary>
    /// Resets the calling client's thirst to 0.
    /// </summary>
    [Command]
    private void Command_RequestThirst()
    {
        sync_currentHydration = 0;
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
            Server_Damage(DamageSource.Hunger, HungryHealthDepletion * Time.fixedDeltaTime);
        }

        // Dying of thirst
        if (sync_currentHydration <= 0)
        {
            Server_Damage(DamageSource.Thirst, ThirstyHealthDepletion * Time.fixedDeltaTime);
        }

        // Healing naturally
        if (
            sync_currentSaturation > SaturationRequiredToHeal &&
            sync_currentHydration > HydrationRequiredToHeal &&
            sync_currentHealth < sync_maxHealth)
        {
            Server_Heal(HealSource.Regeneration, HealRate * Time.fixedDeltaTime);
        }

        // If health is below 0, we kill the player. No need for any checks since this is all called on the server.
        if (sync_currentHealth <= 0)
        {
            Server_Kill();
        }
    }
    
    private float Server_GetThisFrameSaturationCost()
    {
        return CurrentlyInHeadhunterState ?
            (IsMoving ?
                HeadhunterSaturationRunningDepletion :
                HeadhunterSaturationBaseDepletion) * Time.fixedDeltaTime
            : (IsMoving ?
                SurvivorSaturationRunningDepletion :
                SurvivorSaturationBaseDepletion) * Time.fixedDeltaTime;
    }

    private float Server_GetThisFrameHydrationCost()
    {
        return CurrentlyInHeadhunterState ?
            (IsMoving ?
                HeadhunterHydrationRunningDepletion :
                HeadhunterHydrationBaseDepletion) * Time.fixedDeltaTime :
            (IsMoving ?
                SurvivorHydrationRunningDepletion :
                SurvivorHydrationBaseDepletion) * Time.fixedDeltaTime;
    }

    public void DisableInput()
    {
        InputEnabled = false;
        movementInput.isInputEnabled = false;
        lookInput.isInputEnabled = false;
        InteractionManager.Singleton.interactionEnabled = false;
    }

    public void EnableInput()
    {
        InputEnabled = true;
        movementInput.isInputEnabled = true;
        lookInput.isInputEnabled = true;
        InteractionManager.Singleton.interactionEnabled = true;
    }

    public string GetInteractText()
    {
        return CanBeInteractedWith() ?
            headhunterInteractStrings[Random.Range(0, headhunterInteractStrings.Length)] : "";
    }
    public void Interact()
    {
        Command_Damage(DamageSource.Headhunter);
    }

    public bool CanBeInteractedWith()   //BUG: Interaction not working
    {
        return LocalPlayer.sync_isHeadhunter &&
               !sync_isHeadhunter &&
               LocalPlayer.sync_currentHeadhunterState == Headhunter.HeadhunterState.Headhunter;
    }

    [Server]
    private void Server_Damage(DamageSource source, float amount)
    {
        // Ran on the server.

        latestDamageSource = source;

        sync_currentHealth -= Mathf.Max(0, amount);
        
        Target_OnDamaged(source, amount);
    }

    /// <summary>
    /// Called by other clients with a damageSource.
    /// 
    /// Server checks if the supplied source is valid.
    /// </summary>
    /// <param name="source">Where the damage originated from.</param>
    /// <param name="sender">Client which caused damage to this player.</param>
    /// <exception cref="ArgumentOutOfRangeException">If damageSource is not valid</exception>
    [Client]
    [Command(requiresAuthority = false)]
    public void Command_Damage(DamageSource source, NetworkConnectionToClient sender = null)
    {
        float amount = 0;

        if (sender == null)
        {
            Debug.LogWarning("Discarded Damage() command because of an invalid sender.");
        }
        
        // Ran on the server.
        switch (source)
        {
            case DamageSource.Headhunter:
                amount = s_headhunterAttackDamage;
                break;
            case DamageSource.Player:
                amount = s_playerAttackDamage;
                break;
            case DamageSource.Hunger:
            case DamageSource.Thirst:
            case DamageSource.Server:
            case DamageSource.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }

        latestDamageSource = source;

        sync_currentHealth -= Mathf.Max(0, amount);
        
        Target_OnDamaged(source, amount);
    }

    [TargetRpc]
    private void Target_OnDamaged(DamageSource source, float amount)
    {
        latestDamageSource = source;
        
        Debug.Log($"Player damaged {amount} HP by {latestDamageSource}");
    }

    /// <summary>
    /// Called by the server internally, to heal a player the given amount.
    /// </summary>
    /// <param name="source">Origin of the heal action</param>
    /// <param name="amount">Amount to heal.</param>
    [Server]
    private void Server_Heal(HealSource source, float amount)
    {
        // Called by the server, ran on the server.

        latestHealSource = source;
        
        sync_currentHealth += Mathf.Min(sync_maxHealth, amount);
        
        Target_OnHealed(source, amount);
    }
    
    /// <summary>
    /// Called by the owner player with a healSource.
    /// </summary>
    /// <param name="source">Origin of the heal action.</param>
    /// <exception cref="ArgumentOutOfRangeException">If healSource is invalid.</exception>
    [Client]
    [Command]
    public void Command_Heal(HealSource source)
    {
        // Called by owner client, ran on the server.

        float amount = 0;
        
        switch (source)
        {
            case HealSource.Regeneration:
                break;
            case HealSource.Server:
                break;
            case HealSource.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }

        latestHealSource = source;
        
        sync_currentHealth += Mathf.Min(sync_maxHealth, amount);
        
        Target_OnHealed(source, amount);
    }

    [TargetRpc]
    private void Target_OnHealed(HealSource source, float amount)
    {
        // Called by the server, executed on the owner client.
        
        latestHealSource = source;
        
        if(source != HealSource.Regeneration)
            Debug.Log($"Player healed {amount} HP from {latestHealSource}");
    }

    [Server]
    private void Server_Kill()
    {
        // Called by the server, ran on the server.
        
        Debug.LogWarning("Player has died of " + latestDamageSource);
        
        ((HeadhunterRoomManager) NetworkManager.singleton).RespawnPlayerAsHeadhunter(connectionToClient, this, s_afterDeathRespawnTime);
    }

    public override void OnStopClient()
    {
        // Executed only on the owner client
        
        if (!isLocalPlayer) return;
        
        if(IngameUIManager.Singleton == null) return;
        
        IngameUIManager.Singleton.NotifyPlayerOfDeath(latestDamageSource, s_afterDeathRespawnTime);
    }

    private void OnDrawGizmos()
    {
        if (LocalPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(LocalPlayer.transform.position, InteractionManager.Singleton.interactDistance);
        }
    }
}
