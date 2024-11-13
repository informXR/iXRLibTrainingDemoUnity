using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Dropper : MonoBehaviour
{

    private List<GrabbableObjectManager.GrabbableObjectType> queue;
    public float delay = 10;
    public float delayRange = 3;
    public float downForce = 10;
    private float currentDelay = 0;
    private float totalTime = 0;
    private float lastTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        queue = new List<GrabbableObjectManager.GrabbableObjectType>();
        XRSocketInteractor[] targetLocations = GameObject.FindObjectsOfType<XRSocketInteractor>();
        Debug.Log(targetLocations.Length);
        //iXR.EventLevelStart("1", "scriptName=Dropper");
        //iXR.LogInfo(targetLocations.Length.ToString());
        foreach (var targetLocation in targetLocations)
        {
            queue.Add(targetLocation.GetComponentInParent<GridManager>().Type);
            Debug.Log(targetLocation.GetComponentInParent<GridManager>().Type);
        }

        SetDelay();
    }

    void Update()
    {
        totalTime += Time.deltaTime;
        if (totalTime > lastTime + currentDelay)
        {
            SetDelay();
            lastTime = totalTime;
            SpawnRandom();
        }
    }

    private void SetDelay()
    {
        currentDelay = delay + Random.Range(-1f, 1f) * delayRange;
    }


    public void SpawnRandom()
    {
        if (queue.Count == 0) return;

        List<GrabbableObjectManager.GrabbableObjectType> uniqueValues = GetUniqueValues(queue);
        int index = Random.Range(0, uniqueValues.Count);
        GrabbableObjectManager.GrabbableObjectType type = uniqueValues[index];
        Remove(type);
        GameObject obj =Instantiate(GrabbableObjectManager.getInstance().getGrabbableObjectData(type).model, 
        this.transform.position, Quaternion.identity);
        obj.GetComponent<Rigidbody>().AddForce(Vector3.down * downForce);
        obj.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        obj.AddComponent<ElementTag>().type = type;
        obj.GetComponent<Rigidbody>().isKinematic = false;
        obj.GetComponent<Rigidbody>().useGravity = true;
    }

    public void Replace(GrabbableObjectManager.GrabbableObjectType a, GrabbableObjectManager.GrabbableObjectType b)
    {
        Remove(a);
        Add(b);
    }

    public void Add(GrabbableObjectManager.GrabbableObjectType type)
    {
        queue.Add(type);
    }

    public void Remove(GrabbableObjectManager.GrabbableObjectType type)
    {
        queue.Remove(type);
    }

    private List<T> GetUniqueValues<T>(List<T> list)
    {
        List<T> uniqueValues = new List<T>();
        foreach (T entry in list)
            if (!uniqueValues.Contains(entry))
                uniqueValues.Add(entry);
        return uniqueValues;
    }

    public void ResetDropper()
    {
        queue.Clear();
        SetDelay();
    }
}
