using UnityEngine;

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

    private bool hasBlood = false;
    private TrailRenderer currentTrail;
    private PaperWritingDetector currentPaper;

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
        else if (other.CompareTag("Paper") && hasBlood && currentTrail == null)
        {
            var paper = other.GetComponent<PaperWritingDetector>();
            if (paper == null || paper.CanWrite)
            {
                StartStroke();
                currentPaper = paper;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
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
        var inkProperties = new MaterialPropertyBlock();
        currentTrail.GetPropertyBlock(inkProperties);
        inkProperties.SetColor("_BaseColor", bloodColor);
        inkProperties.SetColor("_Color", bloodColor);
        currentTrail.SetPropertyBlock(inkProperties);
    }

    private void OnDisable() => EndStroke();

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
