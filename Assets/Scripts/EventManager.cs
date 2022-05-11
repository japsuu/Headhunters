using System;
using UnityEngine.Events;

public static class EventManager
{
    public static class ClientEvents
    {
        /// <summary>
        /// Bool = isHeadhunter.
        /// </summary>
        public static readonly UnityEvent<bool> OnLocalPlayerSpawned = new();
    
        /// <summary>
        /// PlayerDamageSource = damageSource, float = damageAmount.
        /// </summary>
        public static readonly UnityEvent<PlayerDamageSource, float> OnLocalPlayerDamaged = new();
    
        /// <summary>
        /// PlayerDamageSource = deathCause.
        /// </summary>
        public static readonly UnityEvent<PlayerDamageSource> OnLocalPlayerDied = new();
    }
    
    public static class ServerEvents
    {
        
    }
}