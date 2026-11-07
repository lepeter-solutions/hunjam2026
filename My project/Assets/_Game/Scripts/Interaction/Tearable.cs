using UnityEngine;

namespace HunJam
{
    [DefaultExecutionOrder(30)]
    [RequireComponent(typeof(Grabbable), typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class Tearable : MonoBehaviour
    {
        [Header("Pull and resistance")]
        [SerializeField, Min(0.01f)] private float minimumGripSeparation = 0.06f;
        [SerializeField, Min(0.005f)] private float extensionThreshold = 0.025f;
        [SerializeField, Min(1)] private float springStiffness = 40f;
        [SerializeField, Min(0)] private float forceThreshold = 0.8f;
        [SerializeField, Min(0)] private float minimumPullSpeed = 0.005f;
        [SerializeField, Min(0.03f)] private float tearTravel = 0.18f;
        [Header("Paper and fragments")]
        [SerializeField, Min(0.0001f)] private float minimumFragmentArea = 0.0018f;
        [SerializeField, Range(1, 6)] private int maximumTearDepth = 5;
        [SerializeField, Range(0, 0.03f)] private float edgeCurl = 0.012f;
        [Header("Spatial audio (optional clips)")]
        [Tooltip("Loop played only while the crack advances. Volume follows tearing speed.")]
        [SerializeField] private AudioClip tearingLoop;
        [Tooltip("One-shot when the last paper bridge breaks; empty uses the demo rip sound.")]
        [SerializeField] private AudioClip tearSound;
        [SerializeField, Range(0, 1)] private float tearVolume = 0.4f;

        private Grabbable grab;
        private Mesh original;
        private MeshRenderer paperRenderer;
        private AudioSource tearAudio;
        private Transform first, second;
        private Vector3 firstContact, secondContact;
        private float initialDistance, lastExtension, nextPulse, frontMin, frontMax;
        private Vector2 tangent;
        private bool validGrip, firstPositive;
        private int depth;
        private float paperArea;
        private readonly GameObject[] fragments = new GameObject[2];
        private readonly GameObject[] previews = new GameObject[2];
        private readonly Mesh[] cutMeshes = new Mesh[2], previewMeshes = new Mesh[2];
        private readonly Vector3[][] rest = new Vector3[2][], moving = new Vector3[2][];
        private readonly Vector3[] offsets = new Vector3[2];
        public bool IsTorn { get; private set; }
        public float TearProgress { get; private set; }
        public float Tension { get; private set; }
        public float EstimatedForce { get; private set; }
        public Vector2 LastCutPoint { get; private set; }
        public Vector2 LastCutNormal { get; private set; }
        public GameObject[] Fragments => (GameObject[])fragments.Clone();

        private void Awake()
        {
            grab = GetComponent<Grabbable>();
            original = GetComponent<MeshFilter>().sharedMesh;
            paperArea = PaperMesh.Area(original);
            paperRenderer = GetComponent<MeshRenderer>();
            tearAudio = gameObject.AddComponent<AudioSource>();
            tearAudio.playOnAwake = false; tearAudio.loop = true;
            tearAudio.spatialBlend = 1; tearAudio.dopplerLevel = 0;
            tearAudio.minDistance = 0.15f; tearAudio.maxDistance = 4;
            tearAudio.rolloffMode = AudioRolloffMode.Linear;
        }

        private void OnEnable() { grab.GripChanged += CaptureGrips; }
        private void OnDisable()
        {
            if (grab != null) grab.GripChanged -= CaptureGrips;
            if (tearAudio != null) tearAudio.Stop();
        }

        private void CaptureGrips(Grabbable item)
        {
            first = grab.Primary; second = grab.Secondary;
            validGrip = false; Tension = EstimatedForce = lastExtension = 0;
            tearAudio.Stop();
            if (first == null || second == null || IsTorn || original == null || depth >= maximumTearDepth) return;
            firstContact = grab.PrimaryContact; secondContact = grab.SecondaryContact;
            if (Vector2.Distance(firstContact, secondContact) < minimumGripSeparation) return;
            // Once started, a tear keeps its path. Regripping must straddle that existing path.
            if (cutMeshes[0] != null)
            {
                float a = Vector2.Dot((Vector2)firstContact - LastCutPoint, LastCutNormal);
                float b = Vector2.Dot((Vector2)secondContact - LastCutPoint, LastCutNormal);
                if (a * b >= 0) return;
                firstPositive = a > 0;
            }
            validGrip = true;
            initialDistance = Vector3.Distance(first.position, second.position);
        }

        private void Update()
        {
            if (IsTorn) return;
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
            LastCutNormal = ((Vector2)(secondContact - firstContact)).normalized;
            LastCutPoint = (firstContact + secondContact) * 0.5f;
            tangent = new Vector2(-LastCutNormal.y, LastCutNormal.x);
            // Start at the edge nearest the grip midpoint, rather than opening the whole seam at once.
            frontMin = float.PositiveInfinity; frontMax = float.NegativeInfinity;
            for (int i = 0; i < 2; i++) cutMeshes[i] = PaperMesh.Clip(original, LastCutPoint, LastCutNormal, i == 1);
            if (PaperMesh.Area(cutMeshes[0]) < minimumFragmentArea || PaperMesh.Area(cutMeshes[1]) < minimumFragmentArea)
            { ClearPreview(); return false; }
            foreach (var p in cutMeshes[0].vertices)
                if (Mathf.Abs(Vector2.Dot((Vector2)p - LastCutPoint, LastCutNormal)) < 0.0001f)
                {
                    float along = Vector2.Dot((Vector2)p - LastCutPoint, tangent);
                    frontMin = Mathf.Min(frontMin, along); frontMax = Mathf.Max(frontMax, along);
                }
            if (!float.IsFinite(frontMin) || frontMax - frontMin < 0.005f) { ClearPreview(); return false; }
            if (Mathf.Abs(frontMax) < Mathf.Abs(frontMin))
            { tangent = -tangent; float oldMin = frontMin; frontMin = -frontMax; frontMax = -oldMin; }
            firstPositive = Vector2.Dot((Vector2)firstContact - LastCutPoint, LastCutNormal) > 0;
            for (int i = 0; i < 2; i++)
            {
                previews[i] = new GameObject("Opening paper edge");
                previews[i].layer = gameObject.layer;
                previews[i].transform.SetParent(transform, false);
                previewMeshes[i] = PaperMesh.TearPreview(cutMeshes[i]); previewMeshes[i].MarkDynamic();
                rest[i] = previewMeshes[i].vertices; moving[i] = new Vector3[rest[i].Length];
                previews[i].AddComponent<MeshFilter>().sharedMesh = previewMeshes[i];
                previews[i].AddComponent<MeshRenderer>().sharedMaterials = paperRenderer.sharedMaterials;
            }
            paperRenderer.enabled = false;
            return true;
        }

        private void LateUpdate()
        {
            if (IsTorn || cutMeshes[0] == null) return;
            if (validGrip && first != null && second != null)
            {
                offsets[firstPositive ? 1 : 0] = transform.InverseTransformPoint(first.position) - firstContact;
                offsets[firstPositive ? 0 : 1] = transform.InverseTransformPoint(second.position) - secondContact;
            }
            else
            {
                // A released partial tear stays visibly notched but does not continue tearing.
                for (int i = 0; i < 2; i++)
                    offsets[i] = Vector3.Lerp(offsets[i], (Vector3)LastCutNormal * (i == 0 ? -0.002f : 0.002f), 1 - Mathf.Exp(-12 * Time.deltaTime));
            }
            float length = frontMax - frontMin;
            float feather = Mathf.Min(0.025f, length * 0.2f);
            float front = Mathf.Lerp(frontMin, frontMax + feather, TearProgress);
            for (int side = 0; side < 2; side++)
            {
                for (int i = 0; i < rest[side].Length; i++)
                {
                    Vector3 p = rest[side][i];
                    float along = Mathf.Clamp(Vector2.Dot((Vector2)p - LastCutPoint, tangent), frontMin, frontMax);
                    float opened = Mathf.SmoothStep(0, 1, Mathf.Clamp01((front - along) / feather));
                    float seam = Mathf.Exp(-Mathf.Abs(Vector2.Dot((Vector2)p - LastCutPoint, LastCutNormal)) / 0.035f);
                    float curl = Mathf.Sin(opened * Mathf.PI) * seam * edgeCurl * (side == 0 ? -1 : 1);
                    moving[side][i] = p + offsets[side] * opened + Vector3.forward * curl;
                }
                previewMeshes[side].vertices = moving[side];
                previewMeshes[side].RecalculateNormals(); previewMeshes[side].RecalculateBounds();
            }
            if (TearProgress >= 1) CompleteTear();
        }

        private void UpdateSound(float rate)
        {
            if (tearingLoop == null)
            { if (tearAudio.isPlaying) tearAudio.Stop(); return; }
            tearAudio.clip = tearingLoop;
            float targetVolume = rate > 0 ? tearVolume * Mathf.Clamp(0.2f + rate * 0.6f, 0, 1) : 0;
            tearAudio.volume = Mathf.MoveTowards(tearAudio.isPlaying ? tearAudio.volume : 0, targetVolume, Time.deltaTime * 8);
            tearAudio.pitch = Mathf.Lerp(0.85f, 1.2f, Mathf.Clamp01(rate * 0.4f));
            if (tearAudio.volume <= 0) { tearAudio.Stop(); return; }
            if (!tearAudio.isPlaying && rate > 0) tearAudio.Play();
        }

        private void CompleteTear()
        {
            // The final preview is planar and translated. Preserve that exact pose during handoff.
            for (int i = 0; i < 2; i++) fragments[i] = MakeFragment(cutMeshes[i], offsets[i]);
            var a = first; var b = second;
            InteractionFeedback.Pulse(grab, 0.3f, 0.06f);
            tearAudio.Stop();
            if (tearSound != null) AudioSource.PlayClipAtPoint(tearSound, transform.TransformPoint(LastCutPoint), tearVolume);
            else InteractionFeedback.Play(transform.TransformPoint(LastCutPoint), true, tearVolume);
            IsTorn = true; validGrip = false;
            grab.Release(false);
            fragments[firstPositive ? 1 : 0].GetComponent<Grabbable>().Grab(a);
            fragments[firstPositive ? 0 : 1].GetComponent<Grabbable>().Grab(b);
            foreach (var collider in GetComponents<Collider>()) collider.enabled = false;
            grab.enabled = false; GetComponent<Rigidbody>().isKinematic = true;
            ClearPreview();
        }

        private GameObject MakeFragment(Mesh source, Vector3 offset)
        {
            var mesh = Instantiate(source);
            // Recenter every piece so repeated cuts retain local precision and usable contact bounds.
            Vector3 centre = mesh.bounds.center;
            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) vertices[i] -= centre;
            mesh.vertices = vertices; mesh.RecalculateBounds();
            var obj = new GameObject("Tearable paper fragment") { layer = gameObject.layer };
            obj.transform.SetPositionAndRotation(transform.TransformPoint(centre + offset), transform.rotation);
            obj.transform.localScale = transform.lossyScale;
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterials = paperRenderer.sharedMaterials;
            var hull = PaperMesh.ColliderHull(mesh);
            var collider = obj.AddComponent<MeshCollider>(); collider.sharedMesh = hull; collider.convex = true;
            var body = obj.AddComponent<Rigidbody>();
            body.mass = Mathf.Max(0.001f, GetComponent<Rigidbody>().mass * PaperMesh.Area(mesh) / paperArea);
            body.linearDamping = 0.7f; body.angularDamping = 1f; body.interpolation = RigidbodyInterpolation.Interpolate;
            obj.AddComponent<Grabbable>().ConfigureGrip(Grabbable.GripMode.TwoHands, closure: 0.4f);
            var lifetime = obj.AddComponent<PaperFragment>(); lifetime.Surface = mesh; lifetime.Hull = hull;
            var child = obj.AddComponent<Tearable>();
            child.depth = depth + 1;
            child.minimumGripSeparation = minimumGripSeparation; child.extensionThreshold = extensionThreshold;
            child.springStiffness = springStiffness; child.forceThreshold = forceThreshold; child.minimumPullSpeed = minimumPullSpeed;
            child.tearTravel = tearTravel; child.minimumFragmentArea = minimumFragmentArea; child.maximumTearDepth = maximumTearDepth;
            child.edgeCurl = edgeCurl; child.tearingLoop = tearingLoop; child.tearSound = tearSound; child.tearVolume = tearVolume;
            return obj;
        }

        private void ClearPreview()
        {
            for (int i = 0; i < 2; i++)
            {
                if (previews[i] != null) { previews[i].SetActive(false); Destroy(previews[i]); }
                if (previewMeshes[i] != null) Destroy(previewMeshes[i]);
                if (cutMeshes[i] != null) Destroy(cutMeshes[i]);
                previews[i] = null; previewMeshes[i] = cutMeshes[i] = null;
                rest[i] = moving[i] = null; offsets[i] = Vector3.zero;
            }
        }

        private void ClearFragments()
        {
            for (int i = 0; i < fragments.Length; i++)
            {
                if (fragments[i] == null) continue;
                fragments[i].GetComponent<Tearable>().ClearFragments();
                fragments[i].SetActive(false); Destroy(fragments[i]); fragments[i] = null;
            }
        }

        public void ResetPaper()
        {
            ClearFragments(); ClearPreview(); tearAudio.Stop();
            IsTorn = false; validGrip = false; TearProgress = Tension = EstimatedForce = 0;
            grab.enabled = true; grab.ResetObject();
            foreach (var collider in GetComponents<Collider>()) collider.enabled = true;
            paperRenderer.enabled = true;
        }

        private void OnDestroy() { ClearFragments(); ClearPreview(); }
    }
}
