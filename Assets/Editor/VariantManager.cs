using System;
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

public static class VariantManager
{
    private const string VariantFilePath = "ProjectSettings/SelectedVariant.txt";
    public static string CurrentVariant { get; private set; }

    public static void SetVariant(string variant)
    {
        CurrentVariant = variant;
        ApplyVariantSettings(variant);
        SaveSelectedVariant(variant);
    }

    public static string LoadSelectedVariant()
    {
        if (File.Exists(VariantFilePath))
        {
            return File.ReadAllText(VariantFilePath).Trim();
        }
        return null;
    }

    private static void SaveSelectedVariant(string variant)
    {
        File.WriteAllText(VariantFilePath, variant);
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
            string mainProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            
            if (File.Exists(projectSettingsPath))
            {
                try
                {
                    AssetDatabase.StartAssetEditing();
                    
                    // Read the contents of the variant-specific ProjectSettings file
                    string settingsContent = File.ReadAllText(projectSettingsPath);
                    
                    // Write the contents to the main ProjectSettings.asset file
                    File.WriteAllText(mainProjectSettingsPath, settingsContent);
                    
                    AssetDatabase.ImportAsset(mainProjectSettingsPath, ImportAssetOptions.ForceUpdate);
                    appliedSettings.Add($"ProjectSettings: {projectSettingsPath} -> {mainProjectSettingsPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to apply ProjectSettings: {e.Message}");
                    appliedSettings.Add($"Failed to apply ProjectSettings: {e.Message}");
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }
            else
            {
                appliedSettings.Add($"ProjectSettings file not found: {projectSettingsPath}");
            }

            // Apply Addressables settings
            string addressablesPath = Path.Combine(Application.dataPath, settings.OverrideAddressables.informXR);
            if (File.Exists(addressablesPath))
            {
                // Actually apply the Addressables settings
                string destinationPath = Path.Combine(Application.dataPath, "Resources", "informXR.asset");
                File.Copy(addressablesPath, destinationPath, true);
                AssetDatabase.ImportAsset("Assets/Resources/informXR.asset", ImportAssetOptions.ForceUpdate);
                appliedSettings.Add($"Addressables: {addressablesPath} -> {destinationPath}");
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

    [MenuItem("Builds/Save Current Settings")]
    public static void SaveCurrentVariantSettings()
    {
        string currentVariant = LoadSelectedVariant();
        if (string.IsNullOrEmpty(currentVariant))
        {
            Debug.LogError("No variant currently selected. Please select a variant first.");
            return;
        }

        string jsonPath = Path.Combine(Application.dataPath, "Settings", "ProjectVariants.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"ProjectVariants.json not found at {jsonPath}");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        ProjectVariants projectVariants = JsonUtility.FromJson<ProjectVariants>(json);
        VariantSettings settings = projectVariants.Variants.Find(v => v.Name == currentVariant);

        if (settings == null)
        {
            Debug.LogError($"Variant '{currentVariant}' not found in ProjectVariants.json");
            return;
        }

        // Backup ProjectSettings
        string mainProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        string variantProjectSettingsPath = Path.Combine("ProjectSettings", settings.OverrideSettings.ProjectSettings);

        try
        {
            File.Copy(mainProjectSettingsPath, variantProjectSettingsPath, true);
            Debug.Log($"ProjectSettings backed up to: {variantProjectSettingsPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to backup ProjectSettings: {e.Message}");
        }

        // Backup Addressables settings
        string mainAddressablesPath = "Assets/Resources/informXR.asset";
        string variantAddressablesPath = Path.Combine("Assets", settings.OverrideAddressables.informXR);

        try
        {
            // Simply copy the file without modifying its contents
            File.Copy(mainAddressablesPath, variantAddressablesPath, true);
            AssetDatabase.ImportAsset(variantAddressablesPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Addressables settings backed up to: {variantAddressablesPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to backup Addressables settings: {e.Message}");
        }

        AssetDatabase.Refresh();
        Debug.Log($"Current variant '{currentVariant}' settings have been backed up.");
    }
}

