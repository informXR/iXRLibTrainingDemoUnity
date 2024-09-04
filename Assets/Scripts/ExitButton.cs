using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitButton : MonoBehaviour
{
    public void OnSelect(SelectEnterEventArgs args)
    {
        iXR.LogInfo("Exit button pressed");
        Debug.Log("Exit button pressed");
        StartCoroutine(ExitGame());
    }

    private IEnumerator ExitGame()
    {
        // Perform any cleanup or saving operations here
        // For example: SaveGameState();

        yield return new WaitForSeconds(0.5f); // Short delay for any final operations

        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }
}