using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BasicUI : MonoBehaviour
{
    public Button quitButton;
    public Button restartButton;  
    public GameObject StatusText;
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
