using System;

namespace ThreeMatches.Core
{
    /// <summary>Small float helpers that turn gameplay movement into Animator parameters (GAME_SPEC §24).</summary>
    public static class LocomotionMath
    {
        /// <summary>World speed divided by the character's scale, so one blend tree fits every body size.</summary>
        public static float NormalizedSpeed(float worldSpeed, float characterScale)
        {
            return characterScale > 0.0001f ? worldSpeed / characterScale : 0f;
        }

        /// <summary>Frame-rate independent exponential smoothing toward a target.</summary>
        public static float Smooth(float current, float target, float dt, float smoothTime)
        {
            if (smoothTime <= 0f) return target;
            float t = 1f - (float)Math.Exp(-dt / smoothTime);
            return current + (target - current) * t;
        }

        /// <summary>Yaw (degrees) that makes a +Z facing model look along +X (right) or -X (left).</summary>
        public static float FacingYaw(bool facingRight) => facingRight ? 90f : -90f;

        /// <summary>Turns an angle toward a target at a maximum rate, taking the short way round.</summary>
        public static float TurnYaw(float current, float target, float degreesPerSecond, float dt)
        {
            float delta = target - current;
            while (delta > 180f) delta -= 360f;
            while (delta < -180f) delta += 360f;
            float step = degreesPerSecond * dt;
            if (Math.Abs(delta) <= step) return target;
            return current + Math.Sign(delta) * step;
        }
    }

    /// <summary>
    /// Rafi starts the game seated. He stands up the first time he is free to move (or starts moving),
    /// and stays standing afterwards.
    /// </summary>
    public sealed class SeatedIntro
    {
        private readonly float moveThreshold;

        public SeatedIntro(float moveThreshold = 0.1f)
        {
            this.moveThreshold = moveThreshold;
            Seated = true;
        }

        public bool Seated { get; private set; }

        /// <summary>Returns true on the one frame the stand-up should start.</summary>
        public bool Step(bool inputLocked, float speed)
        {
            if (!Seated) return false;
            if (inputLocked && speed <= moveThreshold) return false;
            Seated = false;
            return true;
        }

        public void Reset() => Seated = true;
    }

    public enum NoorPose { Hidden, Scared, Standing, Sitting }

    /// <summary>Which body language Noor shows for each state of GAME_SPEC §12.</summary>
    public static class NoorPoseRules
    {
        /// <param name="moving">True while she is actually travelling (run or walk blend takes over).</param>
        public static NoorPose Choose(NoorState state, bool moving)
        {
            switch (state)
            {
                case NoorState.Hidden: return NoorPose.Hidden;
                case NoorState.Out: return moving ? NoorPose.Standing : NoorPose.Scared;
                case NoorState.Panicked: return moving ? NoorPose.Standing : NoorPose.Scared;
                case NoorState.Following: return NoorPose.Standing;
                case NoorState.AtWindow: return NoorPose.Sitting;
                default: return NoorPose.Standing;
            }
        }
    }

    /// <summary>Decides when smoke makes Noor cough: only at fire level 2+, at random intervals, never while moving.</summary>
    public sealed class CoughTimer
    {
        public const int MinFireLevel = 2;

        private readonly Func<float> random01;
        private readonly float minInterval;
        private readonly float maxInterval;
        private float untilNext;

        public CoughTimer(Func<float> random01, float minInterval, float maxInterval)
        {
            this.random01 = random01;
            this.minInterval = minInterval;
            this.maxInterval = Math.Max(minInterval, maxInterval);
            untilNext = NextInterval();
        }

        /// <summary>Returns true on the frame a cough should start.</summary>
        public bool Step(float dt, bool canCough, int fireLevel)
        {
            if (fireLevel < MinFireLevel || !canCough)
            {
                untilNext = Math.Max(untilNext, 0.5f);
                return false;
            }

            untilNext -= dt;
            if (untilNext > 0f) return false;
            untilNext = NextInterval();
            return true;
        }

        private float NextInterval() => minInterval + (maxInterval - minInterval) * random01();
    }
}
