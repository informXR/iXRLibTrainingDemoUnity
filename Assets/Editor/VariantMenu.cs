using UnityEditor;

public class VariantMenu
{
    [MenuItem("Builds/Switch to Production")]
    public static void SwitchToProduction()
    {
        VariantManager.SetVariant("Production");
    }

    [MenuItem("Builds/Switch to Development")]
    public static void SwitchToDevelopment()
    {
        VariantManager.SetVariant("Development");
    }
}