using System;
using System.Collections;
using System.Collections.Generic;
using Headhunters.Misc;
using Mirror;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheatCodeManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    [ReadOnly]
    [SerializeField]
    private int currentIndex;

    private const string CheatCodesActivationString = "cheatcodes";

    private void Awake()
    {
        currentIndex = 0;
        inputField.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(CheatCodes.Activated) return;
        
        if (!NetworkServer.active) return;

        if (currentIndex == CheatCodesActivationString.Length)
        {
            inputField.gameObject.SetActive(true);

            CheatCodes.Activated = true;
            
            return;
        }

        if (!Input.anyKeyDown) return;

        Enum.TryParse(CheatCodesActivationString[currentIndex].ToString(), true, out KeyCode code);

        if (Input.GetKeyDown(code))
        {
            currentIndex++;
        }
    }

    public void ActivateCode()
    {
        if (inputField.text == "hulk")
        {
            CheatCodes.Hulk = true;

            inputField.text = "";
        }
    }
}
