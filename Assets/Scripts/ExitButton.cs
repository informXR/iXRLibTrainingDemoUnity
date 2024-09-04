using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitButton : MonoBehaviour
{
    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        interactable.selectEntered.AddListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        iXR.LogInfo("Exit button pressed");
        Debug.Log("Exit button pressed");
        StartCoroutine(ExitGame());
    }

    private IEnumerator ExitGame()
    {
        // Perform any cleanup or saving operations here
        // SaveGameState();

        yield return new WaitForSeconds(0.5f); // Short delay for cleanup

        #if UNITY_ANDROID && !UNITY_EDITOR
            //Application.Quit();
            // Return to launcher instead of quitting
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("moveTaskToBack", true);
        #elif UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}