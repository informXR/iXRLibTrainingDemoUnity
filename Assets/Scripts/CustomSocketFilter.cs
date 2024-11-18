using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomSocketFilter : MonoBehaviour
{
    public GrabbableObjectManager.GrabbableObjectType validType;
    private Renderer socketRenderer;
    private XRGrabInteractable pendingInteractable;
    private bool isValidSocket = false;
    public GameObject statusText;

    Color mainColor;

    void Start()
    {
        // Get the GridManager type for validation
        validType = GetComponentInParent<GridManager>().Type;
        mainColor = GetComponent<Renderer>().material.color;
        // Get the Renderer to change colors
        socketRenderer = GetComponent<Renderer>();
        if (socketRenderer != null)
        {
            socketRenderer.material.color = mainColor; // Default color
        }

        statusText = FindObjectOfType<BasicUI>().StatusText;
    }

    private void OnTriggerEnter(Collider other)
    {
        ElementTag elementTag = other.GetComponent<ElementTag>();
        if (elementTag != null)
        {
            // Check if the object's type matches the socket's valid type
            isValidSocket = (elementTag.type == validType);
            pendingInteractable = other.GetComponent<XRGrabInteractable>();

            if (isValidSocket)
            {
                // Set socket color to green if valid
                socketRenderer.material.color = Color.green;
                statusText.SetActive(true);
            }
            else
            {
                // Set socket color to red if invalid
                socketRenderer.material.color = Color.red;
                statusText.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Reset to default state when the object exits
        if (other.GetComponent<XRGrabInteractable>() == pendingInteractable)
        {
            socketRenderer.material.color = mainColor;
            pendingInteractable = null;
            isValidSocket = false;
        }
    }

    void Update()
    {
        // Check if the user presses the 'E' key and the socket is valid
        if (Input.GetKeyDown(KeyCode.E) && isValidSocket && pendingInteractable != null)
        {
            AttachObjectToSocket(pendingInteractable);

            // Reset the socket color after attaching
            socketRenderer.material.color = mainColor;

            // Clear pending interactable
            pendingInteractable = null;
            isValidSocket = false;
            statusText.SetActive(false);
        }
    }

    private void AttachObjectToSocket(XRGrabInteractable interactable)
    {
        // Parent the object to the socket
        interactable.transform.SetParent(transform);

        // Snap to socket's position and rotation
        interactable.transform.position = transform.position;
        interactable.transform.rotation = transform.rotation;

        // Disable Rigidbody physics for stable placement
        Rigidbody rb = interactable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Notify the GridManagerManager to add a score
        GetComponentInParent<GridManagerManager>().AddScore(1);

        // Optionally disable interactable to prevent further grabbing
        interactable.enabled = false;
    }
}
