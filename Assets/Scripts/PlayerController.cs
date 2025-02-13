using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameObject xrRig;           // Assign your XR Rig object here
    public GameObject desktopCamera;   // Assign your Desktop Camera Rig here
    
    private PlayerControls controls;   // Your Input Action asset

    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake()
    {
        // Instantiate your input actions asset
        controls = new PlayerControls();

        // Bind desktop input actions
        controls.DesktopControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.DesktopControls.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.DesktopControls.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.DesktopControls.Look.canceled += ctx => lookInput = Vector2.zero;

        // You can also bind VR controls if needed
    }

    private void Start()
    {
        // Example: Use compiler directive or runtime check to decide which rig to activate.
        #if UNITY_WEBGL
            ActivateDesktopMode();
        #else
            // Assuming non-WebGL is VR for this example.
            ActivateVRMode();
        #endif
    }

    private void ActivateVRMode()
    {
        if (xrRig != null)
            xrRig.SetActive(true);
        if (desktopCamera != null)
            desktopCamera.SetActive(false);

        // Optionally, enable VR action maps if needed:
        controls.VRControls.Enable();
        controls.DesktopControls.Disable();
    }

    private void ActivateDesktopMode()
    {
        if (xrRig != null)
            xrRig.SetActive(false);
        if (desktopCamera != null)
            desktopCamera.SetActive(true);

        // Enable desktop action map
        controls.DesktopControls.Enable();
        controls.VRControls.Disable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // For desktop mode, update movement and look
        #if UNITY_WEBGL
            HandleDesktopMovement();
        #endif

        // For VR, you might not need to manually update movement if you're using the XR rig’s built-in features.
    }

    private void HandleDesktopMovement()
    {
        // Example movement logic:
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        transform.Translate(moveDirection * Time.deltaTime * 5f, Space.World);

        // Example look logic:
        float lookX = lookInput.x * Time.deltaTime * 2f;
        transform.Rotate(0, lookX, 0);

        // For vertical look, consider rotating the camera child:
        // float lookY = lookInput.y * Time.deltaTime * 2f;
        // Camera.main.transform.Rotate(-lookY, 0, 0);
    }
}
