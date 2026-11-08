using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;

namespace HunJam
{
    // Paper uses the tearing adapter; the quill keeps the authored Meta hand poses.
    [DefaultExecutionOrder(-50)]
    public sealed class TrianonPaperGrip : MonoBehaviour, IGameObjectFilter
    {
        [SerializeField] private Transform leftAnchor;
        [SerializeField] private Transform rightAnchor;
        [SerializeField, Min(0.01f)] private float reach = 0.12f;
        [SerializeField, Range(0.2f, 1f)] private float grabThreshold = 0.65f;
        [SerializeField, Range(0f, 0.19f)] private float releaseThreshold = 0.1f;

        private bool leftUsed;
        private bool rightUsed;
        private bool focused = true;
        private bool leftPenAllowed, rightPenAllowed;
        private QuillWriter[] quills;
        private HandGrabInteractor[] handInteractors;
        private GrabInteractor[] controllerInteractors;

        private void Start()
        {
            quills = FindObjectsByType<QuillWriter>(FindObjectsSortMode.None);
            handInteractors = GetComponentsInChildren<HandGrabInteractor>(true);
            controllerInteractors = GetComponentsInChildren<GrabInteractor>(true);
            foreach (var quill in quills)
            {
                foreach (var interactable in quill.GetComponentsInChildren<HandGrabInteractable>(true))
                    interactable.InjectOptionalInteractorFilters(new List<IGameObjectFilter> { this });
                foreach (var interactable in quill.GetComponentsInChildren<GrabInteractable>(true))
                    interactable.InjectOptionalInteractorFilters(new List<IGameObjectFilter> { this });
            }
            // The solid quill must not drag the dynamic sheet. Tip/surface triggers still detect ink.
            foreach (var quill in quills)
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
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) : 0f, true, ref leftUsed, ref leftPenAllowed);
            Handle(rightAnchor, (connected & OVRInput.Controller.RTouch) != 0 ?
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) : 0f, false, ref rightUsed, ref rightPenAllowed);
        }

        private void Handle(Transform anchor, float squeeze, bool left, ref bool used, ref bool penAllowed)
        {
            if (anchor == null) return;
            // Tearing hands ownership to a new fragment. Always ask the shared registry.
            var held = Grabbable.HeldBy(anchor);
            if (squeeze <= releaseThreshold)
            {
                held?.Release(anchor);
                used = false;
                penAllowed = false;
                return;
            }
            if (used || squeeze < grabThreshold) return;
            // Consume the press even when nothing is nearby. Approaching the paper while
            // already squeezing the pen must never acquire another object.
            used = true;
            if (held != null) return;
            if (HasNativeSelection(left)) { penAllowed = true; return; }
            Physics.SyncTransforms();
            Grabbable closest = null;
            float distance = reach;
            foreach (var collider in Physics.OverlapSphere(anchor.position, reach, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                var candidate = collider.GetComponentInParent<Grabbable>();
                if (candidate == null || !candidate.CanGrab(anchor) ||
                    candidate.GetComponent<Tearable>() == null) continue;
                Vector3 point = collider.ClosestPoint(anchor.position);
                float d = Vector3.Distance(anchor.position, point);
                if (d > distance) continue;
                if (Physics.Linecast(anchor.position, point, out var hit, ~(1 << 2), QueryTriggerInteraction.Ignore)
                    && hit.collider.GetComponentInParent<Grabbable>() != candidate) continue;
                closest = candidate;
                distance = d;
            }
            // Reserve this squeeze for the native quill if it is nearer than the sheet.
            foreach (var quill in quills)
                foreach (var collider in quill.GetComponentsInChildren<Collider>())
                {
                    if (!collider.enabled || collider.isTrigger) continue;
                    Vector3 point = collider.ClosestPoint(anchor.position);
                    if (Vector3.Distance(anchor.position, point) > distance) continue;
                    if (Physics.Linecast(anchor.position, point, out var hit, ~(1 << 2), QueryTriggerInteraction.Ignore)
                        && hit.collider.GetComponentInParent<QuillWriter>() != quill) continue;
                    penAllowed = true;
                    return;
                }
            penAllowed = false;
            if (closest != null) closest.Grab(anchor);
        }

        private static bool TryGetSide(GameObject source, out bool left)
        {
            var handGrab = source.GetComponent<HandGrabInteractor>();
            if (handGrab != null && handGrab.Hand != null)
            { left = handGrab.Hand.Handedness == Handedness.Left; return true; }
            var controller = source.GetComponent<IController>();
            if (controller != null)
            { left = controller.Handedness == Handedness.Left; return true; }
            left = false;
            return false;
        }

        private bool HasNativeSelection(bool left)
        {
            foreach (var interactor in handInteractors)
                if (interactor.HasSelectedInteractable && TryGetSide(interactor.gameObject, out bool side) && side == left) return true;
            foreach (var interactor in controllerInteractors)
                if (interactor.HasSelectedInteractable && TryGetSide(interactor.gameObject, out bool side) && side == left) return true;
            return false;
        }

        public bool Filter(GameObject source)
        {
            if (!focused || !TryGetSide(source, out bool left)) return false;
            var anchor = left ? leftAnchor : rightAnchor;
            return Grabbable.HeldBy(anchor) == null &&
                (!(left ? leftUsed : rightUsed) || (left ? leftPenAllowed : rightPenAllowed));
        }

        private void OnDisable()
        {
            Grabbable.HeldBy(leftAnchor)?.Release(leftAnchor, false);
            Grabbable.HeldBy(rightAnchor)?.Release(rightAnchor, false);
            leftUsed = rightUsed = false;
            leftPenAllowed = rightPenAllowed = false;
        }

        private void OnApplicationFocus(bool focus)
        { focused = focus; if (!focus) OnDisable(); }
    }
}
