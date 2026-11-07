using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace HunJam.Editor
{
    public static class InteractionDemoTools
    {
        private const string Root = "Assets/_Game/";
        public const string TestScene = Root + "Scenes/VRInteractionTest.unity";
        public const string FlightScene = Root + "Scenes/FlightYoke.unity";
        private static Material dark, cyan, amber, white, green, paper;
        [MenuItem("HunJam/Build Interaction Demos")]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Interaction demos", "Adds the interaction zone to DemoWorld and rebuilds VRInteractionTest / FlightYoke and their generated assets. Save manual edits first.", "Build", "Cancel")) return;
            Build();
        }
        public static void Build()
        {
            dark = Material("Structure"); cyan = Material("Cyan"); amber = Material("Amber"); white = Material("White"); green = Material("Green");
            string texturePath = Root + "Art/Contract.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.textureType = TextureImporterType.Default; importer.textureShape = TextureImporterShape.Texture2D;
            importer.maxTextureSize = 2048; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = true;
            importer.SaveAndReimport();
            var contractTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (contractTexture == null) throw new InvalidOperationException("Contract texture did not import.");
            paper = AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/Materials/Contract.mat");
            if (paper == null)
            {
                paper = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                paper.SetFloat("_Cull", 0); paper.SetFloat("_Smoothness", 0);
                AssetDatabase.CreateAsset(paper, Root + "Art/Materials/Contract.mat");
            }
            paper.SetTexture("_BaseMap", contractTexture); paper.SetTexture("_MainTex", contractTexture);
            EditorUtility.SetDirty(paper);
            CreateAssets();
            var demo = EditorSceneManager.OpenScene(DemoProjectTools.ScenePath);
            if (GameObject.Find("Two Hand Interaction Zone") == null)
            {
                var zone = new GameObject("Two Hand Interaction Zone").transform;
                zone.position = new Vector3(0, 0, -0.95f); zone.rotation = Quaternion.Euler(0, 180, 0);
                Station(zone, true);
                EditorSceneManager.SaveScene(demo);
            }
            var test = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Lighting();
            Shape("Floor", PrimitiveType.Cube, null, new Vector3(0, -0.1f, 0), new Vector3(7, 0.2f, 7), dark);
            Player(null);
            var testZone = new GameObject("Interaction stations").transform; Station(testZone, false);
            Label("Title", null, new Vector3(0, 2.1f, 1.35f), "VR INTERACTION LAB\nGrip: hold | second grip: join | release first: transfer", 0.05f);
            Button("Back to DemoWorld", null, new Vector3(-0.7f, 1, 0), "DemoWorld");
            Shape("Hammer stand", PrimitiveType.Cube, null, new Vector3(0.85f, 0.82f, 0.45f), new Vector3(0.25f, 0.1f, 0.55f), dark);
            Instance("Hammer", null, new Vector3(0.85f, 0.91f, 0.35f));
            EditorSceneManager.SaveScene(test, TestScene);
            BuildFlight();
            RegisterScenes(); AssetDatabase.SaveAssets();
            Debug.Log("HUNJAM: Interaction demos built without regenerating the existing world.");
        }
        private static void CreateAssets()
        {
            var cube = Shape("Transfer Cube", PrimitiveType.Cube, null, Vector3.zero, Vector3.one * 0.11f, cyan);
            Dynamic(cube).ConfigureGrip(Grabbable.GripMode.TransferOnly, closure: 0.55f); Save(cube, "TransferCube");
            var bottle = new GameObject("Bottle");
            Shape("Bottle body", PrimitiveType.Cylinder, bottle.transform, Vector3.zero, new Vector3(0.075f, 0.095f, 0.075f), green);
            Shape("Bottle neck", PrimitiveType.Cylinder, bottle.transform, new Vector3(0, 0.12f, 0), new Vector3(0.036f, 0.035f, 0.036f), white);
            Dynamic(bottle).ConfigureGrip(Grabbable.GripMode.TransferOnly, closure: 0.65f); Save(bottle, "Bottle");
            var rod = new GameObject("Two handed rod");
            Shape("Shaft", PrimitiveType.Cube, rod.transform, Vector3.zero, new Vector3(0.7f, 0.035f, 0.035f), white);
            Shape("Primary grip", PrimitiveType.Cube, rod.transform, new Vector3(-0.23f, 0, 0), new Vector3(0.14f, 0.05f, 0.05f), cyan);
            Shape("Secondary grip", PrimitiveType.Cube, rod.transform, new Vector3(0.23f, 0, 0), new Vector3(0.14f, 0.05f, 0.05f), amber);
            // Both ends are valid for the first grip; collider surface proximity chooses either end.
            Dynamic(rod).ConfigureGrip(Grabbable.GripMode.TwoHands, closure: 0.8f); Save(rod, "TwoHandRod");
            string meshPath = Root + "Art/ContractMesh.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null) { mesh = PaperMesh.Create(); AssetDatabase.CreateAsset(mesh, meshPath); }
            var contract = new GameObject("Tearable Contract");
            contract.AddComponent<MeshFilter>().sharedMesh = mesh; contract.AddComponent<MeshRenderer>().sharedMaterial = paper;
            contract.AddComponent<BoxCollider>().size = new Vector3(0.42f, 0.3f, 0.004f);
            Dynamic(contract).ConfigureGrip(Grabbable.GripMode.TwoHands, closure: 0.4f);
            contract.GetComponent<Rigidbody>().mass = 0.03f; contract.AddComponent<Tearable>(); Save(contract, "TearableContract");
            var knife = new GameObject("Blunt practice tool");
            Shape("Handle", PrimitiveType.Cube, knife.transform, Vector3.zero, new Vector3(0.045f, 0.055f, 0.15f), amber);
            Shape("Rounded practice blade", PrimitiveType.Cube, knife.transform, new Vector3(0, 0, 0.16f), new Vector3(0.012f, 0.035f, 0.18f), white);
            var tip = new GameObject("Tip").transform; tip.SetParent(knife.transform, false); tip.localPosition = new Vector3(0, 0, 0.25f);
            var rb = knife.AddComponent<Rigidbody>(); rb.mass = 0.25f; rb.interpolation = RigidbodyInterpolation.Interpolate;
            var stab = knife.AddComponent<StabbingTool>(); stab.Configure(tip);
            var handle = new GameObject("Grip region").transform; handle.SetParent(knife.transform, false);
            stab.ConfigureGrip(Grabbable.GripMode.TransferOnly, handle, handle, 0.13f, 0.8f); Save(knife, "PracticeKnife");
        }
        private static void Station(Transform parent, bool demo)
        {
            Shape("Interaction table", PrimitiveType.Cube, parent, new Vector3(0, 0.82f, 0.7f), new Vector3(1.5f, 0.1f, 0.65f), dark);
            Instance("TransferCube", parent, new Vector3(-0.57f, 0.96f, 0.55f));
            Instance("Bottle", parent, new Vector3(0.57f, 0.98f, 0.58f));
            Instance("TwoHandRod", parent, new Vector3(0, 0.91f, 0.92f));
            var contract = Instance("TearableContract", parent, new Vector3(-0.12f, 0.887f, 0.58f));
            contract.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Instance("PracticeKnife", parent, new Vector3(0.4f, 0.92f, 0.3f));
            var target = Shape("Stab practice foam", PrimitiveType.Cube, parent, new Vector3(0.65f, 1.12f, 1.02f), new Vector3(0.22f, 0.45f, 0.16f), amber);
            target.AddComponent<StabTarget>();
            Label("Contract instruction", parent, new Vector3(-0.1f, 1.35f, 0.96f), "Fogd meg két oldalt, és húzd szét!\nGRIP + GRIP → pull apart", 0.035f);
            Label("Stab instruction", parent, new Vector3(0.65f, 1.5f, 1.05f), "PRACTICE FOAM\nTip first | push / pull", 0.028f);
            var reset = Shape("Reset interactions", PrimitiveType.Cube, parent, new Vector3(-0.68f, 0.96f, 0.25f), Vector3.one * 0.12f, green);
            reset.AddComponent<WorkshopReset>();
            Label("Reset label", parent, new Vector3(-0.68f, 1.13f, 0.3f), "RESET\nTouch + trigger", 0.025f);
            Label("Transfer instruction", parent, new Vector3(-0.65f, 1.55f, 0.95f), "HANDOVER\nOther grip, then release first\nRod: both grips steer", 0.027f);
            if (demo)
            {
                Button("INTERACTION LAB", parent, new Vector3(-0.65f, 1.1f, -0.1f), "VRInteractionTest");
                Button("FLIGHT YOKE", parent, new Vector3(0.65f, 1.1f, -0.1f), "FlightYoke");
            }
            else Button("FLIGHT YOKE", parent, new Vector3(0.7f, 1, -0.1f), "FlightYoke");
        }
        private static void BuildFlight()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); Lighting();
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(0.55f, 0.72f, 0.82f); RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 600; RenderSettings.fogEndDistance = 1400;
            Shape("Landscape", PrimitiveType.Cube, null, new Vector3(0, -5, 0), new Vector3(4000, 10, 4000), green);
            Shape("River", PrimitiveType.Cube, null, new Vector3(200, 0.03f, 0), new Vector3(90, 0.02f, 3500), cyan, false);
            for (int i = 0; i < 24; i++)
            {
                float x = (i % 6 - 2.5f) * 260, z = (i / 6 - 1) * 450 + 200;
                Shape("Low terrain landmark", PrimitiveType.Cube, null, new Vector3(x, 15 + i % 4 * 5, z), new Vector3(90, 30 + i % 4 * 10, 100), dark);
            }
            var plane = new GameObject("Training Aircraft"); plane.transform.position = new Vector3(0, 120, 0);
            var rb = plane.AddComponent<Rigidbody>(); rb.mass = 800; rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var hull = plane.AddComponent<BoxCollider>(); hull.center = new Vector3(0, 0.15f, 0.5f); hull.size = new Vector3(1.4f, 0.4f, 2.7f);
            Shape("Cockpit floor", PrimitiveType.Cube, plane.transform, new Vector3(0, 0, 0.3f), new Vector3(1.45f, 0.14f, 2.5f), dark, false);
            Shape("Left wing", PrimitiveType.Cube, plane.transform, new Vector3(-2.1f, 0.2f, 0.5f), new Vector3(3, 0.09f, 0.85f), white, false);
            Shape("Right wing", PrimitiveType.Cube, plane.transform, new Vector3(2.1f, 0.2f, 0.5f), new Vector3(3, 0.09f, 0.85f), white, false);
            Shape("Seat", PrimitiveType.Cube, plane.transform, new Vector3(0, 0.42f, 0), new Vector3(0.5f, 0.16f, 0.5f), cyan, false);
            Shape("Seat back", PrimitiveType.Cube, plane.transform, new Vector3(0, 0.8f, -0.3f), new Vector3(0.5f, 0.65f, 0.12f), cyan, false);
            Shape("Dashboard", PrimitiveType.Cube, plane.transform, new Vector3(0, 0.8f, 0.9f), new Vector3(1.3f, 0.5f, 0.12f), dark, false);
            var mount = new GameObject("Yoke mounting").transform; mount.SetParent(plane.transform, false); mount.localPosition = new Vector3(0, 0.85f, 0.48f);
            var yokeObj = new GameObject("Flight Yoke"); yokeObj.transform.SetParent(mount, false);
            Shape("Crossbar", PrimitiveType.Cube, yokeObj.transform, Vector3.zero, new Vector3(0.42f, 0.04f, 0.04f), dark, false);
            Shape("Left grip", PrimitiveType.Cube, yokeObj.transform, new Vector3(-0.21f, 0.04f, 0), new Vector3(0.045f, 0.17f, 0.055f), cyan);
            Shape("Right grip", PrimitiveType.Cube, yokeObj.transform, new Vector3(0.21f, 0.04f, 0), new Vector3(0.045f, 0.17f, 0.055f), cyan);
            var yokeBody = yokeObj.AddComponent<Rigidbody>();
            yokeBody.isKinematic = true; yokeBody.useGravity = false;
            yokeBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            var yoke = yokeObj.AddComponent<FlightYoke>(); yoke.ConfigureMount(mount); yoke.ConfigureGrip(Grabbable.GripMode.TwoHands, closure: 0.8f);
            var display = Label("Flight instruments", plane.transform, new Vector3(0, 0.95f, 0.82f), "58 km/h     ALT 120 m\nBANK 0°  PITCH 0°\nPULL: UP   PUSH: DOWN\nTurn yoke to bank | Release to level", 0.015f);
            var vehicle = plane.AddComponent<FlightVehicle>(); vehicle.Configure(yoke, display);
            var player = Player(plane.transform); player.View.farClipPlane = 1600; player.View.backgroundColor = RenderSettings.fogColor;
            player.View.transform.localRotation = Quaternion.Euler(15, 0, 0);
            var eye = new GameObject("Seated eye").transform; eye.SetParent(plane.transform, false); eye.localPosition = new Vector3(0, 1.2f, 0);
            var seat = plane.AddComponent<FlightSeat>(); seat.Configure(player, eye);
            var reset = Button("RESET / RECENTER", plane.transform, new Vector3(-0.48f, 0.87f, 0.45f), null); reset.Configure(null, vehicle, seat);
            Button("DEMO WORLD", plane.transform, new Vector3(0.48f, 0.87f, 0.45f), "DemoWorld");
            foreach (var button in plane.GetComponentsInChildren<DemoSceneButton>())
            {
                var text = plane.GetComponentsInChildren<TextMesh>().First(t => t.name == button.name + " label");
                text.text = button == reset ? "RESET\nTouch + trigger" : "DEMO WORLD\nTouch + trigger";
                text.characterSize = 0.012f * 0.25f;
            }
            Label("Flight instructions", plane.transform, new Vector3(0, 1.38f, 1.8f), "FLIGHT YOKE\nGrip one or both handles | Turn to bank\nPull up / Push down | Release to level", 0.018f);
            PrefabUtility.SaveAsPrefabAssetAndConnect(plane, Root + "Prefabs/TrainingAircraft.prefab", InteractionMode.AutomatedAction);
            EditorSceneManager.SaveScene(scene, FlightScene);
        }
        public static void RegisterScenes()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            foreach (var path in new[] { DemoProjectTools.ScenePath, TestScene, FlightScene })
            {
                if (!File.Exists(path)) continue;
                var existing = scenes.Find(s => s.path == path);
                if (existing != null) existing.enabled = true; else scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        private static DemoPlayer Player(Transform parent)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/QuestPlayer.prefab"));
            obj.transform.SetParent(parent, false); obj.transform.localPosition = Vector3.zero; return obj.GetComponent<DemoPlayer>();
        }
        private static Material Material(string name) => AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/Materials/" + name + ".mat");
        private static Grabbable Dynamic(GameObject obj)
        {
            var rb = obj.AddComponent<Rigidbody>(); rb.mass = 0.2f; rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; return obj.AddComponent<Grabbable>();
        }
        private static void Save(GameObject obj, string name)
        { PrefabUtility.SaveAsPrefabAsset(obj, Root + "Prefabs/" + name + ".prefab"); UnityEngine.Object.DestroyImmediate(obj); }
        private static GameObject Instance(string name, Transform parent, Vector3 position)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/" + name + ".prefab"));
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; return obj;
        }
        private static DemoSceneButton Button(string label, Transform parent, Vector3 position, string scene)
        {
            var obj = Shape(label, PrimitiveType.Cube, parent, position, Vector3.one * 0.1f, green);
            var button = obj.AddComponent<DemoSceneButton>(); button.Configure(scene);
            Label(label + " label", parent, position + Vector3.up * 0.14f, label + "\nTouch + trigger", 0.021f); return button;
        }
        private static GameObject Shape(string name, PrimitiveType shape, Transform parent, Vector3 position, Vector3 scale, Material material, bool solid = true)
        {
            var obj = GameObject.CreatePrimitive(shape); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale; obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) { UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>()); obj.layer = 2; }
            return obj;
        }
        private static TextMesh Label(string name, Transform parent, Vector3 position, string value, float size)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            var text = obj.AddComponent<TextMesh>(); text.text = value; text.fontSize = 64; text.characterSize = size * 0.25f;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = Color.white; return text;
        }
        private static void Lighting()
        {
            var sun = new GameObject("Sun").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(45, -30, 0); sun.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.6f, 0.7f, 0.8f); RenderSettings.skybox = null;
        }
        public static void ValidateAndPreview()
        {
            ShaderUtil.allowAsyncCompilation = false;
            foreach (var path in new[] { DemoProjectTools.ScenePath, TestScene, FlightScene })
            {
                var scene = EditorSceneManager.OpenScene(path);
                foreach (var root in scene.GetRootGameObjects()) foreach (var child in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0) throw new InvalidOperationException("Missing script: " + child.name);
                if (UnityEngine.Object.FindFirstObjectByType<DemoPlayer>() == null) throw new InvalidOperationException("Missing rig: " + path);
                foreach (var sheet in UnityEngine.Object.FindObjectsByType<Tearable>(FindObjectsSortMode.None))
                    if (sheet.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_BaseMap") == null) throw new InvalidOperationException("Missing contract texture: " + path);
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) continue;
                var camera = Camera.main;
                foreach (var hand in UnityEngine.Object.FindObjectsByType<DemoHand>(FindObjectsSortMode.None)) hand.gameObject.SetActive(false);
                if (path == FlightScene) { camera.fieldOfView = 75; camera.transform.position = new Vector3(0, 121.2f, 0); camera.transform.rotation = Quaternion.Euler(18, 0, 0); }
                else { camera.transform.position = path == TestScene ? new Vector3(1.7f, 2.3f, -1.8f) : new Vector3(0.2f, 2.4f, -0.2f); camera.transform.LookAt(path == TestScene ? new Vector3(0, 0.9f, 0.65f) : new Vector3(0, 0.9f, -1.65f)); }
                var target = new RenderTexture(1280, 800, 24); camera.targetTexture = target; camera.Render(); camera.Render();
                var previous = RenderTexture.active; RenderTexture.active = target;
                var image = new Texture2D(1280, 800, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0); image.Apply();
                Directory.CreateDirectory("Builds/Previews"); File.WriteAllBytes("Builds/Previews/" + Path.GetFileNameWithoutExtension(path) + ".png", image.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target);
            }
            Debug.Log("HUNJAM: All three scenes validated.");
        }
        public static void BuildValidationPlayer()
        {
            RegisterScenes();
            Debug.Log("HUNJAM: Android build support installed: " + BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android));
            Directory.CreateDirectory("Builds/WindowsSmoke");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Builds/WindowsSmoke/HunJam.exe", target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Windows validation build failed: " + report.summary.result);
            Debug.Log("HUNJAM: Windows validation build succeeded; this does not validate Quest/Android.");
        }
    }
}
