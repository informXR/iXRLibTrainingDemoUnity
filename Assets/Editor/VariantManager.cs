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
        ApplyVariantSettings(variant);
    }

    private static void ApplyVariantSettings(string variant)
    {
        string jsonPath = Path.Combine(Application.dataPath, "Settings", "ProjectVariants.json");

        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"ProjectVariants.json not found at {jsonPath}");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        ProjectVariants projectVariants = JsonUtility.FromJson<ProjectVariants>(json);

        VariantSettings settings = projectVariants.Variants.Find(v => v.Name == variant);

        if (settings != null)
        {
            List<string> appliedSettings = new List<string>();

            // Apply ProjectSettings
            string projectSettingsPath = Path.Combine("ProjectSettings", settings.OverrideSettings.ProjectSettings);
            
            if (File.Exists(projectSettingsPath))
            {
                AssetDatabase.ImportAsset(projectSettingsPath, ImportAssetOptions.ForceUpdate);
                appliedSettings.Add($"ProjectSettings: {projectSettingsPath}");
            }
            else
            {
                appliedSettings.Add($"ProjectSettings file not found: {projectSettingsPath}");
            }

            // Apply Addressables settings
            string addressablesPath = Path.Combine(Application.dataPath, settings.OverrideAddressables.informXR);
            if (File.Exists(addressablesPath))
            {
                // You might want to add code here to actually apply the Addressables settings
                appliedSettings.Add($"Addressables: {addressablesPath}");
            }
            else
            {
                appliedSettings.Add($"Addressables file not found: {addressablesPath}");
            }

            AssetDatabase.Refresh();

            // Log the variant change and applied settings in a single message
            Debug.Log($"Set project variant to: {variant}\n" +
                      $"Applied settings:\n{string.Join("\n", appliedSettings)}");
        }
        else
        {
            Debug.LogError($"Variant '{variant}' not found in ProjectVariants.json");
        }
    }
}

