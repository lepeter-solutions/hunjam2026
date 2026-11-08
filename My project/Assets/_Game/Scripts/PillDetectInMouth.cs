using UnityEngine;
using UnityEngine.Events;

namespace HunJam
{
    public class PillDetectInMouth : MonoBehaviour
    {
        public ZoneInteractable zoneInteractable;
        public Transform playerHead;
        public Vector3 mouthOffset = new Vector3(0, -0.07f, 0.04f);
        [Min(0.01f)] public float mouthRadius = 0.09f;
        [Min(0)] public float holdTime = 0.35f;
        public UnityEvent events;
        private Oculus.Interaction.Grabbable grabbable;
        private float heldAtMouth;
        private bool consumed;

        private void Awake() => grabbable = GetComponent<Oculus.Interaction.Grabbable>();
        private void Update() => CheckConditions();

        public void CheckConditions()
        {
            if (consumed || GameManager.Instance.InTransition) return;
            bool held = grabbable && grabbable.GrabPoints.Count > 0;
            bool inMouth = playerHead && held && Vector3.Distance(transform.position,
                playerHead.TransformPoint(mouthOffset)) <= mouthRadius;
            if (!inMouth) { heldAtMouth = 0; return; }
            heldAtMouth += Time.unscaledDeltaTime;
            if (heldAtMouth < holdTime) return;
            consumed = true;
            events?.Invoke();
        }
    }
}
