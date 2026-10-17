using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class RescueStateTests
    {
        [Test]
        public void Burn_WorksOnce_ThenRefused()
        {
            var s = new RescueState();
            Assert.IsTrue(s.CanBurn(MemoryId.Closet));
            Assert.IsTrue(s.Burn(MemoryId.Closet));
            Assert.IsFalse(s.CanBurn(MemoryId.Closet));
            Assert.IsFalse(s.Burn(MemoryId.Closet));
            Assert.AreEqual(1, s.BurnedMemories.Count);
        }

        [Test]
        public void CompleteStep_RaisesStepCompletedOnce()
        {
            var s = new RescueState();
            s.Burn(MemoryId.Closet);
            int raised = 0;
            s.StepCompleted += _ => raised++;
            s.CompleteStep(RescueStep.Find);
            s.CompleteStep(RescueStep.Find);
            s.CompleteStep(RescueStep.Calm); // memory not burned: refused, no event
            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Burn_PreservesOrder()
        {
            var s = new RescueState();
            s.Burn(MemoryId.Window);
            s.Burn(MemoryId.Closet);
            CollectionAssert.AreEqual(new[] { MemoryId.Window, MemoryId.Closet }, s.BurnedMemories);
        }

        [Test]
        public void CompleteStep_RefusedWhenMemoryNotBurned()
        {
            var s = new RescueState();
            Assert.IsFalse(s.CompleteStep(RescueStep.Find));
            Assert.IsFalse(s.IsStepCompleted(RescueStep.Find));
            s.Burn(MemoryId.Song); // Calm's memory, not Find's
            Assert.IsFalse(s.CompleteStep(RescueStep.Find));
            Assert.AreEqual(0, s.CompletedStepCount);
        }

        [Test]
        public void CompleteStep_SucceedsOnceWhenMemoryBurned()
        {
            var s = new RescueState();
            s.Burn(MemoryId.Closet);
            Assert.IsTrue(s.CompleteStep(RescueStep.Find));
            Assert.IsFalse(s.CompleteStep(RescueStep.Find));
            Assert.AreEqual(1, s.CompletedStepCount);
        }

        [Test]
        public void Candle_UnlocksNoStep()
        {
            Assert.IsNull(MemoryRules.StepFor(MemoryId.Candle));
        }

        [Test]
        public void SnapshotRestore_RoundTripsAndIsIsolated()
        {
            var s = new RescueState();
            s.Burn(MemoryId.Closet);
            s.Burn(MemoryId.Song);
            s.CompleteStep(RescueStep.Find);
            s.StartMatch();
            s.AdvancePastTime(40f);
            s.SetNoorState(NoorState.Out);
            var snap = s.TakeSnapshot();

            s.Burn(MemoryId.Window);
            s.CompleteStep(RescueStep.Calm);
            s.CompleteStep(RescueStep.Exit);
            s.StartMatch();
            s.AdvancePastTime(40f);
            s.SetNoorState(NoorState.AtWindow);

            s.Restore(snap);
            CollectionAssert.AreEqual(new[] { MemoryId.Closet, MemoryId.Song }, s.BurnedMemories);
            Assert.IsTrue(s.IsStepCompleted(RescueStep.Find));
            Assert.IsFalse(s.IsStepCompleted(RescueStep.Calm));
            Assert.IsFalse(s.IsStepCompleted(RescueStep.Exit));
            Assert.AreEqual(40f, s.PastTime);
            Assert.AreEqual(1, s.MatchesUsed);
            Assert.AreEqual(NoorState.Out, s.NoorState);

            s.Burn(MemoryId.Candle); // mutating after restore must not alter the snapshot
            Assert.AreEqual(2, snap.BurnedMemories.Length);
        }
    }
}
