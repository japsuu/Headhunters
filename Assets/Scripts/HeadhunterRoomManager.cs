using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

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

    public override void OnRoomStartServer()
    {
        base.OnRoomStartServer();
        
        NetworkServer.RegisterHandler<PlayerJoinMessage>(OnPlayerJoinMessageReceived);
    }

    public override void OnRoomClientConnect()
    {
        base.OnRoomClientConnect();
        
        NetworkClient.Send(new PlayerJoinMessage(LocalUsername));
    }

    private void OnPlayerJoinMessageReceived(NetworkConnection conn, PlayerJoinMessage msg)
    {
        Debug.Log("Received join message for connection " + conn.connectionId);
        
        conn.authenticationData = new PlayerData(msg.Username, false);
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName == GameplayScene)
        {
            // Select the random headhunters.
            GameManager.SelectRandomHeadhunters();
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        PlayerData data = (PlayerData) conn.authenticationData;
        
        // Check if the player is a headhunter
        if (data.IsHeadhunter)
        {
            GameManager.SetHeadhunterObject(gamePlayer);
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
}