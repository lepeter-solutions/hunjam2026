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
        private readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        public bool IsVR => XRSettings.isDeviceActive;
        public Camera View => view;
        public bool TrackingReady => floorConfigured;
        public void Configure(Transform space, Camera camera, DemoHand left, DemoHand right)
        { trackingSpace = space; view = camera; }
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
