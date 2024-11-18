using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Dropper : MonoBehaviour
{
    private Dictionary<GrabbableObjectManager.GrabbableObjectType, int> spawnQueue; // Maps types to the count of objects to spawn
    public float delay = 10f;
    public float delayRange = 3f;
    public float downForce = 10f;
    private float currentDelay = 0;
    private float totalTime = 0;
    private float lastTime = 0;

    void Start()
    {
        spawnQueue = new Dictionary<GrabbableObjectManager.GrabbableObjectType, int>();
        PopulateSpawnQueue();

        SetDelay();
    }

    void Update()
    {
        totalTime += Time.deltaTime;
        if (totalTime > lastTime + currentDelay && spawnQueue.Count > 0)
        {
            SetDelay();
            lastTime = totalTime;
            SpawnNext();
        }
    }

    private void SetDelay()
    {
        currentDelay = delay + Random.Range(-1f, 1f) * delayRange;
    }

    private void PopulateSpawnQueue()
    {
        // Clear the spawn queue
        spawnQueue.Clear();

        // Find all GridManagers and calculate required objects
        GridManager[] gridManagers = FindObjectsOfType<GridManager>();
        foreach (var gridManager in gridManagers)
        {
            int emptySpaces = gridManager.blankSpaces; // Number of empty spaces in this grid
            GrabbableObjectManager.GrabbableObjectType type = gridManager.Type;

            if (emptySpaces > 0)
            {
                // Add the type to the spawn queue with the required count
                if (spawnQueue.ContainsKey(type))
                {
                    spawnQueue[type] += emptySpaces;
                }
                else
                {
                    spawnQueue[type] = emptySpaces;
                }
            }
        }
    }

    private void SpawnNext()
    {
        // If the spawn queue is empty, repopulate
        if (spawnQueue.Count == 0)
        {
            PopulateSpawnQueue();
            return;
        }

        // Get the next type to spawn
        var enumerator = spawnQueue.GetEnumerator();
        enumerator.MoveNext();
        var typeToSpawn = enumerator.Current.Key;
        int count = enumerator.Current.Value;

        // Spawn the object
        GameObject obj = Instantiate(
            GrabbableObjectManager.getInstance().getGrabbableObjectData(typeToSpawn).model,
            this.transform.position,
            Quaternion.identity
        );

        // Set up the object
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.down * downForce);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        obj.AddComponent<ElementTag>().type = typeToSpawn;

        // Update the spawn queue
        spawnQueue[typeToSpawn]--;
        if (spawnQueue[typeToSpawn] <= 0)
        {
            spawnQueue.Remove(typeToSpawn);
        }
    }

    public void Replace(GrabbableObjectManager.GrabbableObjectType a, GrabbableObjectManager.GrabbableObjectType b)
    {
        if (spawnQueue.ContainsKey(a))
        {
            int count = spawnQueue[a];
            spawnQueue.Remove(a);

            if (spawnQueue.ContainsKey(b))
            {
                spawnQueue[b] += count;
            }
            else
            {
                spawnQueue[b] = count;
            }
        }
    }

    public void ResetDropper()
    {
        spawnQueue.Clear();
        PopulateSpawnQueue();
        SetDelay();
    }
}
