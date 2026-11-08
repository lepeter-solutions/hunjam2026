using UnityEngine;

namespace HunJam
{
    // Moves only the existing Meta visual root; SDK bones and input anchors stay untouched.
    [DefaultExecutionOrder(10000)]
    public sealed class YokeHandAttachment : MonoBehaviour
    {
        private DemoHand hand;
        private FlightYoke attachedYoke;
        private Vector3 homePosition, gripPosition;
        private Quaternion homeRotation, gripRotation;

        public void Configure(DemoHand source)
        {
            hand = source;
            homePosition = transform.localPosition;
            homeRotation = transform.localRotation;
        }

        private void OnEnable() => Application.onBeforeRender += UpdateVisual;
        private void OnDisable()
        {
            Application.onBeforeRender -= UpdateVisual;
            RestoreTracking();
        }
        private void LateUpdate() => UpdateVisual();

        [BeforeRenderOrder(10000)]
        private void UpdateVisual()
        {
            var yoke = hand != null && hand.isActiveAndEnabled ? hand.HeldItem as FlightYoke : null;
            if (yoke == null)
            {
                RestoreTracking();
                return;
            }
            if (attachedYoke != yoke)
            {
                RestoreTracking();
                attachedYoke = yoke;
                // Preserve the actual wrist-to-grip offset at acquisition, for either hand.
                gripPosition = yoke.transform.InverseTransformPoint(transform.position);
                gripRotation = Quaternion.Inverse(yoke.transform.rotation) * transform.rotation;
            }
            transform.SetPositionAndRotation(yoke.transform.TransformPoint(gripPosition),
                yoke.transform.rotation * gripRotation);
        }

        private void RestoreTracking()
        {
            if (hand == null) return;
            attachedYoke = null;
            transform.SetLocalPositionAndRotation(homePosition, homeRotation);
        }
    }
}
