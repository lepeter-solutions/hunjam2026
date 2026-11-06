using UnityEngine;

namespace HunJam
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnergyCell : MonoBehaviour
    {
        private Rigidbody body;
        private Transform holder;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 releaseVelocity;
        public bool IsHeld => holder != null;
        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            startPosition = transform.position;
            startRotation = transform.rotation;
        }
        public bool Grab(Transform anchor)
        {
            if (IsHeld || !isActiveAndEnabled) return false;
            holder = anchor;
            releaseVelocity = Vector3.zero;
            body.isKinematic = true;
            body.detectCollisions = false;
            return true;
        }
        public void Release()
        {
            if (!IsHeld) return;
            holder = null;
            body.isKinematic = false;
            body.detectCollisions = true;
            body.linearVelocity = releaseVelocity;
        }
        public void ResetCell()
        {
            Release();
            gameObject.SetActive(true);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = startPosition;
            body.rotation = startRotation;
        }
        private void FixedUpdate()
        {
            if (IsHeld)
            {
                releaseVelocity = Vector3.ClampMagnitude((holder.position - body.position) / Time.fixedDeltaTime, 6f);
                body.MovePosition(holder.position);
                body.MoveRotation(holder.rotation);
            }
            else if (body.position.y < -3f) ResetCell();
        }
        private void OnDisable() { if (body != null) Release(); }
    }
}
