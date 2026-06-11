using UnityEditor;
using UnityEditor.Android;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace XRMultiplayer.EditorTools
{
    public static class QuestPassthroughSetup
    {
        const string AndroidXrFeatureSetId = "com.unity.openxr.featureset.android";
        const string MetaFeatureSetId = "com.unity.openxr.featureset.meta";

        static readonly string[] QuestFeatureIds =
        {
            "com.unity.openxr.feature.metaquest",
            "com.unity.openxr.feature.compositionlayers",
            "com.unity.openxr.feature.arfoundation-meta-session",
            "com.unity.openxr.feature.arfoundation-meta-camera",
            "com.unity.openxr.feature.meta-display-utilities",
            "com.unity.openxr.feature.input.oculustouch",
            "com.unity.openxr.feature.input.metaquestplus",
            "com.unity.openxr.feature.input.metaquestpro",
            "MetaOpenXR-OpenXRLifeCycle",
        };

        static readonly string[] UnusedMetaFeatureIds =
        {
            "com.unity.openxr.feature.arfoundation-meta-anchor",
            "com.unity.openxr.feature.arfoundation-meta-bounding-boxes",
            "com.unity.openxr.feature.arfoundation-meta-mesh",
            "com.unity.openxr.feature.arfoundation-meta-occlusion",
            "com.unity.openxr.feature.arfoundation-meta-plane",
            "com.unity.openxr.feature.arfoundation-meta-raycast",
            "com.unity.openxr.feature.meta-boundary-visibility",
            "com.unity.openxr.feature.meta-colocation-discovery",
        };

        [MenuItem("XR Multiplayer Meeting/Configure Quest Passthrough")]
        public static void ConfigureQuestPassthrough()
        {
            ConfigureAndroidPlayerSettings();
            ConfigureOpenXrFeatureSets();

            AssetDatabase.SaveAssets();
            Debug.Log("Quest passthrough setup complete.");
        }

        public static void RunBatchSetup()
        {
            ConfigureQuestPassthrough();
        }

        static void ConfigureAndroidPlayerSettings()
        {
            PlayerSettings.companyName = "BiesenbergerIT";
            PlayerSettings.productName = "XR Multiplayer Meeting";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.biesenbergerit.xrmultiplayermeeting");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        }

        static void ConfigureOpenXrFeatureSets()
        {
            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
            OpenXRFeatureSetManager.InitializeFeatureSets();

            SetFeatureSetEnabled(AndroidXrFeatureSetId, false);
            DisableFeatureSetFeatures(AndroidXrFeatureSetId);

            SetFeatureSetEnabled(MetaFeatureSetId, true);
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Android);

            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
            SetFeaturesEnabled(QuestFeatureIds, true);
            SetFeaturesEnabled(UnusedMetaFeatureIds, false);
            KeepInternetPermissionForMultiplayer();

            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (settings != null)
                EditorUtility.SetDirty(settings);
        }

        static void SetFeatureSetEnabled(string featureSetId, bool enabled)
        {
            var featureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.Android, featureSetId);
            if (featureSet == null)
                return;

            featureSet.isEnabled = enabled;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Android);
        }

        static void DisableFeatureSetFeatures(string featureSetId)
        {
            var featureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.Android, featureSetId);
            if (featureSet?.featureIds == null)
                return;

            SetFeaturesEnabled(featureSet.featureIds, false);
        }

        static void SetFeaturesEnabled(string[] featureIds, bool enabled)
        {
            foreach (var featureId in featureIds)
            {
                var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, featureId);
                if (feature == null)
                    continue;

                feature.enabled = enabled;
                EditorUtility.SetDirty(feature);
            }
        }

        static void KeepInternetPermissionForMultiplayer()
        {
            var metaQuestFeature = FeatureHelpers.GetFeatureWithIdForBuildTarget(
                BuildTargetGroup.Android,
                "com.unity.openxr.feature.metaquest");

            if (metaQuestFeature == null)
                return;

            var serializedFeature = new SerializedObject(metaQuestFeature);
            var forceRemoveInternetPermission = serializedFeature.FindProperty("forceRemoveInternetPermission");
            if (forceRemoveInternetPermission == null)
                return;

            forceRemoveInternetPermission.boolValue = false;
            serializedFeature.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(metaQuestFeature);
        }
    }
}
