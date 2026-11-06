using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;

namespace HunJam
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class DemoPlayer : MonoBehaviour
    {
        [SerializeField] private Transform trackingSpace;
        [SerializeField] private Camera view;
        [SerializeField] private DemoHand leftHand;
        [SerializeField] private DemoHand rightHand;
        [SerializeField] private float speed = 1.8f;
        [SerializeField] private float snapAngle = 30f;
        private CharacterController body;
        private InputAction move, turn;
        private bool snapReady = true;
        private float pitch, gravity;
        private bool floorConfigured;
        private readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        public bool IsVR => XRSettings.isDeviceActive;
        public Camera View => view;

        public void Configure(Transform space, Camera camera, DemoHand left, DemoHand right)
        { trackingSpace = space; view = camera; leftHand = left; rightHand = right; }

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            move = new InputAction("Move", InputActionType.Value, "<XRController>{LeftHand}/primary2DAxis");
            turn = new InputAction("Turn", InputActionType.Value, "<XRController>{RightHand}/primary2DAxis");
            var pose = view.gameObject.AddComponent<TrackedPoseDriver>();
            pose.positionInput = new InputActionProperty(new InputAction("Head position", InputActionType.Value, "<XRHMD>/centerEyePosition"));
            pose.rotationInput = new InputActionProperty(new InputAction("Head rotation", InputActionType.Value, "<XRHMD>/centerEyeRotation"));
            pose.trackingStateInput = new InputActionProperty(new InputAction("Head tracking", InputActionType.Value, "<XRHMD>/trackingState"));
            foreach (var cell in FindObjectsByType<EnergyCell>(FindObjectsSortMode.None))
                Physics.IgnoreCollision(body, cell.GetComponent<Collider>());
        }
        private void OnEnable() { move?.Enable(); turn?.Enable(); }
        private void OnDisable() { move?.Disable(); turn?.Disable(); }
        private void OnDestroy() { move?.Dispose(); turn?.Dispose(); }
        private void Update()
        {
            Vector2 direction = Vector2.zero;
            if (IsVR)
            {
                if (!floorConfigured)
                {
                    SubsystemManager.GetSubsystems(subsystems);
                    foreach (var subsystem in subsystems)
                        if (subsystem.running)
                        {
                            bool floor = subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
                            trackingSpace.localPosition = floor ? Vector3.zero : Vector3.up * 1.65f;
                            floorConfigured = true;
                        }
                }
                direction = move.ReadValue<Vector2>();
                float axis = turn.ReadValue<Vector2>().x;
                if (Mathf.Abs(axis) < 0.25f) snapReady = true;
                if (Mathf.Abs(axis) > 0.7f && snapReady)
                {
                    transform.RotateAround(view.transform.position, Vector3.up, Mathf.Sign(axis) * snapAngle);
                    snapReady = false;
                }
            }
            else
            {
                floorConfigured = false;
                trackingSpace.localPosition = Vector3.zero;
                view.transform.localPosition = Vector3.up * 1.65f;
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    direction = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                        (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
                    if (keyboard.backspaceKey.wasPressedThisFrame) FindFirstObjectByType<DemoGame>().Begin();
                }
                if (Mouse.current != null && Mouse.current.rightButton.isPressed)
                {
                    Vector2 delta = Mouse.current.delta.ReadValue() * 0.12f;
                    transform.Rotate(0, delta.x, 0);
                    pitch = Mathf.Clamp(pitch - delta.y, -80, 80);
                }
                view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            }
            Vector3 localHead = transform.InverseTransformPoint(view.transform.position);
            body.height = Mathf.Clamp(localHead.y, 0.6f, 2.2f);
            body.center = new Vector3(localHead.x, body.height * 0.5f + body.skinWidth, localHead.z);
            Vector3 forward = Vector3.ProjectOnPlane(view.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
            Vector3 movement = (forward * direction.y + Vector3.Cross(Vector3.up, forward) * direction.x);
            gravity = body.isGrounded ? -2f : gravity + Physics.gravity.y * Time.deltaTime;
            body.Move((Vector3.ClampMagnitude(movement, 1) * speed + Vector3.up * gravity) * Time.deltaTime);
            if (transform.position.y < -5f) Teleport(new Vector3(0, 0.05f, -7));
        }
        public bool CanTeleport(RaycastHit hit)
        {
            if (hit.collider.GetComponent<WalkableSurface>() == null || hit.normal.y < 0.9f) return false;
            Vector3 bottom = hit.point + Vector3.up * (body.radius + 0.08f);
            Vector3 top = hit.point + Vector3.up * (body.height - body.radius + 0.08f);
            return !Physics.CheckCapsule(bottom, top, body.radius, ~(1 << 2), QueryTriggerInteraction.Ignore);
        }
        public void Teleport(Vector3 point)
        {
            Vector3 headOffset = Vector3.ProjectOnPlane(view.transform.position - transform.position, Vector3.up);
            body.enabled = false;
            transform.position = point - headOffset + Vector3.up * 0.03f;
            body.enabled = true;
            gravity = 0;
        }
    }
}
