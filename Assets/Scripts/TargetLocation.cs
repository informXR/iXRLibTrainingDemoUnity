using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TargetLocation : MonoBehaviour
{
    public GameObject Target;
    [SerializeField] public UnityEvent<double, double> Calls;
    public double PositionError = .2;
    // Rotation Value Is Done In Euler Angles
    public double RotationError = 3;

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject != Target || Calls == null) return;
        double PositionDistance = Vector3.Distance(collider.transform.position, this.transform.position);
        double RotationDistance = Quaternion.Angle(collider.transform.rotation, this.transform.rotation);
        // Check For Completion and Call All Stored Functions
        if (PositionDistance < PositionError && RotationDistance < PositionError)
        {
            Calls.Invoke(PositionDistance, RotationDistance);
        }
    }
}
