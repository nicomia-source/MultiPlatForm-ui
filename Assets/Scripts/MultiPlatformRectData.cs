using UnityEngine;

[System.Serializable]
public class PlatformRectSettings
{
    public Vector2 anchoredPosition;
    public Vector2 sizeDelta;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
}

public class MultiPlatformRectData : MonoBehaviour
{
    public PlatformRectSettings pcSettings;
    public PlatformRectSettings ps5Settings;
    public PlatformRectSettings androidSettings;
    public PlatformRectSettings iosSettings;

    void Start()
    {
        // Apply settings based on current platform
        Platform current = PlatformManager.Instance.CurrentPlatform;
        ApplySettings(GetSettingsForPlatform(current));
    }

    private PlatformRectSettings GetSettingsForPlatform(Platform platform)
    {
        switch (platform)
        {
            case Platform.PC: return pcSettings;
            case Platform.PS5: return ps5Settings;
            case Platform.Android: return androidSettings;
            case Platform.iOS: return iosSettings;
            default: return pcSettings;
        }
    }

    private void ApplySettings(PlatformRectSettings settings)
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = settings.anchoredPosition;
            rect.sizeDelta = settings.sizeDelta;
            rect.anchorMin = settings.anchorMin;
            rect.anchorMax = settings.anchorMax;
            rect.pivot = settings.pivot;
        }
    }
}