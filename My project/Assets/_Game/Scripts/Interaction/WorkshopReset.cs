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
            foreach (var paper in FindObjectsByType<Tearable>(FindObjectsSortMode.None)) paper.ResetPaper();
            foreach (var item in FindObjectsByType<Grabbable>(FindObjectsSortMode.None))
                if (!(item is EnergyCell) && !(item is Tool) && item.GetComponent<PaperFragment>() == null) item.ResetObject();
        }
    }
}
