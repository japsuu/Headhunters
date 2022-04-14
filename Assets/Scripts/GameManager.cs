using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Enumerable = System.Linq.Enumerable;
using Random = UnityEngine.Random;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static List<Headhunter> AliveHeadhunters;
    public static List<Player> AlivePlayers;

    private static uint HeadhunterCount => (NetworkServer.connections.Count / 4 == 0) ? 1 : (uint)NetworkServer.connections.Count / 4;
    
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        /*
        if (AliveHeadhunters.Count <= 0)
        {
            Debug.LogWarning("Players win");
        }
        
        if (AlivePlayers.Count <= 0)
        {
            Debug.LogWarning("Headhunters win");
        }*/
    }

    private void Initialize()
    {
        AliveHeadhunters = new List<Headhunter>((int)HeadhunterCount);
        AlivePlayers = new List<Player>();
    }

    /// <summary>
    /// Picks the specified amount of headhunters form the connected players.
    /// </summary>
    public static void SelectRandomHeadhunters()
    {
        List<int> headhunters = new List<int>();

        while (headhunters.Count < HeadhunterCount)
        {
            int rnd = Random.Range(0, NetworkServer.connections.Count);

            NetworkConnectionToClient conn = Enumerable.ElementAt(NetworkServer.connections, rnd).Value;

            int id = conn.connectionId;
            
            if(headhunters.Contains(id)) continue;
            
            headhunters.Add(id);
            PlayerData data = (PlayerData) conn.authenticationData;
            data.IsHeadhunter = true;
            conn.authenticationData = data;
        }
    }

    /// <summary>
    /// Assigns a player GameObject to be a headhunter.
    /// </summary>
    /// <param name="player"></param>
    public static void SetHeadhunterObject(GameObject player)
    {
        AliveHeadhunters.Add(player.GetComponent<Headhunter>());
        
        //TODO: Show headhunters they are headhunters (RPC)
    }

    public static void OnHeadhunterKilled(Headhunter headhunter)
    {
        AliveHeadhunters.Remove(headhunter);
    }

    public static void OnPlayerKilled(Player player)
    {
        AlivePlayers.Remove(player);
    }
}
