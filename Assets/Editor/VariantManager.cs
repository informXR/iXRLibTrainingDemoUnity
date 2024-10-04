using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[System.Serializable]
public class ProjectVariants
{
    public List<VariantSettings> Variants;
}

[System.Serializable]
public class VariantSettings
{
    public string Name;
    public OverrideSettings OverrideSettings;
    public OverrideAddressables OverrideAddressables;
}

[System.Serializable]
public class OverrideSettings
{
    public string ProjectSettings;
}

[System.Serializable]
public class OverrideAddressables
{
    public string informXR;
}

public class VariantManager
{
    public static string CurrentVariant { get; private set; }

    public static void SetVariant(string variant)
    {
        CurrentVariant = variant;
        Debug.Log($"Set project variant to: {variant}");
        ApplyVariantSettings(variant);
    }

    private static void ApplyVariantSettings(string variant)
    {
        string jsonPath = Path.Combine(Application.dataPath, "Settings", "ProjectVariants.json");

        if (!File.Exists(jsonPath))
        {
            //Debug.LogError($"ProjectVariants.json not found at {jsonPath}");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        ProjectVariants projectVariants = JsonUtility.FromJson<ProjectVariants>(json);

        VariantSettings settings = projectVariants.Variants.Find(v => v.Name == variant);

        if (settings != null)
        {
            // Apply ProjectSettings
            string projectSettingsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", settings.OverrideSettings.ProjectSettings));
            //Debug.Log($"Attempting to apply ProjectSettings from: {projectSettingsPath}");
            
            if (File.Exists(projectSettingsPath))
            {
                string assetPath = "ProjectSettings/" + Path.GetFileName(projectSettingsPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                Debug.Log($"Applied ProjectSettings: {assetPath}");
            }
            else
            {
                // Debug.LogError($"ProjectSettings file not found: {projectSettingsPath}");
                // Debug.Log($"Current directory: {Directory.GetCurrentDirectory()}");
                // Debug.Log($"Files in ProjectSettings directory:");
                // foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "..", "ProjectSettings")))
                // {
                //    Debug.Log(file);
                // }
            }

            // Apply Addressables settings
            string addressablesPath = Path.Combine(Application.dataPath, settings.OverrideAddressables.informXR);
            if (File.Exists(addressablesPath))
            {
                //Debug.Log($"Addressables settings should be updated to: {addressablesPath}");
            }
            else
            {
                Debug.LogError($"Addressables file not found: {addressablesPath}");
            }

            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError($"Variant '{variant}' not found in ProjectVariants.json");
        }
    }
}
