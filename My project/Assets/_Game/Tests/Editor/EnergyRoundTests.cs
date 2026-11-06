using NUnit.Framework;

namespace HunJam.Tests
{
    public sealed class EnergyRoundTests
    {
        [Test] public void DeliveryRequiresStartedRound()
        {
            var round = new EnergyRound(3);
            Assert.IsFalse(round.Deliver(1));
            Assert.AreEqual(0, round.Count);
        }
        [Test] public void RepeatedPhysicsCallbacksCannotCountSameOrbTwice()
        {
            var round = new EnergyRound(3);
            round.Begin();
            Assert.IsTrue(round.Deliver(7));
            Assert.IsFalse(round.Deliver(7));
            Assert.AreEqual(1, round.Count);
        }
        [Test] public void CompletionRejectsFurtherDeliveryAndRestartClearsIds()
        {
            var round = new EnergyRound(3);
            round.Begin();
            round.Deliver(1); round.Deliver(2); round.Deliver(3);
            Assert.AreEqual(EnergyRound.Phase.Complete, round.State);
            Assert.IsFalse(round.Deliver(4));
            round.Begin();
            Assert.AreEqual(0, round.Count);
            Assert.AreEqual(EnergyRound.Phase.Playing, round.State);
            Assert.IsTrue(round.Deliver(1));
        }
    }
}
