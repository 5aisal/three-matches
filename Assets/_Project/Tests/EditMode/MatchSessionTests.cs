using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class MatchSessionTests
    {
        RescueState state;
        MatchSession session;

        [SetUp]
        public void SetUp()
        {
            state = new RescueState();
            session = new MatchSession(40f, 8f);
        }

        [Test]
        public void CannotStrikeWithoutMatches()
        {
            Assert.IsFalse(session.CanStrike(state, false));
            Assert.AreEqual(0, session.Strike(state, false));
            Assert.AreEqual(0, state.MatchesUsed);
        }

        [Test]
        public void StrikeEntersChoosing()
        {
            Assert.AreEqual(1, session.Strike(state, true));
            Assert.AreEqual(MatchPhase.Choosing, session.Phase);
            Assert.AreEqual(1, state.MatchesUsed);
        }

        [Test]
        public void CannotStrikeWhileSessionActive()
        {
            session.Strike(state, true);
            Assert.AreEqual(0, session.Strike(state, true));
            Assert.AreEqual(1, state.MatchesUsed);
        }

        [Test]
        public void BurnedMemoryCannotBePickedAgain()
        {
            session.Strike(state, true);
            Assert.IsTrue(session.BeginBurn(state, MemoryId.Closet));
            session.FinishBurn();
            session.BeginWindow();
            while (!session.Tick(1f)) { }

            session.Strike(state, true);
            Assert.IsFalse(session.BeginBurn(state, MemoryId.Closet));
            Assert.AreEqual(MatchPhase.Choosing, session.Phase);
            Assert.IsTrue(session.BeginBurn(state, MemoryId.Song));
        }

        [Test]
        public void CannotBurnOutsideChoosing()
        {
            Assert.IsFalse(session.BeginBurn(state, MemoryId.Closet));
            Assert.IsFalse(state.IsBurned(MemoryId.Closet));
        }

        [Test]
        public void CrackStagesFollowMatchNumber()
        {
            session.Strike(state, true);
            session.BeginBurn(state, MemoryId.Closet);
            Assert.AreEqual(2, session.FinishBurn());
            session.BeginWindow();
            while (!session.Tick(5f)) { }

            session.Strike(state, true);
            session.BeginBurn(state, MemoryId.Song);
            Assert.AreEqual(3, session.FinishBurn());
        }

        [Test]
        public void WindowRunsOutAfterFortySeconds()
        {
            session.Strike(state, true);
            session.BeginBurn(state, MemoryId.Closet);
            session.FinishBurn();
            session.BeginWindow();
            Assert.IsFalse(session.Tick(39.9f));
            Assert.IsTrue(session.IsWindowOpen);
            Assert.IsTrue(session.Tick(0.2f));
            Assert.AreEqual(MatchPhase.Idle, session.Phase);
            Assert.AreEqual(0f, session.Remaining);
        }

        [Test]
        public void FlickersInLastEightSeconds()
        {
            session.ForceWindow(1, false);
            session.Tick(31f);
            Assert.IsFalse(session.IsFlickering);
            session.Tick(2f);
            Assert.IsTrue(session.IsFlickering);
        }

        [Test]
        public void ThirdMatchOnlyReportsFinalAndNeverOpensWindow()
        {
            for (int i = 0; i < 2; i++)
            {
                session.Strike(state, true);
                session.BeginBurn(state, (MemoryId)i);
                session.FinishBurn();
                session.BeginWindow();
                while (!session.Tick(5f)) { }
            }

            Assert.AreEqual(MatchSession.FinalMatchNumber, session.Strike(state, true));
            Assert.AreEqual(MatchPhase.Idle, session.Phase);
            Assert.IsFalse(session.CanStrike(state, true));
        }

        [Test]
        public void SandboxNeverRunsOut()
        {
            session.ForceWindow(0, true);
            Assert.IsFalse(session.Tick(500f));
            Assert.AreEqual(1f, session.Fraction);
            Assert.IsFalse(session.IsFlickering);
        }
    }
}
