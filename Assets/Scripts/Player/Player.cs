using CMF;
using Helpers;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour, IInteractable
{
    /// <summary>
    /// The local player instance.
    /// </summary>
    public static Player LocalPlayer;
    
    
    #region SERIALIZED INSTANCE FIELDS
    
    [SerializeField]
    private GameObject s_corpsePrefab;

    [SerializeField]
    private Camera interactionCam;
    
    [SerializeField]
    private Transform s_corpseSpawnpoint;

    [SerializeField]
    private Collider mainCollider;

    #endregion

    
    #region PUBLIC INSTANCE FIELDS

    public VitalsSystem Vitals { get; private set; }
    public NetworkInventory Inventory { get; private set; }
    public Camera InteractionCam => interactionCam;
    public Transform CorpseSpawnpoint => s_corpseSpawnpoint;
    public GameObject CorpsePrefab => s_corpsePrefab;
    
    public bool IsMoving => rb.velocity.magnitude > 2;
    public bool InputEnabled { get; private set; }
    /// <summary>
    /// True, if the player is both a headhunter, and currently in the headhunter state.
    /// </summary>
    public bool CurrentlyInHeadhunterState => IsHeadhunter && CurrentHeadhunterState == Headhunter.HeadhunterState.Headhunter;
    
    #endregion

    
    #region PRIVATE INSTANCE FIELDS
    
    private InputController movementInput;
    private Rigidbody rb;
    private CameraMouseInput lookInput;

    #endregion

    
    #region SYNCVARS
    
    [field: SyncVar]
    public Headhunter.HeadhunterState CurrentHeadhunterState { get; private set; } = Headhunter.HeadhunterState.Survivor;

    [field: SyncVar]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Username { get; private set; }

    [field: SyncVar]
    public bool IsHeadhunter { get; private set; }

    #endregion

    
    #region STATIC FUNCTIONS

    [Server]
    public static Player Server_GetPlayerOfConnection(NetworkConnection target)
    {
        if (target == null)
        {
            return null;
        }
        
        Player player = target.identity.GetComponent<Player>();
        
        if (player != null) return player;
        
        Debug.LogWarning("Could not find Player object for connection " + target.connectionId);
        return null;
    }

    #endregion

    
    #region UNITY CALLBACKS

    private void Awake()
    {
        InputEnabled = true;
        movementInput = GetComponent<InputController>();
        rb = GetComponentInChildren<Rigidbody>();
        lookInput = GetComponentInChildren<CameraMouseInput>();
        Inventory = GetComponent<NetworkInventory>();
        Vitals = GetComponent<VitalsSystem>();
    }
    
    private void Start()
    {
        // Set the layer to Interactable if remote player.
        if(isLocalPlayer) return;

        gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
    }

    private void OnDrawGizmos()
    {
        if (LocalPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(LocalPlayer.transform.position, Constants.MaxInteractDistance);
        }
    }

    #endregion

    
    #region NETWORK CALLBACKS

    public override void OnStartLocalPlayer()
    {
        if(LocalPlayer != null)
            Debug.LogError("Multiple local players set?");
        LocalPlayer = this;

        CursorHelper.SetCursorLockState(true);
        
        EventManager.ClientEvents.OnLocalPlayerSpawned.Invoke(IsHeadhunter);
    }

    public override void OnStartServer()
    {
        // Load the data of the owner client and assign it to the SyncVars
        Headhunters.Networking.PlayerData data = (Headhunters.Networking.PlayerData) connectionToClient.authenticationData;
        IsHeadhunter = data.IsHeadhunter;
        Username = data.Name;
    }
    
    public override void OnStopClient()
    {
        if (!isLocalPlayer) return;
        
        CursorHelper.SetCursorLockState(true);
        
        EventManager.ClientEvents.OnLocalPlayerDied.Invoke(Vitals.LatestDamageSource);
    }

    #endregion

    
    #region PRIVATE SERVER FUNCTIONS
    
    
    
    #endregion


    #region PRIVATE CLIENT FUNCTIONS
    
    [Client]
    [Command(requiresAuthority = false)]
    private void Command_HeadhunterAttack(NetworkConnectionToClient sender = null)
    {
        if (sender == null)
        {
            Debug.LogWarning("Discarded Damage() command because of an invalid sender.");
            return;
        }

        Player damagingPlayer = Server_GetPlayerOfConnection(sender);

        if (damagingPlayer == null)
        {
            Debug.LogWarning($"Connection {sender.connectionId} tried to damage while dead.");
            return;
        }

        if (damagingPlayer.CurrentHeadhunterState != Headhunter.HeadhunterState.Headhunter)
        {
            Debug.LogWarning($"Connection {sender.connectionId} tried to damage a player while not being a headhunter.");
            return;
        }

        Vector3 cameraPos = damagingPlayer.InteractionCam.transform.position;
        if (Vector3.Distance(cameraPos, mainCollider.ClosestPoint(cameraPos)) >
            Constants.MaxInteractDistance + Constants.InteractionDistanceMaxDeviation)
        {
            Debug.LogWarning($"Connection {sender.connectionId} tried to damage a player from too far.");
            return;
        }

        Vitals.Server_Damage(PlayerDamageSource.Headhunter, Constants.HeadhunterAttackDamage);
    }

    #endregion
    
    
    #region PUBLIC CLIENT FUNTIONS
    
    [Client]
    public void DisableInput()
    {
        InputEnabled = false;
        movementInput.isInputEnabled = false;
        lookInput.isInputEnabled = false;
        InteractionManager.Singleton.SetInteractionEnabled(false);
    }

    [Client]
    public void EnableInput()
    {
        InputEnabled = true;
        movementInput.isInputEnabled = true;
        lookInput.isInputEnabled = true;
        InteractionManager.Singleton.SetInteractionEnabled(true);
    }
    
    #endregion


    #region PUBLIC SERVER FUNCTIONS

    [Server]
    public void Server_Kill()
    {
        ((HeadhunterRoomManager) NetworkManager.singleton).RespawnPlayerAsHeadhunter(connectionToClient, this);
    }

    [Server]
    public void Server_SetHeadhunterState(Headhunter.HeadhunterState newState)
    {
        CurrentHeadhunterState = newState;
    }

    #endregion

    
    #region IINTERACTABLE IMPLEMENTATION

    public string GetInteractText()
    {
        return CanBeInteractedWith() ?
            Constants.HeadhunterInteractStrings[Random.Range(0, Constants.HeadhunterInteractStrings.Length)] : "";
    }
    public void Interact()
    {
        Command_HeadhunterAttack();
    }

    public bool CanBeInteractedWith()
    {
        return !IsHeadhunter && LocalPlayer.CurrentlyInHeadhunterState;
    }

    #endregion
}