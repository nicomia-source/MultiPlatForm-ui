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

[RequireComponent(typeof(RectTransform))]
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
    
    // Public methods for setting enabled configurations
    public void SetPositionOverride(bool enabled) => enablePositionOverride = enabled;
    public void SetSizeOverride(bool enabled) => enableSizeOverride = enabled;
    public void SetAnchorsOverride(bool enabled) => enableAnchorsOverride = enabled;
    public void SetPivotOverride(bool enabled) => enablePivotOverride = enabled;
    public void SetRotationOverride(bool enabled) => enableRotationOverride = enabled;
    public void SetScaleOverride(bool enabled) => enableScaleOverride = enabled;
    public void SetAutoRecord(bool enabled) => enableAutoRecord = enabled;
    
    // Public methods for setting runtime options
    public void SetApplyOnStart(bool enabled) => applyOnStart = enabled;
    public void SetApplyOnPlatformChange(bool enabled) => applyOnPlatformChange = enabled;

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
        // 在编辑器模式下总是注册为观察者，以便响应平台切换
        // 在运行时模式下只有当applyOnPlatformChange为true时才注册
        if (PlatformManager.Instance != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || applyOnPlatformChange)
            {
                PlatformManager.Instance.AddObserver(this);
            }
#else
            if (applyOnPlatformChange)
            {
                PlatformManager.Instance.AddObserver(this);
            }
#endif
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
        // 确保RectTransform已初始化
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
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
        // 确保RectTransform已初始化
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        if (rectTransform == null) 
        {
            Debug.LogWarning($"[{gameObject.name}] RectTransform component not found, cannot apply settings. Make sure this component is attached to a UI object.");
            return;
        }

        Debug.Log($"[{gameObject.name}] Applying settings - Position Override: {enablePositionOverride && settings.overrideAnchoredPosition}");

        if (enablePositionOverride && settings.overrideAnchoredPosition)
        {
            var oldPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = settings.anchoredPosition;
            Debug.Log($"[{gameObject.name}] Position changed from {oldPosition} to {settings.anchoredPosition}");
        }
        
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

#if UNITY_EDITOR
        // 在Editor模式下，确保Scene视图立即更新
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(rectTransform);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(gameObject);
            
            // 强制刷新Scene视图
            UnityEditor.SceneView.RepaintAll();
            
            // 如果当前选中的是这个对象，强制刷新Inspector
            if (UnityEditor.Selection.activeGameObject == gameObject)
            {
                UnityEditor.EditorUtility.SetDirty(UnityEditor.Selection.activeGameObject);
                // 强制刷新Inspector显示
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
        }
#endif

        Debug.Log($"[{gameObject.name}] Applied platform settings: Position={enablePositionOverride}, Size={enableSizeOverride}");
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
        Debug.Log($"[{gameObject.name}] Platform changed to: {newPlatform}");
        
        // 在编辑器模式下总是应用设置，在运行时模式下只有当applyOnPlatformChange为true时才应用
#if UNITY_EDITOR
        if (!Application.isPlaying || applyOnPlatformChange)
        {
            var settings = GetSettingsForPlatform(newPlatform);
            Debug.Log($"[{gameObject.name}] Applying settings for {newPlatform}: Position={settings.anchoredPosition}, Override={settings.overrideAnchoredPosition}");
            ApplySettings(settings);
            
            // 强制刷新Scene视图
            UnityEditor.SceneView.RepaintAll();
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
#else
        if (applyOnPlatformChange)
        {
            ApplySettings(GetSettingsForPlatform(newPlatform));
        }
#endif
    }

    // Editor utility methods
    public void CaptureCurrentSettings()
    {
        // 确保RectTransform已初始化
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
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
        // 确保RectTransform已初始化
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
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

    // 精确计算传统存储大小
    private int CalculateActualTraditionalSize()
    {
        if (platformData == null || platformData.Count == 0)
            return 0;

        // 精确计算PlatformRectSettings的大小
        int singleSettingsSize = 
            6 * sizeof(bool) +           // 6个布尔值: 6字节
            5 * (2 * sizeof(float)) +    // 5个Vector2: 40字节
            2 * (3 * sizeof(float));     // 2个Vector3: 24字节
        
        // 考虑C#对象开销和内存对齐
        int objectOverhead = 24;  // 对象头、方法表指针等
        int memoryAlignment = 8;  // 内存对齐填充
        
        int actualSingleSize = singleSettingsSize + objectOverhead;
        // 向上对齐到8字节边界
        actualSingleSize = ((actualSingleSize + memoryAlignment - 1) / memoryAlignment) * memoryAlignment;
        
        // PlatformDataEntry还包含Platform枚举(4字节)和对象开销
        int entryOverhead = sizeof(int) + objectOverhead; // Platform枚举 + 对象开销
        int totalEntrySize = actualSingleSize + entryOverhead;
        
        // List<T>的开销
        int listOverhead = 32; // List对象本身的开销
        
        return (platformData.Count * totalEntrySize) + listOverhead;
    }

    // 存储压缩比较 - 使用精确计算
    public float GetCompressionRatio()
    {
        if (!useBinaryStorage || platformData == null || platformData.Count == 0)
            return 1.0f;

        int traditionalSize = CalculateActualTraditionalSize();
        int binarySize = binaryContainer.GetStorageSize();
        
        return binarySize > 0 ? (float)traditionalSize / binarySize : 1.0f;
    }

    // 获取详细的存储信息对比
    public string GetDetailedStorageInfo()
    {
        if (platformData == null || platformData.Count == 0)
            return "No data available";

        int traditionalSize = CalculateActualTraditionalSize();
        int binarySize = useBinaryStorage ? binaryContainer.GetStorageSize() : 0;
        float compressionRatio = GetCompressionRatio();

        return $"Platform Count: {platformData.Count}\n" +
               $"Traditional Storage: {traditionalSize} bytes\n" +
               $"Binary Storage: {binarySize} bytes\n" +
               $"Compression Ratio: {compressionRatio:F2}x\n" +
               $"Space Saved: {traditionalSize - binarySize} bytes ({((float)(traditionalSize - binarySize) / traditionalSize * 100):F1}%)";
    }
}