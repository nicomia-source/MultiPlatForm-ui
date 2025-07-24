using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(MultiPlatformRectData))]
public class MultiPlatformRectDataEditor : Editor
{
    private SerializedProperty platformDataProp;
    private SerializedProperty defaultSettingsProp;
    private SerializedProperty applyOnStartProp;
    private SerializedProperty applyOnPlatformChangeProp;
    
    private bool[] platformFoldouts = new bool[4];
    private string[] platformNames = { "PC", "PS5", "Android", "iOS" };

    void OnEnable()
    {
        platformDataProp = serializedObject.FindProperty("platformData");
        defaultSettingsProp = serializedObject.FindProperty("defaultSettings");
        applyOnStartProp = serializedObject.FindProperty("applyOnStart");
        applyOnPlatformChangeProp = serializedObject.FindProperty("applyOnPlatformChange");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        MultiPlatformRectData target = (MultiPlatformRectData)this.target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Multi-Platform RectTransform Data", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Current Platform Display and Switching
        EditorGUILayout.LabelField("Platform Control", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Get current platform
        Platform currentPlatform = Platform.PC;
        PlatformManager platformManager = Object.FindObjectOfType<PlatformManager>();
        if (platformManager != null)
        {
            currentPlatform = platformManager.CurrentPlatform;
            EditorGUILayout.LabelField($"当前平台: {currentPlatform}", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.HelpBox("未找到 PlatformManager，请在场景中添加", MessageType.Warning);
        }
        
        // Platform switching buttons
        EditorGUILayout.LabelField("切换平台:", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        
        for (int i = 0; i < platformNames.Length; i++)
        {
            Platform platform = (Platform)i;
            bool isCurrentPlatform = (platformManager != null && platform == currentPlatform);
            
            GUI.enabled = !isCurrentPlatform;
            if (GUILayout.Button(platformNames[i]))
            {
                if (platformManager != null)
                {
                    platformManager.SetPlatform(platform);
                    target.ApplyCurrentPlatformSettings();
                }
                else
                {
                    // Create PlatformManager if it doesn't exist
                    GameObject go = new GameObject("PlatformManager");
                    var manager = go.AddComponent<PlatformManager>();
                    manager.SetPlatform(platform);
                    target.ApplyCurrentPlatformSettings();
                }
            }
            GUI.enabled = true;
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        
        // Auto-Record Settings
        EditorGUILayout.LabelField("自动记录设置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 使用SerializedProperty来访问私有字段
        var enableAutoRecordProp = serializedObject.FindProperty("enableAutoRecord");
        var autoRecordDelayProp = serializedObject.FindProperty("autoRecordDelay");
        
        EditorGUILayout.PropertyField(enableAutoRecordProp, new GUIContent("启用自动记录"));
        EditorGUILayout.PropertyField(autoRecordDelayProp, new GUIContent("记录延迟(秒)"));
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        if (enableAutoRecordProp.boolValue)
        {
            EditorGUILayout.HelpBox("⭐ 自动记录已启用！\n• 在Scene视图中拖动UI组件时会自动保存到当前平台\n• 只保存已启用的属性（位置、尺寸、锚点等）\n• 停止拖动后会延迟保存，避免频繁操作", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        
        // Enabled Properties
        EditorGUILayout.LabelField("属性配置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Enabled Properties", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.BeginVertical();
        var enablePositionProp = serializedObject.FindProperty("enablePositionOverride");
        var enableAnchorsProp = serializedObject.FindProperty("enableAnchorsOverride");
        var enableRotationProp = serializedObject.FindProperty("enableRotationOverride");
        
        EditorGUILayout.PropertyField(enablePositionProp, new GUIContent("启用位置覆盖"));
        EditorGUILayout.PropertyField(enableAnchorsProp, new GUIContent("启用锚点覆盖"));
        EditorGUILayout.PropertyField(enableRotationProp, new GUIContent("启用旋转覆盖"));
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical();
        var enableSizeProp = serializedObject.FindProperty("enableSizeOverride");
        var enablePivotProp = serializedObject.FindProperty("enablePivotOverride");
        var enableScaleProp = serializedObject.FindProperty("enableScaleOverride");
        
        EditorGUILayout.PropertyField(enableSizeProp, new GUIContent("启用尺寸覆盖"));
        EditorGUILayout.PropertyField(enablePivotProp, new GUIContent("启用轴心覆盖"));
        EditorGUILayout.PropertyField(enableScaleProp, new GUIContent("启用缩放覆盖"));
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        
        // Runtime Options
        EditorGUILayout.LabelField("Runtime Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(applyOnStartProp, new GUIContent("Apply On Start"));
        EditorGUILayout.PropertyField(applyOnPlatformChangeProp, new GUIContent("Apply On Platform Change"));
        
        EditorGUILayout.Space();
        
        // Binary Storage Settings
        EditorGUILayout.LabelField("存储设置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Current storage mode
        bool currentBinaryMode = target.UseBinaryStorage;
        EditorGUILayout.LabelField("当前存储模式:", currentBinaryMode ? "二进制存储" : "传统存储");
        
        // Storage info
        EditorGUILayout.LabelField("存储信息:", target.GetStorageInfo());
        
        // Compression ratio
        if (currentBinaryMode)
        {
            float ratio = target.GetCompressionRatio();
            EditorGUILayout.LabelField("压缩比:", $"{ratio:F2}x");
        }
        
        EditorGUILayout.Space();
        
        // Switch storage mode
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(currentBinaryMode ? "切换到传统存储" : "切换到二进制存储"))
        {
            target.UseBinaryStorage = !currentBinaryMode;
            EditorUtility.SetDirty(target);
        }
        
        // Clear cache button
        if (currentBinaryMode && GUILayout.Button("清理缓存"))
        {
            target.ClearStorageCache();
        }
        EditorGUILayout.EndHorizontal();
        
        if (currentBinaryMode)
        {
            EditorGUILayout.HelpBox("⚡ 二进制存储模式已启用！\n• 显著减少内存占用\n• 提高读写性能\n• 数据以紧凑格式存储", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        
        // Quick Actions
        EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("捕获当前设置"))
        {
            target.CaptureCurrentSettings();
            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("应用当前平台"))
        {
            target.ApplyCurrentPlatformSettings();
        }
        
        if (GUILayout.Button("重置为原始"))
        {
            target.ResetToOriginal();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Binary Storage Test Actions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("存储测试", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("性能测试"))
        {
            var testComponent = target.GetComponent<BinaryStorageTest>();
            if (testComponent == null)
            {
                testComponent = target.gameObject.AddComponent<BinaryStorageTest>();
                testComponent.testTarget = target;
            }
            testComponent.RunPerformanceTest();
        }
        
        if (GUILayout.Button("数据完整性测试"))
        {
            var testComponent = target.GetComponent<BinaryStorageTest>();
            if (testComponent == null)
            {
                testComponent = target.gameObject.AddComponent<BinaryStorageTest>();
                testComponent.testTarget = target;
            }
            testComponent.ValidateDataIntegrity();
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        // Platform Settings
        EditorGUILayout.LabelField("Platform Override Settings", EditorStyles.boldLabel);
        
        for (int i = 0; i < platformNames.Length; i++)
        {
            Platform platform = (Platform)i;
            platformFoldouts[i] = EditorGUILayout.Foldout(platformFoldouts[i], 
                $"{platformNames[i]} Platform Settings", true);
            
            if (platformFoldouts[i])
            {
                EditorGUI.indentLevel++;
                DrawPlatformSettings(target, platform);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }
        
        // Default Settings
        EditorGUILayout.LabelField("Fallback Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(defaultSettingsProp, true);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawPlatformSettings(MultiPlatformRectData target, Platform platform)
    {
        var settings = target.GetSettingsForPlatform(platform);
        bool changed = false;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Position Settings
        EditorGUILayout.LabelField("Position", EditorStyles.boldLabel);
        bool newOverridePos = EditorGUILayout.Toggle("Override Anchored Position", settings.overrideAnchoredPosition);
        if (newOverridePos != settings.overrideAnchoredPosition)
        {
            settings.overrideAnchoredPosition = newOverridePos;
            changed = true;
        }
        
        if (settings.overrideAnchoredPosition)
        {
            Vector2 newPos = EditorGUILayout.Vector2Field("Anchored Position", settings.anchoredPosition);
            if (newPos != settings.anchoredPosition)
            {
                settings.anchoredPosition = newPos;
                changed = true;
            }
        }
        
        EditorGUILayout.Space();
        
        // Size Settings
        EditorGUILayout.LabelField("Size", EditorStyles.boldLabel);
        bool newOverrideSize = EditorGUILayout.Toggle("Override Size Delta", settings.overrideSizeDelta);
        if (newOverrideSize != settings.overrideSizeDelta)
        {
            settings.overrideSizeDelta = newOverrideSize;
            changed = true;
        }
        
        if (settings.overrideSizeDelta)
        {
            Vector2 newSize = EditorGUILayout.Vector2Field("Size Delta", settings.sizeDelta);
            if (newSize != settings.sizeDelta)
            {
                settings.sizeDelta = newSize;
                changed = true;
            }
        }
        
        EditorGUILayout.Space();
        
        // Anchor Settings
        EditorGUILayout.LabelField("Anchors", EditorStyles.boldLabel);
        bool newOverrideAnchors = EditorGUILayout.Toggle("Override Anchors", settings.overrideAnchors);
        if (newOverrideAnchors != settings.overrideAnchors)
        {
            settings.overrideAnchors = newOverrideAnchors;
            changed = true;
        }
        
        if (settings.overrideAnchors)
        {
            Vector2 newAnchorMin = EditorGUILayout.Vector2Field("Anchor Min", settings.anchorMin);
            Vector2 newAnchorMax = EditorGUILayout.Vector2Field("Anchor Max", settings.anchorMax);
            if (newAnchorMin != settings.anchorMin || newAnchorMax != settings.anchorMax)
            {
                settings.anchorMin = newAnchorMin;
                settings.anchorMax = newAnchorMax;
                changed = true;
            }
        }
        
        EditorGUILayout.Space();
        
        // Pivot Settings
        EditorGUILayout.LabelField("Pivot", EditorStyles.boldLabel);
        bool newOverridePivot = EditorGUILayout.Toggle("Override Pivot", settings.overridePivot);
        if (newOverridePivot != settings.overridePivot)
        {
            settings.overridePivot = newOverridePivot;
            changed = true;
        }
        
        if (settings.overridePivot)
        {
            Vector2 newPivot = EditorGUILayout.Vector2Field("Pivot", settings.pivot);
            if (newPivot != settings.pivot)
            {
                settings.pivot = newPivot;
                changed = true;
            }
        }
        
        EditorGUILayout.Space();
        
        // Rotation Settings
        EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
        bool newOverrideRotation = EditorGUILayout.Toggle("Override Rotation", settings.overrideRotation);
        if (newOverrideRotation != settings.overrideRotation)
        {
            settings.overrideRotation = newOverrideRotation;
            changed = true;
        }
        
        if (settings.overrideRotation)
        {
            Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", settings.rotation);
            if (newRotation != settings.rotation)
            {
                settings.rotation = newRotation;
                changed = true;
            }
        }
        
        EditorGUILayout.Space();
        
        // Scale Settings
        EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
        bool newOverrideScale = EditorGUILayout.Toggle("Override Scale", settings.overrideScale);
        if (newOverrideScale != settings.overrideScale)
        {
            settings.overrideScale = newOverrideScale;
            changed = true;
        }
        
        if (settings.overrideScale)
        {
            Vector3 newScale = EditorGUILayout.Vector3Field("Scale", settings.scale);
            if (newScale != settings.scale)
            {
                settings.scale = newScale;
                changed = true;
            }
        }
        
        EditorGUILayout.EndVertical();
        
        if (changed)
        {
            target.SetSettingsForPlatform(platform, settings);
            EditorUtility.SetDirty(target);
        }
    }
}