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
        
        // Runtime Options
        EditorGUILayout.LabelField("Runtime Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(applyOnStartProp, new GUIContent("Apply On Start"));
        EditorGUILayout.PropertyField(applyOnPlatformChangeProp, new GUIContent("Apply On Platform Change"));
        
        EditorGUILayout.Space();
        
        // Quick Actions
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Capture Current Settings"))
        {
            target.CaptureCurrentSettings();
            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("Apply Current Platform"))
        {
            target.ApplyCurrentPlatformSettings();
        }
        
        if (GUILayout.Button("Reset to Original"))
        {
            target.ResetToOriginal();
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