using System;
using System.IO;
using HunJam;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace HunJam.Editor
{
    public static class DemoProjectTools
    {
        public const string ScenePath = "Assets/_Game/Scenes/DemoWorld.unity";
        private const string Root = "Assets/_Game/";
        private static Material floor, dark, cyan, orange, white, green;

        [MenuItem("HunJam/Generate Demo World")]
        public static void GenerateMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Regenerate demo?",
                    "This replaces DemoWorld and its generated prefabs/materials. Save manual changes in Git first.", "Regenerate", "Cancel")) return;
            Generate();
        }

        // Batch entry point, used in an isolated validation copy.
        public static void Prepare()
        {
            ConfigureQuest();
            Generate();
            Validate();
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null) CapturePreview();
        }

        [MenuItem("HunJam/Configure Quest")]
        public static void ConfigureQuest()
        {
            Directory.CreateDirectory("Assets/XR");
            AssetDatabase.Refresh();
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget all))
            {
                all = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(all, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, all, true);
            }
            foreach (BuildTargetGroup group in new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone })
            {
                if (!all.HasSettingsForBuildTarget(group)) all.CreateDefaultSettingsForBuildTarget(group);
                if (!all.HasManagerSettingsForBuildTarget(group)) all.CreateDefaultManagerSettingsForBuildTarget(group);
                if (!XRPackageMetadataStore.AssignLoader(all.ManagerSettingsForBuildTarget(group), "UnityEngine.XR.OpenXR.OpenXRLoader", group))
                    throw new InvalidOperationException("OpenXR loader assignment failed: " + group);
                all.SettingsForBuildTarget(group).InitManagerOnStart = true;
                UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(group);
                var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
                if (settings == null) throw new InvalidOperationException("OpenXR settings missing: " + group);
                settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
                var touch = settings.GetFeature<OculusTouchControllerProfile>();
                if (touch == null) throw new InvalidOperationException("Touch interaction profile missing.");
                touch.enabled = true;
                EditorUtility.SetDirty(touch);
                var meta = settings.GetFeature<Meta.XR.MetaXRFeature>();
                if (meta == null) throw new InvalidOperationException("Meta XR Core feature missing. Import the pinned Meta packages first.");
                meta.enabled = true;
                EditorUtility.SetDirty(meta);
                if (group == BuildTargetGroup.Android)
                {
                    var quest = settings.GetFeature<MetaQuestFeature>();
                    if (quest == null) throw new InvalidOperationException("Meta Quest support missing.");
                    quest.enabled = true;
                    EditorUtility.SetDirty(quest);
                }
                EditorUtility.SetDirty(settings);
                EditorUtility.SetDirty(all.SettingsForBuildTarget(group));
                EditorUtility.SetDirty(all.ManagerSettingsForBuildTarget(group));
            }
            EditorUtility.SetDirty(all);
            PlayerSettings.companyName = "Lepeter Solutions";
            PlayerSettings.productName = "HunJam Quest Playground";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.lepetersolutions.hunjam2026");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            var mobile = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
            mobile.msaaSampleCount = 4;
            mobile.supportsHDR = false;
            mobile.renderScale = 1f;
            mobile.shadowDistance = 20f;
            EditorUtility.SetDirty(mobile);
            AssetDatabase.SaveAssets();
            Debug.Log("HUNJAM: Quest and PC OpenXR configured.");
        }

        public static void Generate()
        {
            foreach (string folder in new[] { "Art/Materials", "Audio", "Prefabs", "Scenes" }) Directory.CreateDirectory(Root + folder);
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            floor = MaterialAsset("Floor", new Color(0.12f, 0.19f, 0.25f));
            dark = MaterialAsset("Structure", new Color(0.045f, 0.07f, 0.11f));
            cyan = MaterialAsset("Cyan", new Color(0.05f, 0.8f, 0.9f));
            orange = MaterialAsset("Amber", new Color(1f, 0.48f, 0.08f));
            white = MaterialAsset("White", new Color(0.8f, 0.9f, 0.95f));
            green = MaterialAsset("Green", new Color(0.2f, 0.85f, 0.5f));
            var world = new GameObject("World").transform;
            var ground = Shape("Courtyard", PrimitiveType.Cube, world, new Vector3(0, -0.2f, 0), new Vector3(22, 0.4f, 22), floor);

            Shape("North wall", PrimitiveType.Cube, world, new Vector3(0, 0.65f, 11), new Vector3(22, 1.3f, 0.3f), dark);
            Shape("South wall", PrimitiveType.Cube, world, new Vector3(0, 0.65f, -11), new Vector3(22, 1.3f, 0.3f), dark);
            Shape("East wall", PrimitiveType.Cube, world, new Vector3(11, 0.65f, 0), new Vector3(0.3f, 1.3f, 22), dark);
            Shape("West wall", PrimitiveType.Cube, world, new Vector3(-11, 0.65f, 0), new Vector3(0.3f, 1.3f, 22), dark);
            for (int i = -4; i <= 4; i++)
            {
                Shape("Route marker", PrimitiveType.Cube, world, new Vector3(-6, 0.006f, i * 2), new Vector3(0.12f, 0.01f, 0.8f), cyan, false);
                Shape("Route marker", PrimitiveType.Cube, world, new Vector3(6, 0.006f, i * 2), new Vector3(0.12f, 0.01f, 0.8f), cyan, false);
            }
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -9 : 9;
                float z = i < 2 ? -7 : 7;
                Shape("Beacon pillar", PrimitiveType.Cylinder, world, new Vector3(x, 1, z), new Vector3(0.5f, 1, 0.5f), dark);
                Shape("Beacon light", PrimitiveType.Sphere, world, new Vector3(x, 2.15f, z), Vector3.one * 0.35f, cyan, false);
            }
            Shape("Walk-around obstacle A", PrimitiveType.Cube, world, new Vector3(-3, 0.6f, 2), new Vector3(2, 1.2f, 2), dark);
            Shape("Walk-around obstacle B", PrimitiveType.Cube, world, new Vector3(3, 0.4f, -2), new Vector3(2, 0.8f, 2), dark);
            Text("Welcome", world, new Vector3(0, 2.7f, 9.8f), "HUNJAM / QUEST PLAYGROUND", 0.13f);
            Text("Controls", world, new Vector3(0, 2.4f, 1.4f), "ROOM SCALE WORKSHOP\nWalk and turn physically\nGrip: hold nearby object | Trigger: use / talk\nSwing hammer / blade | Touch RESET + trigger", 0.055f);

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1.3f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.65f, 0.8f);
            RenderSettings.skybox = null;

            var game = new GameObject("Demo Game").AddComponent<DemoGame>();
            var board = Text("Scoreboard", world, new Vector3(-0.9f, 1.9f, 0.9f), "ENERGY STATION\nTalk to the robot to begin", 0.08f);
            Shape("Receiver base", PrimitiveType.Cylinder, world, new Vector3(-0.9f, 0.4f, 0.65f), new Vector3(0.55f, 0.4f, 0.55f), green);
            var receiver = new GameObject("Energy Receiver");
            receiver.transform.SetParent(world);
            receiver.transform.position = new Vector3(-0.9f, 1.1f, 0.65f);
            var zone = receiver.AddComponent<BoxCollider>(); zone.isTrigger = true; zone.size = new Vector3(0.55f, 0.6f, 0.55f);
            receiver.AddComponent<EnergyReceiver>().Configure(game);

            var orbObject = Shape("Energy Orb", PrimitiveType.Sphere, null, Vector3.zero, Vector3.one * 0.28f, orange);
            var rb = orbObject.AddComponent<Rigidbody>(); rb.mass = 0.3f; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            orbObject.AddComponent<EnergyCell>();
            var orbPrefab = PrefabUtility.SaveAsPrefabAsset(orbObject, Root + "Prefabs/EnergyOrb.prefab");
            UnityEngine.Object.DestroyImmediate(orbObject);
            Vector3[] spots = { new Vector3(-0.85f, 1.1f, -0.1f), new Vector3(-0.85f, 1.1f, -0.5f), new Vector3(-0.85f, 1.1f, -0.9f) };
            var cells = new EnergyCell[spots.Length];
            for (int i = 0; i < spots.Length; i++)
            {
                Shape("Orb pedestal " + (i + 1), PrimitiveType.Cylinder, world, spots[i] - Vector3.up * 0.65f, new Vector3(0.35f, 0.45f, 0.35f), dark);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(orbPrefab);
                instance.name = "Energy Orb " + (i + 1); instance.transform.position = spots[i];
                cells[i] = instance.GetComponent<EnergyCell>();
            }
            game.Configure(cells, board);
            CreateRobot(game);
            CreatePlayer();
            CreateWorkshop(world);
            EditorSceneManager.SaveScene(scene, ScenePath);
            InteractionDemoTools.RegisterScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("HUNJAM: DemoWorld generated.");
        }

        private static void CreateRobot(DemoGame game)
        {
            var robot = new GameObject("Guide Robot");
            Shape("Body", PrimitiveType.Capsule, robot.transform, new Vector3(0, 0.85f, 0), new Vector3(0.6f, 0.65f, 0.6f), cyan);
            var head = Shape("Head", PrimitiveType.Sphere, robot.transform, new Vector3(0, 1.65f, 0), Vector3.one * 0.65f, white);
            Shape("Eye L", PrimitiveType.Sphere, head.transform, new Vector3(-0.2f, 0.05f, 0.45f), Vector3.one * 0.18f, dark, false);
            Shape("Eye R", PrimitiveType.Sphere, head.transform, new Vector3(0.2f, 0.05f, 0.45f), Vector3.one * 0.18f, dark, false);
            head.transform.localRotation = Quaternion.Euler(0, 180, 0);
            var speech = Text("Dialogue", robot.transform, new Vector3(0, 2.1f, 0), "HELLO!\nTouch me + Trigger", 0.065f);
            var guide = robot.AddComponent<GuideRobot>(); guide.SetPresentation(speech, head.transform);
            var prefab = PrefabUtility.SaveAsPrefabAsset(robot, Root + "Prefabs/GuideRobot.prefab");
            UnityEngine.Object.DestroyImmediate(robot);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = new Vector3(-1.2f, 0, -1.2f);
            instance.GetComponent<GuideRobot>().Configure(game);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<GuideRobot>());
        }

        private static void CreatePlayer()
        {
            var root = new GameObject("Quest Player"); root.layer = 2;


            var player = root.AddComponent<DemoPlayer>();
            var space = new GameObject("Tracking Space").transform; space.SetParent(root.transform, false);
            var view = new GameObject("Main Camera").AddComponent<Camera>(); view.tag = "MainCamera";
            view.transform.SetParent(space, false); view.transform.localPosition = Vector3.up * 1.65f;
            view.nearClipPlane = 0.05f; view.farClipPlane = 80; view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(0.035f, 0.06f, 0.1f); view.gameObject.AddComponent<AudioListener>();
            view.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            DemoHand Hand(bool left)
            {
                var obj = new GameObject(left ? "Left Hand" : "Right Hand"); obj.transform.SetParent(space, false);
                var visual = UnityHandModels.Create(obj.transform, left);
                var anchor = new GameObject("Hold Anchor").transform; anchor.SetParent(obj.transform, false);
                var hand = obj.AddComponent<DemoHand>(); hand.Configure(left, player, anchor, visual);
                return hand;
            }
            var leftHand = Hand(true); var rightHand = Hand(false);
            player.Configure(space, view, leftHand, rightHand);
            root.AddComponent<MetaRoomScaleRig>().Configure(
                AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/MetaLeftHand.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/MetaRightHand.prefab"),
                AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/Materials/MetaHandSkin.mat"));
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, Root + "Prefabs/QuestPlayer.prefab", InteractionMode.AutomatedAction);
            root.transform.position = Vector3.zero;
        }

        private static void CreateWorkshop(Transform world)
        {
            Shape("Workbench", PrimitiveType.Cube, world, new Vector3(0.45f, 0.75f, 0.85f), new Vector3(1.5f, 0.12f, 0.5f), dark);
            var reset = Shape("Reset workshop", PrimitiveType.Cube, world, new Vector3(1.05f, 0.9f, 0.65f), Vector3.one * 0.12f, green);
            reset.AddComponent<WorkshopReset>();
            Text("Reset label", world, new Vector3(1.05f, 1.15f, 0.9f), "RESET", 0.035f);
            var block = Shape("Cuttable block", PrimitiveType.Cube, world, new Vector3(0.55f, 1.02f, 0.85f), new Vector3(0.3f, 0.4f, 0.25f), orange);
            block.AddComponent<Cuttable>();
            var target = Shape("Hammer target", PrimitiveType.Cube, world, new Vector3(-0.05f, 0.9f, 0.85f), new Vector3(0.3f, 0.18f, 0.3f), cyan);
            var counter = Text("Hit counter", world, new Vector3(-0.05f, 1.3f, 1.05f), "HAMMER HITS: 0", 0.035f);
            target.AddComponent<ImpactTarget>().Configure(counter);

            void MakeTool(string name, Tool.ToolKind kind, Vector3 position)
            {
                Shape(name + " stand", PrimitiveType.Cube, world, position - Vector3.up * 0.15f, new Vector3(0.3f, 0.1f, 0.55f), dark);
                var item = new GameObject(name); item.transform.position = position;
                Shape("Handle", PrimitiveType.Cube, item.transform, Vector3.zero, new Vector3(0.045f, 0.045f, 0.22f), dark);
                Vector3 tipPosition = new Vector3(0, 0, 0.24f);
                if (kind == Tool.ToolKind.Hammer)
                    Shape("Hammer head", PrimitiveType.Cube, item.transform, tipPosition, new Vector3(0.2f, 0.1f, 0.1f), cyan);
                else if (kind == Tool.ToolKind.Blade)
                {
                    Shape("Blade", PrimitiveType.Cube, item.transform, new Vector3(0, 0, 0.24f), new Vector3(0.012f, 0.06f, 0.3f), white);
                    tipPosition.z = 0.37f;
                }
                else Shape("Lamp head", PrimitiveType.Cylinder, item.transform, new Vector3(0, 0, 0.13f), new Vector3(0.08f, 0.04f, 0.08f), orange);
                var tip = new GameObject("Contact point").transform; tip.SetParent(item.transform, false); tip.localPosition = tipPosition;
                Light light = null;
                if (kind == Tool.ToolKind.Lamp)
                {
                    light = tip.gameObject.AddComponent<Light>(); light.type = LightType.Spot;
                    light.range = 5; light.intensity = 5; light.spotAngle = 60; light.enabled = false;
                }
                var body = item.AddComponent<Rigidbody>(); body.mass = 0.5f; body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                item.AddComponent<Tool>().Configure(kind, tip, light);
                PrefabUtility.SaveAsPrefabAssetAndConnect(item, Root + "Prefabs/" + name + ".prefab", InteractionMode.AutomatedAction);
            }
            MakeTool("Hammer", Tool.ToolKind.Hammer, new Vector3(0.85f, 0.95f, 0.05f));
            MakeTool("Blade", Tool.ToolKind.Blade, new Vector3(0.85f, 0.95f, -0.45f));
            MakeTool("Lamp", Tool.ToolKind.Lamp, new Vector3(0.85f, 0.95f, -0.95f));
        }
        private static Material MaterialAsset(string name, Color color, string shaderName = "Universal Render Pipeline/Lit")
        {
            string path = Root + "Art/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find(shaderName)); AssetDatabase.CreateAsset(material, path); }
            material.color = color;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }
        private static GameObject Shape(string name, PrimitiveType primitive, Transform parent, Vector3 position, Vector3 scale, Material material, bool solid = true)
        {
            var obj = GameObject.CreatePrimitive(primitive); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale; obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) { UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>()); obj.layer = 2; }
            return obj;
        }
        private static TextMesh Text(string name, Transform parent, Vector3 position, string value, float size)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            var text = obj.AddComponent<TextMesh>(); text.text = value; text.fontSize = 64; text.characterSize = size * 0.25f;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = Color.white;
            return text;
        }

        [MenuItem("HunJam/Validate Demo")]
        public static void ValidateMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Validate();
        }

        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            foreach (var root in scene.GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0)
                        throw new InvalidOperationException("Missing script: " + transform.name);
            if (UnityEngine.Object.FindObjectsByType<EnergyCell>(FindObjectsSortMode.None).Length != 3) throw new InvalidOperationException("Expected 3 orbs.");
            if (UnityEngine.Object.FindFirstObjectByType<DemoPlayer>() == null) throw new InvalidOperationException("Missing player.");
            Debug.Log("HUNJAM: Scene validation passed.");
        }

        [MenuItem("HunJam/Build Quest APK")]
        public static void BuildQuest()
        {
            ConfigureQuest();
            InteractionDemoTools.RegisterScenes();
            if (Application.isBatchMode && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null) CapturePreview();
            Directory.CreateDirectory("Builds/Quest");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = Array.ConvertAll(Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled), scene => scene.path),
                locationPathName = "Builds/Quest/HunJam.apk", target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Quest build failed: " + report.summary.result);
            Debug.Log("HUNJAM: Quest APK build succeeded.");
        }

        public static void CapturePreview()
        {
            EditorSceneManager.OpenScene(ScenePath);
            // Batch previews must wait for real shaders instead of displaying compile placeholders.
            ShaderUtil.allowAsyncCompilation = false;
            var camera = Camera.main;
            camera.transform.position = new Vector3(3.5f, 3.3f, -4);
            camera.transform.LookAt(new Vector3(0, 0.9f, 0));
            var target = new RenderTexture(1280, 800, 24);
            camera.targetTexture = target;
            camera.Render();
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(1280, 800, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);
            texture.Apply();
            Directory.CreateDirectory("Builds");
            File.WriteAllBytes("Builds/DemoWorld-preview.png", texture.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log("HUNJAM: Preview captured.");
        }
    }
}
