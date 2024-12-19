using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
