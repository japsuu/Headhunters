using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameUIManager : SingletonBehaviour<IngameUIManager>
{
    [Header("References")]
    [SerializeField]
    private TMP_Text roleDisplayText;
    
    [SerializeField]
    private TMP_Text interactText;
    
    [SerializeField]
    private TMP_Text killedText;
    
    [SerializeField]
    private Slider modeSwitchTimeLeftSlider;

    [SerializeField]
    private GameObject statsPanelRoot;

    [SerializeField]
    private Slider healthSlider;

    [SerializeField]
    private Slider hydrationSlider;

    [SerializeField]
    private Slider saturationSlider;

    [Header("Survivor settings")]
    [SerializeField]
    private Color survivorColor = Color.blue;

    [SerializeField]
    private string survivorRoleString = "Your role is <b>survivor</b>.";

    [Header("Headhunter settings")]
    [SerializeField]
    private Color headhunterColor = Color.red;

    [SerializeField]
    private string headhunterRoleString = "Your role is <b>headhunter</b>.";

    /// <summary>
    /// How long does the role text fade in and out.
    /// </summary>
    [Header("Shared settings")]
    [SerializeField]
    private float roleDisplayFadeTime = 2;
    
    /// <summary>
    /// For how long is the player's role shown on screen.
    /// </summary>
    [SerializeField]
    private float roleDisplayShowTime = 2;
    
    /// <summary>
    /// For how long we wait before showing the player's role on screen.
    /// </summary>
    [SerializeField]
    private float roleDisplayDelayTime = 2;

    [SerializeField]
    private float interactionTextWorldSpaceSmoothness = 0.2f;
    
    private void Awake()
    {
        roleDisplayText.alpha = 0;
        killedText.alpha = 0;
        interactText.text = "";

        statsPanelRoot.SetActive(false);
        modeSwitchTimeLeftSlider.gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        EventManager.ClientEvents.OnLocalPlayerSpawned.AddListener(OnLocalPlayerSpawned);
        EventManager.ClientEvents.OnLocalPlayerDied.AddListener(NotifyPlayerOfDeath);
    }

    private void OnDisable()
    {
        EventManager.ClientEvents.OnLocalPlayerSpawned.RemoveListener(OnLocalPlayerSpawned);
        EventManager.ClientEvents.OnLocalPlayerDied.RemoveListener(NotifyPlayerOfDeath);
    }

    private void Update()
    {
        if(Player.LocalPlayer == null) return;
        
        // Update the UI sliders
        healthSlider.value = Player.LocalPlayer.Vitals.sync_currentHealth;
        hydrationSlider.value = Player.LocalPlayer.Vitals.sync_currentHydration;
        saturationSlider.value = Player.LocalPlayer.Vitals.sync_currentSaturation;

        if (Player.LocalPlayer.IsHeadhunter)
        {
            modeSwitchTimeLeftSlider.value = Headhunter.LocalHeadhunter.timeUntilCanTransition;
        }
    }
    
    private void OnLocalPlayerSpawned(bool isHeadhunter)
    {
        StartCoroutine(RevealRole());
        
        statsPanelRoot.SetActive(true);

        // Set the UI sliders' max values
        healthSlider.maxValue = Player.LocalPlayer.Vitals.sync_maxHealth;
        hydrationSlider.maxValue = Player.LocalPlayer.Vitals.sync_maxHydration;
        saturationSlider.maxValue = Player.LocalPlayer.Vitals.sync_maxSaturation;

        if (Player.LocalPlayer.IsHeadhunter)
        {
            modeSwitchTimeLeftSlider.minValue = 0;
            modeSwitchTimeLeftSlider.maxValue = Headhunter.MinimumDelayBetweenTransitions;
        
            modeSwitchTimeLeftSlider.gameObject.SetActive(true);
        }
    }

    public void OnHeadhunterTransitionsEnabled()
    {
        modeSwitchTimeLeftSlider.gameObject.SetActive(false);
    }

    public void OnHeadhunterTransitionsDisabled()
    {
        modeSwitchTimeLeftSlider.gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets the interaction text, and sets the text to visible.
    /// </summary>
    /// <param name="text"></param>
    public void SetInteractText(string text)
    {
        SetInteractTextVisible(true);
        interactText.text = text;
    }

    public void SetInteractTextPosition(Camera targetingCamera, Vector3 targetWorldPos)
    {
        Vector3 screenPos = targetingCamera.WorldToScreenPoint(targetWorldPos);
        interactText.rectTransform.position = Vector3.Lerp(screenPos, interactText.rectTransform.position, Time.deltaTime * interactionTextWorldSpaceSmoothness);
    }

    public void SetInteractTextVisible(bool visible)
    {
        interactText.gameObject.SetActive(visible);
    }

    private void NotifyPlayerOfDeath(PlayerDamageSource deathCause)
    {
        StartCoroutine(NotifyDeath(deathCause, Constants.PlayerRespawnTime));
        
        statsPanelRoot.SetActive(false);
    }

    /// <summary>
    /// Displays a death UI popup to the user.
    /// </summary>
    /// <param name="deathCause">Determines the text shown</param>
    /// <param name="playerRespawnTime">Time after the player will be respawned</param>
    /// <returns></returns>
    private IEnumerator NotifyDeath(PlayerDamageSource deathCause, float playerRespawnTime)
    {
        // Select the correct death text based on the latest damage source
        string deathText = deathCause switch
        {
            PlayerDamageSource.Headhunter => "You have been killed by a headhunter.",
            PlayerDamageSource.Player => "You have been killed by a fellow survivor.",
            PlayerDamageSource.Hunger => "You have starved to death.",
            PlayerDamageSource.Thirst => "You have died due to dehydration.",
            PlayerDamageSource.Server => "You have been killed by the server.",
            _ => "You have been killed by unknown reasons."
        };

        killedText.text = $"{deathText}\nYou will respawn as a headhunter in <b>{playerRespawnTime} seconds</b>.";
        killedText.alpha = 0;
        
        // Fade in the text
        DOTween.To(() => killedText.alpha, x => killedText.alpha = x, 1, roleDisplayFadeTime);

        // Wait until a few seconds before the player respawns
        yield return new WaitForSecondsRealtime(playerRespawnTime);
        
        // Fade out the text
        DOTween.To(() => killedText.alpha, x => killedText.alpha = x, 0, roleDisplayFadeTime);
    }

    
    private IEnumerator RevealRole()
    {
        yield return new WaitForSecondsRealtime(roleDisplayDelayTime);
        
        roleDisplayText.color = Player.LocalPlayer.IsHeadhunter ? headhunterColor : survivorColor;
        roleDisplayText.text = Player.LocalPlayer.IsHeadhunter ? headhunterRoleString : survivorRoleString;
        roleDisplayText.alpha = 0;

        // Fade in the text
        DOTween.To(() => roleDisplayText.alpha, x => roleDisplayText.alpha = x, 1, roleDisplayFadeTime);

        // Wait for the desired amount
        yield return new WaitForSecondsRealtime(roleDisplayFadeTime + roleDisplayShowTime);
        
        // Fade out the text
        DOTween.To(() => roleDisplayText.alpha, x => roleDisplayText.alpha = x, 0, roleDisplayFadeTime);
    }
}
