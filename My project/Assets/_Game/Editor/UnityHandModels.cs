using UnityEditor;
using UnityEngine;
namespace HunJam.Editor
{
    public static class UnityHandModels
    {
        public static GameObject Create(Transform parent, bool left)
        {
            string path = "Assets/_Game/Art/UnityHands/" + (left ? "LeftHand" : "RightHand") + ".fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) throw new System.InvalidOperationException("Missing Unity hand model: " + path);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            visual.name = "Unity " + (left ? "Left" : "Right") + " Hand";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Materials/White.mat");
            foreach (var t in visual.GetComponentsInChildren<Transform>()) t.gameObject.layer = 2;
            visual.AddComponent<ControllerHandPose>();
            return visual;
        }
        public static void Install()
        {
            const string path = "Assets/_Game/Prefabs/QuestPlayer.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var hand in root.GetComponentsInChildren<DemoHand>(true))
            {
                var serialized = new SerializedObject(hand);
                var old = (GameObject)serialized.FindProperty("handVisual").objectReferenceValue;
                if (old != null) Object.DestroyImmediate(old);
                var visual = Create(hand.transform, serialized.FindProperty("left").boolValue);
                serialized.FindProperty("handVisual").objectReferenceValue = visual;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            DemoProjectTools.Validate();
        }
    }
}
