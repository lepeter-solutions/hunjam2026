using UnityEngine;
using UnityEngine.InputSystem;

namespace HunJam
{
    public sealed class DemoHand : MonoBehaviour
    {
        [SerializeField] private bool left;
        [SerializeField] private DemoPlayer player;
        [SerializeField] private Transform anchor;
        [SerializeField] private LineRenderer ray;
        [SerializeField] private Renderer handVisual;
        [SerializeField] private float reach = 3f;
        private InputAction position, rotation, tracked, grip, trigger, teleport;
        private EnergyCell held;
        private bool wasGripping;

        public void Configure(bool isLeft, DemoPlayer rig, Transform holdAnchor, LineRenderer line, Renderer visual)
        { left = isLeft; player = rig; anchor = holdAnchor; ray = line; handVisual = visual; }
        private void Awake()
        {
            string device = left ? "<XRController>{LeftHand}/" : "<XRController>{RightHand}/";
            position = new InputAction("Hand position", InputActionType.Value, device + "devicePosition");
            rotation = new InputAction("Hand rotation", InputActionType.Value, device + "deviceRotation");
            tracked = new InputAction("Hand tracked", InputActionType.Button, device + "isTracked");
            grip = new InputAction("Grab", InputActionType.Button, device + "gripPressed");
            trigger = new InputAction("Interact", InputActionType.Button, device + "triggerPressed");
            teleport = new InputAction("Teleport", InputActionType.Button, device + "primaryButton");
        }
        private void OnEnable()
        { position?.Enable(); rotation?.Enable(); tracked?.Enable(); grip?.Enable(); trigger?.Enable(); teleport?.Enable(); }
        private void OnDisable()
        {
            Drop();
            position?.Disable(); rotation?.Disable(); tracked?.Disable(); grip?.Disable(); trigger?.Disable(); teleport?.Disable();
        }
        private void OnDestroy()
        { position?.Dispose(); rotation?.Dispose(); tracked?.Dispose(); grip?.Dispose(); trigger?.Dispose(); teleport?.Dispose(); }
        private void LateUpdate()
        {
            bool usable = player.IsVR ? tracked.IsPressed() : !left;
            handVisual.enabled = usable && player.IsVR;
            ray.enabled = usable;
            if (!usable) { Drop(); return; }
            if (player.IsVR)
            {
                transform.localPosition = position.ReadValue<Vector3>();
                transform.localRotation = rotation.ReadValue<Quaternion>();
            }
            else transform.SetPositionAndRotation(player.View.transform.position, player.View.transform.rotation);

            // The player and visual hands use Ignore Raycast; scenery blocks interaction rays.
            bool hitSomething = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, ~(1 << 2), QueryTriggerInteraction.Ignore);
            var target = hitSomething && hit.distance <= reach ? hit.collider.GetComponentInParent<DemoInteractable>() : null;
            var cell = hitSomething && hit.distance <= reach ? hit.collider.GetComponentInParent<EnergyCell>() : null;
            bool canTeleport = !left && hitSomething && player.CanTeleport(hit);
            Color color = target != null || cell != null ? Color.cyan : canTeleport ? Color.green : Color.gray;
            ray.startColor = ray.endColor = color;
            ray.SetPosition(0, transform.position);
            ray.SetPosition(1, hitSomething ? hit.point : transform.position + transform.forward * 10);

            bool grabbing = player.IsVR ? grip.IsPressed() : Mouse.current != null && Mouse.current.leftButton.isPressed;
            if (held != null && (!held.IsHeld || !held.isActiveAndEnabled)) held = null;
            if (grabbing && !wasGripping && cell != null && cell.Grab(anchor)) held = cell;
            if (!grabbing) Drop();
            wasGripping = grabbing;
            bool interact = player.IsVR ? trigger.WasPressedThisFrame() : Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            if (interact && target != null) target.Interact();
            if (player.IsVR && !left && teleport.WasPressedThisFrame() && canTeleport) player.Teleport(hit.point);
        }
        private void Drop()
        {
            if (held != null) held.Release();
            held = null;
            wasGripping = false;
        }
        private void OnApplicationFocus(bool focus) { if (!focus) Drop(); }
    }
}
