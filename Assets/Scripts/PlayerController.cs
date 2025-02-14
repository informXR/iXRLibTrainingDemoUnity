using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    public GameObject VRRig;           // Assign your XR Rig object here
    public GameObject DesktopRig;   // Assign your Desktop Camera Rig here
    
    private PlayerControls _controls;   // Your Input Action asset

    private Vector2 _moveInput;
    private Vector2 _lookInput;

    private void Awake()
    {
        // Instantiate your input actions asset
        _controls = new PlayerControls();

        // Bind desktop input actions
        _controls.DesktopControls.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.DesktopControls.Move.canceled += ctx => _moveInput = Vector2.zero;

        _controls.DesktopControls.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _controls.DesktopControls.Look.canceled += ctx => _lookInput = Vector2.zero;
    }

    private void Start()
    {
        #if UNITY_WEBGL
            ActivateDesktopMode();
        #else
            // Assuming non-WebGL is VR
            ActivateVRMode();
        #endif
    }

    private void ActivateVRMode()
    {
        if (VRRig != null)
            VRRig.SetActive(true);
        if (DesktopRig != null)
            DesktopRig.SetActive(false);
        
        _controls.DesktopControls.Disable();
    }

    private void ActivateDesktopMode()
    {
        if (VRRig != null)
            VRRig.SetActive(false);
        if (DesktopRig != null)
            DesktopRig.SetActive(true);

        // Enable desktop action map
        _controls.DesktopControls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void Update()
    {
        // For desktop mode, update movement and look
        #if UNITY_WEBGL
            HandleDesktopMovement();
        #endif
    }

    private void HandleDesktopMovement()
    {
        // Example movement logic:
        Vector3 moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
        transform.Translate(moveDirection * Time.deltaTime * 5, Space.Self);

        if (Mouse.current.rightButton.isPressed)
        {
            // Example look logic:
            float lookX = _lookInput.x * Time.deltaTime * 5;
            transform.Rotate(0, lookX, 0);

            // For vertical look, consider rotating the camera child:
            float lookY = _lookInput.y * Time.deltaTime * 5;
            transform.Rotate(-lookY, 0, 0);
        }
    }
}
