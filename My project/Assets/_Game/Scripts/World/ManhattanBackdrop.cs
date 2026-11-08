using UnityEngine;

namespace HunJam
{
    // The Meta eye camera replaces the startup camera after XR initialization.
    public sealed class ManhattanBackdrop : MonoBehaviour
    {
        [SerializeField, Min(1600f)] private float viewDistance = 6000f;
        private DemoPlayer player;
        private Camera configuredCamera;
        private float originalDistance;

        private void Update()
        {
            if (player == null) player = FindFirstObjectByType<DemoPlayer>();
            var camera = player != null ? player.View : null;
            if (camera == null || camera == configuredCamera) return;
            RestoreCamera();
            configuredCamera = camera;
            originalDistance = camera.farClipPlane;
            camera.farClipPlane = Mathf.Max(originalDistance, viewDistance);
        }

        private void OnDisable() => RestoreCamera();
        private void RestoreCamera()
        {
            if (configuredCamera != null) configuredCamera.farClipPlane = originalDistance;
            configuredCamera = null;
        }
    }
}
