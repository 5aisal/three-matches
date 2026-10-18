using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class AtmosphereRulesTests
    {
        [Test]
        public void FutureWarmthRisesWithEachStep()
        {
            Assert.AreEqual(0f, AtmosphereRules.FutureWarmth(0, 3));
            Assert.AreEqual(1f / 3f, AtmosphereRules.FutureWarmth(1, 3), 1e-5f);
            Assert.AreEqual(1f, AtmosphereRules.FutureWarmth(3, 3));
        }

        [Test]
        public void FutureWarmthIsClamped()
        {
            Assert.AreEqual(0f, AtmosphereRules.FutureWarmth(-1, 3));
            Assert.AreEqual(1f, AtmosphereRules.FutureWarmth(5, 3));
            Assert.AreEqual(0f, AtmosphereRules.FutureWarmth(2, 0));
        }

        [Test]
        public void FireTakeoverGoesFromAmberRoomToFullFire()
        {
            Assert.AreEqual(0f, AtmosphereRules.FireTakeover(0f));
            Assert.AreEqual(1f, AtmosphereRules.FireTakeover(FireBlend.MaxLevel));
        }
    }
}
