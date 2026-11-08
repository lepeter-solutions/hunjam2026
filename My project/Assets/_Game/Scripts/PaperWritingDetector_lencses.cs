using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PaperWritingDetector_lencses : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to your QuillWriter in the scene (auto-found if left empty).")]
    [SerializeField] private QuillWriter_lencses quillWriter;
    [SerializeField] private HunJam.Tearable tearablePaper;

    [Header("Completion Requirements")]
    [Tooltip("Required fraction of the paper covered (0.1 = 10%).")]
    [Range(0.01f, 1f)]
    [SerializeField] private float requiredCoverage = 0.10f;

    [Tooltip("Seconds the quill must not touch the paper after reaching required coverage.")]
    [SerializeField] private float requiredIdleSeconds = 3.0f;

    [Header("Surface Grid Settings")]
    [Tooltip("Grid resolution across the paper surface (25 = 25x25 cells).")]
    [SerializeField] private int gridResolution = 25;

    [Tooltip("Minimum movement in meters required to register new ink.")]
    [SerializeField] private float minMoveDistance = 0.001f;
    [Tooltip("Maximum nib distance from a movable paper's surface, in meters.")]
    [SerializeField, Min(0.001f)] private float inkContactDistance = 0.005f;
    [Tooltip("Small lift above the paper mesh to prevent ink depth flicker, in meters.")]
    [SerializeField, Min(0.0001f)] private float inkSurfaceOffset = 0.0005f;

    [Header("Events")]
    public UnityEvent onWritingCompleted;

    [Header("Debug (Read-Only)")]
    [SerializeField] private float currentCoverage;
    [SerializeField] private float timeNotTouching;
    [SerializeField] private bool isTouchingPaper;
    [SerializeField] private bool isCompleted;

    private bool[,] _visitedCells;
    private int _filledCellCount;
    private int _totalCells;

    private Bounds _localSurfaceBounds;
    private int _axisU;
    private int _axisV;

    private Vector2 _lastUV;
    private bool _hasLastUV;
    private Vector3 _lastTipLocalPos;
    private float _inkSurfaceCoordinate;

    private readonly HashSet<Collider> _touchingQuillColliders = new HashSet<Collider>();
    private Renderer _inkOverlay;
    private Camera _inkCamera;

    public float CurrentCoverage => currentCoverage;
    public bool IsCompleted => isCompleted;
    public bool LocksTearing => isCompleted || currentCoverage > 0f;
    public bool UsesLocalInk => tearablePaper != null;
    public bool CanWrite => !isCompleted && (tearablePaper == null || !tearablePaper.HasStartedTearing);
    public void SetTearablePaper(HunJam.Tearable paper) => tearablePaper = paper;

    private void Awake()
    {
        if (quillWriter == null)
        {
            quillWriter = FindFirstObjectByType<QuillWriter_lencses>();
        }

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

    private void InitializeSurfaceGrid()
    {
        gridResolution = Mathf.Max(5, gridResolution);
        _visitedCells = new bool[gridResolution, gridResolution];
        _totalCells = gridResolution * gridResolution;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (tearablePaper != null && TryGetComponent(out BoxCollider writingSurface))
        {
            // Trianon keeps the old visual mesh disabled; its XZ bounds do not describe the new XY sheet.
            _localSurfaceBounds = new Bounds(writingSurface.center, writingSurface.size);
        }
        else if (mf != null && mf.sharedMesh != null)
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
            _localSurfaceBounds = new Bounds(localCenter, new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z)));
        }

        // Automatically pick the 2 largest local axes as the flat writing plane (supports Planes, Quads, and custom meshes)
        Vector3 size = _localSurfaceBounds.size;
        if (size.x <= size.y && size.x <= size.z) { _axisU = 1; _axisV = 2; }      // YZ plane
        else if (size.y <= size.x && size.y <= size.z) { _axisU = 0; _axisV = 2; } // XZ plane
        else { _axisU = 0; _axisV = 1; }                                           // XY plane
    }

    private void Update()
    {
        if (tearablePaper != null && tearablePaper.HasStartedTearing)
        {
            if (_inkOverlay != null) _inkOverlay.enabled = false;
            if (_inkCamera != null) _inkCamera.enabled = false;
        }
        if (!CanWrite || quillWriter == null || quillWriter.quillTip == null)
            return;

        if (UsesLocalInk)
        {
            isTouchingPaper = quillWriter.IsWritingOn(this);
            if (isTouchingPaper) timeNotTouching = 0f;
            else
            {
                _hasLastUV = false;
                timeNotTouching += Time.deltaTime;
                CompleteIfReady();
            }
            return;
        }

        _touchingQuillColliders.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

        // QuillWriter parents the active TrailRenderer to quillTip while writing and unparents it on exit
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
            // Interpolate between last frame and current frame so fast VR strokes don't skip grid cells
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
        {
            _touchingQuillColliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_touchingQuillColliders.Contains(other))
        {
            _touchingQuillColliders.Remove(other);
        }
    }

    private bool IsQuillCollider(Collider col)
    {
        if (quillWriter == null || col == null) return false;
        return col.transform == quillWriter.transform || col.transform.IsChildOf(quillWriter.transform);
    }
}