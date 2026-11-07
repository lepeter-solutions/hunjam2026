using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace HunJam.Editor
{
    // Settings only: never enters Play mode, builds a player or runs tests.
    // Committed settings already contain this configuration; rerun explicitly when needed.
    public static class MetaXrSetup
    {
        [MenuItem("HunJam/Configure Meta XR Hands")]
        public static void Configure()
        {
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel32)
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            foreach (var group in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
            {
                FeatureHelpers.RefreshFeatures(group);
                var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
                if (settings == null) continue;
                var feature = settings.GetFeature<Meta.XR.MetaXRFeature>();
                if (feature == null)
                {
                    Debug.LogError("Meta XR feature is unavailable after package import: " + group);
                    continue;
                }
                if (!feature.enabled)
                { feature.enabled = true; EditorUtility.SetDirty(feature); EditorUtility.SetDirty(settings); }
            }
            var config = OVRProjectConfig.CachedProjectConfig;
            if (config != null && config.handTrackingSupport != OVRProjectConfig.HandTrackingSupport.ControllersAndHands)
            {
                // Capsense supplies a skeleton while holding Touch controllers. Gameplay stays grip driven.
                config.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
                OVRProjectConfig.CommitProjectConfig(config);
            }
            var runtime = OVRRuntimeSettings.GetRuntimeSettings();
            if (runtime.HandSkeletonVersion != OVRHandSkeletonVersion.OpenXR)
            {
                runtime.HandSkeletonVersion = OVRHandSkeletonVersion.OpenXR;
                OVRRuntimeSettings.CommitRuntimeSettings(runtime);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
