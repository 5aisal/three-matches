using UnityEngine;

namespace ThreeMatches.Strips
{
    /// <summary>
    /// Pure layout rules (GAME_SPEC §4). Produces normalized viewport rects, Past top, Present
    /// middle, Future bottom. Hidden strips get a zero-height rect on their screen edge so a
    /// lerp from hidden to visible grows the strip out of that edge.
    /// </summary>
    public static class StripLayoutCalculator
    {
        public const int StripCount = 3;
        public const int MinStage = 1;
        public const int MaxStage = 3;

        public static int ClampStage(int stage) => Mathf.Clamp(stage, MinStage, MaxStage);

        public static bool IsVisible(int stage, StripId strip)
        {
            switch (ClampStage(stage))
            {
                case 1: return strip == StripId.Present;
                case 2: return strip != StripId.Future;
                default: return true;
            }
        }

        /// <summary>Returns <paramref name="active"/> if that strip exists at this stage, else Present.</summary>
        public static StripId SanitizeActive(int stage, StripId active)
            => IsVisible(stage, active) ? active : StripId.Present;

        /// <summary>Next visible strip after <paramref name="active"/>, wrapping Past, Present, Future.</summary>
        public static StripId NextActive(int stage, StripId active)
        {
            for (int i = 1; i <= StripCount; i++)
            {
                var candidate = (StripId)(((int)active + i) % StripCount);
                if (IsVisible(stage, candidate)) return candidate;
            }
            return StripId.Present;
        }

        /// <summary>The first strip that is visible at <paramref name="stage"/> but not at <paramref name="previousStage"/>.</summary>
        public static bool TryGetNewStrip(int previousStage, int stage, out StripId strip)
        {
            for (int i = 0; i < StripCount; i++)
            {
                var candidate = (StripId)i;
                if (IsVisible(stage, candidate) && !IsVisible(previousStage, candidate))
                {
                    strip = candidate;
                    return true;
                }
            }
            strip = StripId.Present;
            return false;
        }

        public static void Compute(int stage, StripId active, float twoStripActiveFraction,
            float threeStripActiveFraction, Rect[] result)
        {
            stage = ClampStage(stage);
            active = SanitizeActive(stage, active);

            float past = 0f, present = 1f, future = 0f;
            if (stage == 2)
            {
                bool pastActive = active == StripId.Past;
                past = pastActive ? twoStripActiveFraction : 1f - twoStripActiveFraction;
                present = 1f - past;
            }
            else if (stage == 3)
            {
                float other = (1f - threeStripActiveFraction) * 0.5f;
                past = active == StripId.Past ? threeStripActiveFraction : other;
                present = active == StripId.Present ? threeStripActiveFraction : other;
                future = active == StripId.Future ? threeStripActiveFraction : other;
            }

            float top = 1f;
            result[(int)StripId.Past] = past > 0f ? new Rect(0f, top - past, 1f, past) : new Rect(0f, 1f, 1f, 0f);
            top -= past;
            result[(int)StripId.Present] = new Rect(0f, top - present, 1f, present);
            top -= present;
            result[(int)StripId.Future] = future > 0f ? new Rect(0f, top - future, 1f, future) : new Rect(0f, 0f, 1f, 0f);
        }
    }
}
