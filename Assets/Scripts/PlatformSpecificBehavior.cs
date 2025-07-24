using UnityEngine;
using TMPro;

// Platform information display observer
public class PlatformSpecificBehavior : MonoBehaviour, IPlatformObserver
{
    [Header("Platform Info Display")]
    [SerializeField] private TextMeshProUGUI platformInfoText;
    
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
        // 只更新平台信息显示，不再强制隐藏UI组件
        UpdatePlatformInfo(newPlatform);
        Debug.Log($"Platform changed to: {newPlatform}. UI components will be handled by MultiPlatformRectData system.");
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