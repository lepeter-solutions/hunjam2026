using UnityEngine;

namespace HunJam
{
    // One selection path for the quill, the sheet and all runtime paper fragments.
    public sealed class TrianonPaperGrip : MonoBehaviour
    {
        [SerializeField] private Transform leftAnchor;
        [SerializeField] private Transform rightAnchor;
        [SerializeField, Min(0.01f)] private float reach = 0.12f;

        private bool leftUsed;
        private bool rightUsed;
        private bool focused = true;

        private void Update()
        {
            if (!focused) return;
            var connected = OVRInput.GetConnectedControllers();
            Handle(leftAnchor, (connected & OVRInput.Controller.LTouch) != 0 &&
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.65f, ref leftUsed);
            Handle(rightAnchor, (connected & OVRInput.Controller.RTouch) != 0 &&
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.65f, ref rightUsed);
        }

        private void Handle(Transform anchor, bool pressed, ref bool used)
        {
            if (anchor == null) return;
            // Tearing hands ownership to a new fragment. Always ask the shared registry.
            var held = Grabbable.HeldBy(anchor);
            if (!pressed)
            {
                held?.Release(anchor);
                used = false;
                return;
            }
            if (held != null) used = true;
            if (used) return;
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
            if (closest != null && closest.Grab(anchor)) used = true;
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
