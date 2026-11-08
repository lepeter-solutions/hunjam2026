using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace HunJam
{
    public class PillDetectInMouth : MonoBehaviour
    {

        public ZoneInteractable zoneInteractable;
        public UnityEvent events;
        public void CheckConditions()
        {
            if (zoneInteractable.isSnappedToCenter)
            {
                WaitForSeconds(3);
                events?.Invoke();
            }
        }

        private IEnumerable WaitForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
    }
}
