using UnityEngine;

/// <summary>
/// Legacy Plane script - Sekarang digantikan oleh PlayerController.cs
/// Script ini bisa dihapus atau digunakan untuk setup komponen tambahan
/// </summary>
public class Plane : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private SpeedBasedObjectManager speedObjectManager;
    
    private void Awake()
    {
        // Auto-assign components jika belum diset di Inspector
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
            
        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();
            
        if (speedObjectManager == null)
            speedObjectManager = FindObjectOfType<SpeedBasedObjectManager>();
    }
    
    private void Start()
    {
        // Setup connections antar komponen
        SetupComponentConnections();
        
        Debug.Log("Plane Setup Complete! Ready to fly!");
    }
    
    private void SetupComponentConnections()
    {
        // Pastikan CameraFollow mengikuti player ini
        if (cameraFollow != null && playerController != null)
        {
            cameraFollow.SetTarget(playerController.transform);
        }
        
        // Pastikan SpeedObjectManager menggunakan PlayerController ini
        if (speedObjectManager != null && playerController != null)
        {
            speedObjectManager.SetPlayerController(playerController);
        }
    }
    
    // Method untuk debugging atau testing
    [ContextMenu("Test Boost")]
    private void TestBoost()
    {
        if (playerController != null)
        {
            playerController.ForceBoost();
        }
    }
    
    [ContextMenu("Reset Movement")]
    private void TestResetMovement()
    {
        if (playerController != null)
        {
            playerController.ResetMovement();
        }
    }
}
