using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(RectTransform), true)]
public class RectTransformEditor : Editor
{
    private Editor editorInstance;
    private Type nativeEditor;
    private MethodInfo onSceneGui;
    private MethodInfo onValidate;
    
    private bool showMultiPlatformData = false;
    private bool[] platformFoldouts = new bool[4];
    private string[] platformNames = { "PC", "PS5", "Android", "iOS" };
    
    // 缓存变量以提高性能
    private SerializedObject cachedSerializedMultiPlatform;
    private MultiPlatformRectData lastMultiPlatformData;
    
    // 安全访问PlatformManager的方法
    private static PlatformManager GetPlatformManagerInstance()
    {
        // 直接查找PlatformManager实例
        return UnityEngine.Object.FindObjectOfType<PlatformManager>();
    }
    
    private static Platform? GetCurrentPlatform()
    {
        var platformManagerInstance = GetPlatformManagerInstance();
        return platformManagerInstance?.CurrentPlatform;
    }
    
    private static void SetPlatform(Platform platform)
    {
        var platformManagerInstance = GetPlatformManagerInstance();
        platformManagerInstance?.SetPlatform(platform);
    }
    
    // 静态方法：获取当前平台名称
    public static string GetCurrentPlatformName()
    {
        try
        {
            var currentPlatform = GetCurrentPlatform();
            return currentPlatform?.ToString() ?? "PC"; // 默认值
        }
        catch (System.Exception)
        {
            return "PC"; // 发生异常时返回默认值
        }
    }

    public override void OnInspectorGUI()
    {
        // 绘制原生的RectTransform编辑器
        editorInstance.OnInspectorGUI();
        
        // 重置GUI状态，确保没有冲突
        GUI.enabled = true;
        EditorGUI.indentLevel = 0;
        
        RectTransform rect = (RectTransform)target;
        MultiPlatformRectData multiPlatformData = rect.GetComponent<MultiPlatformRectData>();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Multi-Platform Settings", EditorStyles.boldLabel);
        

        
        // Display current platform and platform switching buttons
        var platformManagerInstance = GetPlatformManagerInstance();
        if (platformManagerInstance != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🎮 平台切换", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前平台:", GUILayout.Width(60));
            var currentPlatform = GetCurrentPlatform();
            EditorGUILayout.LabelField(currentPlatform?.ToString() ?? "Unknown", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(3);
            
            // Platform switching buttons with keyboard shortcuts
            EditorGUILayout.LabelField("快速切换 (快捷键: Ctrl+1/2/3/4):", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            
            // Handle keyboard shortcuts
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.control)
            {
                Platform? targetPlatform = null;
                switch (e.keyCode)
                {
                    case KeyCode.Alpha1:
                        targetPlatform = Platform.PC;
                        break;
                    case KeyCode.Alpha2:
                        targetPlatform = Platform.PS5;
                        break;
                    case KeyCode.Alpha3:
                        targetPlatform = Platform.Android;
                        break;
                    case KeyCode.Alpha4:
                        targetPlatform = Platform.iOS;
                        break;
                }
                
                if (targetPlatform.HasValue)
                {
                    SetPlatform(targetPlatform.Value);
                    if (multiPlatformData != null)
                    {
                        multiPlatformData.ApplyCurrentPlatformSettings();
                    }
                    e.Use();
                    Repaint();
                }
            }
            
            // PC Button (Ctrl+1)
            var currentPlatformValue = GetCurrentPlatform();
            GUI.backgroundColor = (currentPlatformValue == Platform.PC) ? Color.green : Color.white;
            if (GUILayout.Button("PC (1)", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
            {
                SetPlatform(Platform.PC);
                // 如果有MultiPlatformRectData组件，自动应用设置
                if (multiPlatformData != null)
                {
                    multiPlatformData.ApplyCurrentPlatformSettings();
                }
                Repaint();
            }
            
            // PS5 Button (Ctrl+2)
            GUI.backgroundColor = (currentPlatformValue == Platform.PS5) ? Color.green : Color.white;
            if (GUILayout.Button("PS5 (2)", EditorStyles.miniButtonMid, GUILayout.Height(20)))
            {
                SetPlatform(Platform.PS5);
                // 如果有MultiPlatformRectData组件，自动应用设置
                if (multiPlatformData != null)
                {
                    multiPlatformData.ApplyCurrentPlatformSettings();
                }
                Repaint();
            }
            
            // Android Button (Ctrl+3)
            GUI.backgroundColor = (currentPlatformValue == Platform.Android) ? Color.green : Color.white;
            if (GUILayout.Button("Android (3)", EditorStyles.miniButtonMid, GUILayout.Height(20)))
            {
                SetPlatform(Platform.Android);
                // 如果有MultiPlatformRectData组件，自动应用设置
                if (multiPlatformData != null)
                {
                    multiPlatformData.ApplyCurrentPlatformSettings();
                }
                Repaint();
            }
            
            // iOS Button (Ctrl+4)
            GUI.backgroundColor = (currentPlatformValue == Platform.iOS) ? Color.green : Color.white;
            if (GUILayout.Button("iOS (4)", EditorStyles.miniButtonRight, GUILayout.Height(20)))
            {
                SetPlatform(Platform.iOS);
                // 如果有MultiPlatformRectData组件，自动应用设置
                if (multiPlatformData != null)
                {
                    multiPlatformData.ApplyCurrentPlatformSettings();
                }
                Repaint();
            }
            
            // Reset background color
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            // 添加额外的实用按钮
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            
            // 保存当前设置到当前平台按钮
            if (multiPlatformData != null && GUILayout.Button("保存当前设置", EditorStyles.miniButtonLeft))
            {
                multiPlatformData.SaveCurrentSettings();
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            // 应用当前平台设置按钮
            if (multiPlatformData != null && GUILayout.Button("应用平台设置", EditorStyles.miniButtonRight))
            {
                multiPlatformData.ApplyCurrentPlatformSettings();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 添加提示信息
            if (multiPlatformData != null)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox("点击平台按钮会自动切换平台并应用对应的UI设置\n使用快捷键 Ctrl+1/2/3/4 快速切换平台", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox("添加MultiPlatformRectData组件后，切换平台时会自动应用UI设置\n使用快捷键 Ctrl+1/2/3/4 快速切换平台", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
        }
        
        // 自动记录功能控制界面
        if (multiPlatformData != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔄 自动记录设置", EditorStyles.boldLabel);
            
            SerializedObject serializedMultiPlatform = new SerializedObject(multiPlatformData);
            SerializedProperty enableAutoRecordProp = serializedMultiPlatform.FindProperty("enableAutoRecord");
            SerializedProperty autoRecordDelayProp = serializedMultiPlatform.FindProperty("autoRecordDelay");
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(enableAutoRecordProp, new GUIContent("启用自动记录", "在Scene视图中拖动UI组件时自动保存到当前平台"));
            
            if (enableAutoRecordProp.boolValue)
            {
                EditorGUILayout.PropertyField(autoRecordDelayProp, new GUIContent("记录延迟(秒)", "停止拖动后多久自动保存，避免频繁保存"));
                
                EditorGUILayout.Space(3);
                EditorGUILayout.HelpBox("✨ 自动记录已启用！\n" +
                    "• 在Scene视图中拖动UI组件时会自动保存到当前平台\n" +
                    "• 只保存已启用的属性（位置、尺寸、锚点等）\n" +
                    "• 停止拖动后会延迟保存，避免频繁操作", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("自动记录已禁用。启用后，在Scene视图中拖动UI组件时会自动保存到当前平台。", MessageType.None);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedMultiPlatform.ApplyModifiedProperties();
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        if (multiPlatformData == null)
        {
            EditorGUILayout.HelpBox("No multi-platform data component found. Add one to enable platform-specific settings.", MessageType.Info);
            if (GUILayout.Button("Add Multi-Platform Data"))
            {
                multiPlatformData = rect.gameObject.AddComponent<MultiPlatformRectData>();
            }
        }
        else
        {
            DrawMultiPlatformDataInline(multiPlatformData);
        }
    }
    
    private void DrawMultiPlatformDataInline(MultiPlatformRectData multiPlatformData)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Property Configuration Section
        EditorGUILayout.LabelField("属性配置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 使用缓存的SerializedObject以提高性能
        if (cachedSerializedMultiPlatform == null || lastMultiPlatformData != multiPlatformData)
        {
            cachedSerializedMultiPlatform = new SerializedObject(multiPlatformData);
            lastMultiPlatformData = multiPlatformData;
        }
        
        // 确保SerializedObject是最新的
        cachedSerializedMultiPlatform.Update();
        
        // 使用固定宽度的标签，确保对齐
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 100;
        
        // 第一行：位置和尺寸
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(cachedSerializedMultiPlatform.FindProperty("enablePositionOverride"), new GUIContent("启用位置覆盖"), GUILayout.MinWidth(120));
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(cachedSerializedMultiPlatform.FindProperty("enableSizeOverride"), new GUIContent("启用尺寸覆盖"), GUILayout.MinWidth(120));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(2);
        
        // 第二行：锚点和轴心
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(cachedSerializedMultiPlatform.FindProperty("enableAnchorsOverride"), new GUIContent("启用锚点覆盖"), GUILayout.MinWidth(120));
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(cachedSerializedMultiPlatform.FindProperty("enablePivotOverride"), new GUIContent("启用轴心覆盖"), GUILayout.MinWidth(120));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(2);
        
        // 第三行：旋转和缩放
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(cachedSerializedMultiPlatform.FindProperty("enableRotationOverride"), new GUIContent("启用旋转覆盖"), GUILayout.MinWidth(120));
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(cachedSerializedMultiPlatform.FindProperty("enableScaleOverride"), new GUIContent("启用缩放覆盖"), GUILayout.MinWidth(120));
        EditorGUILayout.EndHorizontal();
        
        // 恢复原始标签宽度
        EditorGUIUtility.labelWidth = labelWidth;
        
        // 配置预设按钮
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("配置预设", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("UI元素", EditorStyles.miniButtonLeft, GUILayout.Height(18)))
        {
            Undo.RecordObject(multiPlatformData, "Apply UI Element Preset");
            multiPlatformData.ApplyUIElementPreset();
            EditorUtility.SetDirty(multiPlatformData);
            // 强制更新缓存
            cachedSerializedMultiPlatform = null;
            lastMultiPlatformData = null;
            // 强制重绘
            Repaint();
        }
        
        if (GUILayout.Button("动画元素", EditorStyles.miniButtonMid, GUILayout.Height(18)))
        {
            Undo.RecordObject(multiPlatformData, "Apply Animation Element Preset");
            multiPlatformData.ApplyAnimationElementPreset();
            EditorUtility.SetDirty(multiPlatformData);
            // 强制更新缓存
            cachedSerializedMultiPlatform = null;
            lastMultiPlatformData = null;
            // 强制重绘
            Repaint();
        }
        
        if (GUILayout.Button("完全控制", EditorStyles.miniButtonRight, GUILayout.Height(18)))
        {
            Undo.RecordObject(multiPlatformData, "Apply Full Control Preset");
            multiPlatformData.ApplyFullControlPreset();
            EditorUtility.SetDirty(multiPlatformData);
            // 强制更新缓存
            cachedSerializedMultiPlatform = null;
            lastMultiPlatformData = null;
            // 强制重绘
            Repaint();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 应用修改并强制重绘
        if (cachedSerializedMultiPlatform.ApplyModifiedProperties())
        {
            // 当属性发生变化时，强制清除缓存并重绘
            cachedSerializedMultiPlatform = null;
            lastMultiPlatformData = null;
            EditorUtility.SetDirty(multiPlatformData);
            Repaint();
            // 延迟重绘以确保UI更新
            EditorApplication.delayCall += () => {
                if (this != null) 
                {
                    cachedSerializedMultiPlatform = null;
                    lastMultiPlatformData = null;
                    Repaint();
                }
            };
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Binary Storage Settings
        EditorGUILayout.LabelField("💾 存储设置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 显示当前存储模式
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("存储模式:", GUILayout.Width(70));
        string storageMode = multiPlatformData.UseBinaryStorage ? "二进制存储" : "传统存储";
        Color originalColor = GUI.color;
        GUI.color = multiPlatformData.UseBinaryStorage ? Color.cyan : Color.white;
        EditorGUILayout.LabelField(storageMode, EditorStyles.boldLabel);
        GUI.color = originalColor;
        EditorGUILayout.EndHorizontal();
        
        // 显示存储信息
        if (multiPlatformData.UseBinaryStorage)
        {
            string storageInfo = multiPlatformData.GetStorageInfo();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("存储信息:", GUILayout.Width(70));
            EditorGUILayout.LabelField(storageInfo, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("压缩比:", GUILayout.Width(70));
            float compressionRatio = multiPlatformData.GetCompressionRatio();
            string compressionText = compressionRatio > 0 ? $"{compressionRatio:F1}x" : "N/A";
            EditorGUILayout.LabelField(compressionText, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            string storageInfo = multiPlatformData.GetStorageInfo();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("存储信息:", GUILayout.Width(70));
            EditorGUILayout.LabelField(storageInfo, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(3);
        
        // 存储操作按钮
        EditorGUILayout.BeginHorizontal();
        
        // 切换存储模式按钮
        string toggleButtonText = multiPlatformData.UseBinaryStorage ? "切换到传统存储" : "切换到二进制存储";
        if (GUILayout.Button(toggleButtonText, EditorStyles.miniButtonLeft))
        {
            Undo.RecordObject(multiPlatformData, "Toggle Storage Mode");
            multiPlatformData.UseBinaryStorage = !multiPlatformData.UseBinaryStorage;
            EditorUtility.SetDirty(multiPlatformData);
        }
        
        // 清理缓存按钮（仅在二进制模式下显示）
        if (multiPlatformData.UseBinaryStorage)
        {
            if (GUILayout.Button("清理缓存", EditorStyles.miniButtonRight))
            {
                multiPlatformData.ClearStorageCache();
                EditorUtility.SetDirty(multiPlatformData);
            }
        }
        else
        {
            // 占位按钮保持布局一致
            GUI.enabled = false;
            GUILayout.Button("清理缓存", EditorStyles.miniButtonRight);
            GUI.enabled = true;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 存储模式提示信息
        if (multiPlatformData.UseBinaryStorage)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.HelpBox("✨ 二进制存储已启用！\n" +
                "• 数据以压缩的二进制格式存储\n" +
                "• 减少内存占用和序列化大小\n" +
                "• 提高读写性能", MessageType.Info);
        }
        else
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.HelpBox("传统存储模式使用Unity的标准序列化。\n" +
                "切换到二进制存储可以获得更好的性能和更小的存储空间。", MessageType.None);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Quick Actions
        EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        // 使用更好的按钮样式和间距
        if (GUILayout.Button("捕获当前设置", EditorStyles.miniButtonLeft, GUILayout.Height(22)))
        {
            multiPlatformData.CaptureCurrentSettings();
            EditorUtility.SetDirty(multiPlatformData);
        }
        
        if (GUILayout.Button("应用当前平台", EditorStyles.miniButtonMid, GUILayout.Height(22)))
        {
            multiPlatformData.ApplyCurrentPlatformSettings();
        }
        
        if (GUILayout.Button("重置为原始", EditorStyles.miniButtonRight, GUILayout.Height(22)))
        {
            multiPlatformData.ResetToOriginal();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Storage Testing
        EditorGUILayout.LabelField("🧪 存储测试", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        // 性能测试按钮
        if (GUILayout.Button("性能测试", EditorStyles.miniButtonLeft, GUILayout.Height(22)))
        {
            // 获取或添加BinaryStorageTest组件
            BinaryStorageTest testComponent = multiPlatformData.GetComponent<BinaryStorageTest>();
            if (testComponent == null)
            {
                testComponent = multiPlatformData.gameObject.AddComponent<BinaryStorageTest>();
            }
            
            // 执行性能测试
            testComponent.RunPerformanceTest();
        }
        
        // 数据完整性测试按钮
        if (GUILayout.Button("完整性测试", EditorStyles.miniButtonRight, GUILayout.Height(22)))
        {
            // 获取或添加BinaryStorageTest组件
            BinaryStorageTest testComponent = multiPlatformData.GetComponent<BinaryStorageTest>();
            if (testComponent == null)
            {
                testComponent = multiPlatformData.gameObject.AddComponent<BinaryStorageTest>();
            }
            
            // 执行数据完整性测试
            testComponent.ValidateDataIntegrity();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Platform settings foldout
        showMultiPlatformData = EditorGUILayout.Foldout(showMultiPlatformData, "平台覆盖设置", true, EditorStyles.foldoutHeader);
        
        if (showMultiPlatformData)
        {
            EditorGUI.indentLevel++;
            
            // 提示信息
            EditorGUILayout.HelpBox("勾选复选框来启用对应平台的属性覆盖。只有在上方启用的属性才会显示在这里。", MessageType.Info);
            
            for (int i = 0; i < platformNames.Length; i++)
            {
                Platform platform = (Platform)i;
                
                // 确保平台数据存在，如果不存在则创建
                var settings = multiPlatformData.GetSettingsForPlatform(platform);
                if (settings == null)
                {
                    // 如果获取失败，尝试创建新的设置
                    settings = new PlatformRectSettings();
                    multiPlatformData.SetSettingsForPlatform(platform, settings);
                    EditorUtility.SetDirty(multiPlatformData);
                }
                
                // Platform header with current indicator
                EditorGUILayout.BeginHorizontal();
                
                // 构建平台名称，包含当前平台标识
                string platformDisplayName = platformNames[i];
                var currentPlatform = GetCurrentPlatform();
                if (currentPlatform == platform)
                {
                    platformDisplayName += " (当前平台)";
                }
                
                platformFoldouts[i] = EditorGUILayout.Foldout(platformFoldouts[i], platformDisplayName, true);
                EditorGUILayout.EndHorizontal();
                
                if (platformFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    DrawCompactPlatformSettings(multiPlatformData, platform, settings);
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawCompactPlatformSettings(MultiPlatformRectData multiPlatformData, Platform platform, PlatformRectSettings settings)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (settings == null)
        {
            EditorGUILayout.HelpBox("Settings is null for platform: " + platform, MessageType.Error);
            EditorGUILayout.EndVertical();
            return;
        }
        
        RectTransform rectTransform = (target as RectTransform);
        
        // 确保GUI状态正确
        GUI.enabled = true;
        
        // 确保我们有最新的属性状态
        if (cachedSerializedMultiPlatform == null || lastMultiPlatformData != multiPlatformData)
        {
            cachedSerializedMultiPlatform = new SerializedObject(multiPlatformData);
            lastMultiPlatformData = multiPlatformData;
        }
        cachedSerializedMultiPlatform.Update();
        
        // 获取当前启用的属性状态
        bool enablePositionOverride = cachedSerializedMultiPlatform.FindProperty("enablePositionOverride").boolValue;
        bool enableSizeOverride = cachedSerializedMultiPlatform.FindProperty("enableSizeOverride").boolValue;
        bool enableAnchorsOverride = cachedSerializedMultiPlatform.FindProperty("enableAnchorsOverride").boolValue;
        bool enablePivotOverride = cachedSerializedMultiPlatform.FindProperty("enablePivotOverride").boolValue;
        bool enableRotationOverride = cachedSerializedMultiPlatform.FindProperty("enableRotationOverride").boolValue;
        bool enableScaleOverride = cachedSerializedMultiPlatform.FindProperty("enableScaleOverride").boolValue;
        
        // Position - only show if enabled
        if (enablePositionOverride)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position", GUILayout.Width(60));
            
            // 使用EditorGUI.BeginChangeCheck来确保变更检测正确
            EditorGUI.BeginChangeCheck();
            bool newOverridePos = EditorGUILayout.Toggle("", settings.overrideAnchoredPosition, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Toggle Position Override");
                settings.overrideAnchoredPosition = newOverridePos;
                if (newOverridePos && rectTransform != null)
                {
                    settings.anchoredPosition = rectTransform.anchoredPosition;
                }
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUI.BeginDisabledGroup(!settings.overrideAnchoredPosition);
            EditorGUI.BeginChangeCheck();
            Vector2 newPos = EditorGUILayout.Vector2Field("", settings.anchoredPosition);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Change Position");
                settings.anchoredPosition = newPos;
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        // Size - only show if enabled
        if (enableSizeOverride)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size", GUILayout.Width(60));
            
            EditorGUI.BeginChangeCheck();
            bool newOverrideSize = EditorGUILayout.Toggle("", settings.overrideSizeDelta, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Toggle Size Override");
                settings.overrideSizeDelta = newOverrideSize;
                if (newOverrideSize && rectTransform != null)
                {
                    settings.sizeDelta = rectTransform.sizeDelta;
                }
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUI.BeginDisabledGroup(!settings.overrideSizeDelta);
            EditorGUI.BeginChangeCheck();
            Vector2 newSize = EditorGUILayout.Vector2Field("", settings.sizeDelta);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Change Size");
                settings.sizeDelta = newSize;
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        // Anchors - only show if enabled
        if (enableAnchorsOverride)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anchors", GUILayout.Width(60));
            
            EditorGUI.BeginChangeCheck();
            bool newOverrideAnchors = EditorGUILayout.Toggle("", settings.overrideAnchors, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Toggle Anchors Override");
                settings.overrideAnchors = newOverrideAnchors;
                if (newOverrideAnchors && rectTransform != null)
                {
                    settings.anchorMin = rectTransform.anchorMin;
                    settings.anchorMax = rectTransform.anchorMax;
                }
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUI.BeginDisabledGroup(!settings.overrideAnchors);
            EditorGUILayout.BeginVertical();
            
            EditorGUI.BeginChangeCheck();
            Vector2 newAnchorMin = EditorGUILayout.Vector2Field("Min", settings.anchorMin);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Change Anchor Min");
                settings.anchorMin = newAnchorMin;
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUI.BeginChangeCheck();
            Vector2 newAnchorMax = EditorGUILayout.Vector2Field("Max", settings.anchorMax);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Change Anchor Max");
                settings.anchorMax = newAnchorMax;
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        // Pivot - only show if enabled
        if (enablePivotOverride)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pivot", GUILayout.Width(60));
            
            EditorGUI.BeginChangeCheck();
            bool newOverridePivot = EditorGUILayout.Toggle("", settings.overridePivot, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Toggle Pivot Override");
                settings.overridePivot = newOverridePivot;
                if (newOverridePivot && rectTransform != null)
                {
                    settings.pivot = rectTransform.pivot;
                }
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUI.BeginDisabledGroup(!settings.overridePivot);
            EditorGUI.BeginChangeCheck();
            Vector2 newPivot = EditorGUILayout.Vector2Field("", settings.pivot);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Change Pivot");
                settings.pivot = newPivot;
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        // Rotation - only show if enabled
        if (enableRotationOverride)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rotation", GUILayout.Width(60));
            
            EditorGUI.BeginChangeCheck();
            bool newOverrideRotation = EditorGUILayout.Toggle("", settings.overrideRotation, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Toggle Rotation Override");
                settings.overrideRotation = newOverrideRotation;
                if (newOverrideRotation && rectTransform != null)
                {
                    settings.rotation = rectTransform.eulerAngles;
                }
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUI.BeginDisabledGroup(!settings.overrideRotation);
            EditorGUI.BeginChangeCheck();
            Vector3 newRotation = EditorGUILayout.Vector3Field("", settings.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Change Rotation");
                settings.rotation = newRotation;
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        // Scale - only show if enabled
        if (enableScaleOverride)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scale", GUILayout.Width(60));
            
            EditorGUI.BeginChangeCheck();
            bool newOverrideScale = EditorGUILayout.Toggle("", settings.overrideScale, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Toggle Scale Override");
                settings.overrideScale = newOverrideScale;
                if (newOverrideScale && rectTransform != null)
                {
                    settings.scale = rectTransform.localScale;
                }
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            
            EditorGUI.BeginDisabledGroup(!settings.overrideScale);
            EditorGUI.BeginChangeCheck();
            Vector3 newScale = EditorGUILayout.Vector3Field("", settings.scale);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(multiPlatformData, "Change Scale");
                settings.scale = newScale;
                multiPlatformData.SetSettingsForPlatform(platform, settings);
                EditorUtility.SetDirty(multiPlatformData);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }

    private void OnEnable()
    {
        if (nativeEditor == null)
            Initialize();

        nativeEditor.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(editorInstance, null);
        onSceneGui = nativeEditor.GetMethod("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        onValidate = nativeEditor.GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private void OnSceneGUI()
    {
        onSceneGui?.Invoke(editorInstance, null);
    }

    private void OnDisable()
    {
        // 清理缓存
        if (cachedSerializedMultiPlatform != null)
        {
            cachedSerializedMultiPlatform.Dispose();
            cachedSerializedMultiPlatform = null;
        }
        lastMultiPlatformData = null;
        
        nativeEditor.GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(editorInstance, null);
    }

    private void Awake()
    {
        Initialize();
        nativeEditor.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(editorInstance, null);
    }

    private void Initialize()
    {
        nativeEditor = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.RectTransformEditor");
        editorInstance = CreateEditor(target, nativeEditor);
    }

    private void OnDestroy()
    {
        nativeEditor.GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(editorInstance, null);
    }

    private void OnValidate()
    {
        if (nativeEditor == null)
            Initialize();

        onValidate?.Invoke(editorInstance, null);
    }

    private void Reset()
    {
        if (nativeEditor == null)
            Initialize();

        var method = nativeEditor.GetMethod("Reset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(editorInstance, null);
        }
    }
}