using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class FireBlendTests
    {
        [Test]
        public void StepMovesAtOneLevelPerBlendTime()
        {
            float v = FireBlend.Step(0f, 3, 1.5f, 0.75f);
            Assert.AreEqual(0.5f, v, 1e-4f);
        }

        [Test]
        public void StepNeverOvershootsTarget()
        {
            Assert.AreEqual(2f, FireBlend.Step(1.9f, 2, 1.5f, 5f));
            Assert.AreEqual(1f, FireBlend.Step(1.1f, 1, 1.5f, 5f));
        }

        [Test]
        public void StepWithZeroDeltaStaysPut()
        {
            Assert.AreEqual(1.2f, FireBlend.Step(1.2f, 3, 1.5f, 0f), 1e-6f);
        }

        [Test]
        public void SampleInterpolatesBetweenLevels()
        {
            var table = new[] { 0f, 10f, 20f, 40f };
            Assert.AreEqual(15f, FireBlend.Sample(table, 1.5f), 1e-4f);
            Assert.AreEqual(0f, FireBlend.Sample(table, -1f));
            Assert.AreEqual(40f, FireBlend.Sample(table, 9f));
        }
    }
}
