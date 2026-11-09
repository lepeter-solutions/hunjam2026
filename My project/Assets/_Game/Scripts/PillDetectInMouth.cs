using UnityEngine;
using UnityEngine.Events;

namespace HunJam
{
    public class PillDetectInMouth : MonoBehaviour
    {
        public ZoneInteractable zoneInteractable;
        public UnityEvent events;
        private bool consumed;

        public bool mouthIsGrabbed => zoneInteractable.extraCondition;
        
        private void Update() => CheckConditions();

        public void CheckConditions()
        {
            if (consumed || GameManager.Instance.InTransition) return;

            if (mouthIsGrabbed && zoneInteractable.isGrabbed && Vector3.Distance(transform.position, zoneInteractable.targetPoint.position) <= zoneInteractable.catchRadius) 
            {
                consumed = true;
                events?.Invoke();
            }
        }
    }
}
