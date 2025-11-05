using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple Main Menu manager: pressing any key starts the SampleScene.
/// Attach to the main menu scene's canvas or an empty GameObject.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    public string sceneToLoad = "SampleScene";

    // private void Update()
    // {
    //     // Start the game on any key or mouse button press
    //     if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
    //     {
    //         StartGame();
    //     }
    // }

    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
