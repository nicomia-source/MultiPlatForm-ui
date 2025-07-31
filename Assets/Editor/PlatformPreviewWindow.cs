using UnityEngine;
using UnityEditor;
using System.Linq;

public class PlatformPreviewWindow : EditorWindow
{
    private Platform selectedPlatform = Platform.PC;
    private bool autoApplyChanges = true;
    private bool showDebugInfo = false;
    private Vector2 scrollPosition;
    
    private MultiPlatformRectData[] allComponents;
    private int componentCount = 0;
    
    [MenuItem("Window/Platform Preview")]
    public static void ShowWindow()
    {
        GetWindow<PlatformPreviewWindow>("Platform Preview");
    }
    
    private void OnEnable()
    {
        RefreshComponentList();
        // 订阅Selection变化事件
        Selection.selectionChanged += OnSelectionChanged;
    }
    
    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }
    
    private void OnSelectionChanged()
    {
        RefreshComponentList();
        Repaint();
    }
    
    private void RefreshComponentList()
    {
        allComponents = FindObjectsOfType<MultiPlatformRectData>();
        componentCount = allComponents?.Length ?? 0;
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        // 标题
        EditorGUILayout.LabelField("🎮 平台预览工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 当前平台显示
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("当前平台:", GUILayout.Width(60));
        var currentPlatform = GetCurrentPlatform();
        EditorGUILayout.LabelField(currentPlatform?.ToString() ?? "Unknown", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 平台选择器
        EditorGUILayout.LabelField("选择预览平台:", EditorStyles.boldLabel);
        Platform newPlatform = (Platform)EditorGUILayout.EnumPopup(selectedPlatform);
        
        if (newPlatform != selectedPlatform)
        {
            selectedPlatform = newPlatform;
            if (autoApplyChanges)
            {
                ApplyPlatformPreview();
            }
        }
        
        EditorGUILayout.Space();
        
        // 快速切换按钮
        EditorGUILayout.LabelField("快速切换:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        // 使用颜色标识当前平台
        var originalColor = GUI.backgroundColor;
        
        GUI.backgroundColor = (currentPlatform == Platform.PC) ? Color.green : Color.white;
        if (GUILayout.Button("PC", EditorStyles.miniButtonLeft))
        {
            selectedPlatform = Platform.PC;
            ApplyPlatformPreview();
        }
        
        GUI.backgroundColor = (currentPlatform == Platform.PS5) ? Color.green : Color.white;
        if (GUILayout.Button("PS5", EditorStyles.miniButtonMid))
        {
            selectedPlatform = Platform.PS5;
            ApplyPlatformPreview();
        }
        
        GUI.backgroundColor = (currentPlatform == Platform.Android) ? Color.green : Color.white;
        if (GUILayout.Button("Android", EditorStyles.miniButtonMid))
        {
            selectedPlatform = Platform.Android;
            ApplyPlatformPreview();
        }
        
        GUI.backgroundColor = (currentPlatform == Platform.iOS) ? Color.green : Color.white;
        if (GUILayout.Button("iOS", EditorStyles.miniButtonRight))
        {
            selectedPlatform = Platform.iOS;
            ApplyPlatformPreview();
        }
        
        GUI.backgroundColor = originalColor;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 设置选项
        EditorGUILayout.LabelField("设置:", EditorStyles.boldLabel);
        autoApplyChanges = EditorGUILayout.Toggle("自动应用变更", autoApplyChanges);
        showDebugInfo = EditorGUILayout.Toggle("显示调试信息", showDebugInfo);
        
        EditorGUILayout.Space();
        
        // 手动应用按钮
        if (!autoApplyChanges)
        {
            if (GUILayout.Button("应用平台预览", GUILayout.Height(30)))
            {
                ApplyPlatformPreview();
            }
            EditorGUILayout.Space();
        }
        
        // 组件信息
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"📊 场景统计", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"MultiPlatformRectData 组件数量: {componentCount}");
        
        if (GUILayout.Button("刷新组件列表"))
        {
            RefreshComponentList();
        }
        EditorGUILayout.EndVertical();
        
        // 调试信息
        if (showDebugInfo && componentCount > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("🔍 组件详情:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox);
            
            foreach (var component in allComponents)
            {
                if (component != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(component.gameObject.name, GUILayout.Width(150));
                    
                    if (GUILayout.Button("选择", GUILayout.Width(50)))
                    {
                        Selection.activeGameObject = component.gameObject;
                    }
                    
                    if (GUILayout.Button("应用", GUILayout.Width(50)))
                    {
                        component.ApplyCurrentPlatformSettings();
                        EditorUtility.SetDirty(component);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void ApplyPlatformPreview()
    {
        // 设置平台
        var platformManager = FindObjectOfType<PlatformManager>();
        if (platformManager != null)
        {
            platformManager.SetPlatform(selectedPlatform);
        }
        
        // 刷新所有组件
        RefreshComponentList();
        
        // 应用设置到所有组件
        foreach (var component in allComponents)
        {
            if (component != null)
            {
                // 确保组件有RectTransform
                var rectTransform = component.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    Debug.LogWarning($"[{component.gameObject.name}] MultiPlatformRectData component found on non-UI object. Skipping.");
                    continue;
                }
                
                component.ApplyCurrentPlatformSettings();
                EditorUtility.SetDirty(component);
                EditorUtility.SetDirty(component.gameObject);
            }
        }
        
        // 强制刷新Scene视图
        SceneView.RepaintAll();
        
        Debug.Log($"Applied platform preview: {selectedPlatform} to {componentCount} components");
    }
    
    private Platform? GetCurrentPlatform()
    {
        var platformManager = FindObjectOfType<PlatformManager>();
        return platformManager?.CurrentPlatform;
    }
}