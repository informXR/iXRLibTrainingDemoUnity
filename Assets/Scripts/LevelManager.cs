using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public void NextLevel(TargetLocation.CompletionData completionData)
    {
        Debug.Log("Level Completed\nPosition Distance: " + completionData.positionDistance + "\nRotation Distance:" + completionData.rotationDistance);
    }
}
