using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class TargetBox : MonoBehaviour
{
    /*
    REWORKING THE CURRENT TARGET SYSTEM
    PLACE IN A BIG BOX AND U HAVE TO PLACE IT IN THE RIGHT PLACE IN THE BOX
    */
    public GrabbableObjectManager.GrabbableObjectType targetType;
    public UnityEvent<CompletionData> OnCompleted;
    public List<Transform> items;

    public struct CompletionData
    {
        // This is used as a 1 frame delay to avoid errors from some of the xr libraries it's not optimal but...
        public bool completed; // If this value is false outside of this script there is probably a problem!!!
        public bool validPlacement;
        public GameObject usedTarget;
        public GameObject usedObject;
        public GrabbableObjectManager.GrabbableObjectType targetType;
        public GrabbableObjectManager.GrabbableObjectType usedType;
        public float dropDistance;
        public float dropRotation;
        public float positionOffset;
    }

    private CompletionData completionData;
    private bool isCompleted = false;

    public void Start()
    {
        completionData.targetType = this.targetType;
    }
    public void OnTriggerStay(Collider collider)
    {
        if (!items.Contains(collider.gameObject.transform)) items.Add(collider.gameObject.transform);

        // Is Valid Collision
        if (
            collider.gameObject.GetComponent<GrabbableObject>() == null ||
            collider.gameObject.GetComponent<Rigidbody>().velocity == Vector3.zero) return;
        completionData.usedType = collider.gameObject.GetComponent<GrabbableObject>().type;
        completionData.usedObject = collider.gameObject;
        completionData.usedTarget = this.gameObject;
        completionData.dropDistance += collider.gameObject.GetComponent<GrabbableObject>().getPositionChange();
        completionData.dropRotation += collider.gameObject.GetComponent<GrabbableObject>().getRotationChange();


        // Position is measured from the top right corner
        completionData.positionOffset = calculatePositionOffset(collider.gameObject.transform);
    }

    // standard deviation of points distance to nearest neighbor
    public double calculateScore()
    {
        double totalDistance = 0;
        for (int i = 0; i < items.length; i++)
        {
            Vector3 positionOne = calculatePositionOffset(items[i]);
            double minDistance = double.MaxValue;
            for (int j = 0; j < items.length - i; i++)
            {
                if (i == j) continue;
                double positionTwo = calculatePositionOffset(items[j]);
                double distance = Vector3.Distance(positionOne, positionTwo);
                if (minDistance > distance) minDistance = distance;
            }
        }
        double meanDistance = totalDistance / points.Count;
        return nearestNeighborDistances.Average(d => Math.Pow(d - meanDistance, 2));
    }

    public float calculatePositionOffset(Transform transform)
    {
        return transform.position - this.gameObject.transform.position + (Vector3.Scale(Vector3.right + Vector3.forward, this.gameObject.transform.localScale) / 2);
    }

    public void OnTriggerExit(Collider collider)
    {
        completionData.validPlacement = false;
    }
    public void OnRelease()
    {
        string jsonData = JsonUtility.ToJson(completionData);
        iXR.LogInfo(jsonData);
        Debug.Log(jsonData);
        if (!completionData.validPlacement || OnCompleted == null) return;
        isCompleted = true;
    }

    public void Update()
    {
        if (isCompleted)
        {
            OnCompleted.Invoke(completionData);
            isCompleted = false;
        }
    }

    private double CompareQuaternions(Quaternion a, Quaternion b)
    {
        return 1 - Mathf.Abs(Quaternion.Dot(a, b));
    }

    public void ResetState()
    {
        completionData = new CompletionData();
        completionData.targetType = this.targetType;
    }
}
