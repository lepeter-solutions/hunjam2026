using UnityEngine;

public class ElasticBone : MonoBehaviour
{
    [Header("Rubber Band Settings")]
    [Tooltip("How fast the bone snaps back. Higher is faster.")]
    public float snapSpeed = 15f;

    [Header("Stretch Constraints (Local Space)")]
    [Tooltip("Maximum distance the bone can be pulled from its starting position (in meters).")]
    public float maxStretchDistance = 0.05f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    public bool isGrabbed = false;

    void Start()
    {
        // Remember exactly where the bone started relative to the face
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        // We use LateUpdate so this overrides whatever position the Meta SDK tries to force on it

        if (!isGrabbed)
        {
            // SNAP BACK: Smoothly pull the bone back to its starting position
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * snapSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, initialLocalRotation, Time.deltaTime * snapSpeed);
        }
        else
        {
            // CONSTRAIN GRAB: Prevent the bone from stretching too far while held
            Vector3 currentOffset = transform.localPosition - initialLocalPosition;

            // If the player pulls it further than maxStretchDistance, clamp it
            if (currentOffset.magnitude > maxStretchDistance)
            {
                transform.localPosition = initialLocalPosition + currentOffset.normalized * maxStretchDistance;
            }
        }
    }

    // These must be triggered by the Meta SDK Pointable Unity Event Wrapper
    public void OnGrab() { isGrabbed = true; }
    public void OnRelease() { isGrabbed = false; }
}