using UnityEngine;

namespace HunJam
{
    // Connects the Vérszerződés rig's tracked Touch anchors to the existing paper grip API.
    public sealed class TrianonPaperGrip : MonoBehaviour
    {
        [SerializeField] private Grabbable paper;
        [SerializeField] private Transform leftAnchor;
        [SerializeField] private Transform rightAnchor;
        [SerializeField, Min(0.01f)] private float reach = 0.12f;

        private bool leftUsed;
        private bool rightUsed;

        public void Configure(Grabbable target, Transform left, Transform right)
        { paper = target; leftAnchor = left; rightAnchor = right; }

        private void Update()
        {
            Handle(leftAnchor, OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.65f, ref leftUsed);
            Handle(rightAnchor, OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.65f, ref rightUsed);
        }

        private void Handle(Transform anchor, bool pressed, ref bool used)
        {
            if (anchor == null || paper == null) return;
            if (!pressed)
            {
                if (paper.IsHeldBy(anchor)) paper.Release(anchor);
                used = false;
                return;
            }
            if (used || !paper.isActiveAndEnabled) return;
            Physics.SyncTransforms();
            bool nearby = false;
            foreach (var collider in paper.GetComponents<Collider>())
                if (collider.enabled && !collider.isTrigger &&
                    Vector3.Distance(collider.ClosestPoint(anchor.position), anchor.position) <= reach)
                    nearby = true;
            if (nearby && paper.Grab(anchor)) used = true;
        }

        private void OnDisable()
        {
            if (paper != null)
            {
                if (paper.IsHeldBy(leftAnchor)) paper.Release(leftAnchor, false);
                if (paper.IsHeldBy(rightAnchor)) paper.Release(rightAnchor, false);
            }
            leftUsed = rightUsed = false;
        }
    }
}
