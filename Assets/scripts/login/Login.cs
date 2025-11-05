using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Login : MonoBehaviour
{
    [SerializeField] private BackendManager backendManager;
    [SerializeField] private TMP_InputField usernameInput, passwordInput;
    [SerializeField] private string gameSceneName = "SampleScene";
    
    [Header("UI Feedback (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    // Store player data for this session
    public static BackendManager.PlayerData CurrentPlayerData { get; private set; }
    public static BackendManager.PlayerAttributes CurrentPlayerAttributes { get; private set; }

    private void OnEnable()
    {
        // Subscribe to events
        BackendManager.OnUserLoggedIn += OnUserLoginResponse;
        BackendManager.OnAttributesLoaded += OnAttributesLoadResponse;
        BackendManager.OnAttributesCreated += OnAttributesCreatedResponse;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        BackendManager.OnUserLoggedIn -= OnUserLoginResponse;
        BackendManager.OnAttributesLoaded -= OnAttributesLoadResponse;
        BackendManager.OnAttributesCreated -= OnAttributesCreatedResponse;
    }
    
    private void OnUserLoginResponse(bool success, string message, BackendManager.PlayerData playerData)
    {
        if (success && playerData != null)
        {
            Debug.Log($"Login successful! Player ID: {playerData.id}, Username: {playerData.username}");
            
            // Store player data
            CurrentPlayerData = playerData;
            
            // Update status text
            if (statusText != null)
            {
                statusText.text = "Login successful! Loading player data...";
                statusText.color = Color.green;
            }
            
            // Load player attributes
            backendManager.GetAttributes(playerData.id);
        }
        else
        {
            Debug.LogError($"Login failed: {message}");
            
            // Update status text
            if (statusText != null)
            {
                statusText.text = $"Login failed: {message}";
                statusText.color = Color.red;
            }
        }
    }
    
    private void OnAttributesLoadResponse(bool success, string message, BackendManager.PlayerAttributes attributes)
    {
        if (success && attributes != null)
        {
            Debug.Log($"Player attributes loaded! Score: {attributes.score}, Coin: {attributes.coin}");
            
            // Store attributes
            CurrentPlayerAttributes = attributes;
            
            // Update status text
            if (statusText != null)
            {
                statusText.text = "Player data loaded! Starting game...";
                statusText.color = Color.green;
            }
            
            // Load game scene
            StartCoroutine(LoadGameSceneAfterDelay());
        }
        else
        {
            Debug.LogWarning($"Attributes not found for player {CurrentPlayerData.id}. Creating new attributes...");
            
            // Update status text
            if (statusText != null)
            {
                statusText.text = "Creating player profile...";
                statusText.color = Color.yellow;
            }
            
            // Create attributes for this player
            backendManager.CreateAttributes(CurrentPlayerData.id);
        }
    }
    
    private void OnAttributesCreatedResponse(bool success, string message)
    {
        if (success)
        {
            Debug.Log("Player attributes created successfully! Loading attributes...");
            
            // Update status text
            if (statusText != null)
            {
                statusText.text = "Player profile created! Loading data...";
                statusText.color = Color.green;
            }
            
            // Now load the newly created attributes
            backendManager.GetAttributes(CurrentPlayerData.id);
        }
        else
        {
            Debug.LogError($"Failed to create attributes: {message}");
            
            // Create default attributes locally and proceed
            CurrentPlayerAttributes = new BackendManager.PlayerAttributes
            {
                username = CurrentPlayerData.username,
                score = 0,
                coin = 0,
                greenSkin = 0,
                redSkin = 0,
                blueSkin = 0
            };
            
            if (statusText != null)
            {
                statusText.text = "Using default profile. Starting game...";
                statusText.color = Color.yellow;
            }
            
            StartCoroutine(LoadGameSceneAfterDelay());
        }
    }
    
    private IEnumerator LoadGameSceneAfterDelay()
    {
        // Wait 1.5 seconds to show success message
        yield return new WaitForSeconds(1.5f);
        
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }

    // Function to be called by UI Button
    public void OnLoginButtonClicked()
    {
        LoginUser();
    }
    
    // Function for back to register button
    public void BackToDaftar()
    {
        SceneManager.LoadScene("Daftar");
    }

    public void LoginUser()
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

        // Show loading status
        if (statusText != null)
        {
            statusText.text = "Logging in...";
            statusText.color = Color.yellow;
        }

        // Call backend to login
        backendManager.Login(usernameInput.text, passwordInput.text);
    }
    
    // Utility method to access player data from other scripts
    public static bool IsPlayerLoggedIn()
    {
        return CurrentPlayerData != null;
    }
    
    public static void ClearPlayerData()
    {
        CurrentPlayerData = null;
        CurrentPlayerAttributes = null;
    }
}