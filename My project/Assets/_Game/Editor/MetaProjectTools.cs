using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.Rendering.Universal;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Grab;
using Oculus.Interaction.Input;
using Oculus.Interaction.Locomotion;

namespace HunJam.Editor
{
    public static class MetaProjectTools
    {
        public static void ConfigureMetaFeatures()
        {
            foreach (var group in new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone })
            {
                UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(group);
                var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
                var feature = settings.GetFeature<Meta.XR.MetaXRFeature>();
                if (feature == null) throw new InvalidOperationException("Meta XR OpenXR feature missing");
                feature.enabled = true;
                EditorUtility.SetDirty(feature);
                EditorUtility.SetDirty(settings);
            }
        }

        public static GameObject CreateRig()
        {
            var root = new GameObject("Quest Player");
            var cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab");
            var cameraObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab, root.transform);
            var cameraRig = cameraObject.GetComponent<OVRCameraRig>();
            var manager = cameraObject.GetComponent<OVRManager>();
            var settings = new SerializedObject(manager);
            settings.FindProperty("_trackingOriginType").enumValueIndex = (int)OVRManager.TrackingOrigin.FloorLevel;
            settings.ApplyModifiedPropertiesWithoutUndo();
            manager.controllerDrivenHandPosesType = OVRManager.ControllerDrivenHandPosesType.ConformingToController;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.meta.xr.sdk.interaction.ovr/Runtime/Prefabs/OVRComprehensiveInteractionRig.prefab");
            var interaction = (GameObject)PrefabUtility.InstantiatePrefab(prefab, cameraRig.transform);
            foreach (var reference in interaction.GetComponentsInChildren<OVRCameraRigRef>(true))
                reference.InjectInteractionOVRCameraRig(cameraRig);
            foreach (var effect in interaction.GetComponentsInChildren<TunnelingEffect>(true))
                effect.InjectCenterEyeCamera(cameraRig.centerEyeAnchor.GetComponent<Camera>());
            foreach (var hand in cameraRig.trackingSpace.GetComponentsInChildren<OVRHand>(true))
            {
                var renderer = hand.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null) renderer.enabled = false;
                var mesh = hand.GetComponent<OVRMeshRenderer>();
                if (mesh != null) mesh.enabled = false;
            }
            // All SDK locomotion components remain disabled, including input broadcasters.
            foreach (var component in interaction.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null) throw new InvalidOperationException("Missing Meta rig script");
                if ((component.GetType().Namespace ?? "").StartsWith("Oculus.Interaction.Locomotion"))
                    component.enabled = false;
                if (component is RayInteractor || component is DistanceGrabInteractor ||
                    component is DistanceHandGrabInteractor)
                    component.enabled = false;
            }
            foreach (var camera in cameraObject.GetComponentsInChildren<Camera>(true))
            {
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 80;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.035f, 0.06f, 0.1f);
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            }
            cameraRig.centerEyeAnchor.gameObject.tag = "MainCamera";
            foreach (string side in new[] { "Left", "Right" })
            {
                var group = interaction.transform.Find("ComprehensiveInteractors" + side);
                if (group == null) throw new InvalidOperationException("SDK controller group missing: " + side);
                var grabs = group.GetComponentsInChildren<GrabInteractor>(true);
                var handGrabs = group.GetComponentsInChildren<HandGrabInteractor>(true);
                var source = group.GetComponentsInChildren<ControllerSelector>(true).First(s =>
                    s.ControllerButtonUsage == ControllerButtonUsage.GripButton);
                var controller = new SerializedObject(source).FindProperty("_controller").objectReferenceValue as IController;
                if (controller == null) throw new InvalidOperationException("Missing SDK controller input: " + side);
                var triggerObject = new GameObject("Use held item");
                triggerObject.transform.SetParent(group, false);
                var trigger = triggerObject.AddComponent<ControllerSelector>();
                trigger.InjectController(controller);
                trigger.ControllerButtonUsage = ControllerButtonUsage.TriggerButton;
                triggerObject.AddComponent<MetaUseAction>().Configure(trigger, grabs, handGrabs);
            }
            foreach (var component in root.GetComponentsInChildren<Component>(true))
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, "Assets/_Game/Prefabs/QuestPlayer.prefab", InteractionMode.AutomatedAction);
            return root;
        }

        private static T Ensure<T>(GameObject obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            return component != null ? component : obj.AddComponent<T>();
        }
        public static void ConfigureActions()
        {
            foreach (var action in UnityEngine.Object.FindObjectsByType<DemoInteractable>(FindObjectsSortMode.None))
            {
                var body = Ensure<Rigidbody>(action.gameObject);
                body.isKinematic = true;
                body.useGravity = false;
                var pointable = Ensure<PointableElement>(action.gameObject);
                var grab = Ensure<GrabInteractable>(action.gameObject);
                grab.InjectRigidbody(body);
                grab.InjectOptionalPointableElement(pointable);
                var handGrab = Ensure<HandGrabInteractable>(action.gameObject);
                handGrab.InjectRigidbody(body);
                handGrab.InjectOptionalPointableElement(pointable);
                handGrab.InjectSupportedGrabTypes(GrabTypeFlags.Palm);
            }
        }

        public static void Migrate()
        {
            EditorSceneManager.OpenScene(DemoProjectTools.ScenePath);
            var old = GameObject.Find("Quest Player");
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            CreateRig();
            foreach (string name in new[] { "EnergyOrb", "Hammer", "Blade", "Lamp" })
            {
                string path = "Assets/_Game/Prefabs/" + name + ".prefab";
                var prefab = PrefabUtility.LoadPrefabContents(path);
                prefab.GetComponent<GameItem>().ConfigureInteraction();
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                PrefabUtility.UnloadPrefabContents(prefab);
            }
            foreach (var item in UnityEngine.Object.FindObjectsByType<GameItem>(FindObjectsSortMode.None))
                item.ConfigureInteraction();
            ConfigureActions();
            ConfigureMetaFeatures();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            DemoProjectTools.Validate();
            Debug.Log("HUNJAM: Meta XR migration complete");
        }
    }
}
