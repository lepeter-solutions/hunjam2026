using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(Grabbable))]
public class PaperWritingDetector : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to your QuillWriter in the scene (auto-found if left empty).")]
    [SerializeField] private QuillWriter quillWriter;
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private Rigidbody paperRigidbody;

    [Header("Snap Back Settings")]
    [Tooltip("Maximum distance (in meters) from the original position to snap back upon release.")]
    [SerializeField, Min(0.01f)] private float snapBackDistance = 0.25f;

    [Header("Tearing Settings")]
    [Tooltip("Extra distance (in meters) the two grab poses must pull apart after a two-handed grab to tear the paper.")]
    [SerializeField, Min(0.01f)] private float tearStretchThreshold = 0.12f;
    [Tooltip("Optional: Pre-made torn piece GameObjects to activate and detach when torn.")]
    [SerializeField] private GameObject[] tornPieces;
    [Tooltip("Hide this paper's visuals and colliders when torn.")]
    [SerializeField] private bool hideOriginalOnTear = true;

    [Header("Completion Requirements")]
    [Tooltip("Required fraction of the paper covered (0.1 = 10%).")]
    [Range(0.01f, 1f)]
    [SerializeField] private float requiredCoverage = 0.10f;
    [Tooltip("Seconds the quill must not touch the paper after reaching required coverage.")]
    [SerializeField] private float requiredIdleSeconds = 3.0f;

    [Header("Surface Grid & Ink Settings")]
    [Tooltip("Grid resolution across the paper surface (25 = 25x25 cells).")]
    [SerializeField] private int gridResolution = 25;
    [Tooltip("Minimum movement in meters required to register new ink.")]
    [SerializeField] private float minMoveDistance = 0.001f;
    [Tooltip("Enable if QuillWriter should draw local LineRenderer strokes instead of world TrailRenderers.")]
    [SerializeField] private bool useLocalInk = false;
    [Tooltip("Maximum nib distance from the paper's surface when using local ink, in meters.")]
    [SerializeField, Min(0.001f)] private float inkContactDistance = 0.005f;
    [Tooltip("Small lift above the paper mesh to prevent ink depth flicker, in meters.")]
    [SerializeField, Min(0.0001f)] private float inkSurfaceOffset = 0.0005f;

    [Header("Events")]
    public UnityEvent onQuillDipped;
    public UnityEvent onWritingCompleted;
    public UnityEvent onPaperTorn;
    public UnityEvent onPaperSnappedBack;

    [Header("Debug (Read-Only)")]
    [SerializeField] private float currentCoverage;
    [SerializeField] private float timeNotTouching;
    [SerializeField] private bool isTouchingPaper;
    [SerializeField] private bool isCompleted;
    [SerializeField] private bool isGrabbed;
    [SerializeField] private bool isAtOriginalPosition = true;
    [SerializeField] private bool isTorn;
    [SerializeField] private float currentTension;

    private bool[,] _visitedCells;
    private int _filledCellCount;
    private int _totalCells;

    private Bounds _localSurfaceBounds;
    private int _axisU;
    private int _axisV;
    private float _inkSurfaceCoordinate;

    private Vector2 _lastUV;
    private bool _hasLastUV;
    private Vector3 _lastTipLocalPos;
    private bool _dipCalled;

    private float _initialTwoHandDistance;
    private bool _isTwoHandGrabbed;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private readonly HashSet<Collider> _touchingQuillColliders = new HashSet<Collider>();
    private Renderer _inkOverlay;
    private Camera _inkCamera;

    public float CurrentCoverage => currentCoverage;
    public float CurrentTension => currentTension;
    public bool IsCompleted => isCompleted;
    public bool IsGrabbed => isGrabbed;
    public bool HasBeenGrabbed => isGrabbed;
    public bool IsAtOriginalPosition => isAtOriginalPosition;
    public bool IsTorn => isTorn;
    public bool UsesLocalInk => useLocalInk;
    public bool CanWrite => !isCompleted && !isGrabbed && !isTorn;

    private void Awake()
    {
        if (quillWriter == null)
            quillWriter = FindFirstObjectByType<QuillWriter>();

        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();

        if (paperRigidbody == null)
            paperRigidbody = GetComponent<Rigidbody>();

        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        InitializeSurfaceGrid();
        if (tearablePaper != null)
        {
            _inkOverlay = transform.Find("Plane")?.GetComponent<MeshRenderer>();
            _inkCamera = GetComponentInChildren<Camera>();
            // Direct local ink replaces the mismatched camera-to-overlay projection.
            if (_inkOverlay != null) _inkOverlay.enabled = false;
            if (_inkCamera != null) _inkCamera.enabled = false;
            var mesh = tearablePaper.GetComponent<MeshFilter>().sharedMesh;
            if (mesh != null)
                _inkSurfaceCoordinate = transform.InverseTransformPoint(
                    tearablePaper.transform.TransformPoint(mesh.bounds.center)).z;
        }
    }

    private void OnEnable()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    private void OnDisable()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= HandlePointerEvent;
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (isTorn) return;

        int grabCount = grabbable.GrabPoints.Count;

        if (evt.Type == PointerEventType.Select)
        {
            if (grabCount >= 1)
            {
                isGrabbed = true;
                isAtOriginalPosition = false;
                _hasLastUV = false;
                isTouchingPaper = false;

                paperRigidbody.isKinematic = false;
                paperRigidbody.useGravity = true;
            }

            if (grabCount >= 2)
            {
                _initialTwoHandDistance = Vector3.Distance(
                    grabbable.GrabPoints[0].position,
                    grabbable.GrabPoints[1].position
                );
                _isTwoHandGrabbed = true;
            }
        }
        else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
        {
            if (grabCount < 2)
            {
                _isTwoHandGrabbed = false;
                currentTension = 0f;
            }

            if (grabCount == 0)
            {
                isGrabbed = false;
                TrySnapToOriginalPosition();
            }
        }
    }

    private void TrySnapToOriginalPosition()
    {
        if (isTorn) return;

        float sqrDist = (transform.position - _originalPosition).sqrMagnitude;
        if (sqrDist <= snapBackDistance * snapBackDistance)
        {
            if (!paperRigidbody.isKinematic)
            {
                paperRigidbody.linearVelocity = Vector3.zero;
                paperRigidbody.angularVelocity = Vector3.zero;
            }

            paperRigidbody.useGravity = false;
            paperRigidbody.isKinematic = true;

            transform.SetPositionAndRotation(_originalPosition, _originalRotation);
            isAtOriginalPosition = true;
            onPaperSnappedBack?.Invoke();
        }
    }

    private void Update()
    {
        if (isTorn) return;

        if (_isTwoHandGrabbed)
        {
            UpdateTearing();
            if (isTorn) return;
        }

        if (CanWrite && !_dipCalled)
        {
            onQuillDipped?.Invoke();
            _dipCalled = true;
        }

        if (!CanWrite || quillWriter == null || quillWriter.quillTip == null)
            return;

        if (UsesLocalInk)
        {
            isTouchingPaper = quillWriter.IsWritingOn(this);
            if (isTouchingPaper)
            {
                timeNotTouching = 0f;
            }
            else
            {
                _hasLastUV = false;
                timeNotTouching += Time.deltaTime;
                CompleteIfReady();
            }
            return;
        }

        _touchingQuillColliders.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

        TrailRenderer activeTrail = quillWriter.quillTip.GetComponentInChildren<TrailRenderer>();
        bool isActivelyWriting = activeTrail != null && activeTrail.emitting;

        isTouchingPaper = isActivelyWriting || _touchingQuillColliders.Count > 0;

        if (isActivelyWriting)
        {
            SampleQuillPosition(quillWriter.quillTip.position);
        }
        else
        {
            _hasLastUV = false;
        }

        if (isTouchingPaper)
        {
            timeNotTouching = 0f;
        }
        else
        {
            timeNotTouching += Time.deltaTime;
            CompleteIfReady();
        }
    }

    private void UpdateTearing()
    {
        IList<Pose> grabPoints = grabbable.GrabPoints;
        if (grabPoints.Count < 2)
        {
            _isTwoHandGrabbed = false;
            currentTension = 0f;
            return;
        }

        float currentDistance = Vector3.Distance(grabPoints[0].position, grabPoints[1].position);
        float stretch = Mathf.Max(0f, currentDistance - _initialTwoHandDistance);
        currentTension = Mathf.Clamp01(stretch / tearStretchThreshold);

        if (stretch >= tearStretchThreshold)
        {
            TearPaper();
        }
    }

    private void TearPaper()
    {
        if (isTorn) return;
        StartCoroutine(TearRoutine());
    }

    private IEnumerator TearRoutine()
    {
        isTorn = true;
        isGrabbed = false;
        _isTwoHandGrabbed = false;
        currentTension = 1f;

        // 1. Record current interactors before releasing the main paper
        var activeHandInteractors = new List<HandGrabInteractor>();
        foreach (var interactable in GetComponentsInChildren<HandGrabInteractable>())
            activeHandInteractors.AddRange(interactable.SelectingInteractors);

        var activeControllerInteractors = new List<GrabInteractor>();
        foreach (var interactable in GetComponentsInChildren<GrabInteractable>())
            activeControllerInteractors.AddRange(interactable.SelectingInteractors);

        // 2. Release the original paper cleanly
        foreach (var hand in activeHandInteractors) hand.ForceRelease();
        foreach (var ctrl in activeControllerInteractors) ctrl.ForceRelease();

        // 3. Activate pieces (keep kinematic for 1 frame so they don't fall before grab)
        if (tornPieces != null)
        {
            foreach (GameObject piece in tornPieces)
            {
                if (piece == null) continue;
                piece.transform.SetParent(null, true);
                if (piece.TryGetComponent(out Rigidbody pieceRb))
                    pieceRb.isKinematic = true;

                piece.SetActive(true);
            }
        }

        // 4. Hide and disable the original paper
        if (hideOriginalOnTear)
        {
            if (TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer.enabled = false;

            foreach (LineRenderer stroke in GetComponentsInChildren<LineRenderer>())
                stroke.enabled = false;

            foreach (Collider col in GetComponents<Collider>())
                col.enabled = false;

            paperRigidbody.isKinematic = true;
            grabbable.enabled = false;
        }

        // 5. Wait 1 frame so Meta XR initializes Start() on the newly enabled pieces
        yield return null;

        // 6. Enable physics on the pieces and force-select them into the hands
        if (tornPieces != null && tornPieces.Length > 0)
        {
            foreach (GameObject piece in tornPieces)
            {
                if (piece == null) continue;
                if (piece.TryGetComponent(out Rigidbody pieceRb))
                {
                    pieceRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    pieceRb.isKinematic = false;
                    pieceRb.useGravity = true;
                }
            }

            var usedPieces = new HashSet<GameObject>();

            foreach (var handInteractor in activeHandInteractors)
            {
                GameObject closest = GetClosestPiece(handInteractor.transform.position, usedPieces);
                if (closest != null)
                {
                    usedPieces.Add(closest);
                    var targetInteractable = closest.GetComponentInChildren<HandGrabInteractable>();
                    if (targetInteractable != null)
                        handInteractor.ForceSelect(targetInteractable, allowManualRelease: true);
                }
            }

            foreach (var grabInteractor in activeControllerInteractors)
            {
                GameObject closest = GetClosestPiece(grabInteractor.transform.position, usedPieces);
                if (closest != null)
                {
                    usedPieces.Add(closest);
                    var targetInteractable = closest.GetComponentInChildren<GrabInteractable>();
                    if (targetInteractable != null)
                        grabInteractor.ForceSelect(targetInteractable);
                }
            }
        }

        onPaperTorn?.Invoke();
    }

    private GameObject GetClosestPiece(Vector3 interactorPos, HashSet<GameObject> excluded)
    {
        GameObject best = null;
        float minDist = float.MaxValue;

        foreach (GameObject piece in tornPieces)
        {
            if (piece == null || excluded.Contains(piece)) continue;
            float dist = (piece.transform.position - interactorPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                best = piece;
            }
        }
        return best;
    }

    private void CompleteIfReady()
    {
        if (CanWrite && currentCoverage >= requiredCoverage && timeNotTouching >= requiredIdleSeconds)
        {
            isCompleted = true;
            onWritingCompleted?.Invoke();
        }
    }

    public bool TryProjectInkPoint(Vector3 tipWorldPos, out Vector3 localPoint)
    {
        localPoint = transform.InverseTransformPoint(tipWorldPos);
        if (!UsesLocalInk || !CanWrite || !TryGetNormalizedUV(localPoint, out _)) return false;

        float normalScale = transform.TransformVector(Vector3.forward).magnitude;
        if (normalScale < 0.0001f ||
            Mathf.Abs(localPoint.z - _inkSurfaceCoordinate) * normalScale > inkContactDistance)
            return false;

        localPoint.z = _inkSurfaceCoordinate + inkSurfaceOffset / normalScale;
        return true;
    }

    public void RecordLocalInk(Vector3 localPoint)
    {
        if (!UsesLocalInk || !CanWrite) return;
        isTouchingPaper = true;
        timeNotTouching = 0f;
        SampleLocalPosition(localPoint);
    }

    public void EndLocalInk()
    {
        _hasLastUV = false;
        isTouchingPaper = false;
    }

    private void InitializeSurfaceGrid()
    {
        gridResolution = Mathf.Max(5, gridResolution);
        _visitedCells = new bool[gridResolution, gridResolution];
        _totalCells = gridResolution * gridResolution;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            _localSurfaceBounds = mf.sharedMesh.bounds;
        }
        else if (TryGetComponent(out BoxCollider box))
        {
            _localSurfaceBounds = new Bounds(box.center, box.size);
        }
        else
        {
            Collider col = GetComponent<Collider>();
            Vector3 localCenter = transform.InverseTransformPoint(col.bounds.center);
            Vector3 localSize = transform.InverseTransformVector(col.bounds.size);
            _localSurfaceBounds = new Bounds(
                localCenter,
                new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z))
            );
        }

        _inkSurfaceCoordinate = _localSurfaceBounds.center.z;

        Vector3 size = _localSurfaceBounds.size;
        if (size.x <= size.y && size.x <= size.z) { _axisU = 1; _axisV = 2; }      // YZ plane
        else if (size.y <= size.x && size.y <= size.z) { _axisU = 0; _axisV = 2; } // XZ plane
        else { _axisU = 0; _axisV = 1; }                                           // XY plane
    }

    private void CompleteIfReady()
    {
        if (CanWrite && currentCoverage >= requiredCoverage && timeNotTouching >= requiredIdleSeconds)
        {
            isCompleted = true;
            onWritingCompleted?.Invoke();
        }
    }

    public bool TryProjectInkPoint(Vector3 tipWorldPos, out Vector3 localPoint)
    {
        localPoint = transform.InverseTransformPoint(tipWorldPos);
        if (!UsesLocalInk || !CanWrite || !TryGetNormalizedUV(localPoint, out _)) return false;
        float normalScale = transform.TransformVector(Vector3.forward).magnitude;
        if (normalScale < 0.0001f ||
            Mathf.Abs(localPoint.z - _inkSurfaceCoordinate) * normalScale > inkContactDistance) return false;
        localPoint.z = _inkSurfaceCoordinate + inkSurfaceOffset / normalScale;
        return true;
    }

    public void RecordLocalInk(Vector3 localPoint)
    {
        if (!UsesLocalInk || !CanWrite) return;
        isTouchingPaper = true;
        timeNotTouching = 0f;
        SampleLocalPosition(localPoint);
    }

    public void EndLocalInk()
    {
        _hasLastUV = false;
        isTouchingPaper = false;
    }

    private void SampleQuillPosition(Vector3 tipWorldPos)
    { SampleLocalPosition(transform.InverseTransformPoint(tipWorldPos)); }

    private void SampleLocalPosition(Vector3 localPos)
    {
        SampleLocalPosition(transform.InverseTransformPoint(tipWorldPos));
    }

    private void SampleLocalPosition(Vector3 localPos)
    {
        if (_hasLastUV && transform.TransformVector(localPos - _lastTipLocalPos).sqrMagnitude < minMoveDistance * minMoveDistance)
            return;

        _lastTipLocalPos = localPos;

        if (!TryGetNormalizedUV(localPos, out Vector2 currentUV))
        {
            _hasLastUV = false;
            return;
        }

        if (_hasLastUV)
        {
            float cellStep = 0.5f / gridResolution;
            float dist = Vector2.Distance(_lastUV, currentUV);
            int steps = Mathf.CeilToInt(dist / cellStep);

            for (int i = 1; i <= steps; i++)
            {
                Vector2 lerpedUV = Vector2.Lerp(_lastUV, currentUV, (float)i / steps);
                MarkCell(lerpedUV);
            }
        }
        else
        {
            MarkCell(currentUV);
        }

        _lastUV = currentUV;
        _hasLastUV = true;
    }

    private bool TryGetNormalizedUV(Vector3 localPos, out Vector2 uv)
    {
        Vector3 min = _localSurfaceBounds.min;
        Vector3 size = _localSurfaceBounds.size;

        float u = size[_axisU] > 0.0001f ? (localPos[_axisU] - min[_axisU]) / size[_axisU] : 0.5f;
        float v = size[_axisV] > 0.0001f ? (localPos[_axisV] - min[_axisV]) / size[_axisV] : 0.5f;

        uv = new Vector2(u, v);
        return u >= 0f && u <= 1f && v >= 0f && v <= 1f;
    }

    private void MarkCell(Vector2 uv)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(uv.x * gridResolution), 0, gridResolution - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(uv.y * gridResolution), 0, gridResolution - 1);

        if (!_visitedCells[x, y])
        {
            _visitedCells[x, y] = true;
            _filledCellCount++;
            currentCoverage = (float)_filledCellCount / _totalCells;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsQuillCollider(other))
            _touchingQuillColliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _touchingQuillColliders.Remove(other);
    }

    private bool IsQuillCollider(Collider col)
    {
        if (quillWriter == null || col == null) return false;
        return col.transform == quillWriter.transform || col.transform.IsChildOf(quillWriter.transform);
    }
}
