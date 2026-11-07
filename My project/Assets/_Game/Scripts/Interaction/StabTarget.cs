using UnityEngine;
namespace HunJam
{
    public sealed class StabTarget : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 0.1f)] private float maximumDepth = 0.055f;
        [SerializeField, Range(0.1f, 1)] private float insertionRatio = 0.4f;
        public float MaximumDepth => maximumDepth;
        public float InsertionRatio => insertionRatio;
    }
}
