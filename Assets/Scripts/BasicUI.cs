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
        SoundManager.Instance.PlayButtonClickSound();
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    private void RestartLevel()
    {
        // Reloads the current active scene
        SoundManager.Instance.PlayButtonClickSound();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
