using NUnit.Framework;
using ThreeMatches.Core;

namespace ThreeMatches.Tests.EditMode
{
    public class CharacterMotionTests
    {
        [Test]
        public void NormalizedSpeedDividesByScale()
        {
            Assert.AreEqual(2.5f, LocomotionMath.NormalizedSpeed(1.8f, 0.72f), 1e-4f);
            Assert.AreEqual(0f, LocomotionMath.NormalizedSpeed(2f, 0f));
        }

        [Test]
        public void SmoothMovesTowardTargetWithoutOvershoot()
        {
            float v = 0f;
            for (int i = 0; i < 600; i++) v = LocomotionMath.Smooth(v, 2f, 0.016f, 0.1f);
            Assert.AreEqual(2f, v, 1e-3f);
            Assert.AreEqual(5f, LocomotionMath.Smooth(0f, 5f, 0.016f, 0f));
        }

        [Test]
        public void TurnYawTakesTheShortWay()
        {
            Assert.AreEqual(-80f, LocomotionMath.TurnYaw(-90f, 90f, 10f, 1f), 1e-3f);
            float next = LocomotionMath.TurnYaw(170f, -170f, 10f, 1f);
            Assert.AreEqual(180f, next, 1e-3f);
        }

        [Test]
        public void TurnYawSnapsWhenCloseEnough()
        {
            Assert.AreEqual(90f, LocomotionMath.TurnYaw(85f, 90f, 360f, 0.1f));
        }

        [Test]
        public void FacingYawPointsAlongTheWall()
        {
            Assert.AreEqual(90f, LocomotionMath.FacingYaw(true));
            Assert.AreEqual(-90f, LocomotionMath.FacingYaw(false));
        }

        [Test]
        public void SeatedIntroStandsOnceWhenUnlocked()
        {
            var intro = new SeatedIntro();
            Assert.IsFalse(intro.Step(true, 0f));
            Assert.IsTrue(intro.Seated);
            Assert.IsTrue(intro.Step(false, 0f));
            Assert.IsFalse(intro.Seated);
            Assert.IsFalse(intro.Step(false, 0f));
        }

        [Test]
        public void SeatedIntroStandsWhenMovedWhileLocked()
        {
            var intro = new SeatedIntro();
            Assert.IsTrue(intro.Step(true, 1f));
        }

        [Test]
        public void NoorPoseFollowsStateAndMotion()
        {
            Assert.AreEqual(NoorPose.Hidden, NoorPoseRules.Choose(NoorState.Hidden, false));
            Assert.AreEqual(NoorPose.Scared, NoorPoseRules.Choose(NoorState.Out, false));
            Assert.AreEqual(NoorPose.Standing, NoorPoseRules.Choose(NoorState.Panicked, true));
            Assert.AreEqual(NoorPose.Scared, NoorPoseRules.Choose(NoorState.Panicked, false));
            Assert.AreEqual(NoorPose.Standing, NoorPoseRules.Choose(NoorState.Following, false));
            Assert.AreEqual(NoorPose.Sitting, NoorPoseRules.Choose(NoorState.AtWindow, true));
        }

        [Test]
        public void CoughNeedsFireLevelTwoAndStillness()
        {
            var timer = new CoughTimer(() => 0f, 1f, 1f);
            Assert.IsFalse(timer.Step(5f, true, 1));
            Assert.IsFalse(timer.Step(5f, false, 3));

            var ready = new CoughTimer(() => 0f, 1f, 1f);
            Assert.IsFalse(ready.Step(0.5f, true, 2));
            Assert.IsTrue(ready.Step(0.6f, true, 2));
            Assert.IsFalse(ready.Step(0.1f, true, 2));
        }
    }
}
