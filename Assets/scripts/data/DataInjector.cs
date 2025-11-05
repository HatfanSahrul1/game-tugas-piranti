using UnityEngine;

public class DataInjector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DataContainer dataContainer;
    
    private void Start()
    {
        // Auto-find DataContainer if not assigned
        if (dataContainer == null)
        {
            dataContainer = FindObjectOfType<DataContainer>();
        }
        
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer not found! Make sure DataContainer is in the scene.");
        }
    }
    
    // Inject data dari Backend login response
    public void InjectLoginData(BackendManager.PlayerData playerData, BackendManager.PlayerAttributes attributes)
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer is null! Cannot inject data.");
            return;
        }
        
        if (playerData == null || attributes == null)
        {
            Debug.LogError("Player data or attributes is null! Cannot inject data.");
            return;
        }
        
        // Inject semua data ke DataContainer
        dataContainer.SetPlayerData(
            playerData.id,
            playerData.username,
            attributes.score,
            attributes.coin,
            attributes.greenSkin,
            attributes.redSkin,
            attributes.blueSkin
        );
        
        Debug.Log($"Data injected successfully for player: {playerData.username}");
        Debug.Log(dataContainer.GetDataSummary());
    }
    
    // Inject data dari PlayerData saja (untuk fallback)
    public void InjectPlayerDataOnly(BackendManager.PlayerData playerData)
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer is null! Cannot inject data.");
            return;
        }
        
        if (playerData == null)
        {
            Debug.LogError("Player data is null! Cannot inject data.");
            return;
        }
        
        // Inject hanya player data, attributes default
        dataContainer.SetPlayerData(
            playerData.id,
            playerData.username,
            0, // default score
            0, // default coin
            0, // default green skin
            0, // default red skin
            0  // default blue skin
        );
        
        Debug.Log($"Player data injected (default attributes) for: {playerData.username}");
    }
    
    // Force reload data dari PlayerPrefs
    public void ReloadFromPlayerPrefs()
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer is null! Cannot reload data.");
            return;
        }
        
        dataContainer.LoadFromPlayerPrefs();
        Debug.Log("Data reloaded from PlayerPrefs");
        Debug.Log(dataContainer.GetDataSummary());
    }
    
    // Update score dan save
    public void UpdatePlayerScore(int newScore)
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer is null! Cannot update score.");
            return;
        }
        
        dataContainer.UpdateScore(newScore);
        Debug.Log($"Score updated to: {newScore}");
    }
    
    // Update coin dan save
    public void UpdatePlayerCoin(int newCoin)
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer is null! Cannot update coin.");
            return;
        }
        
        dataContainer.UpdateCoin(newCoin);
        Debug.Log($"Coin updated to: {newCoin}");
    }
    
    // Update skins dan save
    public void UpdatePlayerSkins(int green, int red, int blue)
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer is null! Cannot update skins.");
            return;
        }
        
        dataContainer.UpdateSkins(green, red, blue);
        Debug.Log($"Skins updated - Green: {green}, Red: {red}, Blue: {blue}");
    }
    
    // Clear semua data
    public void ClearPlayerData()
    {
        if (dataContainer == null)
        {
            Debug.LogError("DataContainer is null! Cannot clear data.");
            return;
        }
        
        dataContainer.ClearData();
        Debug.Log("Player data cleared by DataInjector");
    }
    
    // Get reference ke DataContainer untuk akses manual
    public DataContainer GetDataContainer()
    {
        return dataContainer;
    }
}