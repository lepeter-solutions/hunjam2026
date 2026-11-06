using UnityEngine;

namespace HunJam
{
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        private Rigidbody body;
        private Transform holder;
        private Vector3 startPosition, previousPosition, velocity, angularVelocity, localOffset;
        private Quaternion startRotation, previousRotation, rotationOffset;
        private float armedAt;
        public bool IsHeld => holder != null;
        public Vector3 Velocity => velocity;
        public bool CanImpact => IsHeld && Time.time >= armedAt;
        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody>();
            startPosition = transform.position;
            startRotation = transform.rotation;
        }
        public bool Grab(Transform anchor)
        {
            if (anchor == null || IsHeld || !isActiveAndEnabled) return false;
            holder = anchor;
            // Preserve contact position and orientation; never snap a blade into a target.
            localOffset = anchor.InverseTransformPoint(body.position);
            rotationOffset = Quaternion.Inverse(anchor.rotation) * body.rotation;
            previousPosition = body.position;
            previousRotation = body.rotation;
            velocity = angularVelocity = Vector3.zero;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.isKinematic = true;
            armedAt = Time.time + 0.15f;
            return true;
        }
        public void Release(bool throwObject = true)
        {
            if (!IsHeld) return;
            holder = null;
            body.isKinematic = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearVelocity = throwObject ? velocity : Vector3.zero;
            body.angularVelocity = throwObject ? angularVelocity : Vector3.zero;
            velocity = angularVelocity = Vector3.zero;
        }
        public virtual void Use() { }
        public virtual void ResetObject()
        {
            Release(false);
            gameObject.SetActive(true);
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            body.position = startPosition;
            body.rotation = startRotation;
        }
        protected virtual void FixedUpdate()
        {
            if (!IsHeld)
            {
                if (body.position.y < -3f) ResetObject();
                return;
            }
            Vector3 target = holder.TransformPoint(localOffset);
            Quaternion rotation = holder.rotation * rotationOffset;
            Vector3 delta = target - previousPosition;
            if (delta.magnitude > 0.5f) { Release(false); return; }
            velocity = Vector3.ClampMagnitude(delta / Time.fixedDeltaTime, 8f);
            Quaternion difference = rotation * Quaternion.Inverse(previousRotation);
            difference.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            angularVelocity = angle == 0 ? Vector3.zero : Vector3.ClampMagnitude(axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime), 20);
            previousPosition = target;
            previousRotation = rotation;
            body.MovePosition(target);
            body.MoveRotation(rotation);
        }
        protected virtual void OnDisable() { if (body != null) Release(false); }
    }
}
