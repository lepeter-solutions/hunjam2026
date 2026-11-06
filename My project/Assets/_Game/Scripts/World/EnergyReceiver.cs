using UnityEngine;

namespace HunJam
{
    public sealed class EnergyReceiver : MonoBehaviour
    {
        [SerializeField] private DemoGame game;
        public void Configure(DemoGame value) => game = value;
        private void OnTriggerStay(Collider other)
        {
            var cell = other.GetComponentInParent<EnergyCell>();
            if (cell != null) game.Deliver(cell);
        }
    }
}
