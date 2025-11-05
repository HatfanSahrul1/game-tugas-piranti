using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerInputManager untuk mengelola prioritas input antara mouse dan gamepad
/// Ketika gamepad terdeteksi dan digunakan, input mouse akan diabaikan
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    [Header("Input Detection Settings")]
    [SerializeField] private float gamepadThreshold = 0.1f;
    [SerializeField] private float mouseMovementThreshold = 5f; // Pixel movement threshold
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("Input Timeout Settings")]
    [SerializeField] private float inputSwitchDelay = 0.5f; // Delay sebelum switch input method
    [SerializeField] private float gamepadInactivityTimeout = 2f; // Timeout untuk switch kembali ke mouse
    
    // Input Device Detection
    private bool isGamepadActive = false;
    private bool isMouseActive = true;
    private float lastGamepadInput;
    private float lastMouseInput;
    private float inputSwitchTimer;
    private Vector2 lastMousePosition;
    
    // Input Actions
    private InputAction gamepadStickAction;
    private InputAction mousePositionAction;
    private InputAction gamepadButtonAction;
    
    // Current Input Values
    private Vector2 currentAimInput;
    private bool currentBoostInput;
    
    // Events untuk notifikasi perubahan input device
    public System.Action<bool> OnInputDeviceChanged; // bool: true = gamepad, false = mouse
    
    // Properties
    public bool IsGamepadActive => isGamepadActive;
    public bool IsMouseActive => isMouseActive;
    public Vector2 AimInput => currentAimInput;
    public bool BoostInput => currentBoostInput;
    public string CurrentInputDevice => isGamepadActive ? "Gamepad" : "Mouse";
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Setup input actions
        SetupInputActions();
        
        // Initialize mouse position
        if (Mouse.current != null)
        {
            lastMousePosition = Mouse.current.position.ReadValue();
        }
    }
    
    private void OnEnable()
    {
        EnableInputActions();
    }
    
    private void OnDisable()
    {
        DisableInputActions();
    }
    
    private void Update()
    {
        DetectActiveInputDevice();
        ProcessInput();
        UpdateInputSwitchTimer();
    }
    
    #endregion
    
    #region Input Actions Setup
    
    private void SetupInputActions()
    {
        // Setup gamepad stick action
        gamepadStickAction = new InputAction(
            name: "GamepadStick",
            type: InputActionType.Value,
            binding: "<Gamepad>/leftStick"
        );
        
        // Setup mouse position action
        mousePositionAction = new InputAction(
            name: "MousePosition",
            type: InputActionType.Value,
            binding: "<Mouse>/position"
        );
        
        // Setup gamepad button action (untuk boost)
        gamepadButtonAction = new InputAction(
            name: "GamepadBoost",
            type: InputActionType.Button,
            binding: "<Gamepad>/rightShoulder"
        );
    }
    
    private void EnableInputActions()
    {
        gamepadStickAction?.Enable();
        mousePositionAction?.Enable();
        gamepadButtonAction?.Enable();
    }
    
    private void DisableInputActions()
    {
        gamepadStickAction?.Disable();
        mousePositionAction?.Disable();
        gamepadButtonAction?.Disable();
    }
    
    #endregion
    
    #region Input Device Detection
    
    private void DetectActiveInputDevice()
    {
        bool gamepadDetected = false;
        bool mouseDetected = false;
        
        // Cek gamepad input
        if (Gamepad.current != null && gamepadStickAction != null)
        {
            Vector2 gamepadInput = gamepadStickAction.ReadValue<Vector2>();
            
            if (gamepadInput.magnitude > gamepadThreshold)
            {
                gamepadDetected = true;
                lastGamepadInput = Time.time;
                
                if (enableDebugLog && !isGamepadActive)
                {
                    Debug.Log("Gamepad input detected!");
                }
            }
        }
        
        // Cek mouse movement
        if (Mouse.current != null && mousePositionAction != null)
        {
            Vector2 currentMousePos = mousePositionAction.ReadValue<Vector2>();
            float mouseMovement = Vector2.Distance(currentMousePos, lastMousePosition);
            
            if (mouseMovement > mouseMovementThreshold)
            {
                mouseDetected = true;
                lastMouseInput = Time.time;
                lastMousePosition = currentMousePos;
                
                if (enableDebugLog && !isMouseActive)
                {
                    Debug.Log("Mouse movement detected!");
                }
            }
        }
        
        // Update active input device
        UpdateActiveInputDevice(gamepadDetected, mouseDetected);
    }
    
    private void UpdateActiveInputDevice(bool gamepadDetected, bool mouseDetected)
    {
        bool previousGamepadState = isGamepadActive;
        
        // Prioritas: Gamepad yang baru digunakan akan mengambil alih
        if (gamepadDetected)
        {
            inputSwitchTimer = inputSwitchDelay;
            
            if (!isGamepadActive && inputSwitchTimer <= 0f)
            {
                SwitchToGamepad();
            }
        }
        // Mouse hanya diaktifkan jika gamepad tidak digunakan dalam timeout tertentu
        else if (mouseDetected && !isGamepadActive)
        {
            SwitchToMouse();
        }
        
        // Auto-switch kembali ke mouse jika gamepad tidak digunakan
        if (isGamepadActive && Time.time - lastGamepadInput > gamepadInactivityTimeout)
        {
            if (enableDebugLog)
            {
                Debug.Log("Gamepad inactive, switching back to mouse");
            }
            SwitchToMouse();
        }
        
        // Notify jika ada perubahan input device
        if (previousGamepadState != isGamepadActive)
        {
            OnInputDeviceChanged?.Invoke(isGamepadActive);
        }
    }
    
    private void SwitchToGamepad()
    {
        isGamepadActive = true;
        isMouseActive = false;
        
        if (enableDebugLog)
        {
            Debug.Log("Switched to Gamepad input");
        }
    }
    
    private void SwitchToMouse()
    {
        isGamepadActive = false;
        isMouseActive = true;
        
        if (enableDebugLog)
        {
            Debug.Log("Switched to Mouse input");
        }
    }
    
    #endregion
    
    #region Input Processing
    
    private void ProcessInput()
    {
        // Process aim input
        if (isGamepadActive)
        {
            ProcessGamepadAimInput();
        }
        else
        {
            ProcessMouseAimInput();
        }
        
        // Process boost input
        ProcessBoostInput();
    }
    
    private void ProcessGamepadAimInput()
    {
        if (Gamepad.current != null && gamepadStickAction != null)
        {
            Vector2 gamepadInput = gamepadStickAction.ReadValue<Vector2>();
            
            // Apply deadzone
            if (gamepadInput.magnitude > gamepadThreshold)
            {
                currentAimInput = gamepadInput.normalized;
            }
            else
            {
                currentAimInput = Vector2.zero;
            }
        }
        else
        {
            currentAimInput = Vector2.zero;
        }
    }
    
    private void ProcessMouseAimInput()
    {
        if (Mouse.current != null && mousePositionAction != null)
        {
            Vector2 mouseScreenPos = mousePositionAction.ReadValue<Vector2>();
            
            // Convert screen position to world direction
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
                Vector2 playerPos = transform.position;
                
                Vector2 direction = (mouseWorldPos - playerPos);
                
                if (direction.magnitude > 0.1f)
                {
                    currentAimInput = direction.normalized;
                }
                else
                {
                    currentAimInput = Vector2.zero;
                }
            }
        }
        else
        {
            currentAimInput = Vector2.zero;
        }
    }
    
    private void ProcessBoostInput()
    {
        bool boostPressed = false;
        
        if (isGamepadActive)
        {
            // Gamepad boost input
            if (Gamepad.current != null && gamepadButtonAction != null)
            {
                boostPressed = gamepadButtonAction.WasPressedThisFrame();
            }
        }
        else
        {
            // Mouse boost input
            if (Mouse.current != null)
            {
                boostPressed = Mouse.current.leftButton.wasPressedThisFrame;
            }
        }
        
        currentBoostInput = boostPressed;
    }
    
    private void UpdateInputSwitchTimer()
    {
        if (inputSwitchTimer > 0f)
        {
            inputSwitchTimer -= Time.deltaTime;
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Force switch ke gamepad
    /// </summary>
    public void ForceGamepadMode()
    {
        SwitchToGamepad();
        lastGamepadInput = Time.time;
    }
    
    /// <summary>
    /// Force switch ke mouse
    /// </summary>
    public void ForceMouseMode()
    {
        SwitchToMouse();
        lastMouseInput = Time.time;
    }
    
    /// <summary>
    /// Set threshold untuk gamepad sensitivity
    /// </summary>
    public void SetGamepadThreshold(float threshold)
    {
        gamepadThreshold = Mathf.Clamp01(threshold);
    }
    
    /// <summary>
    /// Set timeout untuk gamepad inactivity
    /// </summary>
    public void SetGamepadTimeout(float timeout)
    {
        gamepadInactivityTimeout = Mathf.Max(0f, timeout);
    }
    
    /// <summary>
    /// Check apakah input device tertentu tersedia
    /// </summary>
    public bool IsDeviceAvailable(string deviceType)
    {
        switch (deviceType.ToLower())
        {
            case "gamepad":
                return Gamepad.current != null;
            case "mouse":
                return Mouse.current != null;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Get info device yang sedang aktif
    /// </summary>
    public string GetActiveDeviceInfo()
    {
        if (isGamepadActive && Gamepad.current != null)
        {
            return $"Gamepad: {Gamepad.current.displayName}";
        }
        else if (isMouseActive && Mouse.current != null)
        {
            return $"Mouse: {Mouse.current.displayName}";
        }
        
        return "No active input device";
    }
    
    #endregion
    
    #region Debug and UI
    
    private void OnGUI()
    {
        if (!enableDebugLog || !Application.isPlaying) return;
        
        // Debug overlay
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
        GUILayout.Label("=== INPUT MANAGER DEBUG ===");
        GUILayout.Label($"Active Device: {CurrentInputDevice}");
        GUILayout.Label($"Gamepad Available: {IsDeviceAvailable("gamepad")}");
        GUILayout.Label($"Mouse Available: {IsDeviceAvailable("mouse")}");
        GUILayout.Label($"Aim Input: {currentAimInput}");
        GUILayout.Label($"Boost Input: {currentBoostInput}");
        GUILayout.Label($"Last Gamepad: {Time.time - lastGamepadInput:F1}s ago");
        GUILayout.EndArea();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw current input direction
        Gizmos.color = isGamepadActive ? Color.green : Color.blue;
        Vector3 inputDirection = (Vector3)currentAimInput * 2f;
        Gizmos.DrawLine(transform.position, transform.position + inputDirection);
        
        // Draw device indicator
        Gizmos.color = isGamepadActive ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
    }
    
    #endregion
}
