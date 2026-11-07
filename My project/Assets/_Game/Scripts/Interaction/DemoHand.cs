using UnityEngine;
using UnityEngine.InputSystem;

namespace HunJam
{
    public sealed class DemoHand : MonoBehaviour
    {
        [SerializeField] private bool left;
        [SerializeField] private DemoPlayer player;
        [SerializeField] private Transform anchor;
        [SerializeField] private GameObject handVisual;
        [SerializeField] private float reach = 0.12f;
        private InputAction position, rotation, tracked, grip, trigger;
        public bool IsLeft => left;
        public Transform Anchor => anchor;
        public Grabbable HeldItem => Grabbable.HeldBy(anchor);
        private ControllerHandPose handPose;
        private bool gripConsumed, focused = true;
        private float nextGrabAttempt;
        private Transform metaController;
        public void UseMetaTracking(Transform controller)
        {
            metaController = controller;
            if (handVisual != null) handVisual.SetActive(false);
            handPose = null;
        }
        public void Configure(bool isLeft, DemoPlayer rig, Transform holdAnchor, GameObject visual)
        { left = isLeft; player = rig; anchor = holdAnchor; handVisual = visual; }
        private void Awake()
        {
            handPose = handVisual != null ? handVisual.GetComponent<ControllerHandPose>() : null;
            string device = left ? "<XRController>{LeftHand}/" : "<XRController>{RightHand}/";
            position = new InputAction("Hand position", InputActionType.Value, device + "devicePosition");
            rotation = new InputAction("Hand rotation", InputActionType.Value, device + "deviceRotation");
            tracked = new InputAction("Hand tracked", InputActionType.Button, device + "isTracked");
            grip = new InputAction("Grab", InputActionType.Button, device + "grip");
            trigger = new InputAction("Use / interact", InputActionType.Button, device + "trigger");
        }
        private void OnEnable()
        { position?.Enable(); rotation?.Enable(); tracked?.Enable(); grip?.Enable(); trigger?.Enable(); }
        private void OnDisable()
        { Drop(false); position?.Disable(); rotation?.Disable(); tracked?.Disable(); grip?.Disable(); trigger?.Disable(); }
        private void OnDestroy()
        { position?.Dispose(); rotation?.Dispose(); tracked?.Dispose(); grip?.Dispose(); trigger?.Dispose(); }
        public T FindNearby<T>() where T : Component
        {
            // Mounted controls follow an interpolated vehicle in LateUpdate. Their visible
            // transforms must reach PhysX before overlap, closest-point and occlusion queries.
            Physics.SyncTransforms();
            T closest = null;
            float distance = reach;
            foreach (var collider in Physics.OverlapSphere(anchor.position, reach, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                var candidate = collider.GetComponentInParent<T>();
                if (candidate == null) continue;
                if (candidate is Grabbable item && !item.CanGrab(anchor)) continue;
                Vector3 point = collider.ClosestPoint(anchor.position);
                float d = Vector3.Distance(anchor.position, point);
                if (d > distance) continue;
                if (Physics.Linecast(anchor.position, point, out var obstruction, ~(1 << 2), QueryTriggerInteraction.Ignore)
                    && obstruction.collider.GetComponentInParent<T>() != candidate) continue;
                closest = candidate;
                distance = d;
            }
            return closest;
        }
        private void LateUpdate()
        {
            bool usable = focused && player != null && player.IsVR && tracked.IsPressed();
            if (handVisual != null) handVisual.SetActive(usable && metaController == null);
            if (!usable) { Drop(false); return; }
            if (metaController != null)
                transform.SetPositionAndRotation(metaController.position, metaController.rotation);
            else
            {
                transform.localPosition = position.ReadValue<Vector3>();
                transform.localRotation = rotation.ReadValue<Quaternion>();
            }
            var held = HeldItem;
            handPose?.SetPose(grip.ReadValue<float>(), trigger.ReadValue<float>(), held != null ? held.FingerClosure : 1f);
            bool grabbing = grip.IsPressed();
            // Keep an unsuccessful squeeze pending while the hand approaches the surface.
            // Once it owns anything, require release before another acquisition (including
            // after a transfer, reset or forced release). Never steal a second object.
            if (held != null) gripConsumed = true;
            if (grabbing && !gripConsumed && held == null && Time.unscaledTime >= nextGrabAttempt)
            {
                nextGrabAttempt = Time.unscaledTime + 0.05f;
                var item = FindNearby<Grabbable>();
                if (item != null && item.Grab(anchor)) gripConsumed = true;
            }
            if (!grabbing) Drop(true);
            if (trigger.WasPressedThisFrame())
            {
                if (HeldItem != null) HeldItem.Use();
                else FindNearby<DemoInteractable>()?.Interact();
            }
        }
        private void Drop(bool throwing)
        { HeldItem?.Release(anchor, throwing); gripConsumed = false; nextGrabAttempt = 0; }
        public void Pulse(float amplitude, float duration)
        {
            var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(left ? UnityEngine.XR.XRNode.LeftHand : UnityEngine.XR.XRNode.RightHand);
            if (device.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
                device.SendHapticImpulse(0, Mathf.Clamp01(amplitude), Mathf.Max(0, duration));
        }
        private void OnApplicationFocus(bool focus) { focused = focus; if (!focus) Drop(false); }
        private void OnApplicationPause(bool pause) { if (pause) Drop(false); }
    }
}
