using UnityEngine;

public class DataLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DataContainer dataContainer;
    [SerializeField] private DataInjector dataInjector;
    
    [Header("Settings")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool debugMode = true;
    
    private void Start()
    {
        // Auto-find components if not assigned
        if (dataContainer == null)
        {
            dataContainer = FindObjectOfType<DataContainer>();
        }
        
        if (dataInjector == null)
        {
            dataInjector = FindObjectOfType<DataInjector>();
        }
        
        if (loadOnStart)
        {
            LoadPlayerData();
        }
    }
    
    // Load player data dari PlayerPrefs
    public void LoadPlayerData()
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer not found! Cannot load player data.");
            return;
        }
        
        // Load data dari PlayerPrefs
        dataContainer.LoadFromPlayerPrefs();
        
        if (debugMode)
        {
            if (dataContainer.HasPlayerData())
            {
                Debug.Log("Player data loaded successfully:");
                Debug.Log(dataContainer.GetDataSummary());
            }
            else
            {
                Debug.Log("No player data found in PlayerPrefs");
            }
        }
    }
    
    // Check apakah ada saved data
    public bool HasSavedData()
    {
        if (dataContainer == null)
        {
            return false;
        }
        
        return dataContainer.HasPlayerData();
    }
    
    // Force reload dan inject ulang
    public void ForceReloadData()
    {
        if (dataInjector != null)
        {
            dataInjector.ReloadFromPlayerPrefs();
        }
        else
        {
            LoadPlayerData();
        }
    }
    
    // Get current player data untuk debugging
    public void ShowCurrentData()
    {
        if (dataContainer == null)
        {
            Debug.Log("DataContainer is null!");
            return;
        }
        
        Debug.Log("Current Player Data:");
        Debug.Log(dataContainer.GetDataSummary());
    }
    
    // Clear semua saved data
    public void ClearSavedData()
    {
        if (dataContainer != null)
        {
            dataContainer.ClearData();
            Debug.Log("Saved data cleared by DataLoader");
        }
    }
    
    // Utility method untuk mendapatkan specific data
    public int GetPlayerId()
    {
        return dataContainer != null ? dataContainer.playerId : 0;
    }
    
    public string GetUsername()
    {
        return dataContainer != null ? dataContainer.username : "";
    }
    
    public int GetScore()
    {
        return dataContainer != null ? dataContainer.score : 0;
    }
    
    public int GetCoin()
    {
        return dataContainer != null ? dataContainer.coin : 0;
    }
    
    public int GetGreenSkin()
    {
        return dataContainer != null ? dataContainer.greenSkin : 0;
    }
    
    public int GetRedSkin()
    {
        return dataContainer != null ? dataContainer.redSkin : 0;
    }
    
    public int GetBlueSkin()
    {
        return dataContainer != null ? dataContainer.blueSkin : 0;
    }
}