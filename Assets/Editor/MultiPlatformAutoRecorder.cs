using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public static class MultiPlatformAutoRecorder
{
    private static Dictionary<int, Vector2> lastPositions = new Dictionary<int, Vector2>();
    private static Dictionary<int, Vector2> lastSizes = new Dictionary<int, Vector2>();
    private static Dictionary<int, double> changeTimestamps = new Dictionary<int, double>();
    private static double lastCheckTime = 0;
    
    static MultiPlatformAutoRecorder()
    {
        EditorApplication.update += OnEditorUpdate;
    }
    
    private static void OnEditorUpdate()
    {
        if (Application.isPlaying) return;
        
        // 提高检查频率到每50ms一次，以便更好地捕获UI变化
        double currentTime = EditorApplication.timeSinceStartup;
        if (currentTime - lastCheckTime < 0.05) return;
        lastCheckTime = currentTime;
        
        CheckForChanges();
    }
    
    private static void CheckForChanges()
    {
        try
        {
            double currentTime = EditorApplication.timeSinceStartup;
            
            // 使用更可靠的方法查找组件
            var components = UnityEngine.Object.FindObjectsOfType<MultiPlatformRectData>(true);
            var activeIds = new HashSet<int>();
            
            foreach (var component in components)
            {
                if (component == null || component.gameObject == null) continue;
                
                int id = component.GetInstanceID();
                activeIds.Add(id);
                
                // 检查是否启用自动记录
                if (!component.EnableAutoRecord) continue;
                
                var rectTransform = component.GetComponent<RectTransform>();
                if (rectTransform == null) continue;
                
                bool hasChanged = false;
                Vector2 currentPos = rectTransform.anchoredPosition;
                Vector2 currentSize = rectTransform.sizeDelta;
                
                // 检查位置变化 - 降低阈值以捕获更小的变化
                if (lastPositions.ContainsKey(id))
                {
                    if (Vector2.Distance(lastPositions[id], currentPos) > 0.001f)
                    {
                        hasChanged = true;
                        Debug.Log($"[自动记录] 检测到位置变化: {component.gameObject.name} - 从 {lastPositions[id]} 到 {currentPos}");
                    }
                }
                else
                {
                    lastPositions[id] = currentPos;
                }
                
                // 检查尺寸变化 - 降低阈值以捕获更小的变化
                if (lastSizes.ContainsKey(id))
                {
                    if (Vector2.Distance(lastSizes[id], currentSize) > 0.001f)
                    {
                        hasChanged = true;
                        Debug.Log($"[自动记录] 检测到尺寸变化: {component.gameObject.name} - 从 {lastSizes[id]} 到 {currentSize}");
                    }
                }
                else
                {
                    lastSizes[id] = currentSize;
                }
                
                if (hasChanged)
                {
                    lastPositions[id] = currentPos;
                    lastSizes[id] = currentSize;
                    changeTimestamps[id] = currentTime;
                    Debug.Log($"[自动记录] 记录变化时间戳: {component.gameObject.name} - 延迟: {component.AutoRecordDelay}秒");
                }
                
                // 检查是否需要保存
                if (changeTimestamps.ContainsKey(id))
                {
                    double timeSinceChange = currentTime - changeTimestamps[id];
                    if (timeSinceChange >= component.AutoRecordDelay)
                    {
                        Debug.Log($"[自动记录] 准备保存设置: {component.gameObject.name} - 变化后经过时间: {timeSinceChange:F2}秒");
                        SaveSettings(component, rectTransform);
                        changeTimestamps.Remove(id);
                    }
                }
            }
            
            // 清理无效的记录
            CleanupOldEntries(activeIds);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MultiPlatformAutoRecorder error: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private static void CleanupOldEntries(HashSet<int> activeIds)
    {
        var keysToRemove = new List<int>();
        
        foreach (var key in lastPositions.Keys)
        {
            if (!activeIds.Contains(key))
                keysToRemove.Add(key);
        }
        
        foreach (var key in keysToRemove)
        {
            lastPositions.Remove(key);
            lastSizes.Remove(key);
            changeTimestamps.Remove(key);
        }
    }
    
    private static void SaveSettings(MultiPlatformRectData component, RectTransform rectTransform)
    {
        try
        {
            Debug.Log($"[自动记录] 开始保存设置: {component.gameObject.name}");
            
            // 使用反射安全访问PlatformManager，避免Editor和Runtime程序集交叉引用
            var platformManagerType = System.Type.GetType("PlatformManager");
            if (platformManagerType == null) 
            {
                // 尝试从所有程序集中查找
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    platformManagerType = assembly.GetType("PlatformManager");
                    if (platformManagerType != null) break;
                }
                
                if (platformManagerType == null)
                {
                    Debug.LogError("[自动记录] 无法找到PlatformManager类型");
                    return;
                }
            }
            
            var instanceProperty = platformManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty == null) 
            {
                Debug.LogWarning("[自动记录] 无法找到PlatformManager.Instance属性");
                return;
            }
            
            var platformManagerInstance = instanceProperty.GetValue(null);
            if (platformManagerInstance == null) 
            {
                Debug.LogWarning("[自动记录] PlatformManager.Instance为null");
                return;
            }
            
            var currentPlatformProperty = platformManagerType.GetProperty("CurrentPlatform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (currentPlatformProperty == null) 
            {
                Debug.LogWarning("[自动记录] 无法找到CurrentPlatform属性");
                return;
            }
            
            var currentPlatform = currentPlatformProperty.GetValue(platformManagerInstance);
            if (currentPlatform == null) 
            {
                Debug.LogWarning("[自动记录] CurrentPlatform为null");
                return;
            }
            
            Debug.Log($"[自动记录] 当前平台: {currentPlatform}");
            
            var settings = component.GetSettingsForPlatform((Platform)currentPlatform);
            bool saved = false;
            
            if (component.EnablePositionOverride)
            {
                settings.overrideAnchoredPosition = true;
                settings.anchoredPosition = rectTransform.anchoredPosition;
                saved = true;
                Debug.Log($"[自动记录] 保存位置: {rectTransform.anchoredPosition}");
            }
            
            if (component.EnableSizeOverride)
            {
                settings.overrideSizeDelta = true;
                settings.sizeDelta = rectTransform.sizeDelta;
                saved = true;
                Debug.Log($"[自动记录] 保存尺寸: {rectTransform.sizeDelta}");
            }
            
            if (component.EnableAnchorsOverride)
            {
                settings.overrideAnchors = true;
                settings.anchorMin = rectTransform.anchorMin;
                settings.anchorMax = rectTransform.anchorMax;
                saved = true;
                Debug.Log($"[自动记录] 保存锚点: Min={rectTransform.anchorMin}, Max={rectTransform.anchorMax}");
            }
            
            if (component.EnablePivotOverride)
            {
                settings.overridePivot = true;
                settings.pivot = rectTransform.pivot;
                saved = true;
                Debug.Log($"[自动记录] 保存轴心: {rectTransform.pivot}");
            }
            
            if (component.EnableRotationOverride)
            {
                settings.overrideRotation = true;
                settings.rotation = rectTransform.eulerAngles;
                saved = true;
                Debug.Log($"[自动记录] 保存旋转: {rectTransform.eulerAngles}");
            }
            
            if (component.EnableScaleOverride)
            {
                settings.overrideScale = true;
                settings.scale = rectTransform.localScale;
                saved = true;
                Debug.Log($"[自动记录] 保存缩放: {rectTransform.localScale}");
            }
            
            if (saved)
            {
                component.SetSettingsForPlatform((Platform)currentPlatform, settings);
                EditorUtility.SetDirty(component);
                Debug.Log($"[自动记录] ✓ 成功保存 {component.gameObject.name} 到 {currentPlatform} 平台");
            }
            else
            {
                Debug.LogWarning($"[自动记录] 没有启用任何覆盖属性，跳过保存: {component.gameObject.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[自动记录] 保存设置时出错: {e.Message}\n{e.StackTrace}");
        }
    }
}