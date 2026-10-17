using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class FireTimelineTests
    {
        // t, bedroom, hallway, landing (GAME_SPEC §10 table, without Candle)
        [TestCase(0f, 1, 0, 0)]
        [TestCase(19.9f, 1, 0, 0)]
        [TestCase(20f, 3, 1, 0)]
        [TestCase(44.9f, 3, 1, 0)]
        [TestCase(45f, 3, 2, 0)]
        [TestCase(64.9f, 3, 2, 0)]
        [TestCase(65f, 3, 3, 1)]
        [TestCase(79.9f, 3, 3, 1)]
        [TestCase(80f, 3, 3, 2)]
        public void NoCandle_MatchesTable(float t, int bedroom, int hallway, int landing)
        {
            Assert.AreEqual(bedroom, FireTimeline.GetLevel(RoomId.Bedroom, t, false));
            Assert.AreEqual(hallway, FireTimeline.GetLevel(RoomId.Hallway, t, false));
            Assert.AreEqual(landing, FireTimeline.GetLevel(RoomId.Landing, t, false));
        }

        // Same rows with every time multiplied by 1.3: 26, 58.5, 84.5, 104
        [TestCase(0f, 1, 0, 0)]
        [TestCase(25.9f, 1, 0, 0)]
        [TestCase(26.1f, 3, 1, 0)]
        [TestCase(58.4f, 3, 1, 0)]
        [TestCase(58.6f, 3, 2, 0)]
        [TestCase(84.4f, 3, 2, 0)]
        [TestCase(84.6f, 3, 3, 1)]
        [TestCase(103.9f, 3, 3, 1)]
        [TestCase(104.1f, 3, 3, 2)]
        public void WithCandle_ThresholdsScaledBy1Point3(float t, int bedroom, int hallway, int landing)
        {
            Assert.AreEqual(bedroom, FireTimeline.GetLevel(RoomId.Bedroom, t, true));
            Assert.AreEqual(hallway, FireTimeline.GetLevel(RoomId.Hallway, t, true));
            Assert.AreEqual(landing, FireTimeline.GetLevel(RoomId.Landing, t, true));
        }

        [Test]
        public void Candle_SlowsFire()
        {
            Assert.AreEqual(1, FireTimeline.GetLevel(RoomId.Hallway, 22f, false) );
            Assert.AreEqual(2, FireTimeline.GetLevel(RoomId.Hallway, 50f, false));
            Assert.AreEqual(1, FireTimeline.GetLevel(RoomId.Hallway, 50f, true));
        }
    }
}
