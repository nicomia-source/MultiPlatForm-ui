using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Platform-specific behavior observer example
public class PlatformSpecificBehavior : MonoBehaviour, IPlatformObserver
{
    [Header("UI Elements")]
    [SerializeField] private Button[] pcOnlyButtons;
    [SerializeField] private Button[] consoleButtons;
    [SerializeField] private Button[] mobileButtons;
    [SerializeField] private TextMeshProUGUI platformInfoText;
    
    [Header("Game Objects")]
    [SerializeField] private GameObject[] pcOnlyObjects;
    [SerializeField] private GameObject[] consoleOnlyObjects;
    [SerializeField] private GameObject[] mobileOnlyObjects;
    
    private void Start()
    {
        // Register as observer
        PlatformManager.Instance.AddObserver(this);
        
        // Initialize current platform settings
        OnPlatformChanged(PlatformManager.Instance.CurrentPlatform);
    }
    
    private void OnDestroy()
    {
        // Remove observer
        if (PlatformManager.Instance != null)
        {
            PlatformManager.Instance.RemoveObserver(this);
        }
    }
    
    public void OnPlatformChanged(Platform newPlatform)
    {
        // Adjust UI and game objects based on platform
        switch (newPlatform)
        {
            case Platform.PC:
                SetupPCPlatform();
                break;
            case Platform.PS5:
                SetupConsolePlatform("PS5");
                break;
            case Platform.Android:
                SetupMobilePlatform("Android");
                break;
            case Platform.iOS:
                SetupMobilePlatform("iOS");
                break;
        }
        
        UpdatePlatformInfo(newPlatform);
    }
    
    private void SetupPCPlatform()
    {
        // Enable PC-specific UI
        SetButtonsActive(pcOnlyButtons, true);
        SetButtonsActive(consoleButtons, false);
        SetButtonsActive(mobileButtons, false);
        
        // Enable PC-specific game objects
        SetGameObjectsActive(pcOnlyObjects, true);
        SetGameObjectsActive(consoleOnlyObjects, false);
        SetGameObjectsActive(mobileOnlyObjects, false);
        
        Debug.Log("Switched to PC platform configuration: Enabled keyboard/mouse control, high quality settings");
    }
    
    private void SetupConsolePlatform(string consoleName)
    {
        // Enable console-specific UI
        SetButtonsActive(pcOnlyButtons, false);
        SetButtonsActive(consoleButtons, true);
        SetButtonsActive(mobileButtons, false);
        
        // Enable console-specific game objects
        SetGameObjectsActive(pcOnlyObjects, false);
        SetGameObjectsActive(consoleOnlyObjects, true);
        SetGameObjectsActive(mobileOnlyObjects, false);
        
        Debug.Log($"Switched to {consoleName} platform configuration: Enabled controller support, optimized quality settings");
    }
    
    private void SetupMobilePlatform(string mobilePlatform)
    {
        // Enable mobile-specific UI
        SetButtonsActive(pcOnlyButtons, false);
        SetButtonsActive(consoleButtons, false);
        SetButtonsActive(mobileButtons, true);
        
        // Enable mobile-specific game objects
        SetGameObjectsActive(pcOnlyObjects, false);
        SetGameObjectsActive(consoleOnlyObjects, false);
        SetGameObjectsActive(mobileOnlyObjects, true);
        
        Debug.Log($"Switched to {mobilePlatform} platform configuration: Enabled touch control, power saving mode");
    }
    
    private void SetButtonsActive(Button[] buttons, bool active)
    {
        if (buttons != null)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(active);
                }
            }
        }
    }
    
    private void SetGameObjectsActive(GameObject[] objects, bool active)
    {
        if (objects != null)
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
    }
    
    private void UpdatePlatformInfo(Platform platform)
    {
        if (platformInfoText != null)
        {
            string info = GetPlatformInfo(platform);
            platformInfoText.text = info;
        }
    }
    
    private string GetPlatformInfo(Platform platform)
    {
        switch (platform)
        {
            case Platform.PC:
                return "PC Platform\n• Keyboard/Mouse Control\n• High Quality\n• Multi-window Support";
            case Platform.PS5:
                return "PS5 Platform\n• DualSense Controller\n• 4K Quality\n• Fast Loading";
            case Platform.Android:
                return "Android Platform\n• Touch Control\n• Adaptive Quality\n• Power Saving Mode";
            case Platform.iOS:
                return "iOS Platform\n• Touch Control\n• Metal Rendering\n• Game Center";
            default:
                return "Unknown Platform";
        }
    }
}