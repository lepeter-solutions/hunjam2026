using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace HunJam
{
    public sealed class DemoPlayer : MonoBehaviour
    {
        [SerializeField] private Transform trackingSpace;
        [SerializeField] private Camera view;
        private bool floorConfigured;
        private OVRCameraRig metaRig;
        private bool metaTrackingReady;
        private float validTrackingSince = -1f;
        private OVRDisplay subscribedDisplay;
        private readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        public bool IsVR => XRSettings.isDeviceActive;
        public Camera View => view;
        public event System.Action TrackingUpdated;
        public event System.Action TrackingOriginChanged;
        public bool UsesMetaTracking => metaRig != null;
        public bool TrackingReady => metaRig != null ? metaTrackingReady && OVRManager.isHmdPresent : floorConfigured;
        public void Configure(Transform space, Camera camera, DemoHand left, DemoHand right)
        { trackingSpace = space; view = camera; }
        public void UseMetaTracking(OVRCameraRig rig, Camera camera)
        {
            if (metaRig != null) metaRig.UpdatedAnchors -= OnMetaAnchorsUpdated;
            metaRig = rig; trackingSpace = rig.trackingSpace; view = camera;
            metaTrackingReady = false;
            validTrackingSince = -1f;
            metaRig.UpdatedAnchors += OnMetaAnchorsUpdated;
        }
        private void OnMetaAnchorsUpdated(OVRCameraRig rig)
        {
            // HMD presence alone does not mean that this anchor contains a fresh pose.
            bool valid = OVRManager.isHmdPresent &&
                OVRNodeStateProperties.GetNodeStatePropertyQuaternion(UnityEngine.XR.XRNode.CenterEye,
                    NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var rotation) &&
                Quaternion.Angle(rotation, rig.centerEyeAnchor.localRotation) < 0.1f;
            if (!valid) validTrackingSince = -1f;
            else if (validTrackingSince < 0f) validTrackingSince = Time.unscaledTime;
            // Allow stage-origin setup to settle before the seat consumes the first pose.
            metaTrackingReady = valid && Time.unscaledTime - validTrackingSince >= 0.25f;
            TrackingUpdated?.Invoke();
        }
        private void OnTrackingOriginChanged(XRInputSubsystem subsystem) => InvalidateTrackingOrigin();
        private void InvalidateTrackingOrigin()
        {
            metaTrackingReady = false;
            validTrackingSince = -1f;
            TrackingOriginChanged?.Invoke();
        }
        private void OnDestroy()
        {
            if (metaRig != null) metaRig.UpdatedAnchors -= OnMetaAnchorsUpdated;
            if (subscribedDisplay != null) subscribedDisplay.RecenteredPose -= InvalidateTrackingOrigin;
            foreach (var subsystem in subsystems) subsystem.trackingOriginUpdated -= OnTrackingOriginChanged;
        }
        private void Awake()
        {
            var pose = view.gameObject.AddComponent<TrackedPoseDriver>();
            pose.positionInput = new InputActionProperty(new InputAction("Head position", InputActionType.Value, "<XRHMD>/centerEyePosition"));
            pose.rotationInput = new InputActionProperty(new InputAction("Head rotation", InputActionType.Value, "<XRHMD>/centerEyeRotation"));
            pose.trackingStateInput = new InputActionProperty(new InputAction("Head tracking", InputActionType.Value, "<XRHMD>/trackingState"));
        }
        private void Update()
        {
            // The root never moves or turns; locomotion comes only from physical tracking.
            if (!IsVR) { floorConfigured = false; return; }
            if (metaRig != null)
            {
                if (subscribedDisplay != OVRManager.display)
                {
                    if (subscribedDisplay != null) subscribedDisplay.RecenteredPose -= InvalidateTrackingOrigin;
                    subscribedDisplay = OVRManager.display;
                    if (subscribedDisplay != null) subscribedDisplay.RecenteredPose += InvalidateTrackingOrigin;
                }
                // Refresh subscriptions as Link can attach after the scene has loaded.
                foreach (var subsystem in subsystems) subsystem.trackingOriginUpdated -= OnTrackingOriginChanged;
                SubsystemManager.GetSubsystems(subsystems);
                foreach (var subsystem in subsystems) subsystem.trackingOriginUpdated += OnTrackingOriginChanged;
                return; // OVRManager owns the stage origin; no second pose writer.
            }
            if (floorConfigured) return;
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var subsystem in subsystems)
                if (subsystem.running)
                {
                    bool floor = subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
                    trackingSpace.localPosition = floor ? Vector3.zero : Vector3.up * 1.65f;
                    floorConfigured = true;
                    break;
                }
        }
    }
}
