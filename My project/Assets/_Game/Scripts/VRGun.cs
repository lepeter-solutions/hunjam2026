using UnityEngine;
using UnityEngine.Events;

public class VRGun : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Empty GameObject at the tip of the barrel (blue Z-axis pointing forward).")]
    public Transform muzzleTransform;
    [Tooltip("The player's own head/body collider.")]
    public Collider playerCollider;

    public ParticleSystem muzzleFlash;

    [Header("Settings")]
    public string enemyTag = "enemy";
    public float maxDistance = 500f;
    public LayerMask hitLayers = ~0;

    [Header("Controller Trigger Input")]
    [Tooltip("If true, pulling the controller index trigger while holding the gun fires it.")]
    public bool useControllerTrigger = true;
    public OVRInput.RawButton triggerButton = OVRInput.RawButton.RIndexTrigger | OVRInput.RawButton.LIndexTrigger;

    [Header("Events")]
    public UnityEvent onFired;
    public UnityEvent onShotSelf;
    public UnityEvent onShotEnemy;

    public bool isGrabbed = false;

    private void Awake()
    {
        if (muzzleTransform == null)
            muzzleTransform = transform;
    }

    private void Update()
    {
        if (isGrabbed && useControllerTrigger && OVRInput.GetDown(triggerButton))
        {
            Fire();
        }
    }

    // Can also be called directly from Meta XR's HandGrabUse / ActiveStateUnityEventWrapper
    public void Fire()
    {
        onFired?.Invoke();

        Instantiate(muzzleFlash, muzzleTransform.position, muzzleTransform.rotation);

        if (Physics.Raycast(muzzleTransform.position, muzzleTransform.forward, out RaycastHit hit, maxDistance, hitLayers, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.CompareTag("Player"))
            {
                onShotSelf?.Invoke();
            }
            else if (hit.collider.CompareTag(enemyTag))
            {
                onShotEnemy?.Invoke();
            }
        }
    }

    // Wire these to your PointableUnityEventWrapper (WhenSelect / WhenUnselect)
    public void OnGrab() => isGrabbed = true;
    public void OnRelease() => isGrabbed = false;

    private void OnDrawGizmosSelected()
    {
        Transform origin = muzzleTransform != null ? muzzleTransform : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin.position, origin.forward * 2f);
    }
}