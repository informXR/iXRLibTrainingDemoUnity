using UnityEditor;

public class VariantMenu
{
    [MenuItem("Project/Switch Variant/Production")]
    public static void SwitchToProduction()
    {
        VariantManager.SetVariant("Production");
    }

    [MenuItem("Project/Switch Variant/Development")]
    public static void SwitchToDevelopment()
    {
        VariantManager.SetVariant("Development");
    }
}