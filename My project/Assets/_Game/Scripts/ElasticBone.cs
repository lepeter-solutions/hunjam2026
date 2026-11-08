using UnityEngine;

[DefaultExecutionOrder(100)]
public class ElasticBone : MonoBehaviour
{
    [Header("Output")]
    [Tooltip("The target Transform that receives the constrained position and rotation.")]
    public Transform outputTransform;
    [Tooltip("Scales the Z offset of the outputTransform based on how far the bone is stretched from its start position.")]
    public float zOffsetScale = 0f;

    [Header("Rubber Band Settings")]
    [Tooltip("How fast the bone snaps back. Higher is faster.")]
    public float snapSpeed = 15f;

    [Header("Stretch Constraints (Local Space)")]
    [Tooltip("Maximum distance the bone can be pulled from its starting position (in meters).")]
    public float maxStretchDistance = 0.05f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    public bool isGrabbed = false;

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        UpdateOutputTransform(0f);
    }

    private void LateUpdate()
    {
        if (!isGrabbed)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * snapSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialLocalRotation, Time.deltaTime * snapSpeed);
        }
        else
        {
            Vector3 currentOffset = transform.localPosition - initialLocalPosition;
            transform.localPosition = initialLocalPosition + Vector3.ClampMagnitude(currentOffset, maxStretchDistance);
        }

        float distanceFromLip = Vector3.Distance(transform.localPosition, initialLocalPosition);
        UpdateOutputTransform(distanceFromLip);
    }

    private void UpdateOutputTransform(float distanceFromLip)
    {
        if (outputTransform == null) return;

        Vector3 zOffset = transform.forward * (distanceFromLip * zOffsetScale);
        outputTransform.SetPositionAndRotation(transform.position + zOffset, transform.rotation);
    }

    // Triggered by the Meta SDK Pointable Unity Event Wrapper
    public void OnGrab() => isGrabbed = true;
    public void OnRelease() => isGrabbed = false;
}