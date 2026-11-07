using UnityEngine;
namespace HunJam
{
    // Flight-only calibration. Physical head/hand deltas remain in the same tracking space.
    public sealed class FlightSeat : MonoBehaviour
    {
        [SerializeField] private DemoPlayer player;
        [SerializeField] private Transform seatEye;
        private bool calibrated;
        private bool calibratedForVR;
        public void Configure(DemoPlayer rig, Transform eye) { player = rig; seatEye = eye; }
        private void LateUpdate()
        {
            if (player == null || seatEye == null) return;
            if (calibrated && (!player.IsVR || calibratedForVR)) return;
            if (player.IsVR)
            {
                if (!player.TrackingReady) return;
                var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
                if (!device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) || !tracked) return;
            }
            player.transform.position += seatEye.position - player.View.transform.position;
            calibrated = true;
            calibratedForVR = player.IsVR;
        }
        public void Recenter() => calibrated = false;
    }
}
