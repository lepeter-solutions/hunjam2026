using UnityEngine;

[DefaultExecutionOrder(100)]
public class QuillWriter : MonoBehaviour
{
    [Tooltip("The 3D model of the quill tip so we can change its color.")]
    public Renderer tipRenderer;
    [Tooltip("Create an empty GameObject exactly at the tip of the quill and drag it here.")]
    public Transform quillTip;
    [Tooltip("Drag your new InkStroke prefab here.")]
    public GameObject inkStrokePrefab;

    public Color bloodColor = new Color(0.6f, 0f, 0f);
    [Tooltip("Start with ink already on the tip (Trianon); other scenes can still require dipping.")]
    [SerializeField] private bool startsWithInk;
    [Tooltip("Opaque, double-sided URP material for direct strokes on movable paper.")]
    [SerializeField] private Material paperInkMaterial;
    [SerializeField, Min(0.0005f)] private float paperStrokeWidth = 0.002f;
    [SerializeField, Min(0.0002f)] private float paperPointSpacing = 0.001f;

    private bool hasBlood = false;
    private TrailRenderer currentTrail;
    private PaperWritingDetector currentPaper;
    private PaperWritingDetector localSurface;
    private LineRenderer localStroke;
    private Vector3 lastFixedPoint;

    // Completion must use this frame's contact, not the previous LateUpdate's stroke state.
    public bool IsWritingOn(PaperWritingDetector paper) => isActiveAndEnabled && hasBlood &&
        localSurface == paper && paper != null && quillTip != null &&
        paper.TryProjectInkPoint(quillTip.position, out _);

    private void Awake()
    {
        if (startsWithInk) LoadInk();
    }

    private void LoadInk()
    {
        hasBlood = true;
        if (tipRenderer != null) tipRenderer.material.color = bloodColor;
    }

    private void Update()
    {
        if (currentPaper != null && !currentPaper.CanWrite) EndStroke();
    }

    private void LateUpdate()
    {
        if (localSurface == null || !localSurface.isActiveAndEnabled || !hasBlood || quillTip == null ||
            !localSurface.TryProjectInkPoint(quillTip.position, out var point))
        {
            EndLocalStroke();
            return;
        }

        if (localStroke == null)
        {
            var template = inkStrokePrefab != null ? inkStrokePrefab.GetComponent<TrailRenderer>() : null;
            if (paperInkMaterial == null && template == null) return;
            var stroke = new GameObject("Paper ink stroke") { layer = localSurface.gameObject.layer };
            stroke.transform.SetParent(localSurface.transform, false);
            localStroke = stroke.AddComponent<LineRenderer>();
            localStroke.useWorldSpace = false;
            localStroke.alignment = LineAlignment.TransformZ;
            if (paperInkMaterial != null) localStroke.sharedMaterial = paperInkMaterial;
            else localStroke.sharedMaterials = template.sharedMaterials;
            localStroke.widthMultiplier = paperStrokeWidth;
            localStroke.numCapVertices = 4;
            localStroke.numCornerVertices = 4;
            localStroke.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            localStroke.receiveShadows = false;
            localStroke.startColor = localStroke.endColor = bloodColor;
            ApplyInkColor(localStroke);
            localStroke.positionCount = 2;
            localStroke.SetPosition(0, point);
            // A short capped segment makes first contact a visible dot, rather than invisible ink.
            localStroke.SetPosition(1, point + Vector3.right * 0.0001f);
            lastFixedPoint = point;
        }
        else if ((point - lastFixedPoint).sqrMagnitude > 0.00000001f)
        {
            localStroke.SetPosition(localStroke.positionCount - 1, point);
            if ((point - lastFixedPoint).sqrMagnitude >= paperPointSpacing * paperPointSpacing)
            {
                localStroke.positionCount++;
                localStroke.SetPosition(localStroke.positionCount - 1, point);
                lastFixedPoint = point;
            }
        }
        // Detection and the visible mark use exactly the same paper-local sample.
        localSurface.RecordLocalInk(point);
    }

    private void OnTriggerEnter(Collider other)
    { TryWrite(other); }

    private void OnTriggerStay(Collider other)
    { TryWrite(other); }

    private void TryWrite(Collider other)
    {
        if (other.CompareTag("Blood") && !hasBlood)
        {
            LoadInk();
        }
        else if (other.CompareTag("Paper") && hasBlood)
        {
            var paper = other.GetComponent<PaperWritingDetector>();
            if (paper != null && paper.UsesLocalInk)
            {
                if (localSurface != paper) EndLocalStroke();
                EndStroke();
                localSurface = paper;
            }
            else if (currentTrail == null && (paper == null || paper.CanWrite))
            {
                StartStroke();
                currentPaper = paper;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // For movable paper, actual nib/surface geometry decides when a stroke ends.
        // The quill's larger body collider may exit while the nib still touches the sheet.
        if (other.GetComponent<PaperWritingDetector>()?.UsesLocalInk == true) return;
        if (other.CompareTag("Paper"))
        {
            EndStroke();
        }
    }

    private void StartStroke()
    {
        // CRITICAL FIX: If a trail is already attached from a VR jitter, detach it first!
        if (currentTrail != null)
        {
            EndStroke();
        }

        GameObject stroke = Instantiate(inkStrokePrefab, quillTip.position, quillTip.rotation);
        stroke.transform.SetParent(quillTip);

        currentTrail = stroke.GetComponent<TrailRenderer>();

        // Clear the trail to prevent it drawing a sudden line from the world origin
        currentTrail.Clear();
        currentTrail.emitting = true;
        currentTrail.startColor = bloodColor;
        currentTrail.endColor = bloodColor;
        // Also tint shaders which do not multiply by the trail's vertex colors.
        ApplyInkColor(currentTrail);
    }

    private void ApplyInkColor(Renderer renderer)
    {
        var inkProperties = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(inkProperties);
        inkProperties.SetColor("_BaseColor", bloodColor);
        inkProperties.SetColor("_Color", bloodColor);
        renderer.SetPropertyBlock(inkProperties);
    }

    private void OnDisable()
    {
        EndStroke();
        EndLocalStroke();
        localSurface = null;
    }

    private void EndLocalStroke()
    {
        if (localStroke == null) return;
        // Keep the completed geometry parented to the sheet for its whole lifetime.
        localSurface?.EndLocalInk();
        localStroke = null;
    }

    private void EndStroke()
    {
        if (currentTrail != null)
        {
            currentTrail.emitting = false;
            currentTrail.transform.SetParent(null);
            currentTrail = null;
        }
        currentPaper = null;
    }
}
