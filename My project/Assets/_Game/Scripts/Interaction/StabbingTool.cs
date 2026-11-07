using UnityEngine;
namespace HunJam
{
    public sealed class StabbingTool : Grabbable
    {
        [SerializeField] private Transform tip;
        [SerializeField, Min(0.1f)] private float minimumSpeed = 0.6f;
        [SerializeField, Range(0, 1)] private float alignment = 0.8f;
        private StabTarget target;
        private Vector3 entry, axis, previousTip;
        private Quaternion insertionRotation;
        private bool sampling;
        private Collider[] ignored;
        public bool IsInserted => target != null;
        public float Depth { get; private set; }
        public void Configure(Transform contact) => tip = contact;
        protected override void FixedUpdate()
        {
            if (ignored != null && (target == null || !target.isActiveAndEnabled))
            { Detach(); if (!IsHeld) base.FinishRelease(false); }
            base.FixedUpdate();
        }
        private void LateUpdate()
        {
            if (!CanImpact || IsInserted || tip == null) { sampling = false; return; }
            var current = tip.position;
            if (sampling)
            {
                var travel = current - previousTip;
                var velocity = travel / Mathf.Max(0.001f, Time.deltaTime);
                if (travel.magnitude < 0.5f && velocity.magnitude >= minimumSpeed)
                {
                    var hits = Physics.SphereCastAll(previousTip, 0.008f, travel.normalized, travel.magnitude, ~(1 << 2), QueryTriggerInteraction.Ignore);
                    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                    foreach (var hit in hits)
                    {
                        if (hit.collider.GetComponentInParent<Grabbable>() == this) continue;
                        TryInsert(hit.collider.GetComponentInParent<StabTarget>(), hit.point, hit.normal, velocity);
                        break;
                    }
                }
            }
            previousTip = current; sampling = true;
        }
        public bool TryInsert(StabTarget material, Vector3 point, Vector3 normal, Vector3 velocity)
        {
            if (material == null || tip == null || IsInserted || !CanImpact || velocity.magnitude < minimumSpeed
                || Vector3.Dot(velocity.normalized, tip.forward) < alignment || Vector3.Dot(tip.forward, -normal) < alignment) return false;
            target = material; entry = target.transform.InverseTransformPoint(point);
            axis = target.transform.InverseTransformDirection(-normal).normalized;
            insertionRotation = Quaternion.Inverse(target.transform.rotation) * Body.rotation;
            ignored = target.GetComponentsInChildren<Collider>(); IgnoreTarget(true);
            Depth = 0;
            InteractionFeedback.Pulse(this, 0.25f, 0.06f); InteractionFeedback.Play(point, false);
            return true;
        }
        protected override void CalculateTarget(out Vector3 position, out Quaternion rotation)
        {
            base.CalculateTarget(out position, out rotation);
            if (!IsInserted) return;
            rotation = target.transform.rotation * insertionRotation;
            var direction = target.transform.TransformDirection(axis);
            var contact = target.transform.TransformPoint(entry);
            var tipOffset = Quaternion.Inverse(Body.rotation) * (tip.position - Body.position);
            var desiredTip = position + rotation * tipOffset;
            float rawDepth = Vector3.Dot(desiredTip - contact, direction);
            if (rawDepth < -0.025f) { Detach(); return; }
            Depth = Mathf.Clamp(rawDepth * target.InsertionRatio, 0, target.MaximumDepth);
            position = contact + direction * Depth - rotation * tipOffset;
        }
        protected override void FinishRelease(bool throwObject)
        {
            base.FinishRelease(IsInserted ? false : throwObject);
            if (IsInserted) { Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; Body.isKinematic = true; }
        }
        private void IgnoreTarget(bool ignore)
        {
            if (ignored == null) return;
            foreach (var a in GetComponentsInChildren<Collider>()) foreach (var b in ignored)
                if (a != null && b != null) Physics.IgnoreCollision(a, b, ignore);
        }
        private void Detach() { IgnoreTarget(false); target = null; ignored = null; Depth = 0; sampling = false; }
        public override void ResetObject() { Detach(); base.ResetObject(); }
        protected override void OnDisable() { Detach(); base.OnDisable(); }
    }
}
