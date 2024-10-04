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
            string mainProjectSettingsFile = "ProjectSettings/ProjectSettings.asset";
            string variantProjectSettingsFile = $"ProjectSettings/{settings.OverrideSettings.ProjectSettings}";
            if (File.Exists(variantProjectSettingsFile))
            {
                try
                {
                    File.Copy(variantProjectSettingsFile, mainProjectSettingsFile, true);
                    AssetDatabase.ImportAsset(mainProjectSettingsFile, ImportAssetOptions.ForceUpdate);
                    appliedSettings.Add($"ProjectSettings: {variantProjectSettingsFile} -> {mainProjectSettingsFile}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to apply ProjectSettings: {e.Message}");
                    appliedSettings.Add($"ProjectSettings: Failed to apply - {e.Message}");
                }
            }
            else
            {
                appliedSettings.Add($"ProjectSettings file not found: {variantProjectSettingsFile}");
            }

            // Apply Addressables settings
            string mainAddressablesFile = "Assets/Resources/informXR.asset";
            string variantAddressablesFile = $"Assets/Resources/{settings.OverrideAddressables.informXR}";
            if (File.Exists(variantAddressablesFile))
            {
                try
                {
                    File.Copy(variantAddressablesFile, mainAddressablesFile, true);
                    AssetDatabase.ImportAsset(mainAddressablesFile, ImportAssetOptions.ForceUpdate);
                    appliedSettings.Add($"Addressables: {variantAddressablesFile} -> {mainAddressablesFile}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to apply Addressables settings: {e.Message}");
                    appliedSettings.Add($"Addressables: Failed to apply - {e.Message}");
                }
            }
            else
            {
                appliedSettings.Add($"Addressables file not found: {variantAddressablesFile}");
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
        string mainProjectSettingsFile = "ProjectSettings/ProjectSettings.asset";
        string variantProjectSettingsFile = $"ProjectSettings/{settings.OverrideSettings.ProjectSettings}";
        //string variantProjectSettingsFile = Path.Combine("ProjectSettings", settings.OverrideSettings.ProjectSettings);

        try
        {
            File.Copy(mainProjectSettingsFile, variantProjectSettingsFile, true);
            //Debug.Log($"ProjectSettings backed up to: {variantProjectSettingsFile}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to backup ProjectSettings: {e.Message}");
        }

        // Backup Addressables settings
        string mainAddressablesFile = "Assets/Resources/informXR.asset";
        string variantAddressablesFile = $"Assets/Resources/{settings.OverrideAddressables.informXR}";

        try
        {
            // Simply copy the file without modifying its contents
            File.Copy(mainAddressablesFile, variantAddressablesFile, true);
            //Debug.Log($"Addressables settings backed up to: {variantAddressablesFile}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to backup Addressables settings: {e.Message}");
        }

        AssetDatabase.Refresh();
        Debug.Log($"Current variant '{currentVariant}' settings have been backed up.");
    }
}

