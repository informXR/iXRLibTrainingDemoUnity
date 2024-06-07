using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public void NextLevel(double positionError, double rotationError)
    {
        Debug.Log("Level Completed\nPosition Error: " + positionError + "\nRotation Error:" + rotationError);
    }
}
