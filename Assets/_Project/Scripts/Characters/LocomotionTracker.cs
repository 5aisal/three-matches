using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Characters
{
    /// <summary>
    /// Turns a character's per-frame movement into a smoothed speed for the Animator's blend tree. Gameplay
    /// moves the transform; the animation follows, so nothing here feeds back into gameplay.
    /// </summary>
    public sealed class LocomotionTracker
    {
        private readonly float smoothTime;
        private readonly float teleportDistance;
        private Vector2 last;
        private bool hasLast;

        public LocomotionTracker(float smoothTime, float teleportDistance)
        {
            this.smoothTime = smoothTime;
            this.teleportDistance = teleportDistance;
        }

        /// <summary>Smoothed speed in m/s along the screen plane (x, y).</summary>
        public float Speed { get; private set; }

        /// <summary>Smoothed speed in m/s along x only (a walker that only slides up a wall is not walking).</summary>
        public float HorizontalSpeed { get; private set; }

        /// <summary>Signed vertical speed in m/s of the last step (not smoothed).</summary>
        public float VerticalSpeed { get; private set; }

        public void Step(Vector2 position, float dt)
        {
            if (!hasLast || dt <= 0f)
            {
                last = position;
                hasLast = true;
                return;
            }

            Vector2 delta = position - last;
            last = position;

            // A jump (debug key, restored state) is a teleport, not a sprint.
            bool teleported = delta.magnitude > teleportDistance;
            float raw = teleported ? 0f : delta.magnitude / dt;
            float rawX = teleported ? 0f : Mathf.Abs(delta.x) / dt;
            VerticalSpeed = teleported ? 0f : delta.y / dt;
            Speed = LocomotionMath.Smooth(Speed, raw, dt, smoothTime);
            HorizontalSpeed = LocomotionMath.Smooth(HorizontalSpeed, rawX, dt, smoothTime);
        }
    }
}
