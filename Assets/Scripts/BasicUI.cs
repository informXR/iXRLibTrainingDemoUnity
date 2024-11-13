using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BasicUI : MonoBehaviour
{
    public Button quitButton;     // Assign this button in the Inspector for quitting the game
    public Button restartButton;  // Assign this button in the Inspector for restarting the level

    void Start()
    {
        quitButton.onClick.AddListener(QuitGame);
        restartButton.onClick.AddListener(RestartLevel);
    }

    private void QuitGame()
    {
        // Exits the application (does nothing in the editor)
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    private void RestartLevel()
    {
        // Reloads the current active scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
