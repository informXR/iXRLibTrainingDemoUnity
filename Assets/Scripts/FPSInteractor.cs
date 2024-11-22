using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FPSInteractor : MonoBehaviour
{
    public Camera fpsCamera;
    public float interactionRange = 5f;
    public KeyCode grabKey = KeyCode.E;
    public KeyCode releaseKey = KeyCode.R;

    private XRGrabInteractable grabbedObject;
    private XRSocketInteractor currentSocket;
    private GameObject statusText;

    void Start()
    {
        statusText = FindObjectOfType<BasicUI>().StatusText;
        if (statusText != null)
        {
            statusText.SetActive(false); 
        }
        else
        {
            Debug.LogError("StatusText object not found. Please assign it in BasicUI.");
        }
    }

    void Update()
    {
        HandleRaycast();
        HandleInteraction();
    }

    private void HandleRaycast()
    {
        Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            XRGrabInteractable interactable = hit.collider.GetComponent<XRGrabInteractable>();
            if (interactable != null)
            {
                if (statusText != null && !statusText.activeSelf && grabbedObject == null)
                {
                    statusText.SetActive(true); 
                }
                return; 
            }
        }

        if (statusText != null && statusText.activeSelf)
        {
            statusText.SetActive(false);
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(grabKey))
        {
            TryGrabObject();
        }

        // if (Input.GetKeyDown(releaseKey) && grabbedObject != null)
        // {
        //     ReleaseObject();
        // }
    }

    private void TryGrabObject()
    {
        if (grabbedObject != null) return;

        Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {           
            // If not looking at a socket, try grabbing
            XRGrabInteractable interactable = hit.collider.GetComponent<XRGrabInteractable>();

            if (interactable != null && grabbedObject == null)
            {
                grabbedObject = interactable;
                interactable.transform.SetParent(fpsCamera.transform); // Attach to FPS camera
                interactable.enabled = false; // Temporarily disable XRGrabInteractable
                interactable.GetComponent<Rigidbody>().useGravity = false;
                interactable.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    public void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);
            grabbedObject.enabled = true; // Re-enable XRGrabInteractable
            grabbedObject = null;
        }
    }
}
