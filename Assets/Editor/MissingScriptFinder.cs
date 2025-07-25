using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Utility to find and remove missing script references in the scene.
/// This helps clean up after deleting scripts that were attached to GameObjects.
/// </summary>
public class MissingScriptFinder : EditorWindow
{
    private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<MissingScriptFinder>("Missing Script Finder");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Missing Script Finder", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Find Missing Scripts"))
        {
            FindMissingScripts();
        }
        
        GUILayout.Space(10);
        
        if (objectsWithMissingScripts.Count > 0)
        {
            GUILayout.Label($"Found {objectsWithMissingScripts.Count} objects with missing scripts:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (GameObject obj in objectsWithMissingScripts)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = obj;
                }
                
                if (GUILayout.Button("Remove Missing", GUILayout.Width(100)))
                {
                    RemoveMissingScripts(obj);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Remove All Missing Scripts"))
            {
                RemoveAllMissingScripts();
            }
        }
        else
        {
            GUILayout.Label("No missing scripts found.");
        }
    }
    
    private void FindMissingScripts()
    {
        objectsWithMissingScripts.Clear();
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            
            foreach (Component component in components)
            {
                if (component == null)
                {
                    if (!objectsWithMissingScripts.Contains(obj))
                    {
                        objectsWithMissingScripts.Add(obj);
                    }
                    break;
                }
            }
        }
        
        Debug.Log($"Found {objectsWithMissingScripts.Count} objects with missing scripts.");
    }
    
    private void RemoveMissingScripts(GameObject obj)
    {
        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
        Debug.Log($"Removed {removedCount} missing scripts from {obj.name}");
        
        // Refresh the list
        FindMissingScripts();
    }
    
    private void RemoveAllMissingScripts()
    {
        int totalRemoved = 0;
        
        foreach (GameObject obj in objectsWithMissingScripts)
        {
            int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            totalRemoved += removedCount;
        }
        
        Debug.Log($"Removed {totalRemoved} missing scripts total.");
        
        // Clear the list
        objectsWithMissingScripts.Clear();
    }
} 