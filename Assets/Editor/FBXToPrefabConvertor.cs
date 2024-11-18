using UnityEditor;
using UnityEngine;
using System.IO;

public class FBXToPrefabConvertor : EditorWindow
{
    private string fbxFolderPath = "Assets/FBXFolder"; // Path to the folder containing FBX files
    private string prefabFolderPath = "Assets/PrefabFolder"; // Path to save the prefabs

    [MenuItem("Tools/Convert FBX to Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<FBXToPrefabConvertor>("FBX to Prefab Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX to Prefab Converter", EditorStyles.boldLabel);

        fbxFolderPath = EditorGUILayout.TextField("FBX Folder Path", fbxFolderPath);
        prefabFolderPath = EditorGUILayout.TextField("Prefab Folder Path", prefabFolderPath);

        if (GUILayout.Button("Convert FBX Files to Prefabs"))
        {
            ConvertFBXFilesToPrefabs();
        }
    }

    private void ConvertFBXFilesToPrefabs()
    {
        if (!Directory.Exists(fbxFolderPath))
        {
            Debug.LogError("FBX Folder path does not exist!");
            return;
        }

        if (!Directory.Exists(prefabFolderPath))
        {
            Directory.CreateDirectory(prefabFolderPath);
        }

        string[] fbxFiles = Directory.GetFiles(fbxFolderPath, "*.fbx", SearchOption.TopDirectoryOnly);

        foreach (string fbxFilePath in fbxFiles)
        {
            string assetPath = fbxFilePath.Replace(Application.dataPath, "Assets");
            GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (fbxObject != null)
            {
                string prefabPath = Path.Combine(prefabFolderPath, fbxObject.name + ".prefab");
                prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fbxObject, prefabPath);
                if (prefab != null)
                {
                    Debug.Log($"Created prefab: {prefabPath}");
                }
                else
                {
                    Debug.LogError($"Failed to create prefab for {fbxObject.name}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load FBX asset at path: {assetPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("FBX to Prefab conversion completed!");
    }
}
