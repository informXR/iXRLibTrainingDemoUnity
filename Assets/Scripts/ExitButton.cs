using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitButton : MonoBehaviour
{
    public void OnSelect(SelectEnterEventArgs args)
    {
        iXR.LogInfo("Exit BLOCK pressed");
        Debug.Log("Exit BLOCK pressed");
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}