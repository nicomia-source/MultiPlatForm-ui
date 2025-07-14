using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Platform enumeration
public enum Platform
{
    PC,
    PS5,
    Android,
    iOS
}

// Observer interface
public interface IPlatformObserver
{
    void OnPlatformChanged(Platform newPlatform);
}

// Platform manager (subject)
public class PlatformManager : MonoBehaviour
{
    private static PlatformManager instance;
    public static PlatformManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlatformManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PlatformManager");
                    instance = go.AddComponent<PlatformManager>();
                }
            }
            return instance;
        }
    }

    private Platform currentPlatform = Platform.PC;
    private List<IPlatformObserver> observers = new List<IPlatformObserver>();

    public Platform CurrentPlatform => currentPlatform;

    // Add observer
    public void AddObserver(IPlatformObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    // Remove observer
    public void RemoveObserver(IPlatformObserver observer)
    {
        observers.Remove(observer);
    }

    // Notify all observers
    private void NotifyObservers(Platform newPlatform)
    {
        foreach (var observer in observers)
        {
            observer.OnPlatformChanged(newPlatform);
        }
    }

    // Set platform
    public void SetPlatform(Platform platform)
    {
        if (currentPlatform != platform)
        {
            currentPlatform = platform;
            NotifyObservers(currentPlatform);
            Debug.Log($"platform: {currentPlatform}");
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}



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
