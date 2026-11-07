using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectResetter : MonoBehaviour
{
    [Header("Reset Conditions")]
    [Tooltip("How far the object can get from its spawn point before resetting.")]
    public float maxDistanceFromOrigin = 10f;

    [Tooltip("If the object falls below this Y position, it resets (Kill-Z).")]
    public float killHeight = -5f;

    [Tooltip("How often to check the position. 1 second is highly efficient.")]
    public float checkInterval = 1f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Store the starting position and rotation
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Start the background check loop
        StartCoroutine(DistanceCheckRoutine());
    }

    private IEnumerator DistanceCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            // Pause the loop for 'checkInterval' seconds
            yield return wait;

            // Condition 1: Object fell through the floor
            if (transform.position.y < killHeight)
            {
                ResetToOrigin();
            }
            // Condition 2: Object was thrown too far away
            else if (Vector3.Distance(initialPosition, transform.position) > maxDistanceFromOrigin)
            {
                ResetToOrigin();
            }
        }
    }

    // This is public so you can also call it from a UI button or another script if it gets stuck
    public void ResetToOrigin()
    {
        // 1. Reset Transform
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 2. Kill all physics momentum so it doesn't bounce upon resetting
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}