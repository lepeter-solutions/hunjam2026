using UnityEngine;
namespace HunJam
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FlightVehicle : MonoBehaviour
    {
        [SerializeField] private FlightYoke yoke;
        [SerializeField] private TextMesh instruments;
        [SerializeField] private FlightThrottle throttle;
        [Header("Aerodynamics")]
        [SerializeField, Min(0)] private float maximumThrustAcceleration = 3f;
        [SerializeField, Min(0)] private float dragCoefficient = 0.0082f;
        [SerializeField, Min(0)] private float liftSlope = 4f;
        [SerializeField, Range(5, 40)] private float stallAngle = 18f;
        [SerializeField, Min(8)] private float cruiseSpeed = 16f;
        [SerializeField, Range(5, 40)] private float pitchRate = 18f;
        [SerializeField, Range(5, 55)] private float rollRate = 30f;
        [SerializeField, Range(0, 2)] private float stabilization = 0.65f;
        [Header("Automatic Reset")]
        [Tooltip("Master switch for ALL automatic resets, including collisions. Manual RESET still works.")]
        [SerializeField] private bool automaticResetEnabled = false;
        [Header("Altitude Reset")]
        [Tooltip("Reset below Reset Altitude. Disabled by default so lowered maps can be flown.")]
        [SerializeField] private bool resetBelowAltitude = false;
        [Tooltip("Minimum world-space Y in metres when Reset Below Altitude is enabled. Negative values are allowed.")]
        [SerializeField] private float resetAltitude = 8f;
        [SerializeField] private bool resetAboveAltitude = true;
        [SerializeField] private float maximumAltitude = 600f;
        [Header("Attitude Reset")]
        [SerializeField] private bool resetOnBank = true;
        [SerializeField, Range(0, 180)] private float maximumBank = 75f;
        [SerializeField] private bool resetOnPitch = true;
        [SerializeField, Range(0, 90)] private float maximumPitch = 50f;
        [Header("Distance Reset")]
        [SerializeField] private bool resetOutsideRadius = true;
        [Tooltip("Horizontal distance from world origin, in metres.")]
        [SerializeField, Min(0)] private float maximumRadius = 1400f;
        [Header("Collision Reset")]
        [SerializeField] private bool resetOnCollision = true;
        [SerializeField, Min(0)] private float collisionResetSpeed = 2f;
        private Rigidbody body;
        private Vector3 spawn;
        private Quaternion heading;
        private float nextDisplay;
        public float Speed => body != null ? body.linearVelocity.magnitude : 0;
        public int ResetCount { get; private set; }
        public void Configure(FlightYoke control, TextMesh display, FlightThrottle power = null)
        { yoke = control; instruments = display; throttle = power; }
        private void Awake()
        {
            body = GetComponent<Rigidbody>(); spawn = body.position; heading = body.rotation;
            body.useGravity = true; body.interpolation = RigidbodyInterpolation.Interpolate;
            body.linearVelocity = transform.forward * cruiseSpeed;
            // Nested kinematic controls belong to the cockpit; they must not collide with the airframe.
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var a in colliders) foreach (var b in colliders)
                if (a != b && a.attachedRigidbody != b.attachedRigidbody) Physics.IgnoreCollision(a, b, true);
        }
        private void FixedUpdate()
        {
            float forwardSpeed = Vector3.Dot(body.linearVelocity, transform.forward);
            // Lift depends on airflow relative to the wing, not just the nose's orientation.
            // Pushing the nose below the flight path reduces (and can reverse) lift.
            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float angleOfAttack = Mathf.Atan2(-localVelocity.y, Mathf.Max(0.1f, forwardSpeed));
            float liftCoefficient = Mathf.Clamp(1 + liftSlope * angleOfAttack, -1.5f, 2.2f);
            float stall = Mathf.InverseLerp(stallAngle, 60f, Mathf.Abs(angleOfAttack) * Mathf.Rad2Deg);
            liftCoefficient *= 1 - 0.85f * stall;
            float speedRatio = Mathf.Max(0, forwardSpeed) / cruiseSpeed;
            Vector3 liftDirection = Vector3.ProjectOnPlane(transform.up, body.linearVelocity.normalized).normalized;
            float power = throttle != null ? throttle.Value : 0.7f;
            body.AddForce(transform.forward * (power * maximumThrustAcceleration), ForceMode.Acceleration);
            body.AddForce(-body.linearVelocity * (Speed * dragCoefficient * (1 + 2 * stall)), ForceMode.Acceleration);
            body.AddForce(liftDirection * (Physics.gravity.magnitude * speedRatio * speedRatio * liftCoefficient), ForceMode.Acceleration);
            body.AddForce(-transform.right * Vector3.Dot(body.linearVelocity, transform.right) * 0.8f, ForceMode.Acceleration);
            float pitch = yoke != null ? yoke.Pitch : 0, roll = yoke != null ? yoke.Roll : 0;
            float bank = Mathf.DeltaAngle(0, transform.eulerAngles.z) * Mathf.Deg2Rad;
            float nose = Mathf.Asin(Mathf.Clamp(transform.forward.y, -1, 1));
            var rates = new Vector3(-pitch * pitchRate * Mathf.Deg2Rad + nose * stabilization * (1 - Mathf.Abs(pitch)),
                -Mathf.Sin(bank) * Physics.gravity.magnitude / Mathf.Max(forwardSpeed, 8),
                -roll * rollRate * Mathf.Deg2Rad - bank * stabilization * (1 - Mathf.Abs(roll)));
            body.AddRelativeTorque((rates - transform.InverseTransformDirection(body.angularVelocity)) * 3f, ForceMode.Acceleration);
            if (automaticResetEnabled && (
                (resetBelowAltitude && body.position.y < resetAltitude)
                || (resetAboveAltitude && body.position.y > maximumAltitude)
                || (resetOnBank && Mathf.Abs(bank) > maximumBank * Mathf.Deg2Rad)
                || (resetOnPitch && Mathf.Abs(nose) > maximumPitch * Mathf.Deg2Rad)
                || (resetOutsideRadius && new Vector2(body.position.x, body.position.z).magnitude > maximumRadius)))
                ResetFlight();
        }
        private void Update()
        {
            if (instruments == null || Time.time < nextDisplay) return;
            nextDisplay = Time.time + 0.1f;
            instruments.text = $"{Speed * 3.6f:0} km/h  ALT {body.position.y:0} m\nV/S {body.linearVelocity.y:+0.0;-0.0;0.0} m/s  THR {(throttle != null ? throttle.Value : 0.7f) * 100:0}%\nPULL: UP   PUSH: DOWN\nRIGHT LEVER: THROTTLE";
        }
        public void ResetFlight()
        {
            foreach (var hand in GetComponentsInChildren<DemoHand>()) hand.HeldItem?.Release(false);
            body.position = spawn; body.rotation = heading;
            body.linearVelocity = heading * Vector3.forward * cruiseSpeed; body.angularVelocity = Vector3.zero;
            yoke?.ResetObject(); throttle?.ResetObject(); ResetCount++;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (!automaticResetEnabled || !resetOnCollision) return;
            if (collision.transform.IsChildOf(transform)) return;
            if (collision.relativeVelocity.magnitude > collisionResetSpeed) ResetFlight();
        }
    }
}
