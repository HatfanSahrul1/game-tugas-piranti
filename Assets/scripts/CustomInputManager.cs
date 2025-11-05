using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

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
        Auto,             // Otomatis switch berdasarkan input terakhir
        MouseOnly,        // Hanya mouse
        GamepadOnly,      // Hanya gamepad
        SteeringWheelOnly // Hanya steering wheel
    }
    
    // Events untuk memberitahu script lain tentang perubahan input
    public System.Action<bool> OnInputDeviceChanged; // Parameter: isUsingGamepad
    
    // Input state
    private bool isUsingGamepad = false;
    private bool gamepadConnected = false;
    private bool steeringWheelConnected = false;
    private bool isUsingSteeringWheel = false;
    private float lastGamepadInputTime;
    private float lastSteeringWheelInputTime;
    private Vector2 lastGamepadInput;
    private Vector2 lastMousePosition;
    private float lastSteeringInput;
    
    // Properties
    public bool IsUsingGamepad => isUsingGamepad;
    public bool IsUsingSteeringWheel => isUsingSteeringWheel;
    public bool GamepadConnected => gamepadConnected;
    public bool SteeringWheelConnected => steeringWheelConnected;
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
        // Deteksi steering wheel
        DetectSteeringWheel();
        
        // Check if gamepad is already connected
        gamepadConnected = Gamepad.current != null;
        
        // Set initial input type
        if (preferredInputType == InputDeviceType.Auto)
        {
            isUsingSteeringWheel = steeringWheelConnected;
            isUsingGamepad = gamepadConnected && !steeringWheelConnected;
        }
        else if (preferredInputType == InputDeviceType.SteeringWheelOnly)
        {
            isUsingSteeringWheel = true;
            isUsingGamepad = false;
        }
        else
        {
            isUsingGamepad = preferredInputType == InputDeviceType.GamepadOnly;
            isUsingSteeringWheel = false;
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"InputManager initialized. Gamepad connected: {gamepadConnected}, Steering Wheel connected: {steeringWheelConnected}, Using steering wheel: {isUsingSteeringWheel}");
        }
    }
    
    private void DetectSteeringWheel()
    {
        steeringWheelConnected = false;
        
        foreach (var device in InputSystem.devices)
        {
            if (device is HID hidDevice)
            {
                string productName = hidDevice.description.product?.ToLower() ?? "";
                string interfaceName = hidDevice.description.interfaceName?.ToLower() ?? "";
                
                // Check for common steering wheel identifiers
                if (productName.Contains("wheel") || productName.Contains("racing") || 
                    productName.Contains("driving") || interfaceName.Contains("steering") ||
                    productName.Contains("g29") || productName.Contains("g920") || 
                    productName.Contains("t150") || productName.Contains("t300"))
                {
                    steeringWheelConnected = true;
                    if (enableDebugLog)
                    {
                        Debug.Log($"Steering Wheel detected: {hidDevice.description.product}");
                    }
                    return;
                }
            }
        }
    }
    
    private void UpdateInputDetection()
    {
        // Update steering wheel connection status
        bool wasSteeringWheelConnected = steeringWheelConnected;
        DetectSteeringWheel();
        
        if (wasSteeringWheelConnected != steeringWheelConnected && enableDebugLog)
        {
            Debug.Log($"Steering Wheel {(steeringWheelConnected ? "connected" : "disconnected")}");
        }
        
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
        
        // Jika steering wheel terhubung, prioritaskan steering wheel
        if (steeringWheelConnected)
        {
            // Check for steering wheel input (using gamepad as fallback for now)
            if (gamepadConnected)
            {
                Vector2 steeringInput = Gamepad.current.leftStick.ReadValue();
                bool steeringButtonPressed = Gamepad.current.buttonSouth.isPressed || 
                                           Gamepad.current.rightShoulder.isPressed;
                
                if (Mathf.Abs(steeringInput.x) > gamepadDeadZone || steeringButtonPressed)
                {
                    lastSteeringWheelInputTime = Time.time;
                    lastSteeringInput = steeringInput.x;
                    
                    if (!isUsingSteeringWheel)
                    {
                        SwitchToSteeringWheel();
                    }
                }
            }
        }
        
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
                if (isUsingGamepad || isUsingSteeringWheel)
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
                
            case InputDeviceType.SteeringWheelOnly:
                if (!isUsingSteeringWheel && steeringWheelConnected)
                {
                    SwitchToSteeringWheel();
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
        if (!isUsingGamepad && !isUsingSteeringWheel) return;
        
        isUsingGamepad = false;
        isUsingSteeringWheel = false;
        OnInputDeviceChanged?.Invoke(false);
        
        if (enableDebugLog)
        {
            Debug.Log("Switched to Mouse input");
        }
    }
    
    private void SwitchToSteeringWheel()
    {
        if (isUsingSteeringWheel) return;
        
        isUsingGamepad = false;
        isUsingSteeringWheel = true;
        OnInputDeviceChanged?.Invoke(true);
        
        if (enableDebugLog)
        {
            Debug.Log("Switched to Steering Wheel input");
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
        if (preferredInputType == InputDeviceType.SteeringWheelOnly || (isUsingSteeringWheel && preferredInputType == InputDeviceType.Auto))
        {
            // Return steering wheel input (horizontal axis only)
            if (gamepadConnected)
            {
                float steeringAxis = Gamepad.current.leftStick.x.ReadValue();
                return new Vector2(steeringAxis, 0f);
            }
            return Vector2.zero;
        }
        else if (preferredInputType == InputDeviceType.MouseOnly || (!isUsingGamepad && !isUsingSteeringWheel && preferredInputType == InputDeviceType.Auto))
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
        if (preferredInputType == InputDeviceType.SteeringWheelOnly || (isUsingSteeringWheel && preferredInputType == InputDeviceType.Auto))
        {
            return gamepadConnected && Gamepad.current.rightShoulder.isPressed;
        }
        else if (preferredInputType == InputDeviceType.MouseOnly || (!isUsingGamepad && !isUsingSteeringWheel && preferredInputType == InputDeviceType.Auto))
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
        if (preferredInputType == InputDeviceType.SteeringWheelOnly || (isUsingSteeringWheel && preferredInputType == InputDeviceType.Auto))
        {
            return gamepadConnected && Gamepad.current.rightShoulder.wasPressedThisFrame;
        }
        else if (preferredInputType == InputDeviceType.MouseOnly || (!isUsingGamepad && !isUsingSteeringWheel && preferredInputType == InputDeviceType.Auto))
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
            case InputDeviceType.SteeringWheelOnly:
                if (steeringWheelConnected)
                {
                    SwitchToSteeringWheel();
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
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 180, 300, 170));
        GUILayout.Label("=== INPUT MANAGER DEBUG ===");
        string currentInput = isUsingSteeringWheel ? "STEERING WHEEL" : (isUsingGamepad ? "GAMEPAD" : "MOUSE");
        GUILayout.Label($"Current Input: {currentInput}");
        GUILayout.Label($"Gamepad Connected: {gamepadConnected}");
        GUILayout.Label($"Steering Wheel Connected: {steeringWheelConnected}");
        GUILayout.Label($"Input Type: {preferredInputType}");
        GUILayout.Label($"Last Gamepad Input: {Time.time - lastGamepadInputTime:F1}s ago");
        GUILayout.Label($"Last Steering Input: {Time.time - lastSteeringWheelInputTime:F1}s ago");
        
        if (gamepadConnected && Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            GUILayout.Label($"Gamepad Stick: {stick.x:F2}, {stick.y:F2}");
        }
        
        GUILayout.EndArea();
    }
    
    #endregion
}