using UnityEngine;
using UnityEditor;

public class AutoRecordDebugger : EditorWindow
{
    [MenuItem("Tools/Auto Record Debugger")]
    public static void ShowWindow()
    {
        GetWindow<AutoRecordDebugger>("Auto Record Debugger");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("自动记录功能调试器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 检查PlatformManager
        EditorGUILayout.LabelField("PlatformManager 状态:", EditorStyles.boldLabel);
        
        // 直接尝试查找PlatformManager实例
        PlatformManager platformManager = Object.FindObjectOfType<PlatformManager>();
        
        if (platformManager == null)
        {
            EditorGUILayout.HelpBox("❌ 场景中未找到 PlatformManager 实例", MessageType.Warning);
            
            if (GUILayout.Button("创建 PlatformManager 实例"))
            {
                GameObject go = new GameObject("PlatformManager");
                go.AddComponent<PlatformManager>();
                EditorGUILayout.HelpBox("✅ PlatformManager 实例已创建", MessageType.Info);
            }
            
            if (GUILayout.Button("强制重新编译"))
            {
                AssetDatabase.Refresh();
                EditorUtility.RequestScriptReload();
            }
            return;
        }
        else
        {
            EditorGUILayout.HelpBox("✅ PlatformManager 实例已找到", MessageType.Info);
            EditorGUILayout.LabelField($"当前平台: {platformManager.CurrentPlatform}");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("MultiPlatformRectData 组件:", EditorStyles.boldLabel);

        var components = Object.FindObjectsOfType<MultiPlatformRectData>();
        if (components.Length == 0)
        {
            EditorGUILayout.HelpBox("❌ 场景中没有找到 MultiPlatformRectData 组件", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"✅ 找到 {components.Length} 个 MultiPlatformRectData 组件", MessageType.Info);
            
            foreach (var component in components)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"对象: {component.gameObject.name}");
                EditorGUILayout.LabelField($"自动记录启用: {component.EnableAutoRecord}");
                EditorGUILayout.LabelField($"记录延迟: {component.AutoRecordDelay}秒");
                EditorGUILayout.LabelField($"位置覆盖启用: {component.EnablePositionOverride}");
                EditorGUILayout.LabelField($"尺寸覆盖启用: {component.EnableSizeOverride}");
                
                if (GUILayout.Button($"手动触发保存 - {component.gameObject.name}"))
                {
                    var rectTransform = component.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // 手动调用保存逻辑
                        var settings = component.GetSettingsForPlatform(platformManager.CurrentPlatform);
                        
                        if (component.EnablePositionOverride)
                        {
                            settings.overrideAnchoredPosition = true;
                            settings.anchoredPosition = rectTransform.anchoredPosition;
                        }
                        
                        if (component.EnableSizeOverride)
                        {
                            settings.overrideSizeDelta = true;
                            settings.sizeDelta = rectTransform.sizeDelta;
                        }
                        
                        component.SetSettingsForPlatform(platformManager.CurrentPlatform, settings);
                        EditorUtility.SetDirty(component);
                        Debug.Log($"[手动保存] {component.gameObject.name} -> {platformManager.CurrentPlatform}");
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("刷新状态"))
        {
            Repaint();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("使用说明:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. 确保场景中有PlatformManager实例\n" +
            "2. 确保UI对象有MultiPlatformRectData组件\n" +
            "3. 启用自动记录功能\n" +
            "4. 在Scene视图中拖动UI组件测试\n" +
            "5. 查看Console窗口的日志信息", 
            MessageType.Info);
    }
}