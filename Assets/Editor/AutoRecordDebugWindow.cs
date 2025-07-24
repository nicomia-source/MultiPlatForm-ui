using UnityEngine;
using UnityEditor;

public class AutoRecordDebugWindow : EditorWindow
{
    private bool showDebugLogs = true;
    private Vector2 scrollPosition;
    private string debugInfo = "";
    
    [MenuItem("Tools/Auto Record Debug")]
    public static void ShowWindow()
    {
        GetWindow<AutoRecordDebugWindow>("Auto Record Debug");
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("自动记录调试工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        showDebugLogs = EditorGUILayout.Toggle("显示调试日志", showDebugLogs);
        
        if (GUILayout.Button("刷新组件信息"))
        {
            RefreshComponentInfo();
        }
        
        if (GUILayout.Button("清空日志"))
        {
            debugInfo = "";
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("组件信息:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(debugInfo, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        
        // 自动刷新
        if (Event.current.type == EventType.Layout)
        {
            Repaint();
        }
    }
    
    void RefreshComponentInfo()
    {
        debugInfo = "";
        var components = FindObjectsOfType<MultiPlatformRectData>(true);
        
        debugInfo += $"找到 {components.Length} 个 MultiPlatformRectData 组件\n\n";
        
        foreach (var component in components)
        {
            if (component != null && component.gameObject != null)
            {
                debugInfo += $"组件: {component.gameObject.name}\n";
                debugInfo += $"  - 自动记录启用: {component.EnableAutoRecord}\n";
                debugInfo += $"  - 位置覆盖启用: {component.EnablePositionOverride}\n";
                debugInfo += $"  - 尺寸覆盖启用: {component.EnableSizeOverride}\n";
                debugInfo += $"  - 自动记录延迟: {component.AutoRecordDelay}秒\n";
                
                var rectTransform = component.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    debugInfo += $"  - 当前位置: {rectTransform.anchoredPosition}\n";
                    debugInfo += $"  - 当前尺寸: {rectTransform.sizeDelta}\n";
                }
                
                // 显示平台设置
                if (PlatformManager.Instance != null)
                {
                    var currentPlatform = PlatformManager.Instance.CurrentPlatform;
                    var settings = component.GetSettingsForPlatform(currentPlatform);
                    debugInfo += $"  - 当前平台: {currentPlatform}\n";
                    debugInfo += $"  - 位置覆盖: {settings.overrideAnchoredPosition} -> {settings.anchoredPosition}\n";
                    debugInfo += $"  - 尺寸覆盖: {settings.overrideSizeDelta} -> {settings.sizeDelta}\n";
                }
                
                debugInfo += "\n";
            }
        }
        
        if (components.Length == 0)
        {
            debugInfo += "没有找到 MultiPlatformRectData 组件\n";
            debugInfo += "请确保:\n";
            debugInfo += "1. 场景中有UI对象\n";
            debugInfo += "2. UI对象上添加了 MultiPlatformRectData 组件\n";
            debugInfo += "3. 组件的自动记录功能已启用\n";
        }
    }
    
    void OnEnable()
    {
        RefreshComponentInfo();
    }
}