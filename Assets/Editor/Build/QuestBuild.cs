using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace XRMultiplayer.EditorTools
{
    public static class QuestBuild
    {
        const string DefaultBuildPath = "Builds/Quest/XR-Multiplayer-Meeting.apk";

        public static void BuildAndroidApk()
        {
            var buildPath = GetCommandLineArg("-buildOutputPath");
            if (string.IsNullOrWhiteSpace(buildPath))
                buildPath = DefaultBuildPath;

            var fullBuildPath = Path.GetFullPath(buildPath);
            var buildDirectory = Path.GetDirectoryName(fullBuildPath);
            if (!string.IsNullOrEmpty(buildDirectory))
                Directory.CreateDirectory(buildDirectory);

            ConfigureExternalAndroidTools();
            QuestPassthroughSetup.ConfigureQuestPassthrough();

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = fullBuildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            Debug.Log($"Quest APK build finished with result {summary.result}: {fullBuildPath}");

            if (Application.isBatchMode)
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        static string GetCommandLineArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }

            return null;
        }

        static void ConfigureExternalAndroidTools()
        {
            var sdkPath = GetCommandLineArg("-androidSdkPath");
            if (string.IsNullOrWhiteSpace(sdkPath))
                return;

            AndroidExternalToolsSettings.sdkRootPath = Path.GetFullPath(sdkPath);
            Debug.Log($"Using Android SDK path: {AndroidExternalToolsSettings.sdkRootPath}");
        }
    }
}
