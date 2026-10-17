using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class ShadowMoverTests
    {
        // Mirrors House_1996 upstairs: Landing -8.8..-3.2, Hallway -2.8..2.8, Bedroom 3.2..8.8 (borders at -3 and 3).
        private static ShadowMover Make(float startX)
        {
            return new ShadowMover(-8.8f, 8.8f, 3.3f, 5.0f, new[] { -3f, 3f }, 3f, startX, 3.3f);
        }

        private static readonly int[] Calm = { 0, 0, 0 };

        [Test]
        public void MovesAtThreeMetresPerSecond()
        {
            var s = Make(-8f);
            s.Step(1f, 0f, 1f, Calm, 3);
            Assert.AreEqual(-5f, s.X, 1e-4f);
            s.Step(0f, 1f, 0.2f, Calm, 3);
            Assert.AreEqual(3.9f, s.Y, 1e-4f);
        }

        [Test]
        public void DiagonalIsNotFaster()
        {
            var s = Make(0f);
            s.Step(1f, 1f, 0.5f, Calm, 3);
            float dx = s.X;
            float dy = s.Y - 3.3f;
            Assert.AreEqual(1.5f, (float)System.Math.Sqrt(dx * dx + dy * dy), 1e-3f);
        }

        [Test]
        public void StaysInsideTheWalls()
        {
            var s = Make(8.7f);
            s.Step(1f, 1f, 5f, Calm, 3);
            Assert.AreEqual(8.8f, s.X, 1e-4f);
            Assert.AreEqual(5.0f, s.Y, 1e-4f);
            s.Step(-1f, -1f, 20f, Calm, 3);
            Assert.AreEqual(-8.8f, s.X, 1e-4f);
            Assert.AreEqual(3.3f, s.Y, 1e-4f);
        }

        [Test]
        public void CrossesBetweenRooms_WhenFireIsBelowLevel3()
        {
            var s = Make(-2f);
            s.Step(1f, 0f, 3f, new[] { 0, 2, 2 }, 3);
            Assert.AreEqual(7f, s.X, 1e-4f);
            Assert.AreEqual(2, s.RoomAt(s.X));
        }

        [Test]
        public void LevelThreeFire_BlocksEnteringTheRoom()
        {
            var s = Make(2f);
            s.Step(1f, 0f, 1f, new[] { 0, 1, 3 }, 3);
            Assert.Less(s.X, 3f);
            Assert.AreEqual(1, s.RoomAt(s.X));

            var t = Make(-2f);
            t.Step(-1f, 0f, 1f, new[] { 3, 1, 0 }, 3);
            Assert.GreaterOrEqual(t.X, -3f);
            Assert.AreEqual(1, t.RoomAt(t.X));
        }

        [Test]
        public void ShadowInsideARoomThatCatchesFire_CanStillLeave()
        {
            var s = Make(5f);
            s.Step(-1f, 0f, 1f, new[] { 0, 1, 3 }, 3);
            Assert.AreEqual(2f, s.X, 1e-4f);
            Assert.AreEqual(1, s.RoomAt(s.X));
        }
    }
}
