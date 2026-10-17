using NUnit.Framework;
using ThreeMatches.Strips;
using UnityEngine;

namespace ThreeMatches.Tests.EditMode
{
    public class StripLayoutCalculatorTests
    {
        private const float Two = 0.62f;
        private const float Three = 0.5f;
        private const float Eps = 0.0001f;

        private static Rect[] Compute(int stage, StripId active)
        {
            var rects = new Rect[StripLayoutCalculator.StripCount];
            StripLayoutCalculator.Compute(stage, active, Two, Three, rects);
            return rects;
        }

        [Test]
        public void Stage1_PresentFillsScreen()
        {
            var r = Compute(1, StripId.Present);
            Assert.AreEqual(1f, r[(int)StripId.Present].height, Eps);
            Assert.AreEqual(0f, r[(int)StripId.Present].y, Eps);
            Assert.AreEqual(0f, r[(int)StripId.Past].height, Eps);
            Assert.AreEqual(0f, r[(int)StripId.Future].height, Eps);
        }

        [Test]
        public void Stage2_PresentActive_Is38Past62Present_PastOnTop()
        {
            var r = Compute(2, StripId.Present);
            Assert.AreEqual(0.38f, r[(int)StripId.Past].height, Eps);
            Assert.AreEqual(0.62f, r[(int)StripId.Present].height, Eps);
            Assert.AreEqual(1f, r[(int)StripId.Past].yMax, Eps);
            Assert.AreEqual(r[(int)StripId.Present].yMax, r[(int)StripId.Past].yMin, Eps);
            Assert.AreEqual(0f, r[(int)StripId.Present].yMin, Eps);
        }

        [Test]
        public void Stage2_PastActive_Is62Past38Present()
        {
            var r = Compute(2, StripId.Past);
            Assert.AreEqual(0.62f, r[(int)StripId.Past].height, Eps);
            Assert.AreEqual(0.38f, r[(int)StripId.Present].height, Eps);
        }

        [TestCase(StripId.Past, 0.5f, 0.25f, 0.25f)]
        [TestCase(StripId.Present, 0.25f, 0.5f, 0.25f)]
        [TestCase(StripId.Future, 0.25f, 0.25f, 0.5f)]
        public void Stage3_ActiveStripGetsHalf(StripId active, float past, float present, float future)
        {
            var r = Compute(3, active);
            Assert.AreEqual(past, r[(int)StripId.Past].height, Eps);
            Assert.AreEqual(present, r[(int)StripId.Present].height, Eps);
            Assert.AreEqual(future, r[(int)StripId.Future].height, Eps);
            Assert.AreEqual(1f, r[(int)StripId.Past].yMax, Eps);
            Assert.AreEqual(r[(int)StripId.Present].yMax, r[(int)StripId.Past].yMin, Eps);
            Assert.AreEqual(r[(int)StripId.Future].yMax, r[(int)StripId.Present].yMin, Eps);
            Assert.AreEqual(0f, r[(int)StripId.Future].yMin, Eps);
        }

        [Test]
        public void InvalidActiveStrip_FallsBackToPresent()
        {
            Assert.AreEqual(StripId.Present, StripLayoutCalculator.SanitizeActive(1, StripId.Past));
            Assert.AreEqual(StripId.Present, StripLayoutCalculator.SanitizeActive(2, StripId.Future));
            Assert.AreEqual(StripId.Future, StripLayoutCalculator.SanitizeActive(3, StripId.Future));
        }

        [Test]
        public void NextActive_CyclesOnlyVisibleStrips()
        {
            Assert.AreEqual(StripId.Present, StripLayoutCalculator.NextActive(1, StripId.Present));
            Assert.AreEqual(StripId.Past, StripLayoutCalculator.NextActive(2, StripId.Present));
            Assert.AreEqual(StripId.Present, StripLayoutCalculator.NextActive(2, StripId.Past));
            Assert.AreEqual(StripId.Future, StripLayoutCalculator.NextActive(3, StripId.Present));
            Assert.AreEqual(StripId.Past, StripLayoutCalculator.NextActive(3, StripId.Future));
        }

        [Test]
        public void TryGetNewStrip_FindsStripAddedByStageChange()
        {
            Assert.IsTrue(StripLayoutCalculator.TryGetNewStrip(1, 2, out var s2));
            Assert.AreEqual(StripId.Past, s2);
            Assert.IsTrue(StripLayoutCalculator.TryGetNewStrip(2, 3, out var s3));
            Assert.AreEqual(StripId.Future, s3);
            Assert.IsFalse(StripLayoutCalculator.TryGetNewStrip(3, 2, out _));
        }
    }
}
