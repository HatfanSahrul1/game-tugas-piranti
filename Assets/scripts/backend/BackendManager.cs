using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class BackendManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:5000/api";
    
    // Events for callbacks
    public static event Action<bool, string> OnUserCreated; // success, message
    public static event Action<bool, string, PlayerData> OnUserLoggedIn; // success, message, playerData
    public static event Action<bool, string> OnAttributesCreated; // success, message
    public static event Action<bool, string, PlayerAttributes> OnAttributesLoaded; // success, message, attributes
    public static event Action<bool, string> OnAttributesUpdated; // success, message

    // Example usage
    // void Start()
    // {
    //     // Test connection first
    //     TestConnection();
        
    //     // Wait a bit then test other endpoints
    //     StartCoroutine(TestAfterDelay());
    // }

    // Test connection
    public void TestConnection()
    {
        StartCoroutine(GetRequest($"{baseUrl}/ping"));
    }

    // Get Scores
    public void GetScores()
    {
        StartCoroutine(GetRequest($"{baseUrl}/scores"));
    }

    // Create User
    public void CreateUser(string username, string password)
    {
        StartCoroutine(PostRequestForUserCreation($"{baseUrl}/create_user", new UserData(username, password)));
    }

    // Login
    public void Login(string username, string password)
    {
        StartCoroutine(PostRequestForLogin($"{baseUrl}/login", new UserData(username, password)));
    }
    
    // Create Attributes
    public void CreateAttributes(int playerId)
    {
        StartCoroutine(PostRequestForCreateAttributes($"{baseUrl}/create_attributes/{playerId}", new CreateAttributesData()));
    }
    
    // Get Attributes  
    public void GetAttributes(int playerId)
    {
        StartCoroutine(GetRequestForAttributes($"{baseUrl}/user_data/{playerId}"));
    }
    
    // Update Attributes
    public void UpdateAttributes(int playerId, int score, int coin, int greenSkin, int redSkin, int blueSkin)
    {
        UpdateAttributesData data = new UpdateAttributesData(score, coin, greenSkin, redSkin, blueSkin);
        StartCoroutine(PostRequestForUpdateAttributes($"{baseUrl}/update_attributes/{playerId}", data));
    }

    // Coroutine for GET request
    private IEnumerator GetRequest(string url)
    {
        Debug.Log($"Sending GET request to: {url}");
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Set timeout
            webRequest.timeout = 10;
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"GET Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"GET Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
            }
        }
    }

    // Coroutine for POST request
    private IEnumerator PostRequest(string url, UserData data)
    {
        Debug.Log($"Sending POST request to: {url}");
        
        // Serialize data to JSON
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"JSON Data: {jsonData}");

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            // Set timeout
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"POST Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"POST Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
            }
        }
    }

    // Specialized POST request for user creation with callback
    private IEnumerator PostRequestForUserCreation(string url, UserData data)
    {
        Debug.Log($"Sending POST request to: {url}");
        
        // Serialize data to JSON
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"JSON Data: {jsonData}");

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            // Set timeout
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"POST Response: {webRequest.downloadHandler.text}");
                // Invoke success callback
                OnUserCreated?.Invoke(true, "User created successfully!");
            }
            else
            {
                Debug.LogError($"POST Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                // Invoke failure callback
                OnUserCreated?.Invoke(false, $"Registration failed: {webRequest.error}");
            }
        }
    }
    
    // Specialized POST request for login with callback
    private IEnumerator PostRequestForLogin(string url, UserData data)
    {
        Debug.Log($"Sending POST request to: {url}");
        
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"JSON Data: {jsonData}");

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Login Response: {webRequest.downloadHandler.text}");
                
                try
                {
                    // Parse login response
                    LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(webRequest.downloadHandler.text);
                    
                    if (loginResponse.success && loginResponse.user != null)
                    {
                        // Convert to PlayerData format
                        PlayerData playerData = new PlayerData
                        {
                            id = loginResponse.user.id,
                            username = loginResponse.user.username
                        };
                        OnUserLoggedIn?.Invoke(true, "Login successful!", playerData);
                    }
                    else
                    {
                        OnUserLoggedIn?.Invoke(false, "Login failed", null);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing login response: {e.Message}");
                    OnUserLoggedIn?.Invoke(false, "Login response parsing failed", null);
                }
            }
            else
            {
                Debug.LogError($"Login Error: {webRequest.error}");
                OnUserLoggedIn?.Invoke(false, $"Login failed: {webRequest.error}", null);
            }
        }
    }
    
    // Specialized POST request for creating attributes
    private IEnumerator PostRequestForCreateAttributes(string url, CreateAttributesData data)
    {
        Debug.Log($"Sending POST request to: {url}");
        
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"JSON Data: {jsonData}");

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Create Attributes Response: {webRequest.downloadHandler.text}");
                OnAttributesCreated?.Invoke(true, "Attributes created successfully!");
            }
            else
            {
                Debug.LogError($"Create Attributes Error: {webRequest.error}");
                OnAttributesCreated?.Invoke(false, $"Failed to create attributes: {webRequest.error}");
            }
        }
    }
    
    // Specialized GET request for loading attributes
    private IEnumerator GetRequestForAttributes(string url)
    {
        Debug.Log($"Sending GET request to: {url}");
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 10;
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Get Attributes Response: {webRequest.downloadHandler.text}");
                
                try
                {
                    PlayerAttributes attributes = JsonUtility.FromJson<PlayerAttributes>(webRequest.downloadHandler.text);
                    OnAttributesLoaded?.Invoke(true, "Attributes loaded successfully!", attributes);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing attributes response: {e.Message}");
                    OnAttributesLoaded?.Invoke(false, "Failed to parse attributes", null);
                }
            }
            else
            {
                Debug.LogError($"Get Attributes Error: {webRequest.error}");
                OnAttributesLoaded?.Invoke(false, $"Failed to load attributes: {webRequest.error}", null);
            }
        }
    }

    // Helper classes for JSON serialization
    [System.Serializable]
    private class UserData
    {
        public string username;
        public string password;

        public UserData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
    
    [System.Serializable]
    public class LoginResponse
    {
        public bool success;
        public UserInfo user;
    }
    
    [System.Serializable]
    public class UserInfo
    {
        public int id;
        public string username;
    }
    
    [System.Serializable]
    public class PlayerData
    {
        public int id;
        public string username;
        public string created_at;
    }
    
    [System.Serializable]
    public class PlayerAttributes
    {
        public string username;
        public int score;
        public int coin;
        public int greenSkin;
        public int redSkin;
        public int blueSkin;
    }
    
    [System.Serializable]
    private class CreateAttributesData
    {
        public int score = 0;
        public int coin = 0;
        public int greenSkin = 0;
        public int redSkin = 0;
        public int blueSkin = 0;
    }
    
    [System.Serializable]
    private class UpdateAttributesData
    {
        public int score;
        public int coin;
        public int greenSkin;
        public int redSkin;
        public int blueSkin;

        public UpdateAttributesData(int score, int coin, int greenSkin, int redSkin, int blueSkin)
        {
            this.score = score;
            this.coin = coin;
            this.greenSkin = greenSkin;
            this.redSkin = redSkin;
            this.blueSkin = blueSkin;
        }
    }
    
    // Specialized POST request for updating attributes
    private IEnumerator PostRequestForUpdateAttributes(string url, UpdateAttributesData data)
    {
        Debug.Log($"Sending POST request to: {url}");
        
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"JSON Data: {jsonData}");

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Update Attributes Response: {webRequest.downloadHandler.text}");
                // Parse response for success confirmation
                try
                {
                    var response = JsonUtility.FromJson<UpdateResponse>(webRequest.downloadHandler.text);
                    if (response.success)
                    {
                        Debug.Log("Attributes updated successfully in database!");
                        OnAttributesUpdated?.Invoke(true, "Attributes updated successfully!");
                    }
                    else
                    {
                        OnAttributesUpdated?.Invoke(false, "Failed to update attributes");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing update response: {e.Message}");
                    OnAttributesUpdated?.Invoke(false, "Failed to parse update response");
                }
            }
            else
            {
                Debug.LogError($"Update Attributes Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                OnAttributesUpdated?.Invoke(false, $"Update failed: {webRequest.error}");
            }
        }
    }
    
    [System.Serializable]
    private class UpdateResponse
    {
        public bool success;
        public string message;
        public int playerId;
    }
    
    private IEnumerator TestAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        
        // Test create user
        CreateUser("Player100", "password1234");
        
        yield return new WaitForSeconds(2f);
        
        // Test login
        Login("Player100", "password1234");
        
        yield return new WaitForSeconds(2f);
        
        // Test get scores
        GetScores();
    }
}