using UnityEngine;
namespace HunJam
{
    [DefaultExecutionOrder(-100)]
    public sealed class FlightThrottle : Grabbable
    {
        [SerializeField] private Transform mounting;
        [SerializeField, Range(10, 70)] private float maximumAngle = 35f;
        [SerializeField, Range(0, 1)] private float initialThrottle = 0.7f;
        private Vector3 home;
        private Quaternion homeRotation;
        private float startHandAngle, startValue;
        public float Value { get; private set; } = 0.7f;
        protected override bool KeepKinematicOnRelease => true;
        protected override Vector3 TrackingPosition(Transform hand) => mounting.InverseTransformPoint(hand.position);
        public void ConfigureMount(Transform mount) => mounting = mount;
        protected override void Awake()
        {
            base.Awake();
            if (mounting == null) mounting = transform.parent;
            if (mounting == null || mounting == transform || mounting.IsChildOf(transform))
            { Debug.LogError("FlightThrottle needs a cockpit mounting.", this); enabled = false; return; }
            home = mounting.InverseTransformPoint(transform.position);
            homeRotation = Quaternion.Inverse(mounting.rotation) * transform.rotation;
            Body.isKinematic = true; Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.None;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Value = initialThrottle;
        }
        private float HandAngle()
        {
            Vector3 offset = Quaternion.Inverse(homeRotation) * (TrackingPosition(Primary) - home);
            return Mathf.Atan2(offset.z, offset.y) * Mathf.Rad2Deg;
        }
        protected override void OnGripChanged()
        {
            startValue = Value;
            if (Primary != null) startHandAngle = HandAngle();
            base.OnGripChanged();
        }
        protected override void FinishRelease(bool throwObject) => base.FinishRelease(false);
        protected override void CalculateTarget(out Vector3 position, out Quaternion rotation)
        {
            Value = Mathf.Clamp01(startValue + Mathf.DeltaAngle(startHandAngle, HandAngle()) / (2 * maximumAngle));
            GetPose(out position, out rotation);
        }
        private void GetPose(out Vector3 position, out Quaternion rotation)
        {
            position = mounting.TransformPoint(home);
            rotation = mounting.rotation * homeRotation * Quaternion.AngleAxis(Mathf.Lerp(-maximumAngle, maximumAngle, Value), Vector3.right);
        }
        protected override void ApplyTargetPose(Vector3 position, Quaternion rotation)
        { Body.position = position; Body.rotation = rotation; }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            GetPose(out var position, out var rotation); ApplyTargetPose(position, rotation);
        }
        private void LateUpdate()
        {
            GetPose(out var position, out var rotation); transform.SetPositionAndRotation(position, rotation);
        }
        public override void ResetObject()
        {
            Release(false); Value = initialThrottle;
            GetPose(out var position, out var rotation); ApplyTargetPose(position, rotation);
        }
    }
}
