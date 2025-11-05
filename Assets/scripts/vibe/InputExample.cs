using UnityEngine;

/// <summary>
/// Contoh setup script untuk menunjukkan cara menggunakan PlayerInputManager
/// dengan PlayerController untuk prioritas input gamepad vs mouse
/// </summary>
public class InputExample : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInputManager inputManager;
    
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Text inputDeviceText;
    [SerializeField] private UnityEngine.UI.Text instructionText;
    
    private void Start()
    {
        // Auto-assign components
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
            
        if (inputManager == null)
            inputManager = FindObjectOfType<PlayerInputManager>();
        
        // Subscribe to input device change events
        if (inputManager != null)
        {
            inputManager.OnInputDeviceChanged += OnInputDeviceChanged;
        }
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inputManager != null)
        {
            inputManager.OnInputDeviceChanged -= OnInputDeviceChanged;
        }
    }
    
    private void OnInputDeviceChanged(bool isGamepadActive)
    {
        Debug.Log($"Input device changed to: {(isGamepadActive ? "Gamepad" : "Mouse")}");
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (inputManager == null) return;
        
        if (inputDeviceText != null)
        {
            inputDeviceText.text = $"Active Input: {inputManager.CurrentInputDevice}";
        }
        
        if (instructionText != null)
        {
            if (inputManager.IsGamepadActive)
            {
                instructionText.text = "Use LEFT STICK to move, R1 to boost";
            }
            else
            {
                instructionText.text = "Use MOUSE to aim, LEFT CLICK to boost";
            }
        }
    }
    
    private void Update()
    {
        // Update UI setiap frame (atau bisa dibuat lebih efisien dengan event)
        UpdateUI();
    }
}