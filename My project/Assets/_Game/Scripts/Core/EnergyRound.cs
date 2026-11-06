using System.Collections.Generic;

namespace HunJam
{
    // Pure game state: independent of VR input, physics and scene presentation.
    public sealed class EnergyRound
    {
        public enum Phase { Waiting, Playing, Complete }
        private readonly HashSet<int> delivered = new HashSet<int>();
        public Phase State { get; private set; }
        public int Target { get; }
        public int Count => delivered.Count;

        public EnergyRound(int target) => Target = System.Math.Max(1, target);
        public void Begin() { delivered.Clear(); State = Phase.Playing; }
        public bool Deliver(int id)
        {
            if (State != Phase.Playing || !delivered.Add(id)) return false;
            if (Count >= Target) State = Phase.Complete;
            return true;
        }
    }
}
