using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
[RequireComponent(typeof(Collider))]
public class TargetLocation : MonoBehaviour
{
    public GrabbableObjectManager.GrabbableObjectType targetType;
    public double positionError = .2;
    // 0.0000004 = 1 degree
    public double rotationError = 0.000004; // 20 Degrees
    public UnityEvent<CompletionData> OnCompleted;

    public struct CompletionData
    {
        public bool validPlacement;
        public GameObject usedTarget;
        public GameObject usedObject;
        public GrabbableObjectManager.GrabbableObjectType targetType;
        public GrabbableObjectManager.GrabbableObjectType usedType;
        public double positionDistance;
        public double rotationDistance;
    }

    private CompletionData completionData;

    public void Start()
    {
        completionData.targetType = this.targetType;
    }
    public void OnTriggerStay(Collider collider)
    {
        // Is Valid Collision
        if (collider.gameObject.GetComponent<GrabbableObject>() == null) return;
        completionData.positionDistance = Vector3.Distance(collider.transform.position, this.transform.position);
        completionData.rotationDistance = CompareQuaternions(collider.transform.rotation, this.transform.rotation);
        completionData.validPlacement = completionData.positionDistance < positionError && completionData.rotationDistance < rotationError;
        completionData.usedType = collider.gameObject.GetComponent<GrabbableObject>().type;
        completionData.usedObject = collider.gameObject;
        completionData.usedTarget = this.gameObject;
    }

    public void OnTriggerExit(Collider collider)
    {
        completionData.validPlacement = false;
    }

    // This has to be added to the the OnSelectExited event of the interactable object, validObject, through unity. I can't add it automatically because the event is private somehow :( 
    public void OnRelease()
    {
        Debug.Log(completionData);
        if (!completionData.validPlacement || OnCompleted == null) return;
        OnCompleted.Invoke(completionData);
    }

    private double CompareQuaternions(Quaternion a, Quaternion b)
    {
        return 1 - Mathf.Abs(Quaternion.Dot(a, b));
    }


}
