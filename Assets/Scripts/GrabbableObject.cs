using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableObject : MonoBehaviour
{
    public GrabbableObjectManager.GrabbableObjectType type;
    public string Id { get; private set; }

    private XRGrabInteractable _grabInteractable;
    private IIxrService _ixrService;

    private void Awake()
    {
        Id = System.Guid.NewGuid().ToString();
        _grabInteractable = GetComponent<XRGrabInteractable>();
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }

    private void Start()
    {
        _ixrService = ServiceLocator.GetService<IIxrService>();
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        _ixrService.EventInteractionStart($"place_item_{Id}");
    }

    private void OnDestroy()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    private void OnDisable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    private void OnEnable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }
}
