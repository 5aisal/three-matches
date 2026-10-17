using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class WalkerModelTests
    {
        // Mirrors House_2026 (house-local): stairs rise to the left from x=-0.05 (y 0) to x=-2.85 (y 3.1).
        private static WalkerModel Make(float startX = 0f)
        {
            var layout = new WalkerLayout(-8.5f, 8.5f, 0f, 3.1f, -0.05f, -2.85f, 0.35f);
            return new WalkerModel(layout, 2f, startX);
        }

        [Test]
        public void Walks_AtConfiguredSpeed()
        {
            var w = Make(0f);
            w.Step(1f, 0f, 0.5f);
            Assert.AreEqual(1f, w.X, 1e-4f);
            Assert.IsTrue(w.FacingRight);
            w.Step(-1f, 0f, 1f);
            Assert.AreEqual(-1f, w.X, 1e-4f);
            Assert.IsFalse(w.FacingRight);
        }

        [Test]
        public void Walking_IsClampedToTheHouse()
        {
            var w = Make(8f);
            w.Step(1f, 0f, 10f);
            Assert.AreEqual(8.5f, w.X, 1e-4f);
            w.Step(-1f, 0f, 100f);
            Assert.AreEqual(-8.5f, w.X, 1e-4f);
        }

        [Test]
        public void Up_AwayFromStairs_DoesNothing()
        {
            var w = Make(4f);
            w.Step(0f, 1f, 1f);
            Assert.AreEqual(WalkerMode.Ground, w.Mode);
        }

        [Test]
        public void Up_AtStairBottom_Climbs_ToUpperFloor()
        {
            var w = Make(-0.05f);
            w.Step(0f, 1f, 0.01f);
            Assert.AreEqual(WalkerMode.Stairs, w.Mode);
            for (int i = 0; i < 200 && w.Mode == WalkerMode.Stairs; i++) w.Step(0f, 1f, 0.05f);
            Assert.AreEqual(WalkerMode.Upper, w.Mode);
            Assert.AreEqual(-2.85f, w.X, 1e-4f);
            Assert.AreEqual(3.1f, w.Y, 1e-4f);
        }

        [Test]
        public void Down_AtStairTop_Descends_ToGround()
        {
            var w = Make(-0.05f);
            while (w.Mode != WalkerMode.Upper) w.Step(0f, 1f, 0.05f);
            w.Step(0f, -1f, 0.01f);
            Assert.AreEqual(WalkerMode.Stairs, w.Mode);
            for (int i = 0; i < 200 && w.Mode == WalkerMode.Stairs; i++) w.Step(0f, -1f, 0.05f);
            Assert.AreEqual(WalkerMode.Ground, w.Mode);
            Assert.AreEqual(0f, w.Y, 1e-4f);
        }

        [Test]
        public void OnStairs_HeightRisesWithProgress_AndStopsWhenNoInput()
        {
            var w = Make(-0.05f);
            w.Step(0f, 1f, 0.01f);
            w.Step(0f, 1f, 0.7f); // 1.4 m along x, half of the 2.8 m run
            Assert.AreEqual(0.5f, w.StairProgress, 0.02f);
            Assert.AreEqual(1.55f, w.Y, 0.06f);
            float y = w.Y;
            w.Step(0f, 0f, 1f);
            Assert.AreEqual(y, w.Y, 1e-5f);
        }

        [Test]
        public void OnStairs_LeftKey_ClimbsBecauseStairsRiseLeft()
        {
            var w = Make(-0.05f);
            w.Step(0f, 1f, 0.01f);
            float before = w.StairProgress;
            w.Step(-1f, 0f, 0.2f);
            Assert.Greater(w.StairProgress, before);
            Assert.IsFalse(w.FacingRight);
        }

        [Test]
        public void CanClimb_ReportsEntryZone()
        {
            var w = Make(-0.05f);
            Assert.IsTrue(w.CanClimb);
            Assert.IsFalse(w.CanDescend);
            w.Step(1f, 0f, 1f);
            Assert.IsFalse(w.CanClimb);
        }
    }
}
