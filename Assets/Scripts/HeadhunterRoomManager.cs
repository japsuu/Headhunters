using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class HeadhunterRoomManager : NetworkRoomManager
{
    /// <summary>
    /// Struct that contains the initial player data, sent from client to server upon connection.
    /// </summary>
    private readonly struct PlayerJoinMessage : NetworkMessage
    {
        public readonly string Username;

        public PlayerJoinMessage(string username)
        {
            Username = username;
        }
    }
    
    [Header("Custom settings")]
    
    [AssetsOnly]
    [SerializeField]
    private GameObject survivorPrefab;
    
    [AssetsOnly]
    [SerializeField]
    private GameObject headhunterPrefab;

    [ReadOnly]
    public string LocalUsername;
    
    public static List<Headhunter> AliveHeadhunters;
    public static List<Player> AliveSurvivors;

    private static uint HeadhunterCount => (NetworkServer.connections.Count / 4 == 0) ? 1 : (uint)NetworkServer.connections.Count / 4;

    private bool gameStarted;

    public override void OnRoomStartServer()
    {
        base.OnRoomStartServer();
        
        NetworkServer.RegisterHandler<PlayerJoinMessage>(OnPlayerJoinMessageReceived);
        
        AliveHeadhunters = new List<Headhunter>((int)HeadhunterCount);
        AliveSurvivors = new List<Player>();
    }

    /*private void Update()
    {
        if(!gameStarted) return;
        
        if(!NetworkServer.active) return;
        
        if (AliveHeadhunters.Count <= 0)
        {
            Debug.LogWarning("Survivors win");
        }
        
        if (AliveSurvivors.Count <= 0)
        {
            Debug.LogWarning("Headhunters win");
        }
    }*/

    public override void OnRoomClientConnect()
    {
        base.OnRoomClientConnect();
        
        NetworkClient.Send(new PlayerJoinMessage(LocalUsername));
    }

    private void OnPlayerJoinMessageReceived(NetworkConnection conn, PlayerJoinMessage msg)
    {
        //Debug.Log("Received join message for connection " + conn.connectionId);
        
        conn.authenticationData = new PlayerData(msg.Username, false);
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName == GameplayScene)
        {
            // Select the random headhunters.
            SelectRandomHeadhunters();

            gameStarted = true;
        }
        else
        {
            gameStarted = false;
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        PlayerData data = (PlayerData) conn.authenticationData;
        
        // Check if the player is a headhunter
        if (data.IsHeadhunter)
        {
            AliveHeadhunters.Add(gamePlayer.GetComponent<Headhunter>());
        }
        else
        {
            AliveSurvivors.Add(gamePlayer.GetComponent<Player>());
        }

        return true;
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        PlayerData data = (PlayerData) conn.authenticationData;

        // get start position from base class
        Transform startPos = GetStartPosition();

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (startPos != null)
        {
            pos = startPos.position;
            rot = startPos.rotation;
        }

        GameObject gamePlayer = data.IsHeadhunter
            ? Instantiate(headhunterPrefab, pos, rot)
            : Instantiate(survivorPrefab, pos, rot);

        gamePlayer.name = data.IsHeadhunter
            ? $"Headhunter [ConnID= {conn.connectionId}]"
            : $"Survivor [ConnID= {conn.connectionId}]";

        return gamePlayer;
    }

    public void RespawnPlayerAsHeadhunter(NetworkConnectionToClient conn, Player player, float delay)
    {
        StartCoroutine(I_RespawnPlayerAsHeadhunter(conn, player, delay));
    }

    private IEnumerator I_RespawnPlayerAsHeadhunter(NetworkConnectionToClient conn, Player player, float delay)
    {
        OnPlayerKilled(player);
        
        // Destroy the old player obj
        NetworkServer.Destroy(player.gameObject);

        // Spawn the corpse
        GameObject corpse = Instantiate(player.s_corpsePrefab, player.s_corpseSpawnpoint.position, player.s_corpseSpawnpoint.rotation);
        NetworkServer.Spawn(corpse);
        
        yield return new WaitForSecondsRealtime(delay);
        
        // get start position from base class
        Transform startPos = GetStartPosition();

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (startPos != null)
        {
            pos = startPos.position;
            rot = startPos.rotation;
        }

        // Create the new player object
        GameObject gamePlayer = Instantiate(headhunterPrefab, pos, rot);

        gamePlayer.name = $"Headhunter [ConnID= {conn.connectionId}]";
        
        // Update the data so that the player is now a headhunter
        PlayerData data = (PlayerData) conn.authenticationData;
        data.IsHeadhunter = true;
        conn.authenticationData = data;
        
        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, true);
    }

    /// <summary>
    /// Picks the specified amount of headhunters form the connected players.
    /// </summary>
    private static void SelectRandomHeadhunters()
    {
        List<int> headhunters = new List<int>();

        while (headhunters.Count < HeadhunterCount)
        {
            int rnd = Random.Range(0, NetworkServer.connections.Count);

            NetworkConnectionToClient conn = NetworkServer.connections.ElementAt(rnd).Value;

            int id = conn.connectionId;
            
            if(headhunters.Contains(id)) continue;
            
            headhunters.Add(id);
            PlayerData data = (PlayerData) conn.authenticationData;
            data.IsHeadhunter = true;
            conn.authenticationData = data;
        }
    }

    [Server]
    public static void OnPlayerKilled(Player player)
    {
        if (player.sync_isHeadhunter)
        {
            AliveHeadhunters.Remove(player.GetComponent<Headhunter>());
        }
        else
        {
            AliveSurvivors.Remove(player);
        }
    }
}