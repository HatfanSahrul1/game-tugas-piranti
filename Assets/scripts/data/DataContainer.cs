using UnityEngine;

[System.Serializable]
public class DataContainer : MonoBehaviour
{
    [Header("Player Data")]
    public int playerId;
    public string username;
    public int score;
    public int coin;
    public int greenSkin;
    public int redSkin;
    public int blueSkin;
    
    [Header("Optional Backend Sync")]
    [SerializeField] private BackendManager backendManager; // Optional: assign manually
    
    // Static instance untuk akses global
    public static DataContainer Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern tanpa DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Load data dari PlayerPrefs saat start
        LoadFromPlayerPrefs();
    }
    
    // Set all data sekaligus
    public void SetPlayerData(int id, string name, int playerScore, int playerCoin, int green, int red, int blue)
    {
        playerId = id;
        username = name;
        score = playerScore;
        coin = playerCoin;
        greenSkin = green;
        redSkin = red;
        blueSkin = blue;
        
        // Auto save ke PlayerPrefs
        SaveToPlayerPrefs();
    }
    
    // Update score
    public void UpdateScore(int newScore)
    {
        score = newScore;
        SaveToPlayerPrefs();
        SyncWithBackend();
    }
    
    // Update coin
    public void UpdateCoin(int newCoin)
    {
        coin = newCoin;
        SaveToPlayerPrefs();
        SyncWithBackend();
    }
    
    // Update skins
    public void UpdateSkins(int green, int red, int blue)
    {
        greenSkin = green;
        redSkin = red;
        blueSkin = blue;
        SaveToPlayerPrefs();
        
        // Sync dengan backend jika player logged in
        SyncWithBackend();
    }
    
    // Sync semua attributes dengan backend
    public void SyncWithBackend()
    {
        if (playerId > 0) // Player must be logged in
        {
            // Try assigned reference first, then FindObjectOfType
            BackendManager backend = backendManager != null ? backendManager : FindObjectOfType<BackendManager>();
            
            if (backend != null)
            {
                Debug.Log($"Syncing attributes to backend for player {playerId}");
                backend.UpdateAttributes(playerId, score, coin, greenSkin, redSkin, blueSkin);
            }
            else
            {
                Debug.Log("BackendManager not found. Data saved locally only.");
            }
        }
        else
        {
            Debug.Log("Player not logged in. Attributes saved locally only.");
        }
    }
    
    // Save ke PlayerPrefs
    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt("PlayerId", playerId);
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("Coin", coin);
        PlayerPrefs.SetInt("GreenSkin", greenSkin);
        PlayerPrefs.SetInt("RedSkin", redSkin);
        PlayerPrefs.SetInt("BlueSkin", blueSkin);
        PlayerPrefs.Save();
        
        Debug.Log($"Data saved to PlayerPrefs: {username} (ID: {playerId})");
    }
    
    // Load dari PlayerPrefs
    public void LoadFromPlayerPrefs()
    {
        playerId = PlayerPrefs.GetInt("PlayerId", 0);
        username = PlayerPrefs.GetString("Username", "");
        score = PlayerPrefs.GetInt("Score", 0);
        coin = PlayerPrefs.GetInt("Coin", 0);
        greenSkin = PlayerPrefs.GetInt("GreenSkin", 0);
        redSkin = PlayerPrefs.GetInt("RedSkin", 0);
        blueSkin = PlayerPrefs.GetInt("BlueSkin", 0);
        
        Debug.Log($"Data loaded from PlayerPrefs: {username} (ID: {playerId})");
    }
    
    // Clear all data
    public void ClearData()
    {
        playerId = 0;
        username = "";
        score = 0;
        coin = 0;
        greenSkin = 0;
        redSkin = 0;
        blueSkin = 0;
        
        // Clear PlayerPrefs
        PlayerPrefs.DeleteKey("PlayerId");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("Score");
        PlayerPrefs.DeleteKey("Coin");
        PlayerPrefs.DeleteKey("GreenSkin");
        PlayerPrefs.DeleteKey("RedSkin");
        PlayerPrefs.DeleteKey("BlueSkin");
        PlayerPrefs.Save();
        
        Debug.Log("Player data cleared");
    }
    
    // Check if player data exists
    public bool HasPlayerData()
    {
        return playerId > 0 && !string.IsNullOrEmpty(username);
    }
    
    // Get data summary for debugging
    public string GetDataSummary()
    {
        return $"Player: {username} (ID: {playerId})\nScore: {score}, Coin: {coin}\nSkins - Green: {greenSkin}, Red: {redSkin}, Blue: {blueSkin}";
    }
}