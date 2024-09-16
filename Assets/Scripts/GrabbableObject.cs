using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableObject : MonoBehaviour
{
    public GrabbableObjectManager.GrabbableObjectType type;
    public string Id { get; private set; }

    private XRGrabInteractable grabInteractable;


    private Vector3 pPosition;
    private Quaternion pRotation;
    public float getPositionChange()
    {
        float positionChange = (gameObject.transform.position - pPosition).magnitude;
        pPosition = gameObject.transform.position;
        return positionChange;
    }
    public float getRotationChange()
    {
        float rotationChange = 1 - Mathf.Abs(Quaternion.Dot(pRotation, gameObject.transform.localRotation));
        pRotation = gameObject.transform.localRotation;
        return rotationChange;
    }

    public void Update()
    {
        getPositionChange();
        getRotationChange();
    }

    private void Awake()
    {
        Id = System.Guid.NewGuid().ToString();
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        iXR.EventInteractionStart(Id, "place_item");
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
