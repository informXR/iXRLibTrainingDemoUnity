using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class LevelManager : MonoBehaviour
{
    public Dropper dropper;

    public void CompleteTask(TargetLocation.CompletionData completionData)
    {
        Debug.Log("Task Completed");

        if (completionData.usedType != completionData.targetType)
        {
            dropper.Replace(completionData.targetType, completionData.usedType);
            completionData.usedTarget.GetComponent<MeshFilter>().sharedMesh = completionData.usedObject.GetComponent<MeshFilter>().sharedMesh;
        }

        completionData.usedObject.GetComponent<XRGrabInteractable>().colliders.Clear();
        completionData.usedTarget.GetComponent<BoxCollider>().isTrigger = false;
        completionData.usedTarget.GetComponent<MeshRenderer>().materials = GrabbableObjectManager.getInstance().getGrabbableObjectData(completionData.usedType).model.GetComponent<MeshRenderer>().sharedMaterials;

        Destroy(completionData.usedObject);
        Destroy(completionData.usedTarget.GetComponent<Outline>());
        Destroy(completionData.usedTarget.GetComponent<TargetLocation>());
    }
}
