using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ZoneInteractable : MonoBehaviour
{
    [Header("Zone Setup")]
    [Tooltip("The moving bone or object this item should follow (e.g., MouthCenter, Holster, Hand).")]
    public Transform targetBone;

    [Tooltip("The radius (in meters) where the item gets caught. 0.1 = 10cm.")]
    public float catchRadius = 0.1f;

    [Tooltip("If true, the item snaps exactly to the target's center. If false, it freezes wherever it entered the radius, but still follows the animation.")]
    public bool snapToCenter = true;

    private Rigidbody rb;
    public bool isGrabbed = false;
    private bool wasInZone = false;

    // Used to remember where the object was relative to the bone if snapToCenter is false
    private Vector3 relativeOffsetPos;
    private Quaternion relativeOffsetRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        // LateUpdate ensures we calculate this AFTER the character's animation moves the bone for this frame
        if (targetBone == null || isGrabbed)
            return;

        float distance = Vector3.Distance(transform.position, targetBone.position);

        if (distance <= catchRadius)
        {
            // Inside the zone: Turn off real physics
            rb.isKinematic = true;
            rb.useGravity = false;

            if (snapToCenter)
            {
                // Lock the object perfectly to the target
                transform.position = targetBone.position;
                transform.rotation = targetBone.rotation;
            }
            else
            {
                // Freeze it exactly where it entered, but make it move with the animation
                if (!wasInZone)
                {
                    relativeOffsetPos = targetBone.InverseTransformPoint(transform.position);
                    relativeOffsetRot = Quaternion.Inverse(targetBone.rotation) * transform.rotation;
                    wasInZone = true;
                }

                transform.position = targetBone.TransformPoint(relativeOffsetPos);
                transform.rotation = targetBone.rotation * relativeOffsetRot;
            }
        }
        else
        {
            // Outside the zone: Reactivate standard physics
            rb.isKinematic = false;
            rb.useGravity = true;
            wasInZone = false;
        }
    }

    // Triggered by the Meta Pointable Unity Event Wrapper
    public void OnGrab()
    {
        isGrabbed = true;
        wasInZone = false; // Reset offset logic for the next drop
    }

    public void OnRelease()
    {
        isGrabbed = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (targetBone != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(targetBone.position, catchRadius);
        }
    }
}