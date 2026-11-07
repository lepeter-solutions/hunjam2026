using UnityEngine;
namespace HunJam
{
    [DefaultExecutionOrder(-100)] // Align the mounted collider before DemoHand's LateUpdate queries.
    public sealed class FlightYoke : Grabbable
    {
        [SerializeField] private Transform mounting;
        [SerializeField, Range(10, 65)] private float maximumAngle = 40f;
        [SerializeField, Range(0.03f, 0.2f)] private float maximumTravel = 0.09f;
        [Tooltip("Forward/backward tilt at full push/pull, in degrees. Zero disables the tilt.")]
        [SerializeField, Range(0, 25)] private float maximumPushPullTilt = 10f;
        [SerializeField, Range(0, 0.25f)] private float deadZone = 0.06f;
        [SerializeField, Min(0.01f)] private float responseTime = 0.045f;
        [SerializeField, Min(0.01f)] private float returnTime = 0.25f;
        private Vector3 home, firstStart, secondStart;
        private Quaternion homeRotation;
        private float angle, travel, startAngle, startTravel, angleVelocity, travelVelocity;
        protected override bool KeepKinematicOnRelease => true;
        protected override Vector3 TrackingPosition(Transform hand) => mounting.InverseTransformPoint(hand.position);
        public float Roll => ApplyDeadZone(-angle / maximumAngle, deadZone);
        public float Pitch => ApplyDeadZone(-travel / maximumTravel, deadZone);
        public float Angle => angle;
        public float Travel => travel;
        public static float ApplyDeadZone(float value, float zone)
        { return Mathf.Abs(value) <= zone ? 0 : Mathf.Sign(value) * Mathf.Clamp01((Mathf.Abs(value) - zone) / (1 - zone)); }
        public void ConfigureMount(Transform mount) => mounting = mount;
        protected override void Awake()
        {
            base.Awake();
            if (mounting == null) mounting = transform.parent;
            if (mounting == null || mounting == transform || mounting.IsChildOf(transform))
            {
                Debug.LogError("FlightYoke needs a mounting fixed to the cockpit.", this);
                enabled = false;
                return;
            }
            home = mounting.InverseTransformPoint(transform.position);
            homeRotation = Quaternion.Inverse(mounting.rotation) * transform.rotation;
            Body.isKinematic = true; Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.None;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        protected override void OnGripChanged()
        {
            startAngle = angle; startTravel = travel;
            if (Primary != null) firstStart = mounting.InverseTransformPoint(Primary.position) - home;
            if (Secondary != null) secondStart = mounting.InverseTransformPoint(Secondary.position) - home;
            angleVelocity = travelVelocity = 0;
            base.OnGripChanged();
        }
        protected override void FinishRelease(bool throwObject)
        {
            base.FinishRelease(false);
            Body.useGravity = false;
        }
        private void Sample(Transform hand, Vector3 start, out float turn, out float push)
        {
            var current = mounting.InverseTransformPoint(hand.position) - home;
            turn = new Vector2(start.x, start.y).sqrMagnitude < 0.0025f || new Vector2(current.x, current.y).sqrMagnitude < 0.0025f
                ? 0 : Vector2.SignedAngle(new Vector2(start.x, start.y), new Vector2(current.x, current.y));
            push = current.z - start.z;
        }
        protected override void CalculateTarget(out Vector3 position, out Quaternion rotation)
        {
            Sample(Primary, firstStart, out float turn, out float push);
            if (Secondary != null)
            {
                Sample(Secondary, secondStart, out float otherTurn, out float otherPush);
                turn = (turn + otherTurn) * 0.5f; push = (push + otherPush) * 0.5f;
            }
            angle = Mathf.SmoothDamp(angle, Mathf.Clamp(startAngle + turn, -maximumAngle, maximumAngle), ref angleVelocity, responseTime, Mathf.Infinity, Time.fixedDeltaTime);
            travel = Mathf.SmoothDamp(travel, Mathf.Clamp(startTravel + push, -maximumTravel, maximumTravel), ref travelVelocity, responseTime, Mathf.Infinity, Time.fixedDeltaTime);
            Pose(out position, out rotation);
        }
        private void Pose(out Vector3 position, out Quaternion rotation)
        {
            // Clamp the actual state as well as its target, including after handoffs and tuning.
            angle = Mathf.Clamp(angle, -maximumAngle, maximumAngle);
            travel = Mathf.Clamp(travel, -maximumTravel, maximumTravel);
            position = mounting.TransformPoint(home + Vector3.forward * travel);
            float tilt = travel / Mathf.Max(0.001f, maximumTravel) * maximumPushPullTilt;
            rotation = mounting.rotation * homeRotation
                * Quaternion.AngleAxis(tilt, Vector3.right)
                * Quaternion.AngleAxis(angle, Vector3.forward);
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate(); // Also cleans destroyed/disabled anchors.
            if (!IsHeld)
            {
                angle = Mathf.SmoothDamp(angle, 0, ref angleVelocity, returnTime, Mathf.Infinity, Time.fixedDeltaTime);
                travel = Mathf.SmoothDamp(travel, 0, ref travelVelocity, returnTime, Mathf.Infinity, Time.fixedDeltaTime);
            }
            Pose(out var position, out var rotation); ApplyTargetPose(position, rotation);
        }
        protected override void ApplyTargetPose(Vector3 position, Quaternion rotation)
        {
            // A mounted control has no free world-space trajectory between physics steps.
            Body.position = position; Body.rotation = rotation;
        }
        private void LateUpdate()
        {
            // Follow the aircraft's interpolated render pose too; only roll and travel are free.
            Pose(out var position, out var rotation);
            transform.SetPositionAndRotation(position, rotation);
        }
        public override void ResetObject()
        {
            Release(false); angle = travel = angleVelocity = travelVelocity = 0;
            Pose(out var position, out var rotation); Body.position = position; Body.rotation = rotation;
        }
    }
}
