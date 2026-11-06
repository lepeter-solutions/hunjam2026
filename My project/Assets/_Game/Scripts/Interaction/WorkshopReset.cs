using UnityEngine;
namespace HunJam
{
    public sealed class WorkshopReset : DemoInteractable
    {
        public override void Interact()
        {
            foreach (var target in FindObjectsByType<Cuttable>(FindObjectsSortMode.None)) target.ResetTarget();
            foreach (var target in FindObjectsByType<ImpactTarget>(FindObjectsSortMode.None)) target.ResetTarget();
            foreach (var tool in FindObjectsByType<Tool>(FindObjectsSortMode.None)) tool.ResetObject();
        }
    }
}
