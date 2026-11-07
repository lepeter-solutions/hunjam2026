using UnityEngine;
namespace HunJam
{
    public sealed class Tool : GameItem
    {
        public enum ToolKind { Hammer, Blade, Lamp }
        [SerializeField] private ToolKind kind;
        [SerializeField] private Transform tip;
        [SerializeField] private Light lamp;
        [SerializeField] private float minimumSpeed = 1f;
        [SerializeField] private float contactRadius = 0.025f;
        private Vector3 previousTip;
        private bool sampling;
        public ToolKind Kind => kind;
        public bool IsOn => lamp != null && lamp.enabled;
        public void Configure(ToolKind type, Transform contact, Light light = null)
        { kind = type; tip = contact; lamp = light; }
        public override void Use()
        { if (IsHeld && lamp != null) lamp.enabled = !lamp.enabled; }
        public override void ResetObject()
        { base.ResetObject(); sampling = false; if (lamp != null) lamp.enabled = false; }
        private void LateUpdate()
        {
            if (!CanImpact || tip == null || kind == ToolKind.Lamp)
            { sampling = false; return; }
            Vector3 current = tip.position;
            if (sampling)
            {
                Vector3 travel = current - previousTip;
                float speed = travel.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
                if (speed >= minimumSpeed && travel.magnitude < 0.5f)
                {
                    // Closest solid obstruction wins; never cut through the workbench or a wall.
                    var hits = Physics.SphereCastAll(previousTip, contactRadius, travel.normalized,
                        travel.magnitude, ~(1 << 2), QueryTriggerInteraction.Ignore);
                    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                    foreach (var hit in hits)
                    {
                        if (hit.collider.GetComponentInParent<GameItem>() == this) continue;
                        if (kind == ToolKind.Blade) hit.collider.GetComponentInParent<Cuttable>()?.Cut(travel.normalized * speed);
                        if (kind == ToolKind.Hammer) hit.collider.GetComponentInParent<ImpactTarget>()?.Strike(speed);
                        break;
                    }
                }
            }
            previousTip = current;
            sampling = true;
        }
    }
}
