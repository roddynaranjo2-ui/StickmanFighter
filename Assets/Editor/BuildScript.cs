// BuildScript.cs — Build automatizado para CI/CD (GameCI).
// Configura PlayerSettings programáticamente y genera APK o AAB Android.
//
// FIX CI-01 / m-09: -buildNumber N para auto-incrementar bundleVersionCode.
// FIX CI-04: -aab para generar Android App Bundle, -apk (o nada) para APK directo.
// FIX C-02: ResolveScenes ahora re-sincroniza EditorBuildSettings con los GUIDs reales antes de leerlos.
// FIX M-08: sincroniza scriptingBackend en disco (Android: 2 = IL2CPP).
// SPRINT #4: el modo por defecto pasa a ser APK (instalación directa), no AAB.

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private static readonly string[] DefaultScenes = new[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/CombatScene.unity"
    };

    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        // 0. Validación previa: re-sincronizar EditorBuildSettings con GUIDs reales (FIX C-02).
        ProjectFixer.FixBuildScenes();
        ProjectFixer.FixTags();

        // 1. Configuración Player Settings programática
        PlayerSettings.companyName = "StickmanFighter";
        PlayerSettings.productName = "StickmanFighter";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.stickmanfighter.game");
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.Android.bundleVersionCode = ResolveBuildNumber();

        // 2. SDK levels
        PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

        // 3. Architectures (ARMv7 + ARM64)
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        // 4. Scripting Backend & API Level
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard);

        // 5. Orientación
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft  = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait       = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        // 6. Keystore (debug autogenerado por Unity si vacío)
        PlayerSettings.Android.useCustomKeystore = false;

        // 7. APK vs AAB (FIX CI-04 / SPRINT #4)
        // Reglas:
        //   -aab          → AAB (subir a Play Store)
        //   -apk o nada   → APK (instalación directa en dispositivo, modo por defecto)
        bool buildAab = ResolveBuildAab();
        EditorUserBuildSettings.buildAppBundle = buildAab;
        string ext = buildAab ? "aab" : "apk";

        // 8. Output path
        string outputDir = Path.Combine("Builds", "Android");
        Directory.CreateDirectory(outputDir);
        string outputPath = Path.Combine(outputDir, "StickmanFighter." + ext);

        // 9. Build Options
        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = ResolveScenes(),
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = ResolveBuildOptions()
        };

        Debug.Log($"[BuildScript] Building {ext.ToUpper()} | versionCode={PlayerSettings.Android.bundleVersionCode} | scenes={opts.scenes.Length}");

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"BUILD SUCCEEDED: {summary.totalSize / 1024 / 1024} MB -> {outputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"BUILD FAILED: {summary.result} ({summary.totalErrors} errors)");
            EditorApplication.Exit(1);
        }
    }

    private static string[] ResolveScenes()
    {
        var enabled = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        return enabled.Length == 0 ? DefaultScenes : enabled;
    }

    private static int ResolveBuildNumber()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-buildNumber" && int.TryParse(args[i + 1], out int n)) return n;
        }
        return PlayerSettings.Android.bundleVersionCode;
    }

    private static bool ResolveBuildAab()
    {
        string[] args = Environment.GetCommandLineArgs();
        return args.Contains("-aab");
    }

    private static BuildOptions ResolveBuildOptions()
    {
        BuildOptions opts = BuildOptions.None;
        string[] args = Environment.GetCommandLineArgs();
        if (args.Contains("-Development")) opts |= BuildOptions.Development;
        return opts;
    }
}
