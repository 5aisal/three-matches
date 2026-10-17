using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class NoorModelTests
    {
        const float Dt = 0.1f;

        // Mirrors House_1996 upstairs: x -8.8..8.8, window at 6.4.
        private static NoorModel Make(float noorX = 1f)
        {
            var m = new NoorModel(NoorTuning.Default, -8.8f, 8.8f, 6.4f);
            m.Reveal(noorX);
            return m;
        }

        private static NoorEvents Run(NoorModel m, float seconds, float shadowX, bool hum, bool window = false)
        {
            NoorEvents all = NoorEvents.None;
            for (float t = 0f; t < seconds - 1e-4f; t += Dt) all |= m.Step(Dt, shadowX, hum, window);
            return all;
        }

        [Test]
        public void IsHiddenUntilRevealed()
        {
            var m = new NoorModel(NoorTuning.Default, -8.8f, 8.8f, 6.4f);
            Assert.AreEqual(NoorState.Hidden, m.State);
            Assert.AreEqual(NoorEvents.None, Run(m, 1f, 1f, true));
            Assert.AreEqual(NoorState.Hidden, m.State);
            m.Reveal(1f);
            Assert.AreEqual(NoorState.Out, m.State);
        }

        [Test]
        public void ShadowComingWithin3mWithoutHummingPanics()
        {
            var m = Make(1f);
            Run(m, 0.3f, -6f, false);
            NoorEvents e = Run(m, 0.3f, -1.5f, false);
            Assert.IsTrue((e & NoorEvents.Panicked) != 0);
            Assert.AreEqual(NoorState.Panicked, m.State);
        }

        [Test]
        public void PanicRunsAwayAndFreezesThreeSeconds()
        {
            var m = Make(1f);
            Run(m, 0.2f, -6f, false);
            Run(m, 0.1f, -1.5f, false);
            Run(m, 1f, -1.5f, false);
            Assert.Greater(m.X, 1f);
            float frozenAt = m.X;
            Run(m, 2f, -1.5f, false);
            Assert.AreEqual(NoorState.Panicked, m.State);
            Assert.AreEqual(frozenAt, m.X, 1e-4f);
            Run(m, 2f, -1.5f, false);
            Assert.AreEqual(NoorState.Out, m.State);
        }

        [Test]
        public void ShadowStandingBesideHerAtRevealDoesNotPanic()
        {
            var m = Make(1f);
            Run(m, 2f, 0f, false);
            Assert.AreEqual(NoorState.Out, m.State);
        }

        [Test]
        public void HummingWithin4mStartsFollowing()
        {
            var m = Make(1f);
            Run(m, 0.2f, -6f, false);
            NoorEvents e = Run(m, 0.2f, -2.5f, true);
            Assert.IsTrue((e & NoorEvents.StartedFollowing) != 0);
            Assert.IsTrue((e & NoorEvents.FirstFollow) != 0);
            Assert.AreEqual(NoorState.Following, m.State);
        }

        [Test]
        public void HummingBeyond4mDoesNothing()
        {
            var m = Make(1f);
            Run(m, 1f, -4f, true);
            Assert.AreEqual(NoorState.Out, m.State);
        }

        [Test]
        public void HummingWithin3mFollowsInsteadOfPanicking()
        {
            var m = Make(1f);
            Run(m, 0.2f, -6f, false);
            NoorEvents e = Run(m, 0.3f, -1f, true);
            Assert.IsFalse((e & NoorEvents.Panicked) != 0);
            Assert.AreEqual(NoorState.Following, m.State);
        }

        [Test]
        public void FollowsAtOnePointFiveMetresPerSecond()
        {
            var m = Make(1f);
            Run(m, 0.1f, 4f, true);
            float x0 = m.X;
            Run(m, 1f, 8f, true);
            Assert.AreEqual(x0 + 1.5f, m.X, 1e-3f);
        }

        [Test]
        public void DoesNotWalkIntoTheShadow()
        {
            var m = Make(1f);
            Run(m, 10f, 3f, true);
            Assert.AreEqual(3f - NoorTuning.Default.FollowGap, m.X, 1e-3f);
        }

        [Test]
        public void StopsOneAndAHalfSecondsAfterHummingStops()
        {
            var m = Make(1f);
            Run(m, 0.2f, 4f, true);
            Run(m, 1.2f, 4f, false);
            Assert.AreEqual(NoorState.Following, m.State);
            Run(m, 0.5f, 4f, false);
            Assert.AreEqual(NoorState.Out, m.State);
            float stoppedAt = m.X;
            Run(m, 1f, 4f, false);
            Assert.AreEqual(stoppedAt, m.X, 1e-4f);
            Assert.Less(m.X, 4f);
        }

        [Test]
        public void ResumedHummingResetsTheStopTimer()
        {
            var m = Make(1f);
            Run(m, 0.2f, 4f, true);
            Run(m, 1.2f, 4f, false);
            Run(m, 0.1f, 4f, true);
            Run(m, 1.2f, 4f, false);
            Assert.AreEqual(NoorState.Following, m.State);
        }

        [Test]
        public void StoppingNearHerDoesNotMakeHerPanic()
        {
            var m = Make(1f);
            Run(m, 0.2f, 4f, true);
            Run(m, 12f, 4f, false);
            Assert.AreEqual(NoorState.Out, m.State);
        }

        [Test]
        public void FirstFollowIsReportedOnlyOnce()
        {
            var m = Make(1f);
            Run(m, 0.2f, 2f, true);
            Run(m, 2f, 2f, false);
            Assert.AreEqual(NoorState.Out, m.State);
            NoorEvents e = Run(m, 0.2f, 2f, true);
            Assert.IsTrue((e & NoorEvents.StartedFollowing) != 0);
            Assert.IsFalse((e & NoorEvents.FirstFollow) != 0);
        }

        [Test]
        public void ReachesTheOpenWindowOnlyWhenItIsOpen()
        {
            var shut = Make(5.5f);
            Run(shut, 0.2f, 6.4f, true, false);
            Run(shut, 3f, 6.4f, true, false);
            Assert.AreEqual(NoorState.Following, shut.State);

            var open = Make(5.5f);
            Run(open, 0.2f, 6.4f, true, true);
            Run(open, 3f, 6.4f, true, true);
            Assert.AreEqual(NoorState.AtWindow, open.State);
            Assert.AreEqual(6.4f, open.X, 1e-4f);
        }

        [Test]
        public void AtWindowStaysPut()
        {
            var m = Make(5.5f);
            Run(m, 3f, 6.4f, true, true);
            Run(m, 3f, -6f, true, true);
            Assert.AreEqual(NoorState.AtWindow, m.State);
            Assert.AreEqual(6.4f, m.X, 1e-4f);
        }

        [Test]
        public void StaysInsideTheUpperFloor()
        {
            var m = Make(8.5f);
            Run(m, 0.2f, 4f, false);
            Run(m, 0.1f, 6f, false);
            Run(m, 5f, 6f, false);
            Assert.LessOrEqual(m.X, 8.8f);
            Assert.GreaterOrEqual(m.X, -8.8f);
        }

        [Test]
        public void RestoreKeepsAtWindowAndCalmsOtherStates()
        {
            var m = new NoorModel(NoorTuning.Default, -8.8f, 8.8f, 6.4f);
            m.Restore(NoorState.AtWindow, 6.4f);
            Assert.AreEqual(NoorState.AtWindow, m.State);
            m.Restore(NoorState.Following, 2f);
            Assert.AreEqual(NoorState.Out, m.State);
            m.Restore(NoorState.Panicked, 2f);
            Assert.AreEqual(NoorState.Out, m.State);
            m.Restore(NoorState.Hidden, 2f);
            Assert.AreEqual(NoorState.Hidden, m.State);
        }
    }
}
