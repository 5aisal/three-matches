using System;

namespace ThreeMatches.Core
{
    public enum WalkerMode
    {
        Ground,
        Upper,
        Stairs
    }

    /// <summary>Floor and stairs geometry for a walker, in any consistent 1D/height space.</summary>
    public readonly struct WalkerLayout
    {
        public readonly float MinX;
        public readonly float MaxX;
        public readonly float GroundY;
        public readonly float UpperY;
        public readonly float StairBottomX;
        public readonly float StairTopX;
        public readonly float StairEntryRadius;

        public WalkerLayout(float minX, float maxX, float groundY, float upperY,
            float stairBottomX, float stairTopX, float stairEntryRadius)
        {
            MinX = minX;
            MaxX = maxX;
            GroundY = groundY;
            UpperY = upperY;
            StairBottomX = stairBottomX;
            StairTopX = stairTopX;
            StairEntryRadius = stairEntryRadius;
        }
    }

    /// <summary>
    /// Side-on walking rules: left/right along a floor, W/S to take the stairs when standing at
    /// either end. No Unity types, so EditMode tests cover it.
    /// </summary>
    public sealed class WalkerModel
    {
        private const float VerticalThreshold = 0.5f;

        private readonly WalkerLayout layout;
        private readonly float speed;
        private readonly float stairLength;
        private readonly float stairDirX;
        private float stairProgress;

        public WalkerModel(WalkerLayout layout, float speed, float startX)
        {
            this.layout = layout;
            this.speed = speed;
            stairLength = Math.Abs(layout.StairTopX - layout.StairBottomX);
            stairDirX = layout.StairTopX >= layout.StairBottomX ? 1f : -1f;
            X = Clamp(startX, layout.MinX, layout.MaxX);
            Y = layout.GroundY;
            Mode = WalkerMode.Ground;
            FacingRight = true;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public WalkerMode Mode { get; private set; }
        public bool FacingRight { get; private set; }

        /// <summary>0 at the bottom of the stairs, 1 at the top. Only meaningful in Stairs mode.</summary>
        public float StairProgress => stairProgress;

        /// <summary>True when W would start climbing from the current spot.</summary>
        public bool CanClimb => Mode == WalkerMode.Ground
            && Math.Abs(X - layout.StairBottomX) <= layout.StairEntryRadius;

        /// <summary>True when S would start descending from the current spot.</summary>
        public bool CanDescend => Mode == WalkerMode.Upper
            && Math.Abs(X - layout.StairTopX) <= layout.StairEntryRadius;

        /// <param name="horizontal">-1 left .. +1 right.</param>
        /// <param name="vertical">-1 down (S) .. +1 up (W).</param>
        public void Step(float horizontal, float vertical, float dt)
        {
            if (Mode == WalkerMode.Stairs)
            {
                StepStairs(horizontal, vertical, dt);
                return;
            }

            if (horizontal > 0f) FacingRight = true;
            else if (horizontal < 0f) FacingRight = false;
            X = Clamp(X + horizontal * speed * dt, layout.MinX, layout.MaxX);

            if (vertical > VerticalThreshold && CanClimb) EnterStairs(0f);
            else if (vertical < -VerticalThreshold && CanDescend) EnterStairs(1f);
        }

        private void EnterStairs(float progress)
        {
            Mode = WalkerMode.Stairs;
            stairProgress = progress;
            ApplyStairPosition();
        }

        private void StepStairs(float horizontal, float vertical, float dt)
        {
            // W/S drive the climb; A/D also work, walking toward the side the stairs rise to.
            float climb = Math.Abs(vertical) > VerticalThreshold
                ? Math.Sign(vertical)
                : horizontal * stairDirX;
            if (climb == 0f || stairLength <= 0f) return;

            float dxDir = climb * stairDirX;
            if (dxDir > 0f) FacingRight = true;
            else if (dxDir < 0f) FacingRight = false;

            stairProgress += climb * speed * dt / stairLength;
            if (stairProgress >= 1f)
            {
                Mode = WalkerMode.Upper;
                X = layout.StairTopX;
                Y = layout.UpperY;
            }
            else if (stairProgress <= 0f)
            {
                Mode = WalkerMode.Ground;
                X = layout.StairBottomX;
                Y = layout.GroundY;
            }
            else
            {
                ApplyStairPosition();
            }
        }

        private void ApplyStairPosition()
        {
            X = layout.StairBottomX + (layout.StairTopX - layout.StairBottomX) * stairProgress;
            Y = layout.GroundY + (layout.UpperY - layout.GroundY) * stairProgress;
        }

        private static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);
    }
}
