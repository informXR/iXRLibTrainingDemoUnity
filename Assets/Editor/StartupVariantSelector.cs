using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class StartupVariantSelector
{
    static StartupVariantSelector()
    {
        EditorApplication.delayCall += SelectVariant;
    }

    static void SelectVariant()
    {
        string savedVariant = VariantManager.LoadSelectedVariant();
        if (string.IsNullOrEmpty(savedVariant))
        {
            ShowVariantSelectionDialog();
        }
        else
        {
            VariantManager.SetVariant(savedVariant);
        }
    }

    static void ShowVariantSelectionDialog()
    {
        string[] variants = { "Production", "Development", "Local" };
        int choice = EditorUtility.DisplayDialogComplex(
            "Select Project Variant",
            "Choose the project variant to load:",
            variants[0],
            variants[1],
            "Cancel"
        );

        if (choice != 2) // Not cancelled
        {
            VariantManager.SetVariant(variants[choice]);
        }
    }
}