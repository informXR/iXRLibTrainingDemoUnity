using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableObject : MonoBehaviour
{
    public GrabbableObjectManager.GrabbableObjectType type;
    public string Id { get; private set; }

    private XRGrabInteractable grabInteractable;
    private IIxrService _ixrService;

    private void Awake()
    {
        Id = System.Guid.NewGuid().ToString();
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }

    private void Start()
    {
        _ixrService = ServiceLocator.GetService<IIxrService>();
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
#if USE_IXRLIB
        //iXR.EventInteractionStart(Id, "place_item");
#endif
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }
}
