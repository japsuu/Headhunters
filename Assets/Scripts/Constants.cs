public static class Constants
{
    #region ARRAYS

    public static readonly string[] HeadhunterInteractStrings =
    {
        "Slash",
        "Tear",
        "Bite",
        "Shred",
        "Taste"
    };

    #endregion

    
    #region VITALS

    // Survivor
    public const float SurvivorMaxHealth = 100;
    public const float SurvivorMAXHydration = 100;
    public const float SurvivorMAXSaturation = 100;
    public const float SurvivorHydrationBaseDepletion = 0.3f;
    public const float SurvivorSaturationBaseDepletion = 0.25f;
    public const float SurvivorHydrationRunningDepletion = 0.45f;
    public const float SurvivorSaturationRunningDepletion = 0.3f;
    
    // Headhunter
    public const float HeadhunterMaxHealth = 60;
    public const float HeadhunterMAXHydration = 100;
    public const float HeadhunterMAXSaturation = 120;
    public const float HeadhunterHydrationBaseDepletion = 0.4f;
    public const float HeadhunterSaturationBaseDepletion = 0.3f;
    public const float HeadhunterHydrationRunningDepletion = 0.7f;
    public const float HeadhunterSaturationRunningDepletion = 0.5f;
    
    // Shared
    public const float HungryHealthDepletion = 3f;
    public const float ThirstyHealthDepletion = 2f;
    public const float HydrationRequiredToHeal = 25f;
    public const float SaturationRequiredToHeal = 35f;
    public const float HealRate = 0.5f;

    #endregion


    #region DAMAGE AND ATTACKING
    
    public const float HeadhunterAttackDamage = 40f;

    #endregion
    

    #region INTERACTIONS

    public const float MaxInteractDistance = 3;

    #endregion

    
    #region ANTICHEAT

    public const float InteractionDistanceMaxDeviation = 2f;

    #endregion


    #region MISC
    
    public const float PlayerRespawnTime = 15;

    #endregion
}