using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Dropper : MonoBehaviour
{

    private List<GrabbableObjectManager.GrabbableObjectType> queue;
    public float delay = 10;
    public float delayRange = 3;
    private float currentDelay = 0;
    private float totalTime = 0;
    private float lastTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        queue = new List<GrabbableObjectManager.GrabbableObjectType>();
        TargetLocation[] targetLocations = GameObject.FindObjectsOfType<TargetLocation>();
        Debug.Log(targetLocations.Length);
        foreach(TargetLocation targetLocation in targetLocations){
            queue.Add(targetLocation.targetType);
            Debug.Log(targetLocation.targetType);
        }

        SetDelay();
    }

    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;
        if(totalTime > lastTime + currentDelay){
            SetDelay();
            lastTime = totalTime;
            SpawnRandom();
        }
    }

    private void SetDelay(){
        currentDelay = delay + Random.Range(-1f, 1f) * delayRange;
    }


    public void SpawnRandom(){
        List<GrabbableObjectManager.GrabbableObjectType> uniqueValues = GetUniqueValues(queue);
        GrabbableObjectManager.GrabbableObjectType type = uniqueValues[(Random.Range(0,uniqueValues.Count-1))];
        Remove(type);
       // Debug.Log(type);
        GrabbableObjectManager.getInstance().CreateGrabbableObject(type, this.transform);
    }

    // Gets rid of A, adds B
    public void Replace(GrabbableObjectManager.GrabbableObjectType a, GrabbableObjectManager.GrabbableObjectType b){
        Remove(a);
        Add(b);
    }

    public void Add(GrabbableObjectManager.GrabbableObjectType type){
        queue.Add(type);
    }

    public void Remove(GrabbableObjectManager.GrabbableObjectType type){
        queue.Remove(type);
    }

    private List<T> GetUniqueValues<T>(List<T> list){
        List<T> uniqueValues = new List<T>();
        foreach(T entry in list)
            if(!uniqueValues.Contains(entry)) 
                uniqueValues.Add(entry);
        return uniqueValues;
    }
}
