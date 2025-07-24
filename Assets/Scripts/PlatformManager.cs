using System.Collections.Generic;
using UnityEngine;

// Platform enumeration
public enum Platform
{
    PC,
    PS5,
    Android,
    iOS
}

// Observer interface
public interface IPlatformObserver
{
    void OnPlatformChanged(Platform newPlatform);
}

// Platform manager (subject)
public class PlatformManager : MonoBehaviour
{
    private static PlatformManager instance;
    public static PlatformManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlatformManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PlatformManager");
                    instance = go.AddComponent<PlatformManager>();
                }
            }
            return instance;
        }
    }

    private Platform currentPlatform = Platform.PC;
    private List<IPlatformObserver> observers = new List<IPlatformObserver>();

    public Platform CurrentPlatform => currentPlatform;

    // Add observer
    public void AddObserver(IPlatformObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    // Remove observer
    public void RemoveObserver(IPlatformObserver observer)
    {
        observers.Remove(observer);
    }

    // Notify all observers
    private void NotifyObservers(Platform newPlatform)
    {
        foreach (var observer in observers)
        {
            observer.OnPlatformChanged(newPlatform);
        }
    }

    // Set platform
    public void SetPlatform(Platform platform)
    {
        if (currentPlatform != platform)
        {
            currentPlatform = platform;
            NotifyObservers(currentPlatform);
            Debug.Log($"platform: {currentPlatform}");
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // 只在运行时使用DontDestroyOnLoad，避免编辑器模式下的清理问题
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    // 在编辑器模式下，当场景切换时清理实例
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
#endif
}