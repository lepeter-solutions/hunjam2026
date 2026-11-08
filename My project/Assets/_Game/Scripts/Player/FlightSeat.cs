using UnityEngine;
namespace HunJam
{
    // Flight-only seating; other scenes keep normal room-scale tracking.
    [DefaultExecutionOrder(-150)]
    public sealed class FlightSeat : MonoBehaviour
    {
        [SerializeField] private DemoPlayer player;
        [SerializeField] private Transform seatEye;
        [Tooltip("Pilot eye position and initial forward direction in this cockpit.")]
        [SerializeField] private Transform pilotPivot;
        [SerializeField] private bool lockToPilotPivot;
        private bool calibrated;
        private bool calibratedForVR;
        private Camera calibratedView;
        public void Configure(DemoPlayer rig, Transform eye) { player = rig; seatEye = eye; }
        private void OnEnable()
        {
            if (player != null)
            {
                player.TrackingUpdated += UpdateTrackedSeat;
                player.TrackingOriginChanged += Recenter;
            }
            Application.onBeforeRender += UpdateSeat;
        }
        private void OnDisable()
        {
            if (player != null)
            {
                player.TrackingUpdated -= UpdateTrackedSeat;
                player.TrackingOriginChanged -= Recenter;
            }
            Application.onBeforeRender -= UpdateSeat;
            calibrated = calibratedForVR = false;
            calibratedView = null;
        }
        private void LateUpdate() => UpdateSeat();
        private void UpdateSeat() => UpdateSeat(false);
        private void UpdateTrackedSeat() => UpdateSeat(true);
        private void UpdateSeat(bool freshMetaAnchors)
        {
            Transform target = pilotPivot != null ? pilotPivot : seatEye;
            if (player == null || player.View == null || target == null) return;
            // Meta replaces the startup camera after XR connects. Its tracking-space yaw
            // can differ from the old camera, so it needs its own seat calibration.
            bool needsCalibration = !calibrated || calibratedView != player.View || (player.IsVR && !calibratedForVR);
            // Only the SDK's post-anchor callback can finalize Meta yaw calibration.
            // LateUpdate/onBeforeRender may run before the SDK replaces its cached pose.
            if (needsCalibration && player.IsVR && (!player.UsesMetaTracking || !freshMetaAnchors)) return;
            if (!lockToPilotPivot && !needsCalibration) return;
            if (player.IsVR)
            {
                if (!player.TrackingReady) return;
                var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
                if (!device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) || !tracked) return;
            }
            if (needsCalibration && pilotPivot != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(player.View.transform.forward, target.up);
                Vector3 targetForward = Vector3.ProjectOnPlane(target.forward, target.up);
                // Looking straight up/down has no usable heading. Retry instead of
                // recording a successful calibration with the wrong orientation.
                if (forward.sqrMagnitude <= 0.0001f || targetForward.sqrMagnitude <= 0.0001f) return;
                player.transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(forward, targetForward, target.up), target.up) * player.transform.rotation;
            }
            // Move the common rig root: eyes stay at the seat, hands remain relative to the head.
            // Do not overwrite the SDK's eye rotations or individual controller poses.
            player.transform.position += target.position - player.View.transform.position;
            calibrated = true;
            calibratedForVR = player.IsVR;
            calibratedView = player.View;
        }
        public void Recenter() => calibrated = false;
    }
}
