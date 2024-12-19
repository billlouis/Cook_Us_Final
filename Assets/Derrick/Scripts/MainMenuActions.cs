using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuActions : MonoBehaviour
{
    // Function to start the game
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // Replace "GameScene" with the name of your game scene
    }

    // Function to open settings
    public void OpenSettings()
    {
        // Code to load the settings scene
        SceneManager.LoadScene("SettingsScene");
    }

    // Function to quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
