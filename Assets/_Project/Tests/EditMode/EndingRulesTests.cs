using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class EndingRulesTests
    {
        static RescueState Make(MemoryId[] burned, RescueStep[] steps)
        {
            var s = new RescueState();
            foreach (var m in burned) s.Burn(m);
            foreach (var st in steps) Assert.IsTrue(s.CompleteStep(st));
            return s;
        }

        // Closet + Song burned, FIND + CALM done (the F4 state).
        static RescueState FindCalm() =>
            Make(new[] { MemoryId.Closet, MemoryId.Song }, new[] { RescueStep.Find, RescueStep.Calm });

        // Candle + Song burned, nothing done: the greyed-out Present case.
        static RescueState CandleSong() =>
            Make(new[] { MemoryId.Candle, MemoryId.Song }, new RescueStep[0]);

        [Test]
        public void TitlesMatchSpec()
        {
            Assert.AreEqual("She grew up. He didn't grow old.", EndingRules.TitleFor(EndingId.WalkIntoFire));
            Assert.AreEqual("She lived. He forgot.", EndingRules.TitleFor(EndingId.SaveHerForgetHer));
            Assert.AreEqual("He couldn't save her. He never forgot her.", EndingRules.TitleFor(EndingId.KeepHer));
        }

        [Test]
        public void AAndC_AreAlwaysAvailable_BOnlyWhenAffordable()
        {
            var poor = CandleSong();
            Assert.IsTrue(EndingRules.IsAvailable(poor, EndingId.WalkIntoFire));
            Assert.IsTrue(EndingRules.IsAvailable(poor, EndingId.KeepHer));
            Assert.IsFalse(EndingRules.IsAvailable(poor, EndingId.SaveHerForgetHer));
            Assert.IsNull(EndingRules.Build(poor, EndingId.SaveHerForgetHer));

            Assert.IsTrue(EndingRules.IsAvailable(FindCalm(), EndingId.SaveHerForgetHer));
        }

        [Test]
        public void A_SongBurned_AddsNoorSongVariant()
        {
            var plan = EndingRules.Build(FindCalm(), EndingId.WalkIntoFire);
            Assert.IsTrue(plan.SongBurnedBefore);
            Assert.AreEqual("You hummed my song.", EndingRules.NoorSongLineA);

            var noSong = Make(new[] { MemoryId.Closet, MemoryId.Window }, new[] { RescueStep.Find, RescueStep.Exit });
            Assert.IsFalse(EndingRules.Build(noSong, EndingId.WalkIntoFire).SongBurnedBefore);
        }

        [Test]
        public void A_ExitDone_CarriesThroughWindow_ElseFlames()
        {
            var withExit = Make(new[] { MemoryId.Closet, MemoryId.Window }, new[] { RescueStep.Find, RescueStep.Exit });
            Assert.IsTrue(EndingRules.Build(withExit, EndingId.WalkIntoFire).ExitDone);
            Assert.IsFalse(EndingRules.Build(FindCalm(), EndingId.WalkIntoFire).ExitDone);
        }

        [Test]
        public void B_BurnsMissingStepMemory_AndKeepsOne()
        {
            var plan = EndingRules.Build(FindCalm(), EndingId.SaveHerForgetHer);
            CollectionAssert.AreEqual(new[] { MemoryId.Window }, plan.BurnedNow);
            CollectionAssert.AreEqual(new[] { MemoryId.Candle }, plan.Kept);
            Assert.AreEqual(MemoryId.Candle, plan.KeptMemory);
            Assert.IsTrue(plan.SongBurnedBefore);
        }

        [Test]
        public void B_NothingKept_WhenEverythingIsBurned()
        {
            var s = Make(new[] { MemoryId.Song, MemoryId.Window }, new[] { RescueStep.Exit });
            var plan = EndingRules.Build(s, EndingId.SaveHerForgetHer);
            Assert.AreEqual(2, plan.BurnedNow.Count);
            Assert.IsNull(plan.KeptMemory);
        }

        [Test]
        public void B_SongBurnedByTheEnding_IsNotASongBeforeVariant()
        {
            // Closet + Window burned and done; CALM missing, so B burns the Song itself.
            var s = Make(new[] { MemoryId.Closet, MemoryId.Window }, new[] { RescueStep.Find, RescueStep.Exit });
            var plan = EndingRules.Build(s, EndingId.SaveHerForgetHer);
            CollectionAssert.AreEqual(new[] { MemoryId.Song }, plan.BurnedNow);
            Assert.IsFalse(plan.SongBurnedBefore);
        }

        [Test]
        public void C_KeepsEveryUnburnedMemory_AndBurnsNothing()
        {
            var plan = EndingRules.Build(FindCalm(), EndingId.KeepHer);
            CollectionAssert.IsEmpty(plan.BurnedNow);
            CollectionAssert.AreEqual(new[] { MemoryId.Window, MemoryId.Candle }, plan.Kept);
        }

        [Test]
        public void HoverText_UsesRealCost_Singular_Plural_AndGreyedOut()
        {
            Assert.AreEqual("Burn 1 more memory to save her.", FinalChoiceRules.PresentHover(FindCalm()));

            var songWindow = Make(new[] { MemoryId.Song, MemoryId.Window }, new[] { RescueStep.Exit });
            Assert.AreEqual("Burn 2 more memories to save her.", FinalChoiceRules.PresentHover(songWindow));

            Assert.AreEqual("Not enough left to give.", FinalChoiceRules.PresentHover(CandleSong()));

            Assert.AreEqual("Go yourself. You won't come back.", FinalChoiceRules.HoverText(FindCalm(), EndingId.WalkIntoFire));
            Assert.AreEqual("Blow it out. Keep what's left.", FinalChoiceRules.HoverText(FindCalm(), EndingId.KeepHer));
        }

        [Test]
        public void ChooseAgain_RestoresExactStateBeforeFinalMatch()
        {
            var s = FindCalm();
            s.StartMatch();
            s.StartMatch();
            var before = s.TakeSnapshot();

            s.StartMatch(); // final match
            foreach (var m in EndingRules.Build(s, EndingId.SaveHerForgetHer).BurnedNow) s.Burn(m);
            Assert.AreEqual(3, s.BurnedMemories.Count); // the ending really changed the state
            s.Restore(before);

            Assert.AreEqual(2, s.MatchesUsed);
            Assert.AreEqual(2, s.BurnedMemories.Count);
            Assert.IsTrue(s.IsStepCompleted(RescueStep.Find));
            Assert.IsTrue(s.IsStepCompleted(RescueStep.Calm));
            Assert.IsFalse(s.IsStepCompleted(RescueStep.Exit));
        }
    }

    public class GameFlowModelTests
    {
        [Test]
        public void FullRun_TitleToCredits()
        {
            var m = new GameFlowModel();
            Assert.AreEqual(GamePhase.Title, m.Phase);
            Assert.IsTrue(m.DismissTitle());
            Assert.IsTrue(m.MatchLit(1));
            Assert.AreEqual(GamePhase.Match1, m.Phase);
            Assert.IsTrue(m.MatchOut(1));
            Assert.AreEqual(GamePhase.Between, m.Phase);
            Assert.IsTrue(m.MatchLit(2));
            Assert.IsTrue(m.MatchOut(2));
            Assert.AreEqual(GamePhase.Final, m.Phase);
            Assert.IsTrue(m.FinalMatchLit());
            Assert.IsTrue(m.ChooseEnding(EndingId.KeepHer));
            Assert.AreEqual(EndingId.KeepHer, m.Ending);
            Assert.IsTrue(m.EndingFinished());
            Assert.IsTrue(m.ShowCredits());
            Assert.AreEqual(GamePhase.Credits, m.Phase);
            Assert.IsTrue(m.BackToEndCard());
        }

        [Test]
        public void OutOfOrderEvents_AreRefused()
        {
            var m = new GameFlowModel();
            Assert.IsFalse(m.MatchLit(1));
            Assert.IsFalse(m.FinalMatchLit());
            m.DismissTitle();
            Assert.IsFalse(m.MatchLit(2));
            Assert.IsFalse(m.ChooseEnding(EndingId.WalkIntoFire));
            Assert.IsFalse(m.ChooseAgain());
        }

        [Test]
        public void ChooseAgain_GoesBackToFinal()
        {
            var m = new GameFlowModel();
            m.DismissTitle();
            m.SyncToState(2, false);
            m.FinalMatchLit();
            m.ChooseEnding(EndingId.WalkIntoFire);
            m.EndingFinished();
            Assert.IsTrue(m.ChooseAgain());
            Assert.AreEqual(GamePhase.Final, m.Phase);
            Assert.IsNull(m.Ending);
            Assert.IsTrue(m.FinalMatchLit());
        }

        [Test]
        public void PhaseFor_MatchesDebugJumps()
        {
            Assert.AreEqual(GamePhase.Act1, GameFlowModel.PhaseFor(0, false));
            Assert.AreEqual(GamePhase.Match1, GameFlowModel.PhaseFor(1, true));    // F2
            Assert.AreEqual(GamePhase.Between, GameFlowModel.PhaseFor(1, false));
            Assert.AreEqual(GamePhase.Match2, GameFlowModel.PhaseFor(2, true));    // F3
            Assert.AreEqual(GamePhase.Final, GameFlowModel.PhaseFor(2, false));    // F4
            Assert.AreEqual(GamePhase.FinalChoice, GameFlowModel.PhaseFor(3, false));
        }
    }

    public class CreditsFormatterTests
    {
        [Test]
        public void Headings_Lists_Tables_AndLinks()
        {
            string md = "# Credits\n\n## Art\n| Asset | Author |\n|---|---|\n| Kit | Kenney |\n- item [site](https://x.y)\n";
            string rich = CreditsFormatter.ToRichText(md);

            StringAssert.Contains("<b>Credits</b>", rich);
            StringAssert.Contains("<b>Art</b>", rich);
            StringAssert.Contains("Kit  -  Kenney", rich);
            StringAssert.DoesNotContain("|---", rich);
            StringAssert.Contains("• item site (https://x.y)", rich);
        }
    }
}
