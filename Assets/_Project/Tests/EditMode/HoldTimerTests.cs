using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class HoldTimerTests
    {
        [Test]
        public void CompletesAfterTheFullHold()
        {
            var t = new HoldTimer(2f);
            Assert.IsFalse(t.Tick(true, 1f));
            Assert.AreEqual(0.5f, t.Progress, 1e-4f);
            Assert.IsTrue(t.Tick(true, 1f));
            Assert.AreEqual(1f, t.Progress, 1e-4f);
        }

        [Test]
        public void ReleasingResetsProgress()
        {
            var t = new HoldTimer(2f);
            t.Tick(true, 1.5f);
            t.Tick(false, 0.1f);
            Assert.AreEqual(0f, t.Progress, 1e-4f);
            Assert.IsFalse(t.Tick(true, 1.5f));
        }

        [Test]
        public void CompletesOnlyOncePerHold()
        {
            var t = new HoldTimer(1f);
            Assert.IsTrue(t.Tick(true, 1.2f));
            Assert.IsFalse(t.Tick(true, 0.5f));
        }
    }
}
