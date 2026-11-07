using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Grab;
using MetaGrabbable = Oculus.Interaction.Grabbable;

namespace HunJam
{
    // Gameplay state only. Selection, following the hand and throwing belong to Meta ISDK.
    [RequireComponent(typeof(Rigidbody), typeof(MetaGrabbable), typeof(GrabInteractable))]
    [RequireComponent(typeof(HandGrabInteractable))]
    public class GameItem : MonoBehaviour
    {
        private MetaGrabbable grabbable;
        private Rigidbody body;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private float armedAt;
        public bool IsHeld => grabbable != null && grabbable.SelectingPointsCount > 0;
        public bool CanImpact => IsHeld && Time.time >= armedAt;
        private T Ensure<T>() where T : Component
        {
            var component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
        public void ConfigureInteraction()
        {
            body = GetComponent<Rigidbody>();
            // ISDK transformers write the Transform directly; Rigidbody interpolation would overwrite it.
            body.interpolation = RigidbodyInterpolation.None;
            grabbable = Ensure<MetaGrabbable>();
            grabbable.InjectOptionalRigidbody(body);
            grabbable.MaxGrabPoints = 1;
            grabbable.TransferOnSecondSelection = true;
            var grab = Ensure<GrabInteractable>();
            grab.InjectRigidbody(body);
            grab.InjectOptionalPointableElement(grabbable);
            grab.MaxSelectingInteractors = 1;
            var handGrab = Ensure<HandGrabInteractable>();
            handGrab.InjectRigidbody(body);
            handGrab.InjectOptionalPointableElement(grabbable);
            handGrab.InjectSupportedGrabTypes(GrabTypeFlags.Palm);
            handGrab.MaxSelectingInteractors = 1;
        }
        protected virtual void Awake()
        {
            ConfigureInteraction();
            startPosition = transform.position;
            startRotation = transform.rotation;
        }
        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointer;
        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointer;
        private void OnPointer(PointerEvent evt)
        { if (evt.Type == PointerEventType.Select) armedAt = Time.time + 0.15f; }
        public virtual void Use() { }
        public virtual void ResetObject()
        {
            // Disabling SDK components sends cancellation and releases their kinematic lock.
            var grab = Ensure<GrabInteractable>();
            var handGrab = GetComponent<HandGrabInteractable>();
            handGrab.enabled = false;
            grab.enabled = false;
            grabbable.enabled = false;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            // Collected items are inactive, so restore their Transform as well as the physics pose.
            transform.SetPositionAndRotation(startPosition, startRotation);
            body.position = startPosition;
            body.rotation = startRotation;
            gameObject.SetActive(true);
            grabbable.enabled = true;
            grab.enabled = true;
            handGrab.enabled = true;
        }
        private void FixedUpdate()
        { if (!IsHeld && body.position.y < -3f) ResetObject(); }
    }
}
