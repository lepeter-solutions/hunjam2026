using UnityEngine;

namespace HunJam
{
    [DefaultExecutionOrder(30)]
    [RequireComponent(typeof(Grabbable), typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class Tearable : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float minimumGripSeparation = 0.12f;
        [SerializeField, Min(0.01f)] private float extensionThreshold = 0.09f;
        [SerializeField, Min(1)] private float springStiffness = 40f;
        [SerializeField, Min(0)] private float forceThreshold = 3f;
        [SerializeField, Min(0)] private float minimumPullSpeed = 0.04f;
        [Tooltip("Local XY bounds; points must lie on opposite sides of the centre margin.")]
        [SerializeField] private Vector2 gripHalfExtents = new Vector2(0.22f, 0.16f);
        [SerializeField, Min(0)] private float centreMargin = 0.025f;
        [SerializeField] private AudioClip tearSound;
        private Grabbable grab;
        private MeshFilter surface;
        private Mesh original, deforming;
        private Vector3[] restVertices, deformedVertices;
        private Transform first, second;
        private Vector3 firstContact, secondContact;
        private float initialDistance, lastExtension, nextPulse;
        private bool validGrip;
        private readonly GameObject[] fragments = new GameObject[2];
        public bool IsTorn { get; private set; }
        public float Tension { get; private set; }
        public float EstimatedForce { get; private set; }
        public Vector2 LastCutPoint { get; private set; }
        public Vector2 LastCutNormal { get; private set; }
        public GameObject[] Fragments => (GameObject[])fragments.Clone();
        private void Awake()
        {
            grab = GetComponent<Grabbable>(); surface = GetComponent<MeshFilter>(); original = surface.sharedMesh;
            if (original != null)
            {
                deforming = Instantiate(original); restVertices = original.vertices;
                deformedVertices = new Vector3[restVertices.Length]; surface.sharedMesh = deforming;
            }
        }
        private void OnEnable() { if (grab != null) grab.GripChanged += CaptureGrips; }
        private void OnDisable() { if (grab != null) grab.GripChanged -= CaptureGrips; }
        private void CaptureGrips(Grabbable item)
        {
            first = grab.Primary; second = grab.Secondary;
            validGrip = false; Tension = EstimatedForce = lastExtension = 0;
            tearAudio.Stop();
            //if (first == null || second == null || IsTorn || original == null || depth >= maximumTearDepth
                //|| (writingDetector != null && writingDetector.LocksTearing)) return;
            firstContact = grab.PrimaryContact; secondContact = grab.SecondaryContact;
            bool InRegion(Vector3 p) => Mathf.Abs(p.x) <= gripHalfExtents.x && Mathf.Abs(p.y) <= gripHalfExtents.y && Mathf.Abs(p.z) <= 0.12f;
            validGrip = InRegion(firstContact) && InRegion(secondContact)
                && firstContact.x * secondContact.x < -centreMargin * centreMargin
                && Vector2.Distance(firstContact, secondContact) >= minimumGripSeparation;
            initialDistance = Vector3.Distance(first.position, second.position);
        }
        private void FixedUpdate()
        {
            //if (IsTorn || (writingDetector != null && writingDetector.LocksTearing)) return;
            float advance = 0;
            if (validGrip && first != null && second != null)
            {
                float extension = Mathf.Max(0, Vector3.Distance(first.position, second.position) - initialDistance);
                float delta = Mathf.Max(0, extension - lastExtension);
                float speed = delta / Mathf.Max(Time.deltaTime, 0.0001f);
                EstimatedForce = extension * springStiffness;
                Tension = Mathf.Clamp01(extension / Mathf.Max(extensionThreshold, 0.005f));
                if (cutMeshes[0] == null && extension >= extensionThreshold && EstimatedForce >= forceThreshold && speed >= minimumPullSpeed)
                {
                    if (!BeginTear()) validGrip = false;
                    // Starting the crack consumes the preload; a single tracking jump cannot split a sheet.
                    delta = 0;
                }
                if (cutMeshes[0] != null && extension >= extensionThreshold)
                {
                    float before = TearProgress;
                    float travel = Mathf.Max(0.04f, tearTravel * Mathf.Sqrt(paperArea / 0.126f));
                    TearProgress = Mathf.Min(1, TearProgress + Mathf.Min(delta / travel, Time.deltaTime * 2.5f));
                    advance = TearProgress - before;
                }
                lastExtension = extension;
            }
            UpdateSound(advance / Mathf.Max(Time.deltaTime, 0.0001f));
            if (advance > 0 && Time.time >= nextPulse)
            {
                InteractionFeedback.Pulse(grab, 0.08f + Tension * 0.12f, 0.025f);
                nextPulse = Time.time + 0.07f;
            }
        }

        private bool BeginTear()
        {
            //if (writingDetector != null && writingDetector.LocksTearing) return false;
            LastCutNormal = ((Vector2)(secondContact - firstContact)).normalized;
            LastCutPoint = (firstContact + secondContact) * 0.5f;
            var negative = PaperMesh.Clip(original, LastCutPoint, LastCutNormal, false);
            var positive = PaperMesh.Clip(original, LastCutPoint, LastCutNormal, true);
            if (negative.vertexCount < 3 || positive.vertexCount < 3)
            { Destroy(negative); Destroy(positive); return; }
            // Both valid fragments exist before ownership changes; no free physics frame occurs.
            fragments[0] = MakeFragment(negative); fragments[1] = MakeFragment(positive);
            var a = first; var b = second;
            bool aPositive = Vector2.Dot((Vector2)firstContact - LastCutPoint, LastCutNormal) >= 0;
            InteractionFeedback.Pulse(grab, 0.35f, 0.08f);
            if (tearSound != null) AudioSource.PlayClipAtPoint(tearSound, transform.position, 0.4f);
            else InteractionFeedback.Play(transform.position, true);
            IsTorn = true; validGrip = false;
            grab.Release(false);
            fragments[aPositive ? 1 : 0].GetComponent<Grabbable>().Grab(a);
            fragments[aPositive ? 0 : 1].GetComponent<Grabbable>().Grab(b);
            foreach (var collider in GetComponents<Collider>()) collider.enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
            grab.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
        private GameObject MakeFragment(Mesh mesh)
        {
            var obj = new GameObject("Contract fragment");
            obj.transform.SetPositionAndRotation(transform.position, transform.rotation); obj.transform.localScale = transform.lossyScale;
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterials = GetComponent<MeshRenderer>().sharedMaterials;
            var hull = PaperMesh.ColliderHull(mesh);
            var collider = obj.AddComponent<MeshCollider>(); collider.sharedMesh = hull; collider.convex = true;
            var body = obj.AddComponent<Rigidbody>(); body.mass = 0.015f; body.linearDamping = 0.7f; body.angularDamping = 1f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            obj.AddComponent<Grabbable>().ConfigureGrip(Grabbable.GripMode.TransferOnly, closure: 0.4f);
            var lifetime = obj.AddComponent<PaperFragment>(); lifetime.Surface = mesh; lifetime.Hull = hull;
            return obj;
        }
        private void RestoreSurface()
        { if (deforming != null) { deforming.vertices = restVertices; deforming.RecalculateNormals(); deforming.RecalculateBounds(); } }
        public void ResetPaper()
        {
            foreach (var fragment in fragments) if (fragment != null) { fragment.SetActive(false); Destroy(fragment); }
            IsTorn = false; validGrip = false; Tension = EstimatedForce = 0;
            RestoreSurface(); grab.enabled = true; grab.ResetObject();
            foreach (var collider in GetComponents<Collider>()) collider.enabled = true;
            GetComponent<MeshRenderer>().enabled = true;
        }
        private void OnDestroy()
        {
            foreach (var fragment in fragments) if (fragment != null) Destroy(fragment);
            if (deforming != null) Destroy(deforming);
        }
    }
}
