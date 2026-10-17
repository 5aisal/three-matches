using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class FinalChoiceRulesTests
    {
        static RescueState Make(MemoryId[] burned, RescueStep[] steps)
        {
            var s = new RescueState();
            foreach (var m in burned) s.Burn(m);
            foreach (var st in steps) Assert.IsTrue(s.CompleteStep(st));
            return s;
        }

        [Test]
        public void ClosetSong_FindCalmDone()
        {
            var s = Make(new[] { MemoryId.Closet, MemoryId.Song }, new[] { RescueStep.Find, RescueStep.Calm });
            CollectionAssert.AreEqual(new[] { RescueStep.Exit }, FinalChoiceRules.MissingSteps(s));
            Assert.AreEqual(1, FinalChoiceRules.Cost(s));
            Assert.IsTrue(FinalChoiceRules.IsPresentAvailable(s));
            CollectionAssert.AreEqual(new[] { MemoryId.Window }, FinalChoiceRules.MemoriesBurnedByEndingB(s));
            CollectionAssert.AreEqual(new[] { MemoryId.Candle }, FinalChoiceRules.KeptAfter(s, EndingId.SaveHerForgetHer));
        }

        [Test]
        public void ClosetWindow_FindExitDone()
        {
            var s = Make(new[] { MemoryId.Closet, MemoryId.Window }, new[] { RescueStep.Find, RescueStep.Exit });
            CollectionAssert.AreEqual(new[] { RescueStep.Calm }, FinalChoiceRules.MissingSteps(s));
            Assert.AreEqual(1, FinalChoiceRules.Cost(s));
            Assert.IsTrue(FinalChoiceRules.IsPresentAvailable(s));
            CollectionAssert.AreEqual(new[] { MemoryId.Song }, FinalChoiceRules.MemoriesBurnedByEndingB(s));
            CollectionAssert.AreEqual(new[] { MemoryId.Candle }, FinalChoiceRules.KeptAfter(s, EndingId.SaveHerForgetHer));
        }

        [Test]
        public void SongWindow_ExitDone()
        {
            var s = Make(new[] { MemoryId.Song, MemoryId.Window }, new[] { RescueStep.Exit });
            CollectionAssert.AreEqual(new[] { RescueStep.Find, RescueStep.Calm }, FinalChoiceRules.MissingSteps(s));
            Assert.AreEqual(2, FinalChoiceRules.Cost(s));
            Assert.IsTrue(FinalChoiceRules.IsPresentAvailable(s));
            // Find's memory (Closet) first, Calm's (Song) is already burned, then Candle fills in.
            CollectionAssert.AreEqual(new[] { MemoryId.Closet, MemoryId.Candle }, FinalChoiceRules.MemoriesBurnedByEndingB(s));
            CollectionAssert.IsEmpty(FinalChoiceRules.KeptAfter(s, EndingId.SaveHerForgetHer));
        }

        [Test]
        public void CandleSong_NothingDone_PresentUnavailable()
        {
            var s = Make(new[] { MemoryId.Candle, MemoryId.Song }, new RescueStep[0]);
            Assert.AreEqual(3, FinalChoiceRules.Cost(s));
            Assert.IsFalse(FinalChoiceRules.IsPresentAvailable(s));
            CollectionAssert.IsEmpty(FinalChoiceRules.MemoriesBurnedByEndingB(s));
        }

        [Test]
        public void KeptAfter_AAndC_KeepEveryUnburnedMemory()
        {
            var s = Make(new[] { MemoryId.Closet, MemoryId.Song }, new[] { RescueStep.Find, RescueStep.Calm });
            var expected = new[] { MemoryId.Window, MemoryId.Candle };
            CollectionAssert.AreEqual(expected, FinalChoiceRules.KeptAfter(s, EndingId.WalkIntoFire));
            CollectionAssert.AreEqual(expected, FinalChoiceRules.KeptAfter(s, EndingId.KeepHer));
        }
    }
}
