using UnityEngine;

namespace HunJam
{
    // One selection path for the quill, the sheet and all runtime paper fragments.
    public sealed class TrianonPaperGrip : MonoBehaviour
    {
        [SerializeField] private Transform leftAnchor;
        [SerializeField] private Transform rightAnchor;
        [SerializeField, Min(0.01f)] private float reach = 0.12f;
        [SerializeField, Range(0.2f, 1f)] private float grabThreshold = 0.65f;
        [SerializeField, Range(0f, 0.19f)] private float releaseThreshold = 0.1f;

        private bool leftUsed;
        private bool rightUsed;
        private bool focused = true;

        private void Start()
        {
            // The solid quill must not drag the dynamic sheet. Tip/surface triggers still detect ink.
            foreach (var quill in FindObjectsByType<QuillWriter>(FindObjectsSortMode.None))
                foreach (var sheet in FindObjectsByType<Tearable>(FindObjectsSortMode.None))
                    foreach (var penCollider in quill.GetComponentsInChildren<Collider>())
                        foreach (var paperCollider in sheet.GetComponents<Collider>())
                            if (!penCollider.isTrigger && !paperCollider.isTrigger)
                                Physics.IgnoreCollision(penCollider, paperCollider);
        }

        private void Update()
        {
            if (!focused) return;
            var connected = OVRInput.GetConnectedControllers();
            Handle(leftAnchor, (connected & OVRInput.Controller.LTouch) != 0 ?
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) : 0f, ref leftUsed);
            Handle(rightAnchor, (connected & OVRInput.Controller.RTouch) != 0 ?
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) : 0f, ref rightUsed);
        }

        private void Handle(Transform anchor, float squeeze, ref bool used)
        {
            if (anchor == null) return;
            // Tearing hands ownership to a new fragment. Always ask the shared registry.
            var held = Grabbable.HeldBy(anchor);
            if (squeeze <= releaseThreshold)
            {
                held?.Release(anchor);
                used = false;
                return;
            }
            if (used || squeeze < grabThreshold) return;
            // Consume the press even when nothing is nearby. Approaching the paper while
            // already squeezing the pen must never acquire another object.
            used = true;
            if (held != null) return;
            Physics.SyncTransforms();
            Grabbable closest = null;
            float distance = reach;
            foreach (var collider in Physics.OverlapSphere(anchor.position, reach, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                var candidate = collider.GetComponentInParent<Grabbable>();
                if (candidate == null || !candidate.CanGrab(anchor) ||
                    (candidate.GetComponent<Tearable>() == null && candidate.GetComponent<QuillWriter>() == null)) continue;
                Vector3 point = collider.ClosestPoint(anchor.position);
                float d = Vector3.Distance(anchor.position, point);
                if (d > distance) continue;
                if (Physics.Linecast(anchor.position, point, out var hit, ~(1 << 2), QueryTriggerInteraction.Ignore)
                    && hit.collider.GetComponentInParent<Grabbable>() != candidate) continue;
                closest = candidate;
                distance = d;
            }
            if (closest != null) closest.Grab(anchor);
        }

        private void OnDisable()
        {
            Grabbable.HeldBy(leftAnchor)?.Release(leftAnchor, false);
            Grabbable.HeldBy(rightAnchor)?.Release(rightAnchor, false);
            leftUsed = rightUsed = false;
        }

        private void OnApplicationFocus(bool focus)
        { focused = focus; if (!focus) OnDisable(); }
    }
}
