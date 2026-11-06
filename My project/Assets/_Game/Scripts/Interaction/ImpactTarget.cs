using UnityEngine;
namespace HunJam
{
    public sealed class ImpactTarget : MonoBehaviour
    {
        [SerializeField] private TextMesh label;
        private float nextHit;
        public int Hits { get; private set; }
        public void Configure(TextMesh text) { label = text; ResetTarget(); }
        public void Strike(float speed)
        {
            if (speed < 1f || Time.time < nextHit) return;
            Hits++;
            nextHit = Time.time + 0.25f;
            if (label != null) label.text = "HAMMER HITS: " + Hits;
        }
        public void ResetTarget()
        { Hits = 0; nextHit = 0; if (label != null) label.text = "HAMMER HITS: 0"; }
    }
}
