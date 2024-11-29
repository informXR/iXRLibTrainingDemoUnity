#if UNITY_EDITOR
using System.IO;
using UnityEditor;

[InitializeOnLoad]
public class PackageDefineAdder
{
    static PackageDefineAdder()
    {
        const string define = "USE_IXRLIB";
        const string packageName = "com.informxr.unity";
        string packageManifest = File.ReadAllText("Packages/manifest.json");
        if (packageManifest.Contains(packageName))
        {
            AddDefine(define);
        }
        else
        {
            RemoveDefine(define);
        }
    }

    private static void AddDefine(string define)
    {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!defines.Contains(define))
        {
            defines += ";" + define;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        }
    }

    private static void RemoveDefine(string define)
    {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (defines.Contains(define))
        {
            defines = defines.Replace(define, "").Replace(";;", ";");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        }
    }
}
#endif