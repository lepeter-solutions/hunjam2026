using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class CustomHandBoneMapper : MonoBehaviour
{
    [Header("Hand Renderers")]
    [Tooltip("Drag the original OculusHand_L or OculusHand_R SkinnedMeshRenderer here.")]
    public SkinnedMeshRenderer sourceHandRenderer;

    [Tooltip("Your custom long-hand SkinnedMeshRenderer (auto-fills if on this object).")]
    public SkinnedMeshRenderer customHandRenderer;

    [Header("Settings")]
    public bool hideSourceHand = true;
    [Tooltip("Enable only if the custom hand is the exact same scale as the original. Keep false for larger hands.")]
    public bool copyFingerPositions = false;

    [Header("Live Alignment Offsets")]
    public Vector3 wristPositionOffset = Vector3.zero;
    public Vector3 wristRotationOffset = Vector3.zero;
    public Vector3 fingerRotationOffset = Vector3.zero;

    private struct BonePair
    {
        public Transform source;
        public Transform target;
    }

    private readonly List<BonePair> _bonePairs = new List<BonePair>();
    private Transform _sourceRoot;
    private Transform _targetRoot;

    private void Awake()
    {
        if (customHandRenderer == null)
            customHandRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (sourceHandRenderer == null || customHandRenderer == null)
        {
            Debug.LogError("[CustomHandBoneMapper] Assign both Source and Custom SkinnedMeshRenderers.", this);
            enabled = false;
            return;
        }

        customHandRenderer.updateWhenOffscreen = true;
        MapBones();
    }

    private void OnEnable()
    {
        if (sourceHandRenderer != null && hideSourceHand)
            sourceHandRenderer.enabled = false;
    }

    private void OnDisable()
    {
        if (sourceHandRenderer != null && hideSourceHand)
            sourceHandRenderer.enabled = true;
    }

    [ContextMenu("Rebuild Bone Map")]
    public void MapBones()
    {
        _bonePairs.Clear();

        _sourceRoot = sourceHandRenderer.rootBone != null ? sourceHandRenderer.rootBone : sourceHandRenderer.transform;
        _targetRoot = customHandRenderer.rootBone != null ? customHandRenderer.rootBone : customHandRenderer.transform;

        var sourceBoneMap = new Dictionary<string, Transform>();
        foreach (Transform t in _sourceRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!sourceBoneMap.ContainsKey(t.name))
                sourceBoneMap.Add(t.name, t);
        }

        foreach (Transform targetBone in _targetRoot.GetComponentsInChildren<Transform>(true))
        {
            if (targetBone == _targetRoot) continue;

            if (sourceBoneMap.TryGetValue(targetBone.name, out Transform sourceBone))
            {
                _bonePairs.Add(new BonePair { source = sourceBone, target = targetBone });
            }
        }
    }

    private void LateUpdate()
    {
        if (_sourceRoot == null || _targetRoot == null) return;

        // 1. Snap wrist/root in world space
        _targetRoot.position = _sourceRoot.TransformPoint(wristPositionOffset);
        _targetRoot.rotation = _sourceRoot.rotation * Quaternion.Euler(wristRotationOffset);

        // 2. Copy finger bone rotations (preserves custom hand scale & bone lengths)
        Quaternion fingerOffset = Quaternion.Euler(fingerRotationOffset);
        for (int i = 0; i < _bonePairs.Count; i++)
        {
            BonePair pair = _bonePairs[i];
            if (copyFingerPositions)
                pair.target.position = pair.source.position;

            pair.target.localRotation = pair.source.localRotation * fingerOffset;
        }
    }
}