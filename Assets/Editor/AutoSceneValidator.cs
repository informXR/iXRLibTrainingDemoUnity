using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically validates and fixes scene issues when Unity loads or scenes change.
/// This runs in the Editor and doesn't rely on scene-based MonoBehaviour components,
/// so it works even when GUIDs are out of sync after fresh git checkouts.
/// </summary>
[InitializeOnLoad]
public static class AutoSceneValidator
{
    private static bool hasRunForCurrentSession = false;

    static AutoSceneValidator()
    {
        // Run validation when Unity starts up
        EditorApplication.delayCall += OnEditorStartup;
        
        // Run validation when scenes are loaded
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnEditorStartup()
    {
        if (!hasRunForCurrentSession)
        {
            hasRunForCurrentSession = true;
            
            // Longer delay to ensure Unity is fully ready for serialization changes
            EditorApplication.delayCall += () => {
                EditorApplication.delayCall += () => {
                    //Debug.Log("AutoSceneValidator: Unity startup - checking for missing script issues...");
                    ValidateCurrentScene();
                };
            };
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        Debug.Log($"AutoSceneValidator: Scene '{scene.name}' opened - validating...");
        EditorApplication.delayCall += () => ValidateCurrentScene();
    }
    


    private static void ValidateCurrentScene()
    {
        if (!Application.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            // Ensure we're not in the middle of asset refresh or other Unity operations
            if (EditorApplication.timeSinceStartup < 2.0f)
            {
                // Too early in Unity startup, retry later
                EditorApplication.delayCall += () => ValidateCurrentScene();
                return;
            }
            
            try
            {
                bool foundIssues = FixMissingScriptReferences();
                
                if (foundIssues)
                {
                    Debug.Log("AutoSceneValidator: Found and reconnected missing scripts. Cleaning up broken references...");
                    
                    // Use Unity's built-in cleanup - it's cleaner and doesn't cause serialization errors
                    TryBuiltInCleanup();
                    
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
                else
                {
                    //Debug.Log("AutoSceneValidator: No missing script references found.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AutoSceneValidator: Validation failed, will retry later: {e.Message}");
                // Retry after a longer delay
                EditorApplication.delayCall += () => {
                    EditorApplication.delayCall += () => ValidateCurrentScene();
                };
            }
        }
        else if (!Application.isPlaying)
        {
            // Unity is busy, retry later
            EditorApplication.delayCall += () => ValidateCurrentScene();
        }
    }

    private static bool FixMissingScriptReferences()
    {
        bool foundIssues = false;
        
        // Build a lookup of available MonoScripts by GUID
        var scriptLookup = BuildScriptGuidLookup();
        
        // Find all GameObjects with missing script components
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            
            // Check for missing script components
            foreach (Component component in components)
            {
                if (component == null)
                {
                    // Try to find and reconnect the script
                    string missingScriptGuid = TryGetMissingScriptGuid(obj, 0);
                    
                    if (!string.IsNullOrEmpty(missingScriptGuid) && scriptLookup.ContainsKey(missingScriptGuid))
                    {
                        // Attempt to reconnect the script
                        MonoScript foundScript = scriptLookup[missingScriptGuid];
                        if (TryReconnectScript(obj, foundScript, missingScriptGuid))
                        {
                            Debug.Log($"AutoSceneValidator: Reconnected missing script '{foundScript.GetClass().Name}' on '{obj.name}'");
                        }
                        else
                        {
                            Debug.Log($"AutoSceneValidator: Script '{foundScript.GetClass().Name}' already exists on '{obj.name}'");
                        }
                    }
                    
                    // Mark that we found missing scripts
                    foundIssues = true;
                    break; // Exit the component loop since we found missing scripts on this object
                }
            }
        }
        
        return foundIssues;
    }
    
    private static void TryBuiltInCleanup()
    {
        if (!Application.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            int totalCleaned = 0;
            
            foreach (GameObject obj in allObjects)
            {
                try
                {
                    int before = obj.GetComponents<Component>().Length;
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    int after = obj.GetComponents<Component>().Length;
                    
                    int cleaned = before - after;
                    if (cleaned > 0)
                    {
                        totalCleaned += cleaned;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"AutoSceneValidator: Cleanup failed for '{obj.name}': {e.Message}");
                }
            }
            
            if (totalCleaned > 0)
            {
                Debug.Log($"AutoSceneValidator: Cleaned up {totalCleaned} missing script references");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
    
    private static System.Collections.Generic.Dictionary<string, MonoScript> BuildScriptGuidLookup()
    {
        var lookup = new System.Collections.Generic.Dictionary<string, MonoScript>();
        
        string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript");
        foreach (string guid in scriptGuids)
        {
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
            if (script != null && script.GetClass() != null)
            {
                lookup[guid] = script;
            }
        }
        
        return lookup;
    }
    
    private static string TryGetMissingScriptGuid(GameObject obj, int componentIndex)
    {
        try
        {
            // This is tricky - we need to read the raw serialized data to find the GUID
            // Unity stores this in the scene file, but it's not easily accessible via API
            
            // For now, we'll try a different approach - look for common script patterns
            string objName = obj.name.ToLower();
            
            // Map common object names to expected script GUIDs (from your project)
            if (objName.Contains("exitcube") || objName.Contains("exit"))
                return "42723a225ff734044a8a6347d95587da"; // ExitButton.cs
            else if (objName.Contains("resetcube") || objName.Contains("reset"))
                return "699c84de7cce3466390bb7034346e5c1"; // ResetButton.cs
            else if (objName.Contains("reauthcube") || objName.Contains("reauth"))
                return "1ca6d1626b20642a69e1ba67a52e2141"; // ReAuthenticateButton.cs
            else if (objName.Contains("player") || objName.Contains("xr"))
            {
                // Player can have multiple scripts - check which ones are missing
                // Try PlayerOrientationFix first
                if (obj.GetComponent<PlayerOrientationFix>() == null)
                    return "26eb48bbc567e4af6bd41e66f9b7b69e"; // PlayerOrientationFix.cs
                
                // Try DesktopInputController if PlayerOrientationFix exists
                if (obj.GetComponent<DesktopInputController>() == null)
                    return "4450ca0bf4077466ebbed106da8a7205"; // DesktopInputController.cs
                
                // If both exist, this might be a duplicate reference - let removal handle it
                return null;
            }
                
            return null;
        }
        catch
        {
            return null;
        }
    }
    
    private static bool TryReconnectScript(GameObject obj, MonoScript script, string guid)
    {
        try
        {
            System.Type scriptType = script.GetClass();
            if (scriptType != null)
            {
                // Check if the component already exists
                Component existing = obj.GetComponent(scriptType);
                if (existing == null)
                {
                    // Add the component
                    obj.AddComponent(scriptType);
                    return true;
                }
                else
                {
                    // Component already exists - don't add duplicate
                    return false;
                }
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AutoSceneValidator: Failed to reconnect script {script.name}: {e.Message}");
            return false;
        }
    }

    // Manual validation menu item for troubleshooting
    [MenuItem("Tools/Validate Current Scene (Auto)")]
    public static void ManualValidation()
    {
        Debug.Log("AutoSceneValidator: Manual validation requested...");
        ValidateCurrentScene();
    }
    
    // Aggressive cleanup that uses Unity's built-in missing script removal
    [MenuItem("Tools/Force Clean Missing Scripts")]
    public static void ForceCleanMissingScripts()
    {
        Debug.Log("AutoSceneValidator: Force cleaning all missing script references...");
        
        // Get all GameObjects in the scene
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        int totalCleaned = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Use Unity's built-in method to remove missing scripts
            int before = obj.GetComponents<Component>().Length;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            int after = obj.GetComponents<Component>().Length;
            
            int cleaned = before - after;
            if (cleaned > 0)
            {
                Debug.Log($"AutoSceneValidator: Removed {cleaned} missing script(s) from '{obj.name}'");
                totalCleaned += cleaned;
            }
        }
        
        if (totalCleaned > 0)
        {
            Debug.Log($"AutoSceneValidator: Force cleanup complete - removed {totalCleaned} missing script references total");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        else
        {
            Debug.Log("AutoSceneValidator: No missing script references found to clean");
        }
    }
}