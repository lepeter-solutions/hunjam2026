using UnityEngine;

public class VRTextFollow : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distance = 1.5f;
    [SerializeField] private float smoothSpeed = 8f;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 targetPos = new Vector3(cameraTransform.position.x + cameraTransform.forward.x * distance, transform.position.y, cameraTransform.position.z + cameraTransform.forward.z * distance);  //cameraTransform.position + cameraTransform.forward * distance;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(transform.position - cameraTransform.position),
            Time.deltaTime * smoothSpeed
        );
    }
}