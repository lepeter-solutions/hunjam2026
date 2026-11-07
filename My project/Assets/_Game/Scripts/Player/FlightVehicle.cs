using UnityEngine;
namespace HunJam
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FlightVehicle : MonoBehaviour
    {
        [SerializeField] private FlightYoke yoke;
        [SerializeField] private TextMesh instruments;
        [SerializeField, Min(8)] private float cruiseSpeed = 16f;
        [SerializeField, Range(5, 40)] private float pitchRate = 18f;
        [SerializeField, Range(5, 55)] private float rollRate = 30f;
        [SerializeField, Range(0, 2)] private float stabilization = 0.65f;
        private Rigidbody body;
        private Vector3 spawn;
        private Quaternion heading;
        private float nextDisplay;
        public float Speed => body != null ? body.linearVelocity.magnitude : 0;
        public int ResetCount { get; private set; }
        public void Configure(FlightYoke control, TextMesh display) { yoke = control; instruments = display; }
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
            float speedRatio = Mathf.Clamp(forwardSpeed / cruiseSpeed, 0, 1.5f);
            body.AddForce(transform.forward * ((cruiseSpeed - forwardSpeed) * 1.6f + 0.5f), ForceMode.Acceleration);
            body.AddForce(-body.linearVelocity * 0.03f, ForceMode.Acceleration);
            body.AddForce(transform.up * (Physics.gravity.magnitude * speedRatio * speedRatio), ForceMode.Acceleration);
            body.AddForce(-transform.right * Vector3.Dot(body.linearVelocity, transform.right) * 0.8f, ForceMode.Acceleration);
            float pitch = yoke != null ? yoke.Pitch : 0, roll = yoke != null ? yoke.Roll : 0;
            float bank = Mathf.DeltaAngle(0, transform.eulerAngles.z) * Mathf.Deg2Rad;
            float nose = Mathf.Asin(Mathf.Clamp(transform.forward.y, -1, 1));
            var rates = new Vector3(-pitch * pitchRate * Mathf.Deg2Rad + nose * stabilization * (1 - Mathf.Abs(pitch)),
                -Mathf.Sin(bank) * Physics.gravity.magnitude / Mathf.Max(forwardSpeed, 8),
                -roll * rollRate * Mathf.Deg2Rad - bank * stabilization * (1 - Mathf.Abs(roll)));
            body.AddRelativeTorque((rates - transform.InverseTransformDirection(body.angularVelocity)) * 3f, ForceMode.Acceleration);
            if (body.position.y < 8 || body.position.y > 600 || Mathf.Abs(bank) > 75 * Mathf.Deg2Rad || Mathf.Abs(nose) > 50 * Mathf.Deg2Rad
                || new Vector2(body.position.x, body.position.z).magnitude > 1400) ResetFlight();
        }
        private void Update()
        {
            if (instruments == null || Time.time < nextDisplay) return;
            nextDisplay = Time.time + 0.1f;
            instruments.text = $"{Speed * 3.6f:0} km/h     ALT {body.position.y:0} m\nBANK {Mathf.DeltaAngle(0, transform.eulerAngles.z):0}°  PITCH {-Mathf.DeltaAngle(0, transform.eulerAngles.x):0}°\nPULL: UP   PUSH: DOWN\nTurn yoke to bank | Release to level";
        }
        public void ResetFlight()
        {
            foreach (var hand in GetComponentsInChildren<DemoHand>()) hand.HeldItem?.Release(false);
            body.position = spawn; body.rotation = heading;
            body.linearVelocity = heading * Vector3.forward * cruiseSpeed; body.angularVelocity = Vector3.zero;
            yoke?.ResetObject(); ResetCount++;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.IsChildOf(transform)) return;
            if (collision.relativeVelocity.magnitude > 2) ResetFlight();
        }
    }
}
