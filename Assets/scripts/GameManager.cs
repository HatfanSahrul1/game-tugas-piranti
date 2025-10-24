using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contoh GameManager yang menunjukkan cara menggunakan semua komponen pesawat
/// Script ini bisa digunakan sebagai referensi implementasi
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private SpeedBasedObjectManager speedObjectManager;
    
    [Header("UI Elements")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Text speedText;
    [SerializeField] private Text boostText;
    [SerializeField] private Image boostCooldownImage;
    
    [Header("Game Settings")]
    [SerializeField] private float gameTime = 60f;
    [SerializeField] private bool enableSpeedUI = true;
    
    // Game State
    private float currentGameTime;
    private bool gameActive = true;
    private float totalDistance;
    private Vector3 lastPosition;
    
    // Properties
    public float CurrentSpeed => playerController != null ? playerController.CurrentSpeed : 0f;
    public float SpeedPercentage => playerController != null ? playerController.SpeedPercentage : 0f;
    public bool IsGameActive => gameActive;
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void Update()
    {
        if (!gameActive) return;
        
        UpdateGameTime();
        UpdateUI();
        UpdateStats();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeGame()
    {
        // Auto-find components jika tidak diassign
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
            
        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();
            
        if (speedObjectManager == null)
            speedObjectManager = FindObjectOfType<SpeedBasedObjectManager>();
        
        // Setup initial state
        currentGameTime = gameTime;
        lastPosition = playerController != null ? playerController.transform.position : Vector3.zero;
        
        // Setup UI
        SetupUI();
        
        Debug.Log("Game Initialized! Controls: Mouse to steer, Left Click to boost");
    }
    
    private void SetupUI()
    {
        if (speedSlider != null)
        {
            speedSlider.minValue = 0f;
            speedSlider.maxValue = playerController != null ? playerController.MaxSpeed : 10f;
        }
        
        UpdateBoostUI();
    }
    
    #endregion
    
    #region Game Logic
    
    private void UpdateGameTime()
    {
        currentGameTime -= Time.deltaTime;
        
        if (currentGameTime <= 0f)
        {
            EndGame();
        }
    }
    
    private void UpdateStats()
    {
        if (playerController != null)
        {
            // Calculate distance traveled
            Vector3 currentPosition = playerController.transform.position;
            totalDistance += Vector3.Distance(lastPosition, currentPosition);
            lastPosition = currentPosition;
        }
    }
    
    private void EndGame()
    {
        gameActive = false;
        
        // Show final stats
        Debug.Log($"Game Over! Final Stats:");
        Debug.Log($"- Total Distance: {totalDistance:F2} units");
        Debug.Log($"- Average Speed: {totalDistance / gameTime:F2} units/sec");
        Debug.Log($"- Game Time: {gameTime} seconds");
        
        // Stop player movement
        if (playerController != null)
        {
            playerController.ResetMovement();
        }
    }
    
    #endregion
    
    #region UI Updates
    
    private void UpdateUI()
    {
        if (!enableSpeedUI) return;
        
        UpdateSpeedUI();
        UpdateBoostUI();
        UpdateGameUI();
    }
    
    private void UpdateSpeedUI()
    {
        if (playerController == null) return;
        
        // Update speed slider
        if (speedSlider != null)
        {
            speedSlider.value = CurrentSpeed;
        }
        
        // Update speed text
        if (speedText != null)
        {
            speedText.text = $"Speed: {CurrentSpeed:F1} / {playerController.MaxSpeed:F1}";
        }
    }
    
    private void UpdateBoostUI()
    {
        if (playerController == null) return;
        
        // Update boost status text
        if (boostText != null)
        {
            if (playerController.IsBoosting)
            {
                boostText.text = "BOOSTING!";
                boostText.color = Color.yellow;
            }
            else
            {
                boostText.text = "Boost Ready";
                boostText.color = Color.white;
            }
        }
        
        // Update boost cooldown visual (requires custom implementation in PlayerController)
        if (boostCooldownImage != null)
        {
            // This would require exposing cooldown timer from PlayerController
            // For now, just show boost status
            boostCooldownImage.color = playerController.IsBoosting ? Color.yellow : Color.white;
        }
    }
    
    private void UpdateGameUI()
    {
        // Update game time, score, etc.
        // This is where you'd update other UI elements
    }
    
    #endregion
    
    #region Public Methods - Game Control
    
    /// <summary>
    /// Restart game
    /// </summary>
    public void RestartGame()
    {
        // Reset game state
        gameActive = true;
        currentGameTime = gameTime;
        totalDistance = 0f;
        
        // Reset player
        if (playerController != null)
        {
            playerController.ResetMovement();
            playerController.transform.position = Vector3.zero;
        }
        
        // Reset camera
        if (cameraFollow != null)
        {
            cameraFollow.SnapToTarget();
        }
        
        // Reset speed objects
        if (speedObjectManager != null)
        {
            speedObjectManager.ForceUpdateAllObjects();
        }
        
        lastPosition = Vector3.zero;
        
        Debug.Log("Game Restarted!");
    }
    
    /// <summary>
    /// Pause/Resume game
    /// </summary>
    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
        Debug.Log($"Game {(Time.timeScale == 0f ? "Paused" : "Resumed")}");
    }
    
    /// <summary>
    /// Change player max speed (for power-ups, difficulty, etc.)
    /// </summary>
    public void ChangePlayerSpeed(float newMaxSpeed)
    {
        if (playerController != null)
        {
            playerController.SetMaxSpeed(newMaxSpeed);
            
            // Update UI slider max value
            if (speedSlider != null)
            {
                speedSlider.maxValue = newMaxSpeed;
            }
            
            Debug.Log($"Player max speed changed to: {newMaxSpeed}");
        }
    }
    
    #endregion
    
    #region Public Methods - Effects and Power-ups
    
    /// <summary>
    /// Activate temporary speed boost (power-up)
    /// </summary>
    public void ActivateSpeedPowerUp(float multiplier, float duration)
    {
        StartCoroutine(SpeedPowerUpCoroutine(multiplier, duration));
    }
    
    private System.Collections.IEnumerator SpeedPowerUpCoroutine(float multiplier, float duration)
    {
        if (playerController == null) yield break;
        
        float originalMaxSpeed = playerController.MaxSpeed;
        float boostedSpeed = originalMaxSpeed * multiplier;
        
        // Apply boost
        playerController.SetMaxSpeed(boostedSpeed);
        Debug.Log($"Speed Power-Up! {originalMaxSpeed} -> {boostedSpeed} for {duration}s");
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Restore original speed
        playerController.SetMaxSpeed(originalMaxSpeed);
        Debug.Log("Speed Power-Up ended");
    }
    
    /// <summary>
    /// Add speed-based object dynamically
    /// </summary>
    public void AddSpeedObject(GameObject obj, float minSpeed, float maxSpeed, bool activeWhenSlow)
    {
        if (speedObjectManager != null)
        {
            speedObjectManager.AddSpeedBasedObject(obj, minSpeed, maxSpeed, activeWhenSlow);
            Debug.Log($"Added speed object: {obj.name}");
        }
    }
    
    #endregion
    
    #region Debug and Testing
    
    [ContextMenu("Test Speed Boost")]
    private void TestSpeedBoost()
    {
        if (playerController != null)
        {
            playerController.ForceBoost();
        }
    }
    
    [ContextMenu("Test Speed Power-Up")]
    private void TestSpeedPowerUp()
    {
        ActivateSpeedPowerUp(1.5f, 5f);
    }
    
    [ContextMenu("Show Game Stats")]
    private void ShowGameStats()
    {
        Debug.Log("=== GAME STATS ===");
        Debug.Log($"Current Speed: {CurrentSpeed:F2}");
        Debug.Log($"Speed Percentage: {SpeedPercentage * 100:F1}%");
        Debug.Log($"Total Distance: {totalDistance:F2}");
        Debug.Log($"Game Time Remaining: {currentGameTime:F1}");
        Debug.Log($"Active Speed Objects: {(speedObjectManager?.ActiveObjectCount ?? 0)}");
    // CameraDistance no longer provided by CameraFollow; show whether cameraFollow is assigned instead
    Debug.Log($"Camera Follow Assigned: {(cameraFollow != null)}");
    }
    
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Simple debug overlay
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 200));
        GUILayout.Label("=== DEBUG INFO ===");
        GUILayout.Label($"Speed: {CurrentSpeed:F1}");
        GUILayout.Label($"Boost: {(playerController?.IsBoosting ?? false)}");
        GUILayout.Label($"Distance: {totalDistance:F1}");
        GUILayout.Label($"Time: {currentGameTime:F1}");
        
        if (GUILayout.Button("Force Boost"))
        {
            TestSpeedBoost();
        }
        
        if (GUILayout.Button("Restart"))
        {
            RestartGame();
        }
        
        if (GUILayout.Button("Toggle Pause"))
        {
            TogglePause();
        }
        
        GUILayout.EndArea();
    }
    
    #endregion
}