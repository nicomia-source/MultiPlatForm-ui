using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Concrete observer example - Platform display
public class PlatformDisplay : MonoBehaviour, IPlatformObserver
{
    [SerializeField] private TextMeshProUGUI displayText;

    private void Start()
    {
        // Register as observer
        PlatformManager.Instance.AddObserver(this);
        
        // Display current platform
        OnPlatformChanged(PlatformManager.Instance.CurrentPlatform);
    }

    private void OnDestroy()
    {
        // Remove observer
        if (PlatformManager.Instance != null)
        {
            PlatformManager.Instance.RemoveObserver(this);
        }
    }

    public void OnPlatformChanged(Platform newPlatform)
    {
        if (displayText != null)
        {
            displayText.text = $"platform: {newPlatform}";
        }
    }
}