using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Triggers game over when fuel is empty (or manually). Pauses the game and shows a UI panel.
/// Attach to an empty GameObject (e.g., GameManager) and assign references in Inspector.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    public FuelSystem fuelSystem;           // optional: auto-find if null
    public UIManager uiManager;             // optional: auto-find if null
    public GameObject gameOverPanel;        // UI panel to enable on game over
    public Text finalTimeText;              // text inside panel to show survival time
    public Button restartButton;            // optional: restart
    public Button quitButton;               // optional: quit

    [Header("Behaviour")]
    public bool autoPause = true;           // pause game on game over
    public string mainMenuSceneName = "MainMenu";

    private bool isGameOver = false;

    private void Start()
    {
        // Do not auto-register listeners. Keep this manager minimal and explicit.
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Manual trigger for game over (can be called from other systems).
    /// </summary>
    /// <summary>
    /// Show the Game Over UI and pause the game. Call this explicitly when needed.
    /// </summary>
    public void ShowGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // stop UI timer
        uiManager?.StopTimer();

        // pause game time (physics/Update depend on timeScale)
        if (autoPause)
            Time.timeScale = 0f;

        // show panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // set final time text if available
        if (finalTimeText != null)
        {
            float t = uiManager != null ? uiManager.SurvivalTime : 0f;
            finalTimeText.text = $"Survived: {t:F1}s";
        }
    }

    private void Update()
    {
        if (!isGameOver) return;

        // Press any key or mouse button to go to main menu
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            // ensure timeScale restored
            Time.timeScale = 1f;
            // load the main menu scene (name set in inspector)
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Restart current scene (unpauses first).
    /// </summary>
    public void Restart()
    {
        // restore time scale before reloading
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Quit application (works in build).
    /// </summary>
    public void QuitGame()
    {
        // restore time scale in case running in editor
        Time.timeScale = 1f;
        Application.Quit();
    }
}
