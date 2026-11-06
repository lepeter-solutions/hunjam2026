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
        private Grabbable held;
        private bool wasGripping, focused = true;
        public void Configure(bool isLeft, DemoPlayer rig, Transform holdAnchor, GameObject visual)
        { left = isLeft; player = rig; anchor = holdAnchor; handVisual = visual; }
        private void Awake()
        {
            string device = left ? "<XRController>{LeftHand}/" : "<XRController>{RightHand}/";
            position = new InputAction("Hand position", InputActionType.Value, device + "devicePosition");
            rotation = new InputAction("Hand rotation", InputActionType.Value, device + "deviceRotation");
            tracked = new InputAction("Hand tracked", InputActionType.Button, device + "isTracked");
            grip = new InputAction("Grab", InputActionType.Button, device + "gripPressed");
            trigger = new InputAction("Use / interact", InputActionType.Button, device + "triggerPressed");
        }
        private void OnEnable()
        { position?.Enable(); rotation?.Enable(); tracked?.Enable(); grip?.Enable(); trigger?.Enable(); }
        private void OnDisable()
        { Drop(false); position?.Disable(); rotation?.Disable(); tracked?.Disable(); grip?.Disable(); trigger?.Disable(); }
        private void OnDestroy()
        { position?.Dispose(); rotation?.Dispose(); tracked?.Dispose(); grip?.Dispose(); trigger?.Dispose(); }
        public T FindNearby<T>() where T : Component
        {
            T closest = null;
            float distance = reach;
            foreach (var collider in Physics.OverlapSphere(anchor.position, reach, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                var candidate = collider.GetComponentInParent<T>();
                if (candidate == null) continue;
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
            bool usable = focused && player.IsVR && tracked.IsPressed();
            handVisual.SetActive(usable);
            if (!usable) { Drop(false); return; }
            transform.localPosition = position.ReadValue<Vector3>();
            transform.localRotation = rotation.ReadValue<Quaternion>();
            bool grabbing = grip.IsPressed();
            if (held != null && (!held.IsHeld || !held.isActiveAndEnabled)) held = null;
            if (grabbing && !wasGripping && held == null)
            {
                var item = FindNearby<Grabbable>();
                if (item != null && item.Grab(anchor)) held = item;
            }
            if (!grabbing) Drop(true);
            wasGripping = grabbing;
            if (trigger.WasPressedThisFrame())
            {
                if (held != null) held.Use();
                else FindNearby<DemoInteractable>()?.Interact();
            }
        }
        private void Drop(bool throwing)
        { if (held != null) held.Release(throwing); held = null; wasGripping = false; }
        private void OnApplicationFocus(bool focus) { focused = focus; if (!focus) Drop(false); }
        private void OnApplicationPause(bool pause) { if (pause) Drop(false); }
    }
}
