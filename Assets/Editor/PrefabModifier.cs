using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;

public class PrefabModifier : EditorWindow
{
    private string sourceFolderPath = "Assets/Prefabs"; // Folder containing the original prefabs
    private string targetFolderPath = "Assets/ModifiedPrefabs"; // Folder to save modified prefabs

    private string sourceFolderPathForOutline = "Assets/SourcePrefabs";
    private Material outlineMaterial;

    [MenuItem("Tools/Modify Prefabs for Sockets")]
    public static void ShowWindow()
    {
        GetWindow<PrefabModifier>("Modify Prefabs for Sockets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Modification Tool", EditorStyles.boldLabel);

        sourceFolderPath = EditorGUILayout.TextField("Source Folder Path", sourceFolderPath);
        targetFolderPath = EditorGUILayout.TextField("Target Folder Path", targetFolderPath);

        if (GUILayout.Button("Modify Prefabs"))
        {
            ModifyPrefabs();
        }

        sourceFolderPath = EditorGUILayout.TextField("Source Folder Path For Outline", sourceFolderPath);
        outlineMaterial = (Material)EditorGUILayout.ObjectField("Outline Material", outlineMaterial, typeof(Material), false);

        if (GUILayout.Button("Add Outline Material to Prefabs"))
        {
            AddOutlineMaterialToPrefabsInPlace();
        }
    }

    private void ModifyPrefabs()
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError("Source folder path does not exist!");
            return;
        }

        if (!Directory.Exists(targetFolderPath))
        {
            Directory.CreateDirectory(targetFolderPath);
        }

        string[] prefabPaths = Directory.GetFiles(sourceFolderPath, "*.prefab", SearchOption.TopDirectoryOnly);

        foreach (string prefabPath in prefabPaths)
        {
            string assetPath = prefabPath.Replace(Application.dataPath, "Assets");
            GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (originalPrefab != null)
            {
                // Duplicate the prefab
                GameObject modifiedPrefab = Instantiate(originalPrefab);

                // Remove XRGrabInteractable
                XRGrabInteractable grabComponent = modifiedPrefab.GetComponent<XRGrabInteractable>();
                if (grabComponent != null)
                {
                    DestroyImmediate(grabComponent, true);
                }

                // Add XRSocketInteractor
                XRSocketInteractor socketComponent = modifiedPrefab.GetComponent<XRSocketInteractor>();
                if (socketComponent == null)
                {
                    socketComponent = modifiedPrefab.AddComponent<XRSocketInteractor>();
                }

                // Modify BoxCollider to be a trigger
                BoxCollider boxCollider = modifiedPrefab.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.isTrigger = true;
                }
                else
                {
                    Debug.LogWarning($"No BoxCollider found on {modifiedPrefab.name}");
                }

                // Save modified prefab
                string newPrefabPath = Path.Combine(targetFolderPath, originalPrefab.name + "_Socket.prefab");
                newPrefabPath = AssetDatabase.GenerateUniqueAssetPath(newPrefabPath);
                PrefabUtility.SaveAsPrefabAsset(modifiedPrefab, newPrefabPath);
                
                DestroyImmediate(modifiedPrefab);
                
                Debug.Log($"Modified prefab saved at: {newPrefabPath}");
            }
            else
            {
                Debug.LogError($"Failed to load prefab at path: {assetPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Prefab modification completed!");
    }

    private void AddOutlineMaterialToPrefabsInPlace()
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError("Source folder path does not exist!");
            return;
        }

        if (outlineMaterial == null)
        {
            Debug.LogError("Outline material is not assigned!");
            return;
        }

        string[] prefabPaths = Directory.GetFiles(sourceFolderPath, "*.prefab", SearchOption.TopDirectoryOnly);

        foreach (string prefabPath in prefabPaths)
        {
            string assetPath = prefabPath.Replace(Application.dataPath, "Assets");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                // Open prefab for editing
                GameObject prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);

                // Add outline material to all renderers
                Renderer[] renderers = prefabInstance.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    if (!System.Array.Exists(materials, mat => mat == outlineMaterial))
                    {
                        Material[] newMaterials = new Material[materials.Length + 1];
                        materials.CopyTo(newMaterials, 0);
                        newMaterials[materials.Length] = outlineMaterial;
                        renderer.sharedMaterials = newMaterials;
                    }
                }

                // Save modified prefab
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                PrefabUtility.UnloadPrefabContents(prefabInstance);

                Debug.Log($"Modified prefab saved at: {assetPath}");
            }
            else
            {
                Debug.LogError($"Failed to load prefab at path: {assetPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Added outline material to all prefabs in place!");
    }
}
