using System.Collections.Generic;
using UnityEngine;

namespace HunJam
{
    public sealed class ControllerHandPose : MonoBehaviour
    {
        [SerializeField, Range(0, 0.4f)] private float relaxedCurl = 0.14f;
        [SerializeField, Min(1)] private float response = 28f;
        private float currentGrip, currentTrigger;
        private readonly List<Transform> joints = new List<Transform>();
        private readonly List<Quaternion> rest = new List<Quaternion>();
        private readonly List<Vector3> axes = new List<Vector3>();
        private void Awake()
        {
            foreach (var joint in GetComponentsInChildren<Transform>())
            {
                if (!joint.name.Contains("Proximal") && !joint.name.Contains("Intermediate") && !joint.name.Contains("Distal")) continue;
                joints.Add(joint);
                rest.Add(joint.localRotation);
                axes.Add(joint.InverseTransformDirection(transform.right));
            }
        }
        public void SetPose(float grip, float trigger, float objectClosure = 1f)
        {
            float blend = 1f - Mathf.Exp(-response * Time.deltaTime);
            currentGrip = Mathf.Lerp(currentGrip, Mathf.Lerp(relaxedCurl, objectClosure, Mathf.Clamp01(grip)), blend);
            currentTrigger = Mathf.Lerp(currentTrigger, Mathf.Max(currentGrip * 0.8f, Mathf.Clamp01(trigger)), blend);
            for (int i = 0; i < joints.Count; i++)
            {
                float amount = joints[i].name.Contains("Index") ? currentTrigger : currentGrip;
                float angle = joints[i].name.Contains("Thumb") ? 35 : 70;
                joints[i].localRotation = rest[i] * Quaternion.AngleAxis(angle * amount, axes[i]);
            }
        }
    }
}
