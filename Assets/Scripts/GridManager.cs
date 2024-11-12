using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

public class GridManager : MonoBehaviour
{
    public GameObject[] objectPrefabs; // Prefabs for each object type
    public GameObject emptySlotPrefab; // Prefab for empty slots
    public Vector2 gridSpacing = new Vector2(2, 2); // Spacing between grid objects
    public int rows = 5; // Number of rows in the grid (Z axis)
    public int columns = 5; // Number of columns in the grid (X axis)
    public int blankSpaces = 5; // Number of blank spaces in the grid
    public Vector3 initialPosition = new Vector3(0, 1, 0); // Position for the first element (Y should match shelfHeight)
    public Vector3 InitialRotation;
    public float shelfHeight = 1.0f; // Height of the shelf surface

    private List<Vector3> occupiedPositions = new List<Vector3>();
    public GrabbableObjectManager.GrabbableObjectType Type; // Type for this GridManager
    public int score = 0; // Score counter

    private void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        ClearGrid();

        // Create blank spaces at random positions
        HashSet<Vector3> blankPositions = new HashSet<Vector3>();
        while (blankPositions.Count < blankSpaces)
        {
            Vector3 randomPosition = GetRandomGridPosition();
            blankPositions.Add(randomPosition);
        }

        // Place objects in grid, adding empty slots at blank positions
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localPosition = initialPosition + new Vector3(x * gridSpacing.x, 0, z * gridSpacing.y);
                if (blankPositions.Contains(localPosition))
                {
                    SpawnEmptySlot(localPosition);
                }
                else
                {
                    SpawnObjectAtLocalPosition(localPosition);
                }
            }
        }
    }

    private Vector3 GetRandomGridPosition()
    {
        int x = Random.Range(0, columns);
        int z = Random.Range(0, rows);
        return initialPosition + new Vector3(x * gridSpacing.x, 0, z * gridSpacing.y);
    }

    private void SpawnObjectAtLocalPosition(Vector3 localPosition)
    {
        if (objectPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, objectPrefabs.Length);
        GameObject obj = Instantiate(objectPrefabs[randomIndex], transform);
        obj.transform.localPosition = localPosition;
        Destroy(obj.GetComponent<XRGrabInteractable>());
        obj.transform.localRotation = Quaternion.Euler(InitialRotation);
        occupiedPositions.Add(localPosition);
    }
    public void AssignSocketEventHandlers(XRSocketInteractor socket, GrabbableObjectManager.GrabbableObjectType allowedType)
    {
        socket.selectEntered.AddListener((args) => OnObjectSocketed(args, allowedType));
        socket.selectExited.AddListener(OnObjectRemoved);
    }
    private void SpawnEmptySlot(Vector3 localPosition)
    {
        if (emptySlotPrefab == null) return;

        GameObject emptySlot = Instantiate(emptySlotPrefab, transform);
        emptySlot.transform.localPosition = localPosition;
        emptySlot.transform.localRotation = Quaternion.Euler(InitialRotation);

        // Add XRSocketInteractor and configure it to validate types
        XRSocketInteractor socket = emptySlot.GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener((s) => {
            OnObjectSocketed(s, Type);
        });

        // Make the empty slot transparent
        MakeMaterialTransparent(emptySlot);

        occupiedPositions.Add(localPosition);
    }

    private void OnObjectRemoved(SelectExitEventArgs args)
    {
        // Optional: Handle when objects are removed from a socket if needed.
        Debug.Log("Object removed from socket.");
    }

    public void OnObjectSocketed(SelectEnterEventArgs args, GrabbableObjectManager.GrabbableObjectType allowedType)
    {
        if (args == null)
        {
            Debug.LogError("SelectEnterEventArgs is null.");
            return;
        }

        XRSocketInteractor socket = args.interactorObject as XRSocketInteractor;
        if (socket == null)
        {
            Debug.LogError("Socket is null or not an XRSocketInteractor.");
            return;
        }

        XRGrabInteractable grabbedObject = args.interactableObject as XRGrabInteractable;
        if (grabbedObject == null)
        {
            Debug.LogError("Grabbed object is null or not an XRGrabInteractable.");
            return;
        }

        ElementTag elementTag = grabbedObject.GetComponent<ElementTag>();
        if (elementTag == null)
        {
            Debug.LogError("Grabbed object does not have an ElementTag component.", grabbedObject);
            return;
        }

        // Now that we've confirmed everything is not null, proceed with the logic
        if (elementTag.type == this.Type)
        {
            // Increment score, and other game logic
            Debug.Log("Scored! Object type matches grid type.");
            score++;
            GetComponentInParent<GridManagerManager>().AddScore(1);
        }
        else
        {
            Debug.LogWarning("Object type does not match grid type. Ignoring.");
        }
    }


    private void MakeMaterialTransparent(GameObject emptySlot)
    {
        MeshRenderer meshRenderer = emptySlot.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            foreach (Material material in meshRenderer.materials)
            {
                if (material.shader.name.Contains("Universal Render Pipeline"))
                {
                    material.SetFloat("_Surface", 1);  // Transparent surface type
                    material.SetFloat("_Blend", 1);    // Transparent blend mode
                    material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0);   // Disable depth writing
                    material.SetFloat("_AlphaClip", 0); // Disable alpha clipping

                    Color color = material.color;
                    color.a = 0.3f;
                    material.color = color;
                    material.renderQueue = 3000;
                }
                else
                {
                    Debug.LogWarning($"Material {material.name} is not using a URP shader!");
                }
            }
        }
    }

    public void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        occupiedPositions.Clear();
    }
}
