using UnityEngine;

// Platform performance settings observer
public class PlatformPerformanceSettings : MonoBehaviour, IPlatformObserver
{
    [Header("Quality Settings")]
    [SerializeField] private int pcTargetFrameRate = 60;
    [SerializeField] private int consoleTargetFrameRate = 60;
    [SerializeField] private int mobileTargetFrameRate = 30;
    
    private void Start()
    {
        PlatformManager.Instance.AddObserver(this);
        OnPlatformChanged(PlatformManager.Instance.CurrentPlatform);
    }
    
    private void OnDestroy()
    {
        if (PlatformManager.Instance != null)
        {
            PlatformManager.Instance.RemoveObserver(this);
        }
    }
    
    public void OnPlatformChanged(Platform newPlatform)
    {
        switch (newPlatform)
        {
            case Platform.PC:
                ApplyPCSettings();
                break;
            case Platform.PS5:
                ApplyConsoleSettings();
                break;
            case Platform.Android:
            case Platform.iOS:
                ApplyMobileSettings();
                break;
        }
    }
    
    private void ApplyPCSettings()
    {
        Application.targetFrameRate = pcTargetFrameRate;
        QualitySettings.SetQualityLevel(5); // Highest quality
        Debug.Log("Applied PC performance settings: Highest quality, 60FPS");
    }
    
    private void ApplyConsoleSettings()
    {
        Application.targetFrameRate = consoleTargetFrameRate;
        QualitySettings.SetQualityLevel(4); // High quality
        Debug.Log("Applied console performance settings: High quality, 60FPS");
    }
    
    private void ApplyMobileSettings()
    {
    Application.targetFrameRate = mobileTargetFrameRate;
        QualitySettings.SetQualityLevel(2); // Medium quality
        Debug.Log("Applied mobile performance settings: Medium quality, 30FPS");
    }
}