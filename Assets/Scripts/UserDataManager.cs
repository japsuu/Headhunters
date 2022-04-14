using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public static class UserDataManager
{
    public static void SaveUsername(string username)
    {
        PlayerPrefs.SetString("username", username);
        
        PlayerPrefs.Save();

        ((HeadhunterRoomManager)NetworkManager.singleton).LocalUsername = username;
    }

    public static string LoadUsername()
    {
        ((HeadhunterRoomManager)NetworkManager.singleton).LocalUsername = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(((HeadhunterRoomManager)NetworkManager.singleton).LocalUsername))
        {
            ((HeadhunterRoomManager)NetworkManager.singleton).LocalUsername = "Guest " + Random.Range(1111, 9999);
        }
        
        return ((HeadhunterRoomManager)NetworkManager.singleton).LocalUsername;
    }
}
