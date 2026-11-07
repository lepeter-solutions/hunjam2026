using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

namespace HunJam
{
    // Maps an SDK selector event to game actions; contains no tracking or selection algorithm.
    public sealed class MetaUseAction : MonoBehaviour
    {
        [SerializeField] private ControllerSelector trigger;
        [SerializeField] private GrabInteractor[] grabs;
        [SerializeField] private HandGrabInteractor[] handGrabs;
        public void Configure(ControllerSelector selector, GrabInteractor[] controllers, HandGrabInteractor[] hands)
        { trigger = selector; grabs = controllers; handGrabs = hands; }
        private void OnEnable() => trigger.WhenSelected += Use;
        private void OnDisable() => trigger.WhenSelected -= Use;
        private void Use()
        {
            foreach (var grab in grabs)
                if (grab.isActiveAndEnabled && grab.State == InteractorState.Select && grab.Interactable != null && TryUse(grab.Interactable.gameObject, true)) return;
            foreach (var grab in handGrabs)
                if (grab.isActiveAndEnabled && grab.State == InteractorState.Select && grab.Interactable != null && TryUse(grab.Interactable.gameObject, true)) return;
            foreach (var grab in grabs)
                if (grab.isActiveAndEnabled && grab.Interactable != null && TryUse(grab.Interactable.gameObject, false)) return;
            foreach (var grab in handGrabs)
                if (grab.isActiveAndEnabled && grab.Interactable != null && TryUse(grab.Interactable.gameObject, false)) return;
        }
        private static bool TryUse(GameObject target, bool selected)
        {
            var item = target.GetComponentInParent<GameItem>();
            if (selected && item != null && item.IsHeld) { item.Use(); return true; }
            var action = target.GetComponentInParent<DemoInteractable>();
            if (action == null) return false;
            action.Interact();
            return true;
        }
    }
}
