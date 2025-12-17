#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

/// <summary>
/// Post-build processor that fixes Xcode build issues:
/// 1. Adds dummy Swift file to properly link Swift runtime (required by ARKit)
/// 2. Replaces -ld64 with -ld_classic to avoid Xcode 26 linker bugs
/// </summary>
public static class iOSBuildFix
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(pbxProjectPath);

        string unityFrameworkGuid = pbxProject.GetUnityFrameworkTargetGuid();

        // Fix 1: Add dummy Swift file - ARKit and other frameworks require Swift runtime
        string swiftFilePath = Path.Combine(pathToBuiltProject, "Classes", "SwiftSupport.swift");
        string swiftFileContent = @"import Foundation
import ARKit
// This file forces Xcode to properly link Swift runtime libraries.
// Required because ARKit has Swift dependencies.
";
        File.WriteAllText(swiftFilePath, swiftFileContent);

        string swiftFileGuid = pbxProject.AddFile(
            "Classes/SwiftSupport.swift",
            "Classes/SwiftSupport.swift",
            PBXSourceTree.Source
        );
        pbxProject.AddFileToBuild(unityFrameworkGuid, swiftFileGuid);
        pbxProject.SetBuildProperty(unityFrameworkGuid, "SWIFT_VERSION", "5.0");
        
        pbxProject.WriteToFile(pbxProjectPath);

        // Fix 2: Replace -ld64 with -ld_classic (Xcode 26 new linker has bugs)
        string pbxContent = File.ReadAllText(pbxProjectPath);
        pbxContent = pbxContent.Replace("\"-ld64\"", "\"-ld_classic\"");
        File.WriteAllText(pbxProjectPath, pbxContent);

        UnityEngine.Debug.Log("[iOSBuildFix] Added Swift support and applied Xcode 26 linker fix.");
    }
}
#endif

