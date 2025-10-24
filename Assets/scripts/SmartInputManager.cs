using UnityEngine;
using UnityEngine.InputSystem;

public class SmartInputManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float gamepadThreshold = 0.1f;
    [SerializeField] private float mouseThreshold = 5f;
    [SerializeField] private float gamepadTimeout = 2f;
    [SerializeField] private bool debugMode = false;
    
    // State
    private bool gamepadActive = false;
    private float lastGamepadTime;
    private Vector2 lastMousePos;
    private Vector2 currentAimInput;
    private bool currentBoostInput;
    
    // Input Actions
    private InputAction gamepadStick;
    private InputAction mousePos;
    private InputAction gamepadBoost;
    
    // Properties
    public Vector2 AimInput => currentAimInput;
    public bool BoostInput => currentBoostInput;
    public bool IsGamepadActive => gamepadActive;
    
    void Awake()
    {
        // Setup input actions
        gamepadStick = new InputAction(binding: "<Gamepad>/leftStick");
        mousePos = new InputAction(binding: "<Mouse>/position");
        gamepadBoost = new InputAction(binding: "<Gamepad>/rightShoulder");
        
        gamepadStick.Enable();
        mousePos.Enable();
        gamepadBoost.Enable();
        
        if (Mouse.current != null)
            lastMousePos = Mouse.current.position.ReadValue();
    }
    
    void Update()
    {
        DetectInput();
        ProcessInput();
    }
    
    void DetectInput()
    {
        bool gamepadDetected = false;
        bool mouseDetected = false;
        
        // Check gamepad
        if (Gamepad.current != null)
        {
            Vector2 stick = gamepadStick.ReadValue<Vector2>();
            if (stick.magnitude > gamepadThreshold)
            {
                gamepadDetected = true;
                lastGamepadTime = Time.time;
            }
        }
        
        // Check mouse
        if (Mouse.current != null)
        {
            Vector2 currentMousePos = mousePos.ReadValue<Vector2>();
            float mouseDelta = Vector2.Distance(currentMousePos, lastMousePos);
            
            if (mouseDelta > mouseThreshold)
            {
                mouseDetected = true;
                lastMousePos = currentMousePos;
            }
        }
        
        // Update active input
        if (gamepadDetected)
        {
            if (!gamepadActive && debugMode)
                Debug.Log("Switched to Gamepad");
            gamepadActive = true;
        }
        else if (mouseDetected && !gamepadActive)
        {
            // Mouse can only take control if gamepad is not active
        }
        
        // Timeout check
        if (gamepadActive && Time.time - lastGamepadTime > gamepadTimeout)
        {
            if (debugMode)
                Debug.Log("Switched to Mouse");
            gamepadActive = false;
        }
    }
    
    void ProcessInput()
    {
        // Process aim input
        if (gamepadActive && Gamepad.current != null)
        {
            Vector2 stick = gamepadStick.ReadValue<Vector2>();
            currentAimInput = stick.magnitude > gamepadThreshold ? stick.normalized : Vector2.zero;
        }
        else if (Mouse.current != null)
        {
            Vector2 mouseScreenPos = mousePos.ReadValue<Vector2>();
            Camera cam = Camera.main;
            
            if (cam != null)
            {
                Vector2 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);
                Vector2 direction = mouseWorldPos - (Vector2)transform.position;
                currentAimInput = direction.magnitude > 0.1f ? direction.normalized : Vector2.zero;
            }
        }
        
        // Process boost input
        if (gamepadActive && Gamepad.current != null)
        {
            currentBoostInput = gamepadBoost.WasPressedThisFrame();
        }
        else if (Mouse.current != null)
        {
            currentBoostInput = Mouse.current.leftButton.wasPressedThisFrame;
        }
    }
    
    void OnDestroy()
    {
        gamepadStick?.Disable();
        mousePos?.Disable();
        gamepadBoost?.Disable();
    }
}