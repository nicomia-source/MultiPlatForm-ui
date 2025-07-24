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
    [Header("Enabled Properties")]
    [SerializeField] private bool enablePositionOverride = true;
    [SerializeField] private bool enableSizeOverride = true;
    [SerializeField] private bool enableAnchorsOverride = true;
    [SerializeField] private bool enablePivotOverride = false;
    [SerializeField] private bool enableRotationOverride = false;
    [SerializeField] private bool enableScaleOverride = true;
    
    [Header("Platform Override Data")]
    [SerializeField] private List<PlatformDataEntry> platformData = new List<PlatformDataEntry>();
    
    [Header("Binary Storage (Experimental)")]
    [SerializeField] private bool useBinaryStorage = false;
    [SerializeField] private BinaryPlatformData.BinaryPlatformContainer binaryContainer = new BinaryPlatformData.BinaryPlatformContainer();
    
    [Header("Fallback Settings")]
    [SerializeField] private PlatformRectSettings defaultSettings = new PlatformRectSettings();
    
    [Header("Runtime Options")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool applyOnPlatformChange = true;
    
    [Header("Auto-Record Settings")]
    [SerializeField] private bool enableAutoRecord = true;
    [Tooltip("自动记录延迟（秒），避免频繁保存")]
    [SerializeField] private float autoRecordDelay = 0.5f;
    
    private RectTransform rectTransform;
    private PlatformRectSettings originalSettings;
    
    // Public properties for accessing enabled configurations
    public bool EnablePositionOverride => enablePositionOverride;
    public bool EnableSizeOverride => enableSizeOverride;
    public bool EnableAnchorsOverride => enableAnchorsOverride;
    public bool EnablePivotOverride => enablePivotOverride;
    public bool EnableRotationOverride => enableRotationOverride;
    public bool EnableScaleOverride => enableScaleOverride;
    
    // Public properties for auto-record settings
    public bool EnableAutoRecord => enableAutoRecord;
    public float AutoRecordDelay => autoRecordDelay;

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
        if (useBinaryStorage)
        {
            var compactSettings = binaryContainer.GetPlatformSettings(platform);
            return compactSettings.ToPlatformRectSettings();
        }
        else
        {
            var entry = platformData.FirstOrDefault(data => data.platform == platform);
            if (entry != null)
            {
                return entry.settings;
            }
            
            // 如果没有找到对应平台的设置，创建一个新的设置并添加到列表中
            var newSettings = new PlatformRectSettings();
            platformData.Add(new PlatformDataEntry(platform) { settings = newSettings });
            return newSettings;
        }
    }

    public void SetSettingsForPlatform(Platform platform, PlatformRectSettings settings)
    {
        if (useBinaryStorage)
        {
            var compactSettings = new BinaryPlatformData.CompactPlatformSettings(settings);
            binaryContainer.SetPlatformSettings(platform, compactSettings);
        }
        else
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
    }

    public void ApplySettings(PlatformRectSettings settings)
    {
        if (rectTransform == null) return;

        if (enablePositionOverride && settings.overrideAnchoredPosition)
            rectTransform.anchoredPosition = settings.anchoredPosition;
        
        if (enableSizeOverride && settings.overrideSizeDelta)
            rectTransform.sizeDelta = settings.sizeDelta;
        
        if (enableAnchorsOverride && settings.overrideAnchors)
        {
            rectTransform.anchorMin = settings.anchorMin;
            rectTransform.anchorMax = settings.anchorMax;
        }
        
        if (enablePivotOverride && settings.overridePivot)
            rectTransform.pivot = settings.pivot;
        
        if (enableRotationOverride && settings.overrideRotation)
            rectTransform.eulerAngles = settings.rotation;
        
        if (enableScaleOverride && settings.overrideScale)
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

    // 保存当前设置到当前平台 - Editor专用方法
    public void SaveCurrentSettings()
    {
        if (PlatformManager.Instance != null && rectTransform != null)
        {
            Platform current = PlatformManager.Instance.CurrentPlatform;
            var settings = GetSettingsForPlatform(current);
            
            // 只保存启用的属性
            if (enablePositionOverride)
            {
                settings.overrideAnchoredPosition = true;
                settings.anchoredPosition = rectTransform.anchoredPosition;
            }
            
            if (enableSizeOverride)
            {
                settings.overrideSizeDelta = true;
                settings.sizeDelta = rectTransform.sizeDelta;
            }
            
            if (enableAnchorsOverride)
            {
                settings.overrideAnchors = true;
                settings.anchorMin = rectTransform.anchorMin;
                settings.anchorMax = rectTransform.anchorMax;
            }
            
            if (enablePivotOverride)
            {
                settings.overridePivot = true;
                settings.pivot = rectTransform.pivot;
            }
            
            if (enableRotationOverride)
            {
                settings.overrideRotation = true;
                settings.rotation = rectTransform.eulerAngles;
            }
            
            if (enableScaleOverride)
            {
                settings.overrideScale = true;
                settings.scale = rectTransform.localScale;
            }
            
            SetSettingsForPlatform(current, settings);
            Debug.Log($"Saved current RectTransform settings to {current} platform");
        }
    }

    // 配置预设方法
    public void ApplyUIElementPreset()
    {
        enablePositionOverride = true;
        enableSizeOverride = true;
        enableAnchorsOverride = true;
        enablePivotOverride = false;
        enableRotationOverride = false;
        enableScaleOverride = false;
        Debug.Log("Applied UI Element preset configuration");
    }

    public void ApplyAnimationElementPreset()
    {
        enablePositionOverride = true;
        enableSizeOverride = true;
        enableAnchorsOverride = false;
        enablePivotOverride = true;
        enableRotationOverride = true;
        enableScaleOverride = true;
        Debug.Log("Applied Animation Element preset configuration");
    }

    public void ApplyFullControlPreset()
    {
        enablePositionOverride = true;
        enableSizeOverride = true;
        enableAnchorsOverride = true;
        enablePivotOverride = true;
        enableRotationOverride = true;
        enableScaleOverride = true;
        Debug.Log("Applied Full Control preset configuration");
    }

    // 配置验证方法
    public bool ValidateConfiguration()
    {
        return enablePositionOverride || enableSizeOverride || enableAnchorsOverride || 
               enablePivotOverride || enableRotationOverride || enableScaleOverride;
    }

    public int GetEnabledPropertiesCount()
    {
        int count = 0;
        if (enablePositionOverride) count++;
        if (enableSizeOverride) count++;
        if (enableAnchorsOverride) count++;
        if (enablePivotOverride) count++;
        if (enableRotationOverride) count++;
        if (enableScaleOverride) count++;
        return count;
    }

    // 二进制存储管理方法
    public bool UseBinaryStorage
    {
        get => useBinaryStorage;
        set
        {
            if (useBinaryStorage != value)
            {
                if (value)
                {
                    ConvertToBinaryStorage();
                }
                else
                {
                    ConvertFromBinaryStorage();
                }
                useBinaryStorage = value;
            }
        }
    }

    // 从传统存储转换到二进制存储
    public void ConvertToBinaryStorage()
    {
        if (platformData != null && platformData.Count > 0)
        {
            binaryContainer.FromPlatformDataList(platformData);
            Debug.Log($"Converted {platformData.Count} platform entries to binary storage. Size: {binaryContainer.GetStorageSize()} bytes");
        }
    }

    // 从二进制存储转换到传统存储
    public void ConvertFromBinaryStorage()
    {
        if (binaryContainer != null)
        {
            platformData = binaryContainer.ToPlatformDataList();
            Debug.Log($"Converted binary storage to {platformData.Count} platform entries");
        }
    }

    // 获取存储信息
    public string GetStorageInfo()
    {
        if (useBinaryStorage)
        {
            int binarySize = binaryContainer.GetStorageSize();
            return $"Binary Storage: {binarySize} bytes";
        }
        else
        {
            int entryCount = platformData?.Count ?? 0;
            // 估算传统存储大小 (每个PlatformRectSettings约200字节)
            int estimatedSize = entryCount * 200;
            return $"Traditional Storage: ~{estimatedSize} bytes ({entryCount} entries)";
        }
    }

    // 清理存储缓存
    public void ClearStorageCache()
    {
        if (useBinaryStorage)
        {
            binaryContainer.ClearCache();
        }
    }

    // 存储压缩比较
    public float GetCompressionRatio()
    {
        if (!useBinaryStorage || platformData == null || platformData.Count == 0)
            return 1.0f;

        int traditionalSize = platformData.Count * 200; // 估算
        int binarySize = binaryContainer.GetStorageSize();
        
        return binarySize > 0 ? (float)traditionalSize / binarySize : 1.0f;
    }
}