using UnityEngine;
using UnityEngine.Events;

public class BucketBloodFiller : MonoBehaviour
{
    [Header("Liquid Plane")]
    [Tooltip("The plane Transform inside the bucket that moves up as it fills.")]
    [SerializeField] private Transform liquidPlane;

    [Tooltip("Maximum local Y position of the plane when the bucket is full.")]
    [SerializeField] private float maxLocalY = 0.25f;

    [Tooltip("How fast the plane rises (in local units per second).")]
    [SerializeField] private float fillSpeed = 0.05f;

    [Header("Particle Detection (Above Bucket)")]
    [Tooltip("Optional: Assign your Blood Particle Prefab or scene ParticleSystem. If left empty, any playing ParticleSystem above the bucket will fill it.")]
    [SerializeField] private ParticleSystem targetParticleSystem;

    [Tooltip("Horizontal radius of the detection zone above the bucket.")]
    [SerializeField] private float bucketRadius = 0.2f;

    [Tooltip("Maximum height above the bucket origin where bleeding is detected.")]
    [SerializeField] private float maxHeightAbove = 1.0f;

    [Tooltip("Minimum height relative to the bucket origin.")]
    [SerializeField] private float minHeightOffset = 0.0f;

    private ParticleSystem[] _cachedSystems = new ParticleSystem[0];
    private float _nextScanTime;
    private float _lastParticleCollisionTime = -999f;

    private void Update()
    {
        if (liquidPlane == null || liquidPlane.localPosition.y >= maxLocalY)
            return;

        if (IsBleedingAboveBucket() || Time.time - _lastParticleCollisionTime <= 0.1f)
        {
            Vector3 localPos = liquidPlane.localPosition;
            localPos.y = Mathf.MoveTowards(localPos.y, maxLocalY, fillSpeed * Time.deltaTime);
            liquidPlane.localPosition = localPos;
        }
    }

    private bool IsBleedingAboveBucket()
    {
        // Fast path if a scene instance is directly assigned
        if (targetParticleSystem != null && targetParticleSystem.gameObject.scene.IsValid())
        {
            return IsSystemActiveAndAbove(targetParticleSystem);
        }

        // Refresh active spawned ParticleSystems every 0.1s (works with runtime-spawned blood prefabs)
        if (Time.time >= _nextScanTime)
        {
            _nextScanTime = Time.time + 0.1f;
            _cachedSystems = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        for (int i = 0; i < _cachedSystems.Length; i++)
        {
            ParticleSystem ps = _cachedSystems[i];
            if (ps == null) continue;

            // If a prefab is assigned, match its name against spawned clones
            if (targetParticleSystem != null && !ps.name.StartsWith(targetParticleSystem.name))
                continue;

            if (IsSystemActiveAndAbove(ps))
                return true;
        }

        return false;
    }

    private bool IsSystemActiveAndAbove(ParticleSystem ps)
    {
        if (!ps.isPlaying) return false;

        Vector3 bucketPos = transform.position;
        Vector3 psPos = ps.transform.position;

        float heightDiff = psPos.y - bucketPos.y;
        if (heightDiff < minHeightOffset || heightDiff > maxHeightAbove)
            return false;

        float horizontalDistSqr = (new Vector2(psPos.x - bucketPos.x, psPos.z - bucketPos.z)).sqrMagnitude;
        return horizontalDistSqr <= bucketRadius * bucketRadius;
    }

    // Fallback: Also triggers if Particle System Collision (World + Send Collision Messages) is enabled
    private void OnParticleCollision(GameObject other)
    {
        if (targetParticleSystem != null && !other.name.StartsWith(targetParticleSystem.name))
            return;

        _lastParticleCollisionTime = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Vector3 bottom = transform.position + Vector3.up * minHeightOffset;
        Vector3 top = transform.position + Vector3.up * maxHeightAbove;

        Gizmos.DrawWireSphere(bottom, bucketRadius);
        Gizmos.DrawWireSphere(top, bucketRadius);
        Gizmos.DrawLine(bottom + Vector3.forward * bucketRadius, top + Vector3.forward * bucketRadius);
        Gizmos.DrawLine(bottom - Vector3.forward * bucketRadius, top - Vector3.forward * bucketRadius);
        Gizmos.DrawLine(bottom + Vector3.right * bucketRadius, top + Vector3.right * bucketRadius);
        Gizmos.DrawLine(bottom - Vector3.right * bucketRadius, top + Vector3.right * bucketRadius);
    }
}