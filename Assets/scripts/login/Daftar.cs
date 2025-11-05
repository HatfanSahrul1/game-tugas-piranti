using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Daftar : MonoBehaviour
{
    [SerializeField] private BackendManager backendManager;
    [SerializeField] private TMP_InputField usernameInput, passwordInput, confirmPasswordInput;
    [SerializeField] private string loginSceneName = "login";
    
    [Header("UI Feedback (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;

    private void OnEnable()
    {
        // Subscribe to events
        BackendManager.OnUserCreated += OnUserCreationResponse;
        BackendManager.OnAttributesCreated += OnAttributesCreationResponse;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        BackendManager.OnUserCreated -= OnUserCreationResponse;
        BackendManager.OnAttributesCreated -= OnAttributesCreationResponse;
    }
    
    private void OnUserCreationResponse(bool success, string message)
    {
        if (success)
        {
            Debug.Log("Registration successful! User created, proceeding to login...");
            
            // Update status text if available
            if (statusText != null)
            {
                statusText.text = "Registration successful! Redirecting to login...";
                statusText.color = Color.green;
            }
            
            // Directly go to login scene
            // Attributes will be created automatically on first login if they don't exist
            StartCoroutine(LoadLoginSceneAfterDelay());
        }
        else
        {
            Debug.LogError($"Registration failed: {message}");
            
            // Update status text if available
            if (statusText != null)
            {
                statusText.text = $"Registration failed: {message}";
                statusText.color = Color.red;
            }
        }
    }
    
    private void OnAttributesCreationResponse(bool success, string message)
    {
        if (success)
        {
            Debug.Log("Player attributes created successfully! Loading login scene...");
            
            // Update status text if available
            if (statusText != null)
            {
                statusText.text = "Account setup complete! Loading login...";
                statusText.color = Color.green;
            }
            
            // Wait a moment then load login scene
            StartCoroutine(LoadLoginSceneAfterDelay());
        }
        else
        {
            Debug.LogError($"Failed to create attributes: {message}");
            
            // Even if attributes creation fails, still proceed to login
            // User can create attributes later
            if (statusText != null)
            {
                statusText.text = "Registration complete! Loading login...";
                statusText.color = Color.yellow;
            }
            
            StartCoroutine(LoadLoginSceneAfterDelay());
        }
    }
    
    private IEnumerator LoadLoginSceneAfterDelay()
    {
        // Wait 1.5 seconds to show success message
        yield return new WaitForSeconds(1.5f);
        
        // Load login scene
        SceneManager.LoadScene(loginSceneName);
    }

    // Function to be called by UI Button
    public void OnDaftarButtonClicked()
    {
        RegisterUser();
    }

    public void RegisterUser()
    {
        // Validate inputs
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            string errorMsg = "Username and password cannot be empty!";
            Debug.LogError(errorMsg);
            
            if (statusText != null)
            {
                statusText.text = errorMsg;
                statusText.color = Color.red;
            }
            return;
        }
        
        if (passwordInput.text != confirmPasswordInput.text)
        {
            string errorMsg = "Passwords do not match!";
            Debug.LogError(errorMsg);
            
            if (statusText != null)
            {
                statusText.text = errorMsg;
                statusText.color = Color.red;
            }
            return;
        }

        // Show loading status
        if (statusText != null)
        {
            statusText.text = "Creating account...";
            statusText.color = Color.yellow;
        }

        // Call backend to create user
        backendManager.CreateUser(usernameInput.text, passwordInput.text);
    }
}
