using UnityEngine;

/// <summary>
/// Simple test script untuk SmartInputManager
/// Attach ke GameObject yang sama dengan SmartInputManager
/// </summary>
public class InputTest : MonoBehaviour
{
    private SmartInputManager inputManager;
    
    void Start()
    {
        inputManager = GetComponent<SmartInputManager>();
        
        if (inputManager == null)
        {
            Debug.LogError("SmartInputManager not found on this GameObject!");
        }
        else
        {
            Debug.Log("SmartInputManager found and ready!");
        }
    }
    
    void Update()
    {
        if (inputManager == null) return;
        
        // Test input values
        Vector2 aim = inputManager.AimInput;
        bool boost = inputManager.BoostInput;
        bool gamepadActive = inputManager.IsGamepadActive;
        
        // Debug output setiap detik
        if (Time.time % 1f < Time.deltaTime)
        {
            Debug.Log($"Input Device: {(gamepadActive ? "Gamepad" : "Mouse")} | Aim: {aim} | Boost: {boost}");
        }
        
        // Visual feedback
        if (aim.magnitude > 0.1f)
        {
            Debug.DrawRay(transform.position, (Vector3)aim * 2f, gamepadActive ? Color.green : Color.blue, Time.deltaTime);
        }
    }
}