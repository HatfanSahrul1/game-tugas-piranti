using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BackendManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:5000/api";

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
        StartCoroutine(PostRequest($"{baseUrl}/create_user", new UserData(username, password)));
    }

    // Login
    public void Login(string username, string password)
    {
        StartCoroutine(PostRequest($"{baseUrl}/login", new UserData(username, password)));
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

    // Helper class for JSON serialization
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