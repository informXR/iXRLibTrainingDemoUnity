using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MissingScriptFinder : EditorWindow
{
    private Vector2 scrollPosition;
    private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
    private bool showInactiveObjects = true;
    private bool showPrefabs = true;

    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<MissingScriptFinder>("Missing Script Finder");
    }

    private void OnEnable()
    {
        FindMissingScripts();
    }

    private void FindMissingScripts()
    {
        objectsWithMissingScripts.Clear();

        // Find all GameObjects in the scene
        GameObject[] allObjects = showInactiveObjects ? 
            Resources.FindObjectsOfTypeAll<GameObject>() : 
            FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Skip if it's a prefab and we don't want to show prefabs
            if (!showPrefabs && PrefabUtility.IsPartOfPrefabAsset(obj))
                continue;

            // Check if this object has missing scripts
            Component[] components = obj.GetComponents<Component>();
            bool hasMissingScript = components.Any(comp => comp == null);

            if (hasMissingScript)
            {
                objectsWithMissingScripts.Add(obj);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Missing Script Finder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Options
        EditorGUILayout.BeginHorizontal();
        showInactiveObjects = EditorGUILayout.Toggle("Include Inactive Objects", showInactiveObjects);
        showPrefabs = EditorGUILayout.Toggle("Include Prefabs", showPrefabs);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Refresh"))
        {
            FindMissingScripts();
        }

        GUILayout.Space(10);

        if (objectsWithMissingScripts.Count == 0)
        {
            EditorGUILayout.HelpBox("No objects with missing scripts found!", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox($"Found {objectsWithMissingScripts.Count} object(s) with missing scripts:", MessageType.Warning);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (GameObject obj in objectsWithMissingScripts)
        {
            EditorGUILayout.BeginHorizontal("box");

            // Object name and selection button
            if (GUILayout.Button(obj.name, GUILayout.Width(200)))
            {
                Selection.activeGameObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            // Object type indicator
            string typeInfo = "";
            if (PrefabUtility.IsPartOfPrefabAsset(obj))
                typeInfo = "[Prefab]";
            else if (PrefabUtility.IsPartOfPrefabInstance(obj))
                typeInfo = "[Prefab Instance]";
            else
                typeInfo = "[Scene Object]";

            GUILayout.Label(typeInfo, GUILayout.Width(100));

            // Missing script count
            Component[] components = obj.GetComponents<Component>();
            int missingCount = components.Count(comp => comp == null);
            GUILayout.Label($"{missingCount} missing", GUILayout.Width(80));

            // Remove missing scripts button
            if (GUILayout.Button("Remove Missing", GUILayout.Width(100)))
            {
                RemoveMissingScripts(obj);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Instructions:\n" +
            "1. Click on an object name to select it in the hierarchy\n" +
            "2. Use 'Remove Missing' to remove missing script components\n" +
            "3. For prefabs, you may need to open them in Prefab Edit mode\n" +
            "4. Some missing scripts may require manual removal in the Inspector",
            MessageType.Info
        );
    }

    private void RemoveMissingScripts(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        int removedCount = 0;

        // Remove missing scripts from the end to avoid index issues
        for (int i = components.Length - 1; i >= 0; i--)
        {
            if (components[i] == null)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                removedCount++;
                break; // RemoveMonoBehavioursWithMissingScript removes all missing scripts at once
            }
        }

        if (removedCount > 0)
        {
            Debug.Log($"Removed {removedCount} missing script(s) from {obj.name}");
            FindMissingScripts(); // Refresh the list
        }
    }
} 