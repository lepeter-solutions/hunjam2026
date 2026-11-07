using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

namespace HunJam
{
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : Oculus.Interaction.Grabbable, ITransformer
    {
        public enum GripMode { TransferOnly, TwoHands }
        public enum GrabState { Free, HeldLeft, HeldRight, HeldBoth, TransferPending, Releasing, Thrown }
        [SerializeField] private GripMode gripMode;
        [Tooltip("Optional grip zones; empty means free surface grabbing. Offsets never snap.")]
        [SerializeField] private Transform primaryGrip, secondaryGrip;
        [SerializeField, Min(0.01f)] private float gripRadius = 0.14f;
        [SerializeField, Range(0, 1)] private float fingerClosure = 0.75f;
        private static readonly Dictionary<Transform, Grabbable> owners = new Dictionary<Transform, Grabbable>();
        protected Rigidbody Body { get; private set; }
        protected virtual bool KeepKinematicOnRelease => false;
        public Transform Primary { get; private set; }
        public Transform Secondary { get; private set; }
        public Vector3 PrimaryContact { get; private set; }
        public Vector3 SecondaryContact { get; private set; }
        public GrabState State { get; private set; }
        public event Action<Grabbable> GripChanged;
        public bool IsHeld => Primary != null || Secondary != null;
        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocity { get; private set; }
        public float FingerClosure => fingerClosure;
        public GripMode Mode => gripMode;
        public bool CanImpact => IsHeld && Time.time >= armedAt;
        private Vector3 startPosition, previousPosition, localOffset, previousPrimary, previousSecondary;
        private Quaternion startRotation, previousRotation, rotationOffset, twoHandOffset;
        private Vector3 frameUp;
        private bool twoHandFrameValid;
        private float armedAt;
        private bool sdkStarted, applyingPhysics;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearOwners() => owners.Clear();
        public static Grabbable HeldBy(Transform anchor) => anchor != null && owners.TryGetValue(anchor, out var item) ? item : null;
        public bool IsHeldBy(Transform anchor) => anchor != null && (Primary == anchor || Secondary == anchor);
        public void ConfigureGrip(GripMode mode, Transform first = null, Transform second = null, float radius = 0.14f, float closure = 0.75f)
        { gripMode = mode; primaryGrip = first; secondaryGrip = second; gripRadius = radius; fingerClosure = closure; }
        protected override void Awake()
        {
            base.Awake();
            Body = GetComponent<Rigidbody>();
            startPosition = Body.position;
            startRotation = Body.rotation;
            // The native Meta Grabbable owns the pointer/transformer lifecycle.
            // This transformer adds our primary-contact, yoke and stabbing constraints.
            InjectOptionalTargetTransform(transform);
            InjectOptionalRigidbody(Body);
            InjectOptionalOneGrabTransformer(this);
            InjectOptionalTwoGrabTransformer(this);
            InjectOptionalKinematicWhileSelected(false);
            InjectOptionalThrowWhenUnselected(false);
            MaxGrabPoints = 2;
            TransferOnSecondSelection = false;
        }
        protected override void Start()
        {
            if (sdkStarted) return;
            base.Start();
            sdkStarted = true;
        }
        private void SendPointer(Transform anchor, PointerEventType type)
        {
            // Fragments can be selected in their creation frame, before Unity calls Start.
            if (!sdkStarted) Start();
            Pose pose = anchor != null ? new Pose(anchor.position, anchor.rotation) : Pose.identity;
            base.ProcessPointerEvent(new PointerEvent(anchor.GetInstanceID(), type, pose));
        }
        public bool CanGrab(Transform anchor)
        {
            if (anchor == null || !isActiveAndEnabled || IsHeldBy(anchor) || Secondary != null || HeldBy(anchor) != null) return false;
            var zone = IsHeld ? secondaryGrip : primaryGrip;
            if (zone != null) return Vector3.Distance(zone.position, anchor.position) <= gripRadius;
            if (!IsHeld) return true; // Initial proximity/occlusion is checked by DemoHand.
            foreach (var collider in GetComponentsInChildren<Collider>())
                if (collider.enabled && !collider.isTrigger && Vector3.Distance(collider.ClosestPoint(anchor.position), anchor.position) <= gripRadius) return true;
            return false;
        }
        public bool Grab(Transform anchor)
        {
            if (!CanGrab(anchor)) return false;
            owners[anchor] = this;
            if (Primary == null)
            {
                Primary = anchor;
                PrimaryContact = transform.InverseTransformPoint(anchor.position);
                Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                Body.isKinematic = true;
                Velocity = AngularVelocity = Vector3.zero;
            }
            else
            {
                Secondary = anchor;
                SecondaryContact = transform.InverseTransformPoint(anchor.position);
            }
            Rebase();
            UpdateState();
            SendPointer(anchor, PointerEventType.Hover);
            SendPointer(anchor, PointerEventType.Select);
            OnGripChanged();
            anchor.GetComponentInParent<DemoHand>()?.Pulse(0.18f, 0.035f);
            return true;
        }
        private void UpdateState()
        {
            State = Secondary != null ? (gripMode == GripMode.TwoHands ? GrabState.HeldBoth : GrabState.TransferPending)
                : Primary == null ? GrabState.Free
                : Primary.GetComponentInParent<DemoHand>()?.IsLeft == true ? GrabState.HeldLeft : GrabState.HeldRight;
        }
        private void Rebase()
        {
            if (Primary == null) return;
            localOffset = Primary.InverseTransformPoint(Body.position);
            rotationOffset = Quaternion.Inverse(Primary.rotation) * Body.rotation;
            previousPosition = Body.position;
            previousRotation = Body.rotation;
            previousPrimary = Primary.position;
            previousSecondary = Secondary != null ? Secondary.position : Vector3.zero;
            twoHandFrameValid = false;
            frameUp = Primary.up;
            if (Secondary != null && TryGripFrame(out var frame))
            { twoHandOffset = Quaternion.Inverse(frame) * Body.rotation; twoHandFrameValid = true; }
            armedAt = Time.time + 0.15f;
        }
        private bool TryGripFrame(out Quaternion frame)
        {
            Vector3 direction = Secondary.position - Primary.position;
            frame = Quaternion.identity;
            if (direction.sqrMagnitude < 0.0016f) return false;
            direction.Normalize();
            // Parallel transport the up vector to avoid flips when hands cross the vertical axis.
            Vector3 up = Vector3.ProjectOnPlane(frameUp, direction);
            if (up.sqrMagnitude < 0.001f) up = Vector3.ProjectOnPlane(Primary.forward, direction);
            if (up.sqrMagnitude < 0.001f) up = Vector3.ProjectOnPlane(Primary.right, direction);
            frameUp = up.normalized;
            frame = Quaternion.LookRotation(direction, frameUp);
            return true;
        }
        public void Release(Transform anchor, bool throwObject = true)
        {
            bool primary = ReferenceEquals(Primary, anchor), secondary = ReferenceEquals(Secondary, anchor);
            if ((!primary && !secondary) || ReferenceEquals(anchor, null)) return;
            SendPointer(anchor, PointerEventType.Unselect);
            SendPointer(anchor, PointerEventType.Unhover);
            owners.Remove(anchor);
            if (primary) { Primary = Secondary; PrimaryContact = SecondaryContact; Secondary = null; }
            else Secondary = null;
            if (Primary != null) { Rebase(); UpdateState(); }
            else FinishRelease(throwObject);
            OnGripChanged();
        }
        public void Release(bool throwObject = true)
        {
            bool hadHands = !ReferenceEquals(Primary, null) || !ReferenceEquals(Secondary, null);
            if (!ReferenceEquals(Secondary, null)) Release(Secondary, throwObject);
            if (!ReferenceEquals(Primary, null)) Release(Primary, throwObject);
            if (!hadHands) { FinishRelease(throwObject); OnGripChanged(); }
        }
        protected virtual void FinishRelease(bool throwObject)
        {
            State = GrabState.Releasing;
            Body.isKinematic = KeepKinematicOnRelease;
            Body.collisionDetectionMode = KeepKinematicOnRelease
                ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.ContinuousDynamic;
            if (!KeepKinematicOnRelease)
            {
                Body.linearVelocity = throwObject ? Velocity : Vector3.zero;
                Body.angularVelocity = throwObject ? AngularVelocity : Vector3.zero;
            }
            State = throwObject && Velocity.sqrMagnitude > 0.01f ? GrabState.Thrown : GrabState.Free;
            Velocity = AngularVelocity = Vector3.zero;
        }
        protected virtual void OnGripChanged() => GripChanged?.Invoke(this);
        public virtual void Use() { }
        public virtual void ResetObject()
        {
            Release(false);
            gameObject.SetActive(true);
            Body.position = startPosition;
            Body.rotation = startRotation;
        }
        protected virtual void CalculateTarget(out Vector3 target, out Quaternion rotation)
        {
            rotation = Primary.rotation * rotationOffset;
            if (Secondary != null && gripMode == GripMode.TwoHands)
            {
                if (TryGripFrame(out var frame))
                {
                    if (!twoHandFrameValid) { twoHandOffset = Quaternion.Inverse(frame) * Body.rotation; twoHandFrameValid = true; }
                    rotation = frame * twoHandOffset;
                }
                else { rotation = previousRotation; twoHandFrameValid = false; }
            }
            // Rotate around the captured primary contact, never around the mesh centre.
            Vector3 contactFromCentre = Quaternion.Inverse(previousRotation) * (previousPrimary - previousPosition);
            target = Secondary != null && gripMode == GripMode.TwoHands
                ? Primary.position - rotation * contactFromCentre : Primary.TransformPoint(localOffset);
        }
        protected virtual void FixedUpdate()
        {
            if (!ReferenceEquals(Primary, null) && (Primary == null || !Primary.gameObject.activeInHierarchy)) Release(Primary, false);
            if (!ReferenceEquals(Secondary, null) && (Secondary == null || !Secondary.gameObject.activeInHierarchy)) Release(Secondary, false);
            if (!IsHeld)
            {
                if (Body.position.y < -3f) ResetObject();
                return;
            }
            if (Vector3.Distance(Primary.position, previousPrimary) > 0.5f) { Release(Primary, false); return; }
            if (Secondary != null && Vector3.Distance(Secondary.position, previousSecondary) > 0.5f) { Release(Secondary, false); return; }
            // Update both native grab points, then apply one physics step through Meta's transformer.
            if (Secondary != null) SendPointer(Secondary, PointerEventType.Move);
            applyingPhysics = true;
            try { SendPointer(Primary, PointerEventType.Move); }
            finally { applyingPhysics = false; }
        }
        void ITransformer.Initialize(IGrabbable grabbable) { }
        void ITransformer.BeginTransform() { }
        void ITransformer.EndTransform() { }
        void ITransformer.UpdateTransform()
        {
            if (!applyingPhysics || !IsHeld) return;
            CalculateTarget(out var target, out var rotation);
            Velocity = Vector3.ClampMagnitude((target - previousPosition) / Time.fixedDeltaTime, 8f);
            Quaternion difference = rotation * Quaternion.Inverse(previousRotation);
            difference.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            AngularVelocity = Mathf.Abs(angle) < 0.001f ? Vector3.zero : Vector3.ClampMagnitude(axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime), 20);
            previousPosition = target; previousRotation = rotation;
            previousPrimary = Primary.position;
            previousSecondary = Secondary != null ? Secondary.position : Vector3.zero;
            ApplyTargetPose(target, rotation);
        }
        protected virtual void ApplyTargetPose(Vector3 position, Quaternion rotation)
        { Body.MovePosition(position); Body.MoveRotation(rotation); }
        protected override void OnDisable()
        {
            if (Body != null) Release(false);
            base.OnDisable();
        }
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (primaryGrip != null) Gizmos.DrawWireSphere(primaryGrip.position, gripRadius);
            Gizmos.color = Color.yellow;
            if (secondaryGrip != null) Gizmos.DrawWireSphere(secondaryGrip.position, gripRadius);
        }
    }
}
