using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    public GameObject VRRig;        // Assign your XR Rig object here
    public GameObject DesktopRig;   // Assign your Desktop Camera Rig here
    
    private PlayerControls _controls;   // Your Input Action asset

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    
    public Camera desktopCamera;
    private GameObject _grabbedObject = null;
    private Vector3 _grabOffset = Vector3.zero;
    private float _distance;

    // Assign the layer(s) that contain grabbable objects
    public LayerMask grabbableLayer;

    private void Awake()
    {
        // Instantiate your input actions asset
        _controls = new PlayerControls();

        // Bind desktop input actions
        _controls.DesktopControls.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.DesktopControls.Move.canceled += ctx => _moveInput = Vector2.zero;

        _controls.DesktopControls.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _controls.DesktopControls.Look.canceled += ctx => _lookInput = Vector2.zero;
        
        _controls.DesktopControls.Click.performed += OnMouseClick;
        _controls.DesktopControls.Click.canceled += OnMouseRelease;
    }
    
    private void OnEnable() => _controls.DesktopControls.Enable();
    private void OnDisable() => _controls.DesktopControls.Disable();

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

    private void Update()
    {
        // For desktop mode, update movement and look
        #if UNITY_WEBGL
            HandleDesktopMovement();
        #endif
        
        if (_grabbedObject != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Calculate the distance from the camera to the grabbed object
            if (_distance == 0)
            {
                _distance = Vector3.Distance(desktopCamera.transform.position, _grabbedObject.transform.position) - 0.5f;
            }

            // Convert the mouse position to a world point using the calculated distance (z coordinate)
            Vector3 screenPoint = new Vector3(mousePos.x, mousePos.y, _distance);
            Vector3 worldPos = desktopCamera.ScreenToWorldPoint(screenPoint);
            
            // Apply the previously calculated offset so the object moves exactly from where it was grabbed
            _grabbedObject.transform.position = worldPos + _grabOffset;
        }
    }
    
    // Called when the left mouse button is pressed.
    private void OnMouseClick(InputAction.CallbackContext context)
    {
        if (_grabbedObject != null) return;
        
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = desktopCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, grabbableLayer))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            _grabbedObject = hit.collider.gameObject;

            // Calculate offset between object's pivot and the hit point
            _grabOffset = _grabbedObject.transform.position - hit.point;
        }
        else
        {
            Debug.Log("No grabbable object hit.");
        }
    }

    // Called when the left mouse button is released.
    private void OnMouseRelease(InputAction.CallbackContext context)
    {
        if (_grabbedObject == null) return;
        
        _grabbedObject = null;
        _grabOffset = Vector3.zero;
        _distance = 0;
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
        else
        {
            // Get the current Euler angles.
            Vector3 currentEuler = transform.rotation.eulerAngles;

            // Keep the current pitch (x) and yaw (y), but reset the roll (z) to zero.
            transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 0);
        }
    }
}
