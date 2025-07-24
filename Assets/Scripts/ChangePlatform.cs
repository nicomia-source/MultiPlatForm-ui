using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Main platform switching controller
public class ChangePlatform : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown platformDropdown;

    private void Start()
    {
        SetupDropdown();
    }

    private void SetupDropdown()
    {
        if (platformDropdown != null)
        {
            // Clear existing options
            platformDropdown.ClearOptions();
            
            // Add platform options
            List<string> platformNames = new List<string>();
            foreach (Platform platform in Enum.GetValues(typeof(Platform)))
            {
                platformNames.Add(platform.ToString());
            }
            platformDropdown.AddOptions(platformNames);
            
            // Set current selection
            platformDropdown.value = (int)PlatformManager.Instance.CurrentPlatform;
            
            // Add listener
            platformDropdown.onValueChanged.AddListener(OnPlatformDropdownChanged);
        }
    }

    private void OnPlatformDropdownChanged(int index)
    {
        Platform selectedPlatform = (Platform)index;
        PlatformManager.Instance.SetPlatform(selectedPlatform);
    }

    private void OnDestroy()
    {
        if (platformDropdown != null)
        {
            platformDropdown.onValueChanged.RemoveListener(OnPlatformDropdownChanged);
        }
    }
}
