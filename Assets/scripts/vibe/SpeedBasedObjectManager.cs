using System.Collections.Generic;
using UnityEngine;

public class SpeedBasedObjectManager : MonoBehaviour
{
    [System.Serializable]
    public class SpeedBasedObject
    {
        [Header("Object Settings")]
        public GameObject targetObject;
        public string objectName; // Untuk debugging
        
        [Header("Speed Conditions")]
        [Tooltip("Kecepatan minimum untuk object ini aktif")]
        public float minSpeedThreshold = 0f;
        [Tooltip("Kecepatan maksimum untuk object ini aktif")]
        public float maxSpeedThreshold = 100f;
        [Tooltip("Apakah object aktif saat kecepatan DI BAWAH threshold?")]
        public bool activeWhenBelow = true;
        
        [Header("Transition Settings")]
        [Tooltip("Delay sebelum object spawn/despawn")]
        public float transitionDelay = 0.2f;
        [Tooltip("Apakah menggunakan fade effect?")]
        public bool useFadeEffect = false;
        [Tooltip("Durasi fade in/out")]
        public float fadeDuration = 0.5f;
        
        // Internal state
        [System.NonSerialized]
        public bool isCurrentlyActive;
        [System.NonSerialized]
        public float transitionTimer;
        [System.NonSerialized]
        public bool isTransitioning;
        [System.NonSerialized]
        public CanvasGroup canvasGroup; // Untuk fade effect
        [System.NonSerialized]
        public Renderer objectRenderer; // Untuk fade effect pada 3D objects
    }
    
    [Header("Target Player")]
    [SerializeField] private PlayerController playerController;
    
    [Header("Speed-Based Objects")]
    [SerializeField] private List<SpeedBasedObject> speedBasedObjects = new List<SpeedBasedObject>();
    
    [Header("Global Settings")]
    [SerializeField] private bool enableDebugLog = false;
    [SerializeField] private float updateInterval = 0.1f; // Update setiap 0.1 detik untuk performance
    
    // Private variables
    private float updateTimer;
    private float currentPlayerSpeed;
    private float maxPlayerSpeed;
    
    // Properties
    public float CurrentPlayerSpeed => currentPlayerSpeed;
    public int ActiveObjectCount { get; private set; }
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Auto-assign player controller jika tidak diset
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        
        // Initialize objects
        InitializeObjects();
    }
    
    private void Start()
    {
        if (playerController != null)
        {
            maxPlayerSpeed = playerController.MaxSpeed;
        }
        
        // Set initial state
        UpdateAllObjects(true);
    }
    
    private void Update()
    {
        if (playerController == null) return;
        
        // Update dengan interval untuk performance
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateSpeedTracking();
            UpdateAllObjects(false);
            updateTimer = 0f;
        }
        
        // Update transitions
        UpdateTransitions();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeObjects()
    {
        foreach (var speedObj in speedBasedObjects)
        {
            if (speedObj.targetObject == null) continue;
            
            // Initialize name jika kosong
            if (string.IsNullOrEmpty(speedObj.objectName))
            {
                speedObj.objectName = speedObj.targetObject.name;
            }
            
            // Setup fade components jika diperlukan
            if (speedObj.useFadeEffect)
            {
                SetupFadeComponents(speedObj);
            }
            
            // Set initial state
            speedObj.isCurrentlyActive = speedObj.targetObject.activeInHierarchy;
            speedObj.transitionTimer = 0f;
            speedObj.isTransitioning = false;
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"SpeedBasedObjectManager: Initialized {speedBasedObjects.Count} objects");
        }
    }
    
    private void SetupFadeComponents(SpeedBasedObject speedObj)
    {
        // Cek untuk CanvasGroup (UI elements)
        speedObj.canvasGroup = speedObj.targetObject.GetComponent<CanvasGroup>();
        if (speedObj.canvasGroup == null)
        {
            speedObj.canvasGroup = speedObj.targetObject.AddComponent<CanvasGroup>();
        }
        
        // Cek untuk Renderer (3D/2D objects)
        speedObj.objectRenderer = speedObj.targetObject.GetComponent<Renderer>();
    }
    
    #endregion
    
    #region Speed Tracking
    
    private void UpdateSpeedTracking()
    {
        currentPlayerSpeed = playerController.CurrentSpeed;
        maxPlayerSpeed = playerController.MaxSpeed;
    }
    
    #endregion
    
    #region Object Management
    
    private void UpdateAllObjects(bool forceImmediate = false)
    {
        ActiveObjectCount = 0;
        
        foreach (var speedObj in speedBasedObjects)
        {
            if (speedObj.targetObject == null) continue;
            
            bool shouldBeActive = ShouldObjectBeActive(speedObj);
            
            if (shouldBeActive != speedObj.isCurrentlyActive)
            {
                if (forceImmediate)
                {
                    SetObjectState(speedObj, shouldBeActive, true);
                }
                else
                {
                    StartTransition(speedObj, shouldBeActive);
                }
            }
            
            if (speedObj.isCurrentlyActive)
            {
                ActiveObjectCount++;
            }
        }
    }
    
    private bool ShouldObjectBeActive(SpeedBasedObject speedObj)
    {
        bool withinRange = currentPlayerSpeed >= speedObj.minSpeedThreshold && 
                          currentPlayerSpeed <= speedObj.maxSpeedThreshold;
        
        if (speedObj.activeWhenBelow)
        {
            // Object aktif ketika kecepatan di bawah threshold
            return currentPlayerSpeed < speedObj.maxSpeedThreshold;
        }
        else
        {
            // Object aktif ketika kecepatan dalam range
            return withinRange;
        }
    }
    
    private void StartTransition(SpeedBasedObject speedObj, bool targetState)
    {
        if (speedObj.isTransitioning) return;
        
        speedObj.isTransitioning = true;
        speedObj.transitionTimer = 0f;
        
        if (enableDebugLog)
        {
            Debug.Log($"Starting transition for {speedObj.objectName}: {speedObj.isCurrentlyActive} -> {targetState}");
        }
        
        // Jika tidak ada delay dan tidak menggunakan fade, langsung set
        if (speedObj.transitionDelay <= 0f && !speedObj.useFadeEffect)
        {
            SetObjectState(speedObj, targetState, true);
        }
    }
    
    private void SetObjectState(SpeedBasedObject speedObj, bool isActive, bool immediate = false)
    {
        speedObj.isCurrentlyActive = isActive;
        speedObj.isTransitioning = false;
        
        if (immediate || !speedObj.useFadeEffect)
        {
            // Langsung set active/inactive
            speedObj.targetObject.SetActive(isActive);
            
            if (speedObj.useFadeEffect && speedObj.canvasGroup != null)
            {
                speedObj.canvasGroup.alpha = isActive ? 1f : 0f;
            }
        }
        else
        {
            // Start fade effect
            StartCoroutine(FadeObject(speedObj, isActive));
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"Set {speedObj.objectName} to {(isActive ? "ACTIVE" : "INACTIVE")} (Speed: {currentPlayerSpeed:F2})");
        }
    }
    
    #endregion
    
    #region Transitions and Effects
    
    private void UpdateTransitions()
    {
        foreach (var speedObj in speedBasedObjects)
        {
            if (!speedObj.isTransitioning) continue;
            
            speedObj.transitionTimer += Time.deltaTime;
            
            // Cek apakah delay sudah selesai
            if (speedObj.transitionTimer >= speedObj.transitionDelay)
            {
                bool targetState = ShouldObjectBeActive(speedObj);
                SetObjectState(speedObj, targetState);
            }
        }
    }
    
    private System.Collections.IEnumerator FadeObject(SpeedBasedObject speedObj, bool fadeIn)
    {
        if (speedObj.canvasGroup == null && speedObj.objectRenderer == null)
        {
            // Fallback ke simple SetActive
            speedObj.targetObject.SetActive(fadeIn);
            yield break;
        }
        
        // Pastikan object aktif saat fade in
        if (fadeIn)
        {
            speedObj.targetObject.SetActive(true);
        }
        
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        
        // Set initial alpha
        SetObjectAlpha(speedObj, startAlpha);
        
        while (elapsedTime < speedObj.fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / speedObj.fadeDuration);
            SetObjectAlpha(speedObj, currentAlpha);
            yield return null;
        }
        
        // Ensure final alpha
        SetObjectAlpha(speedObj, targetAlpha);
        
        // Deactivate object setelah fade out
        if (!fadeIn)
        {
            speedObj.targetObject.SetActive(false);
        }
    }
    
    private void SetObjectAlpha(SpeedBasedObject speedObj, float alpha)
    {
        // Set alpha untuk CanvasGroup (UI)
        if (speedObj.canvasGroup != null)
        {
            speedObj.canvasGroup.alpha = alpha;
        }
        
        // Set alpha untuk Renderer (3D/2D objects)
        if (speedObj.objectRenderer != null)
        {
            Material[] materials = speedObj.objectRenderer.materials;
            foreach (var material in materials)
            {
                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
            }
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Tambah object baru ke management
    /// </summary>
    public void AddSpeedBasedObject(GameObject obj, float minSpeed, float maxSpeed, bool activeWhenBelow = true)
    {
        var newSpeedObj = new SpeedBasedObject
        {
            targetObject = obj,
            objectName = obj.name,
            minSpeedThreshold = minSpeed,
            maxSpeedThreshold = maxSpeed,
            activeWhenBelow = activeWhenBelow,
            transitionDelay = 0.2f,
            useFadeEffect = false
        };
        
        speedBasedObjects.Add(newSpeedObj);
        
        // Initialize new object
        if (Application.isPlaying)
        {
            SetupFadeComponents(newSpeedObj);
            UpdateAllObjects(true);
        }
    }
    
    /// <summary>
    /// Remove object dari management
    /// </summary>
    public void RemoveSpeedBasedObject(GameObject obj)
    {
        speedBasedObjects.RemoveAll(x => x.targetObject == obj);
    }
    
    /// <summary>
    /// Set player controller reference
    /// </summary>
    public void SetPlayerController(PlayerController newPlayerController)
    {
        playerController = newPlayerController;
        if (playerController != null)
        {
            maxPlayerSpeed = playerController.MaxSpeed;
        }
    }
    
    /// <summary>
    /// Force update semua objects
    /// </summary>
    public void ForceUpdateAllObjects()
    {
        UpdateAllObjects(true);
    }
    
    /// <summary>
    /// Get status object berdasarkan nama
    /// </summary>
    public bool IsObjectActive(string objectName)
    {
        var speedObj = speedBasedObjects.Find(x => x.objectName == objectName);
        return speedObj?.isCurrentlyActive ?? false;
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        if (playerController == null) return;
        
        // Draw speed thresholds
        Vector3 playerPos = playerController.transform.position;
        
        foreach (var speedObj in speedBasedObjects)
        {
            if (speedObj.targetObject == null) continue;
            
            // Color berdasarkan status
            if (Application.isPlaying)
            {
                Gizmos.color = speedObj.isCurrentlyActive ? Color.green : Color.red;
            }
            else
            {
                Gizmos.color = Color.white;
            }
            
            // Draw line dari player ke object
            Gizmos.DrawLine(playerPos, speedObj.targetObject.transform.position);
            
            // Draw object boundary
            Gizmos.DrawWireCube(speedObj.targetObject.transform.position, Vector3.one * 0.5f);
        }
    }
    
    #endregion
}