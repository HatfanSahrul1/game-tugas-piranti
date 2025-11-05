using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 8f;
    [SerializeField] private float turnDeceleration = 12f;
    [SerializeField] private float movementSmoothness = 8f;
    
    [Header("Boost Settings")]
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private float boostDuration = 1f;
    [SerializeField] private float boostCooldown = 3f;
    
    [Header("Speed Curve Settings")]
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float speedBuildUpTime = 2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem boostEffect;
    [SerializeField] private Transform planeVisual;
    [SerializeField] private float rotationSpeed = 180f;
    
    // Input System - Custom Input Manager
    [Header("Input Settings")]
    [SerializeField] private CustomInputManager inputManager;
    [SerializeField] private bool enableInputDebug = false;
    
    // Movement Components
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    // Movement Variables
    private Vector2 targetDirection;
    private Vector2 currentVelocity;
    private float currentSpeed;
    private float speedBuildUpTimer;
    private float targetAngle;
    private bool isMovingStraight;
    private Vector2 lastDirection;
    
    // Boost Variables
    private bool isBoosting;
    private float boostTimer;
    private float boostCooldownTimer;
    private bool canBoost = true;
    
    // Properties untuk akses dari script lain
    public float CurrentSpeed => currentSpeed;
    public float MaxSpeed => maxSpeed;
    public float SpeedPercentage => currentSpeed / maxSpeed;
    public bool IsBoosting => isBoosting;
    public bool IsMoving => currentSpeed > 0.1f;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
        
        // Auto-find CustomInputManager if not assigned
        if (inputManager == null)
            inputManager = FindObjectOfType<CustomInputManager>();
    }
    
    private void OnEnable()
    {
        // Subscribe to input manager events if available
        if (inputManager != null)
        {
            inputManager.OnInputDeviceChanged += OnInputDeviceChanged;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from input manager events
        if (inputManager != null)
        {
            inputManager.OnInputDeviceChanged -= OnInputDeviceChanged;
        }
    }
    
    private void OnInputDeviceChanged(bool isUsingGamepad)
    {
        if (enableInputDebug)
        {
            string deviceType = inputManager.IsUsingSteeringWheel ? "Steering Wheel" : 
                              (isUsingGamepad ? "Gamepad" : "Mouse/Keyboard");
            Debug.Log($"PlayerController: Input device changed to {deviceType}");
        }
    }
    
    private void Update()
    {
        HandleInput();
        UpdateMovement();
        UpdateBoost();
        UpdateVisuals();
    }
    
    private void FixedUpdate()
    {
        ApplyMovement();
    }
    
    #endregion
    
    #region Input Handling
    
    private void HandleInput()
    {
        if (inputManager == null) return;
        
        // Get input from CustomInputManager
        Vector2 aimInput = inputManager.GetAimInput();
        
        if (inputManager.IsUsingSteeringWheel)
        {
            // Steering wheel: convert horizontal input to direction
            float steering = aimInput.x;
            if (Mathf.Abs(steering) > 0.1f)
            {
                // Calculate direction based on current velocity + steering
                Vector2 currentDir = rb.velocity.normalized;
                if (currentDir.magnitude < 0.1f) currentDir = transform.up; // fallback direction
                
                float currentAngle = Mathf.Atan2(currentDir.y, currentDir.x);
                float steeringAngle = steering * 2f * Time.deltaTime; // adjust steering sensitivity
                float newAngle = currentAngle + steeringAngle;
                
                targetDirection = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
                
                if (enableInputDebug)
                {
                    Debug.Log($"Steering Wheel Input: {steering:F2}, Target Direction: {targetDirection}");
                }
            }
            else
            {
                // No steering input, maintain current direction or stop
                targetDirection = rb.velocity.normalized;
                if (targetDirection.magnitude < 0.1f) targetDirection = Vector2.zero;
            }
        }
        else if (inputManager.IsUsingGamepad)
        {
            // Gamepad: direct stick input
            if (aimInput.magnitude > 0.1f)
            {
                targetDirection = aimInput.normalized;
                
                if (enableInputDebug)
                {
                    Debug.Log($"Gamepad Input: {aimInput}, Target Direction: {targetDirection}");
                }
            }
            else
            {
                targetDirection = Vector2.zero;
            }
        }
        else
        {
            // Mouse: convert screen to world position
            if (aimInput != Vector2.zero)
            {
                Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(aimInput);
                Vector2 direction = (mouseWorldPos - (Vector2)transform.position);
                
                if (direction.magnitude > 0.1f)
                {
                    targetDirection = direction.normalized;
                    
                    if (enableInputDebug)
                    {
                        Debug.Log($"Mouse Input: {aimInput}, Target Direction: {targetDirection}");
                    }
                }
                else
                {
                    targetDirection = Vector2.zero;
                }
            }
            else
            {
                targetDirection = Vector2.zero;
            }
        }
        
        // Handle boost input
        if (inputManager.GetBoostInputDown() && canBoost && !isBoosting)
        {
            StartBoost();
        }
    }
    

    
    #endregion
    
    #region Movement System
    
    private void UpdateMovement()
    {
        // Cek apakah pesawat bergerak lurus atau belok
        if (targetDirection != Vector2.zero)
        {
            float angleChange = Vector2.Angle(lastDirection, targetDirection);
            isMovingStraight = angleChange < 10f; // Threshold untuk dianggap lurus
            
            if (isMovingStraight)
            {
                // Saat lurus, tingkatkan timer untuk speed buildup
                speedBuildUpTimer += Time.deltaTime;
                speedBuildUpTimer = Mathf.Clamp(speedBuildUpTimer, 0f, speedBuildUpTime);
            }
            else
            {
                // Saat belok, reset timer dan kurangi kecepatan
                speedBuildUpTimer = Mathf.Max(0f, speedBuildUpTimer - Time.deltaTime * 2f);
            }
            
            lastDirection = targetDirection;
        }
        else
        {
            // Tidak ada input, kurangi kecepatan
            speedBuildUpTimer = Mathf.Max(0f, speedBuildUpTimer - Time.deltaTime * 3f);
        }
        
        // Hitung target speed menggunakan logistic curve
        float speedProgress = speedBuildUpTimer / speedBuildUpTime;
        float curveValue = speedCurve.Evaluate(speedProgress);
        float targetSpeed = Mathf.Lerp(minSpeed, maxSpeed, curveValue);
        
        // Apply boost multiplier
        if (isBoosting)
        {
            targetSpeed *= boostMultiplier;
        }
        
        // Smooth speed transition
        float speedChangeRate = isMovingStraight ? acceleration : 
                               (targetDirection == Vector2.zero ? deceleration : turnDeceleration);
        
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
    }
    
    private void ApplyMovement()
    {
        // Determine desired facing angle based on input
        float desiredAngle = transform.eulerAngles.z;
        if (targetDirection != Vector2.zero)
        {
            desiredAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;
        }

        // Smoothly rotate the rigidbody toward desired angle
        float currentAngle = rb.rotation;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);

        // Compute forward vector from newAngle
        Vector2 forward = (Quaternion.Euler(0f, 0f, newAngle) * Vector3.up);

        if (targetDirection != Vector2.zero)
        {
            // Move forward along facing direction. Steer existing velocity toward desired forward velocity
            Vector2 desiredVelocity = forward * currentSpeed;
            float steerRate = movementSmoothness; // how quickly we steer velocity to face-forward
            rb.velocity = Vector2.MoveTowards(rb.velocity, desiredVelocity, steerRate * Time.fixedDeltaTime);
        }
        else
        {
            // No input: decelerate to stop smoothly (prevents sliding)
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Keep currentVelocity in sync for other systems/visuals
        currentVelocity = rb.velocity;
    }
    
    #endregion
    
    #region Boost System
    
    private void StartBoost()
    {
        isBoosting = true;
        boostTimer = boostDuration;
        canBoost = false;
        boostCooldownTimer = boostCooldown;
        
        // Activate boost visual effects
        if (boostEffect != null && !boostEffect.isPlaying)
        {
            boostEffect.Play();
        }
        
        Debug.Log("Boost Activated!");
    }
    
    private void UpdateBoost()
    {
        // Update boost timer
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                EndBoost();
            }
        }
        
        // Update cooldown timer
        if (!canBoost)
        {
            boostCooldownTimer -= Time.deltaTime;
            if (boostCooldownTimer <= 0f)
            {
                canBoost = true;
            }
        }
    }
    
    private void EndBoost()
    {
        isBoosting = false;
        boostTimer = 0f;
        
        // Deactivate boost visual effects
        if (boostEffect != null && boostEffect.isPlaying)
        {
            boostEffect.Stop();
        }
        
        Debug.Log("Boost Ended!");
    }
    
    #endregion
    
    #region Visual Updates
    
    private void UpdateVisuals()
    {
        if (planeVisual != null && currentVelocity.magnitude > 0.1f)
        {
            // Calculate target rotation based on movement direction
            targetAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg - 90f;
            
            // Smooth rotation
            float currentAngle = planeVisual.eulerAngles.z;
            if (currentAngle > 180f) currentAngle -= 360f;
            
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime / 180f);
            planeVisual.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Force boost activation (untuk debugging atau power-ups)
    /// </summary>
    public void ForceBoost()
    {
        if (!isBoosting)
        {
            StartBoost();
        }
    }
    
    /// <summary>
    /// Set kecepatan maksimum secara dinamis
    /// </summary>
    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxSpeed = Mathf.Max(minSpeed, newMaxSpeed);
    }
    
    /// <summary>
    /// Reset semua movement state
    /// </summary>
    public void ResetMovement()
    {
        currentSpeed = 0f;
        currentVelocity = Vector2.zero;
        speedBuildUpTimer = 0f;
        rb.velocity = Vector2.zero;
        
        if (isBoosting)
        {
            EndBoost();
        }
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        // Draw movement direction
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(targetDirection * 2f));
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(currentVelocity.normalized * 2f));
        }
    }
    
    #endregion
}