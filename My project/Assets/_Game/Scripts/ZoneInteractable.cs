using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ZoneInteractable : MonoBehaviour
{
    [Header("Zone Setup")]
    public Transform targetPoint;

    public float catchRadius = 0.1f;

    public bool snapToCenter = true;

    [Header("State")]
    public bool isGrabbed = false;
    public bool isSnappedToCenter = false;

    private Rigidbody rb;
    private bool wasInZone = false;

    // Used to remember where the object was relative to the bone if snapToCenter is false
    private Vector3 relativeOffsetPos;
    private Quaternion relativeOffsetRot;

    [HideInInspector]
    public bool extraCondition = true;

    public void SetExtraCondition(bool newVal) => extraCondition = newVal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isSnappedToCenter = false;
    }

    void LateUpdate()
    {
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        if (distance <= catchRadius)
        {
            rb.isKinematic = true;
            rb.useGravity = false;

            if (snapToCenter && extraCondition)
            {
                transform.position = targetPoint.position;
                transform.rotation = targetPoint.rotation;
                isSnappedToCenter = true;
            }
            else
            {
                // Freeze it exactly where it entered, but make it move with the animation
                if (!wasInZone)
                {
                    relativeOffsetPos = targetPoint.InverseTransformPoint(transform.position);
                    relativeOffsetRot = Quaternion.Inverse(targetPoint.rotation) * transform.rotation;
                    wasInZone = true;
                }

                transform.position = targetPoint.TransformPoint(relativeOffsetPos);
                transform.rotation = targetPoint.rotation * relativeOffsetRot;
                isSnappedToCenter = false;
            }
        }
        else
        {
            // Outside the zone: Reactivate standard physics
            rb.isKinematic = false;
            rb.useGravity = true;
            wasInZone = false;
            isSnappedToCenter = false;
        }
    }

    // Triggered by the Meta Pointable Unity Event Wrapper
    public void OnGrab()
    {
        isGrabbed = true;
        wasInZone = false; // Reset offset logic for the next drop
        isSnappedToCenter = false;
    }

    public void OnRelease()
    {
        isGrabbed = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (targetPoint != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(targetPoint.position, catchRadius);
        }
    }
}