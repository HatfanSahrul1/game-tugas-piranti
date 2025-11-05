using UnityEngine;
using UnityEngine.InputSystem;

// Simple Input Manager untuk mengganti SmartInputManager
// Jika masih ada error kompilasi
public class SimpleInputManager : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    
    private bool gamepadActive = false;
    private Vector2 aimInput;
    private bool boostInput;
    
    public Vector2 AimInput => aimInput;
    public bool BoostInput => boostInput;
    public bool IsGamepadActive => gamepadActive;
    
    void Update()
    {
        // Reset boost input setiap frame
        boostInput = false;
        
        // Check gamepad
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            
            if (stick.magnitude > 0.1f)
            {
                if (!gamepadActive && debugMode)
                    Debug.Log("Gamepad detected");
                    
                gamepadActive = true;
                aimInput = stick.normalized;
            }
            
            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                boostInput = true;
            }
        }
        
        // Check mouse (only if gamepad not active)
        if (!gamepadActive && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 direction = worldPos - (Vector2)transform.position;
            
            if (direction.magnitude > 0.1f)
            {
                aimInput = direction.normalized;
            }
            
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                boostInput = true;
            }
        }
        
        // Timeout for gamepad
        if (gamepadActive && Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.magnitude < 0.05f)
            {
                // Could add timeout logic here
            }
        }
    }
}