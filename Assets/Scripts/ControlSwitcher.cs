using UnityEngine;
using UnityEngine.XR.Management;

public class ControlSwitcher : MonoBehaviour
{
    public GameObject vrRig;
    public GameObject nonVRCamera;

    void Start()
    {
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            // No VR headset, use non-VR controls
            vrRig.SetActive(false);
            nonVRCamera.SetActive(true);
            // Enable your non-VR movement script here
        }
        else
        {
            // VR headset detected, use VR controls
            vrRig.SetActive(true);
            nonVRCamera.SetActive(false);
        }
    }
}