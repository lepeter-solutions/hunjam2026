using UnityEngine;

namespace HunJam
{
    public sealed class DemoGame : MonoBehaviour
    {
        [SerializeField] private EnergyCell[] cells;
        [SerializeField] private TextMesh scoreboard;
        public EnergyRound Round { get; private set; }
        public void Configure(EnergyCell[] items, TextMesh board) { cells = items; scoreboard = board; }
        private void Awake()
        {
            Round = new EnergyRound(cells.Length);
            UpdateBoard();
        }
        public void Begin()
        {
            foreach (var cell in cells) cell.ResetCell();
            Round.Begin();
            UpdateBoard();
        }
        public void Deliver(EnergyCell cell)
        {
            if (cell.IsHeld || System.Array.IndexOf(cells, cell) < 0 || !Round.Deliver(cell.GetInstanceID())) return;
            cell.gameObject.SetActive(false);
            UpdateBoard();
        }
        private void UpdateBoard()
        {
            scoreboard.text = Round.State == EnergyRound.Phase.Waiting
                ? "ENERGY STATION\nTalk to the robot to begin"
                : Round.State == EnergyRound.Phase.Complete
                    ? "POWER RESTORED!\n3 / 3  -  Talk to robot to replay"
                    : $"ENERGY STATION\n{Round.Count} / {Round.Target}\nRelease the orbs inside";
        }
    }
}
