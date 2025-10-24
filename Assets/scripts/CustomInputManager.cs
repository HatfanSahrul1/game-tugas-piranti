using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manager untuk menangani prioritas input antara mouse dan gamepad
/// Ketika gamepad terdeteksi dan digunakan, input mouse akan diabaikan
/// </summary>
public class CustomInputManager : MonoBehaviour
{
    [Header("Input Detection Settings")]
    [SerializeField] private float gamepadDeadZone = 0.1f;
    [SerializeField] private float gamepadIdleTime = 2f; // Waktu sebelum kembali ke mouse jika gamepad idle
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("Input Priority")]
    [SerializeField] private InputDeviceType preferredInputType = InputDeviceType.Auto;
    
    public enum InputDeviceType
    {
        Auto,       // Otomatis switch berdasarkan input terakhir
        MouseOnly,  // Hanya mouse
        GamepadOnly // Hanya gamepad
    }
    
    // Events untuk memberitahu script lain tentang perubahan input
    public System.Action<bool> OnInputDeviceChanged; // Parameter: isUsingGamepad
    
    // Input state
    private bool isUsingGamepad = false;
    private bool gamepadConnected = false;
    private float lastGamepadInputTime;
    private Vector2 lastGamepadInput;
    private Vector2 lastMousePosition;
    
    // Properties
    public bool IsUsingGamepad => isUsingGamepad;
    public bool GamepadConnected => gamepadConnected;
    public InputDeviceType CurrentInputType => preferredInputType;
    
    // Singleton pattern (opsional)
    public static CustomInputManager Instance { get; private set; }
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize input detection
        InitializeInputDetection();
    }
    
    private void Start()
    {
        lastMousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }
    
    private void Update()
    {
        UpdateInputDetection();
        CheckInputDeviceSwitch();
    }
    
    private void OnEnable()
    {
        // Subscribe to input system events
        InputSystem.onDeviceChange += OnDeviceChange;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from input system events
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
    
    #endregion
    
    #region Input Detection
    
    private void InitializeInputDetection()
    {
        // Check if gamepad is already connected
        gamepadConnected = Gamepad.current != null;
        
        // Set initial input type
        if (preferredInputType == InputDeviceType.Auto)
        {
            isUsingGamepad = gamepadConnected;
        }
        else
        {
            isUsingGamepad = preferredInputType == InputDeviceType.GamepadOnly;
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"InputManager initialized. Gamepad connected: {gamepadConnected}, Using gamepad: {isUsingGamepad}");
        }
    }
    
    private void UpdateInputDetection()
    {
        // Update gamepad connection status
        bool wasConnected = gamepadConnected;
        gamepadConnected = Gamepad.current != null;
        
        // Log connection changes
        if (wasConnected != gamepadConnected && enableDebugLog)
        {
            Debug.Log($"Gamepad {(gamepadConnected ? "connected" : "disconnected")}");
        }
        
        // Don't process input if forced to specific type
        if (preferredInputType != InputDeviceType.Auto) return;
        
        // Check gamepad input
        if (gamepadConnected)
        {
            Vector2 currentGamepadInput = Gamepad.current.leftStick.ReadValue();
            bool gamepadButtonPressed = Gamepad.current.buttonSouth.isPressed || 
                                       Gamepad.current.rightShoulder.isPressed ||
                                       Gamepad.current.buttonEast.isPressed ||
                                       Gamepad.current.buttonNorth.isPressed ||
                                       Gamepad.current.buttonWest.isPressed;
            
            // Check if gamepad is being used
            if (currentGamepadInput.magnitude > gamepadDeadZone || gamepadButtonPressed)
            {
                lastGamepadInputTime = Time.time;
                lastGamepadInput = currentGamepadInput;
                
                if (!isUsingGamepad)
                {
                    SwitchToGamepad();
                }
            }
        }
        
        // Check mouse input (only if not using gamepad or gamepad has been idle)
        if (Mouse.current != null)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            bool mouseButtonPressed = Mouse.current.leftButton.isPressed || 
                                     Mouse.current.rightButton.isPressed ||
                                     Mouse.current.middleButton.isPressed;
            
            // Check if mouse moved or clicked
            bool mouseActive = Vector2.Distance(currentMousePosition, lastMousePosition) > 1f || mouseButtonPressed;
            
            if (mouseActive)
            {
                lastMousePosition = currentMousePosition;
                
                // Switch to mouse if gamepad has been idle or no gamepad
                bool gamepadIdle = (Time.time - lastGamepadInputTime) > gamepadIdleTime;
                
                if ((isUsingGamepad && gamepadIdle) || !gamepadConnected)
                {
                    SwitchToMouse();
                }
            }
        }
    }
    
    private void CheckInputDeviceSwitch()
    {
        // Handle forced input types
        switch (preferredInputType)
        {
            case InputDeviceType.MouseOnly:
                if (isUsingGamepad)
                {
                    SwitchToMouse();
                }
                break;
                
            case InputDeviceType.GamepadOnly:
                if (!isUsingGamepad && gamepadConnected)
                {
                    SwitchToGamepad();
                }
                break;
        }
    }
    
    #endregion
    
    #region Input Switching
    
    private void SwitchToGamepad()
    {
        if (isUsingGamepad) return;
        
        isUsingGamepad = true;
        OnInputDeviceChanged?.Invoke(true);
        
        if (enableDebugLog)
        {
            Debug.Log("Switched to Gamepad input");
        }
    }
    
    private void SwitchToMouse()
    {
        if (!isUsingGamepad) return;
        
        isUsingGamepad = false;
        OnInputDeviceChanged?.Invoke(false);
        
        if (enableDebugLog)
        {
            Debug.Log("Switched to Mouse input");
        }
    }
    
    #endregion
    
    #region Device Events
    
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    gamepadConnected = true;
                    if (preferredInputType == InputDeviceType.Auto || preferredInputType == InputDeviceType.GamepadOnly)
                    {
                        SwitchToGamepad();
                    }
                    if (enableDebugLog) Debug.Log($"Gamepad connected: {device.name}");
                    break;
                    
                case InputDeviceChange.Removed:
                    gamepadConnected = Gamepad.current != null; // Check if other gamepads still connected
                    if (!gamepadConnected && isUsingGamepad && preferredInputType == InputDeviceType.Auto)
                    {
                        SwitchToMouse();
                    }
                    if (enableDebugLog) Debug.Log($"Gamepad disconnected: {device.name}");
                    break;
            }
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Get input vector berdasarkan device yang aktif
    /// </summary>
    public Vector2 GetAimInput()
    {
        if (preferredInputType == InputDeviceType.MouseOnly || (!isUsingGamepad && preferredInputType == InputDeviceType.Auto))
        {
            // Return mouse input (akan diconvert ke world space di PlayerController)
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        }
        else if (gamepadConnected && (preferredInputType == InputDeviceType.GamepadOnly || isUsingGamepad))
        {
            // Return gamepad stick input
            return Gamepad.current.leftStick.ReadValue();
        }
        
        return Vector2.zero;
    }
    
    /// <summary>
    /// Get boost input berdasarkan device yang aktif
    /// </summary>
    public bool GetBoostInput()
    {
        if (preferredInputType == InputDeviceType.MouseOnly || (!isUsingGamepad && preferredInputType == InputDeviceType.Auto))
        {
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
        }
        else if (gamepadConnected && (preferredInputType == InputDeviceType.GamepadOnly || isUsingGamepad))
        {
            return Gamepad.current.rightShoulder.isPressed;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if boost was pressed this frame
    /// </summary>
    public bool GetBoostInputDown()
    {
        if (preferredInputType == InputDeviceType.MouseOnly || (!isUsingGamepad && preferredInputType == InputDeviceType.Auto))
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }
        else if (gamepadConnected && (preferredInputType == InputDeviceType.GamepadOnly || isUsingGamepad))
        {
            return Gamepad.current.rightShoulder.wasPressedThisFrame;
        }
        
        return false;
    }
    
    /// <summary>
    /// Force switch ke input type tertentu
    /// </summary>
    public void SetInputType(InputDeviceType inputType)
    {
        preferredInputType = inputType;
        
        switch (inputType)
        {
            case InputDeviceType.MouseOnly:
                SwitchToMouse();
                break;
            case InputDeviceType.GamepadOnly:
                if (gamepadConnected)
                {
                    SwitchToGamepad();
                }
                break;
            case InputDeviceType.Auto:
                // Let the auto detection handle it
                break;
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"Input type set to: {inputType}");
        }
    }
    
    /// <summary>
    /// Toggle debug logging
    /// </summary>
    public void SetDebugLog(bool enabled)
    {
        enableDebugLog = enabled;
    }
    
    #endregion
    
    #region Debug
    
    private void OnGUI()
    {
        if (!enableDebugLog || !Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
        GUILayout.Label("=== INPUT MANAGER DEBUG ===");
        GUILayout.Label($"Current Input: {(isUsingGamepad ? "GAMEPAD" : "MOUSE")}");
        GUILayout.Label($"Gamepad Connected: {gamepadConnected}");
        GUILayout.Label($"Input Type: {preferredInputType}");
        GUILayout.Label($"Last Gamepad Input: {Time.time - lastGamepadInputTime:F1}s ago");
        
        if (gamepadConnected && Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            GUILayout.Label($"Gamepad Stick: {stick.x:F2}, {stick.y:F2}");
        }
        
        GUILayout.EndArea();
    }
    
    #endregion
}