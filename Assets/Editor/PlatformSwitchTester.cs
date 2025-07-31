using UnityEngine;
using UnityEditor;

public class PlatformSwitchTester : EditorWindow
{
    [MenuItem("Tools/Platform Switch Tester")]
    public static void ShowWindow()
    {
        GetWindow<PlatformSwitchTester>("Platform Switch Tester");
    }

    private void OnGUI()
    {
        GUILayout.Label("Platform Switch Tester", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 显示当前平台
        if (PlatformManager.Instance != null)
        {
            GUILayout.Label($"Current Platform: {PlatformManager.Instance.CurrentPlatform}", EditorStyles.helpBox);
        }
        
        GUILayout.Space(10);

        // 平台切换按钮
        GUILayout.Label("Switch Platform:", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("PC"))
        {
            PlatformManager.Instance?.SetPlatform(Platform.PC);
        }
        if (GUILayout.Button("PS5"))
        {
            PlatformManager.Instance?.SetPlatform(Platform.PS5);
        }
        if (GUILayout.Button("Android"))
        {
            PlatformManager.Instance?.SetPlatform(Platform.Android);
        }
        if (GUILayout.Button("iOS"))
        {
            PlatformManager.Instance?.SetPlatform(Platform.iOS);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // 设置测试数据按钮
        GUILayout.Label("Setup Test Data:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup Different Platform Positions"))
        {
            SetupTestData();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Current Platform Settings"))
        {
            ApplyCurrentPlatformSettings();
        }

        GUILayout.Space(10);

        // 显示选中对象的MultiPlatformRectData信息
        if (Selection.activeGameObject != null)
        {
            var multiPlatform = Selection.activeGameObject.GetComponent<MultiPlatformRectData>();
            if (multiPlatform != null)
            {
                GUILayout.Label($"Selected: {Selection.activeGameObject.name}", EditorStyles.helpBox);
                
                // 显示当前平台的设置
                if (PlatformManager.Instance != null)
                {
                    var currentSettings = multiPlatform.GetSettingsForPlatform(PlatformManager.Instance.CurrentPlatform);
                    GUILayout.Label($"Position: {currentSettings.anchoredPosition}");
                    GUILayout.Label($"Size: {currentSettings.sizeDelta}");
                }
            }
        }
    }

    private void SetupTestData()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject with MultiPlatformRectData component", "OK");
            return;
        }

        var multiPlatform = Selection.activeGameObject.GetComponent<MultiPlatformRectData>();
        if (multiPlatform == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected GameObject doesn't have MultiPlatformRectData component", "OK");
            return;
        }

        // 确保启用位置覆盖
        multiPlatform.SetPositionOverride(true);

        // 为不同平台设置明显不同的位置
        var pcSettings = new PlatformRectSettings
        {
            overrideAnchoredPosition = true,
            anchoredPosition = new Vector2(0, 0) // PC: 中心位置
        };
        
        var ps5Settings = new PlatformRectSettings
        {
            overrideAnchoredPosition = true,
            anchoredPosition = new Vector2(-200, 100) // PS5: 左上
        };
        
        var androidSettings = new PlatformRectSettings
        {
            overrideAnchoredPosition = true,
            anchoredPosition = new Vector2(200, -100) // Android: 右下
        };
        
        var iosSettings = new PlatformRectSettings
        {
            overrideAnchoredPosition = true,
            anchoredPosition = new Vector2(-200, -100) // iOS: 左下
        };

        multiPlatform.SetSettingsForPlatform(Platform.PC, pcSettings);
        multiPlatform.SetSettingsForPlatform(Platform.PS5, ps5Settings);
        multiPlatform.SetSettingsForPlatform(Platform.Android, androidSettings);
        multiPlatform.SetSettingsForPlatform(Platform.iOS, iosSettings);

        EditorUtility.SetDirty(multiPlatform);
        
        Debug.Log("Test data setup complete! Different positions set for each platform.");
        EditorUtility.DisplayDialog("Success", "Test data setup complete!\n\nPC: (0, 0)\nPS5: (-200, 100)\nAndroid: (200, -100)\niOS: (-200, -100)", "OK");
    }

    private void ApplyCurrentPlatformSettings()
    {
        var multiPlatformComponents = FindObjectsOfType<MultiPlatformRectData>();
        foreach (var component in multiPlatformComponents)
        {
            component.ApplyCurrentPlatformSettings();
        }
        
        Debug.Log($"Applied current platform settings to {multiPlatformComponents.Length} components");
    }
}