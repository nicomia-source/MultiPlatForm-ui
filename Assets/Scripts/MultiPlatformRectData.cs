using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlatformRectSettings
{
    [Header("Position Settings")]
    public bool overrideAnchoredPosition = false;
    public Vector2 anchoredPosition;
    
    [Header("Size Settings")]
    public bool overrideSizeDelta = false;
    public Vector2 sizeDelta;
    
    [Header("Anchor Settings")]
    public bool overrideAnchors = false;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    
    [Header("Pivot Settings")]
    public bool overridePivot = false;
    public Vector2 pivot;
    
    [Header("Rotation Settings")]
    public bool overrideRotation = false;
    public Vector3 rotation;
    
    [Header("Scale Settings")]
    public bool overrideScale = false;
    public Vector3 scale = Vector3.one;

    // Constructor to initialize default values
    public PlatformRectSettings()
    {
        anchoredPosition = Vector2.zero;
        sizeDelta = Vector2.zero;
        anchorMin = Vector2.zero;
        anchorMax = Vector2.one;
        pivot = new Vector2(0.5f, 0.5f);
        rotation = Vector3.zero;
        scale = Vector3.one;
    }
}

[System.Serializable]
public class PlatformDataEntry
{
    public Platform platform;
    public PlatformRectSettings settings;
    
    public PlatformDataEntry(Platform platform)
    {
        this.platform = platform;
        this.settings = new PlatformRectSettings();
    }
}

public class MultiPlatformRectData : MonoBehaviour, IPlatformObserver
{
    [Header("Platform Override Data")]
    [SerializeField] private List<PlatformDataEntry> platformData = new List<PlatformDataEntry>();
    
    [Header("Fallback Settings")]
    [SerializeField] private PlatformRectSettings defaultSettings = new PlatformRectSettings();
    
    [Header("Runtime Options")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool applyOnPlatformChange = true;
    
    private RectTransform rectTransform;
    private PlatformRectSettings originalSettings;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        SaveOriginalSettings();
        InitializePlatformData();
    }

    void Start()
    {
        if (applyOnStart)
        {
            Platform current = PlatformManager.Instance.CurrentPlatform;
            ApplySettings(GetSettingsForPlatform(current));
        }
    }

    void OnEnable()
    {
        if (applyOnPlatformChange && PlatformManager.Instance != null)
        {
            PlatformManager.Instance.AddObserver(this);
        }
    }

    void OnDisable()
    {
        if (PlatformManager.Instance != null)
        {
            PlatformManager.Instance.RemoveObserver(this);
        }
    }

    private void InitializePlatformData()
    {
        // Ensure all platforms have entries
        var allPlatforms = System.Enum.GetValues(typeof(Platform)).Cast<Platform>();
        foreach (Platform platform in allPlatforms)
        {
            if (!platformData.Any(entry => entry.platform == platform))
            {
                platformData.Add(new PlatformDataEntry(platform));
            }
        }
    }

    private void SaveOriginalSettings()
    {
        if (rectTransform != null)
        {
            originalSettings = new PlatformRectSettings
            {
                anchoredPosition = rectTransform.anchoredPosition,
                sizeDelta = rectTransform.sizeDelta,
                anchorMin = rectTransform.anchorMin,
                anchorMax = rectTransform.anchorMax,
                pivot = rectTransform.pivot,
                rotation = rectTransform.eulerAngles,
                scale = rectTransform.localScale
            };
        }
    }

    public PlatformRectSettings GetSettingsForPlatform(Platform platform)
    {
        var entry = platformData.FirstOrDefault(data => data.platform == platform);
        return entry?.settings ?? defaultSettings;
    }

    public void SetSettingsForPlatform(Platform platform, PlatformRectSettings settings)
    {
        var entry = platformData.FirstOrDefault(data => data.platform == platform);
        if (entry != null)
        {
            entry.settings = settings;
        }
        else
        {
            platformData.Add(new PlatformDataEntry(platform) { settings = settings });
        }
    }

    public void ApplySettings(PlatformRectSettings settings)
    {
        if (rectTransform == null) return;

        if (settings.overrideAnchoredPosition)
            rectTransform.anchoredPosition = settings.anchoredPosition;
        
        if (settings.overrideSizeDelta)
            rectTransform.sizeDelta = settings.sizeDelta;
        
        if (settings.overrideAnchors)
        {
            rectTransform.anchorMin = settings.anchorMin;
            rectTransform.anchorMax = settings.anchorMax;
        }
        
        if (settings.overridePivot)
            rectTransform.pivot = settings.pivot;
        
        if (settings.overrideRotation)
            rectTransform.eulerAngles = settings.rotation;
        
        if (settings.overrideScale)
            rectTransform.localScale = settings.scale;

        Debug.Log($"Applied {settings.GetType().Name} settings for platform");
    }

    public void ApplyCurrentPlatformSettings()
    {
        if (PlatformManager.Instance != null)
        {
            Platform current = PlatformManager.Instance.CurrentPlatform;
            ApplySettings(GetSettingsForPlatform(current));
        }
    }

    public void ResetToOriginal()
    {
        if (originalSettings != null)
        {
            ApplySettings(originalSettings);
        }
    }

    // Platform observer interface implementation
    public void OnPlatformChanged(Platform newPlatform)
    {
        if (applyOnPlatformChange)
        {
            ApplySettings(GetSettingsForPlatform(newPlatform));
        }
    }

    // Editor utility methods
    [ContextMenu("Capture Current Settings for Current Platform")]
    public void CaptureCurrentSettings()
    {
        if (PlatformManager.Instance != null && rectTransform != null)
        {
            Platform current = PlatformManager.Instance.CurrentPlatform;
            var settings = new PlatformRectSettings
            {
                overrideAnchoredPosition = true,
                anchoredPosition = rectTransform.anchoredPosition,
                overrideSizeDelta = true,
                sizeDelta = rectTransform.sizeDelta,
                overrideAnchors = true,
                anchorMin = rectTransform.anchorMin,
                anchorMax = rectTransform.anchorMax,
                overridePivot = true,
                pivot = rectTransform.pivot,
                overrideRotation = true,
                rotation = rectTransform.eulerAngles,
                overrideScale = true,
                scale = rectTransform.localScale
            };
            
            SetSettingsForPlatform(current, settings);
            Debug.Log($"Captured current settings for {current} platform");
        }
    }
}