using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class LevelManager : MonoBehaviour
{
    public Dropper dropper;

    public void CompleteTask(TargetLocation.CompletionData completionData)
    {
        completionData.usedObject.GetComponent<XRGrabInteractable>().colliders.Clear();
        Destroy(completionData.usedObject);
        Destroy(completionData.usedTarget.GetComponent<Outline>());
        Destroy(completionData.usedTarget.GetComponent<TargetLocation>());
        completionData.usedTarget.GetComponent<MeshRenderer>().materials = GrabbableObjectManager.getInstance().getGrabbableObjectData(completionData.usedType).model.GetComponent<MeshRenderer>().sharedMaterials;
        completionData.usedTarget.GetComponent<BoxCollider>().isTrigger = false;

        if(completionData.usedType != completionData.targetType){
            dropper.Replace(completionData.targetType, completionData.usedType);
        }
    }
}
