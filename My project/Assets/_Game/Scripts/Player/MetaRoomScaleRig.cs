using UnityEngine;

namespace HunJam
{
    /// <summary>Meta camera and native Capsense hands, attached to the existing room-scale root.</summary>
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(DemoPlayer))]
    public sealed class MetaRoomScaleRig : MonoBehaviour
    {
        [SerializeField] private GameObject cameraRigPrefab;
        [SerializeField] private GameObject leftHandPrefab, rightHandPrefab;
        [SerializeField] private Material handMaterial;
        [SerializeField] private OVRManager.ControllerDrivenHandPosesType handPoses = OVRManager.ControllerDrivenHandPosesType.Natural;
        private DemoPlayer player;
        private OVRCameraRig rig;

        public void Configure(GameObject cameraRig, GameObject left, GameObject right, Material material)
        { cameraRigPrefab = cameraRig; leftHandPrefab = left; rightHandPrefab = right; handMaterial = material; }

        private void Awake() => player = GetComponent<DemoPlayer>();

        private void Update()
        {
            // Leave the existing static camera available when no XR runtime is connected.
            if (rig != null || !player.IsVR) return;
            if (cameraRigPrefab == null || leftHandPrefab == null || rightHandPrefab == null)
            {
                Debug.LogError($"Meta XR prefab references are missing on QuestPlayer. " +
                    $"Camera rig: {(cameraRigPrefab != null ? "OK" : "MISSING")}, " +
                    $"left hand: {(leftHandPrefab != null ? "OK" : "MISSING")}, " +
                    $"right hand: {(rightHandPrefab != null ? "OK" : "MISSING")}", this);
                enabled = false;
                return;
            }
            var oldView = player.View;
            var instance = Instantiate(cameraRigPrefab, transform, false);
            instance.name = "Meta XR Room Scale Rig";
            rig = instance.GetComponent<OVRCameraRig>();
            rig.EnsureGameObjectIntegrity();
            var manager = instance.GetComponent<OVRManager>();
            manager.controllerDrivenHandPosesType = handPoses;
            manager.trackingOriginType = OVRManager.TrackingOrigin.Stage;

            var camera = rig.centerEyeAnchor.GetComponent<Camera>();
            camera.CopyFrom(oldView);
            camera.tag = "MainCamera";
            camera.enabled = true;
            oldView.gameObject.SetActive(false);
            oldView.tag = "Untagged";
            player.UseMetaTracking(rig, camera);

            var leftVisual = CreateHand(leftHandPrefab, rig.leftHandOnControllerAnchor);
            var rightVisual = CreateHand(rightHandPrefab, rig.rightHandOnControllerAnchor);
            foreach (var hand in GetComponentsInChildren<DemoHand>())
            {
                hand.transform.SetParent(rig.trackingSpace, false);
                hand.UseMetaTracking(hand.IsLeft ? rig.leftControllerAnchor : rig.rightControllerAnchor);
                var visual = hand.IsLeft ? leftVisual : rightVisual;
                visual.AddComponent<YokeHandAttachment>().Configure(hand);
            }
        }

        private GameObject CreateHand(GameObject prefab, Transform parent)
        {
            var hand = Instantiate(prefab, parent, false);
            foreach (var node in hand.GetComponentsInChildren<Transform>(true)) node.gameObject.layer = 2;
            if (handMaterial != null)
                foreach (var renderer in hand.GetComponentsInChildren<SkinnedMeshRenderer>(true)) renderer.sharedMaterial = handMaterial;
            return hand;
        }
    }
}
