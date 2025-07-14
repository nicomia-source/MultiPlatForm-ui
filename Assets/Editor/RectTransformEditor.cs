using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(RectTransform), true)]
public class RectTransformEditor : Editor
{
    private Editor editorInstance;
    private Type nativeEditor;
    private MethodInfo onSceneGui;
    private MethodInfo onValidate;

    public override void OnInspectorGUI()
    {
        editorInstance.OnInspectorGUI();
        if (GUILayout.Button("添加多平台数据"))
        {
            RectTransform rect = (RectTransform)target;
            rect.gameObject.AddComponent<MultiPlatformRectData>();
            Debug.Log("已添加多平台Rect数据组件");
        }
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