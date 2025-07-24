using UnityEngine;
using UnityEditor;

public class AutoRecordStatusChecker : EditorWindow
{
    [MenuItem("Tools/Auto Record Status Checker")]
    public static void ShowWindow()
    {
        GetWindow<AutoRecordStatusChecker>("Auto Record Status");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Record Status Checker", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Check All Components"))
        {
            CheckAllComponents();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Check PlatformManager"))
        {
            CheckPlatformManager();
        }
    }
    
    private void CheckAllComponents()
    {
        var components = UnityEngine.Object.FindObjectsOfType<MultiPlatformRectData>(true);
        Debug.Log($"=== 找到 {components.Length} 个 MultiPlatformRectData 组件 ===");
        
        foreach (var component in components)
        {
            if (component == null) continue;
            
            Debug.Log($"--- 组件: {component.gameObject.name} ---");
            Debug.Log($"  启用自动记录: {component.EnableAutoRecord}");
            Debug.Log($"  自动记录延迟: {component.AutoRecordDelay}秒");
            Debug.Log($"  启用位置覆盖: {component.EnablePositionOverride}");
            Debug.Log($"  启用尺寸覆盖: {component.EnableSizeOverride}");
            Debug.Log($"  启用锚点覆盖: {component.EnableAnchorsOverride}");
            Debug.Log($"  启用轴心覆盖: {component.EnablePivotOverride}");
            Debug.Log($"  启用旋转覆盖: {component.EnableRotationOverride}");
            Debug.Log($"  启用缩放覆盖: {component.EnableScaleOverride}");
            
            var rectTransform = component.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"  当前位置: {rectTransform.anchoredPosition}");
                Debug.Log($"  当前尺寸: {rectTransform.sizeDelta}");
            }
        }
    }
    
    private void CheckPlatformManager()
    {
        try
        {
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
                    Debug.LogError("❌ 无法找到PlatformManager类型");
                    return;
                }
            }
            Debug.Log("✓ 找到PlatformManager类型");
            
            var instanceProperty = platformManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty == null)
            {
                Debug.LogError("❌ 无法找到PlatformManager.Instance属性");
                return;
            }
            Debug.Log("✓ 找到Instance属性");
            
            var platformManagerInstance = instanceProperty.GetValue(null);
            if (platformManagerInstance == null)
            {
                Debug.LogError("❌ PlatformManager.Instance为null");
                return;
            }
            Debug.Log("✓ PlatformManager.Instance不为null");
            
            var currentPlatformProperty = platformManagerType.GetProperty("CurrentPlatform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (currentPlatformProperty == null)
            {
                Debug.LogError("❌ 无法找到CurrentPlatform属性");
                return;
            }
            Debug.Log("✓ 找到CurrentPlatform属性");
            
            var currentPlatform = currentPlatformProperty.GetValue(platformManagerInstance);
            if (currentPlatform == null)
            {
                Debug.LogError("❌ CurrentPlatform为null");
                return;
            }
            Debug.Log($"✓ 当前平台: {currentPlatform}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 检查PlatformManager时出错: {e.Message}");
        }
    }
}