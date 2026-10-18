using System;

namespace ThreeMatches.Core
{
    /// <summary>Smooth fire-level maths (GAME_SPEC §19: fire level changes blend over 1.5 s). Plain C# so it can be tested.</summary>
    public static class FireBlend
    {
        public const int MaxLevel = 3;

        /// <summary>Moves a blended level toward the target at 1 level per <paramref name="blendSeconds"/>.</summary>
        public static float Step(float current, int targetLevel, float blendSeconds, float deltaTime)
        {
            if (blendSeconds <= 0f) return targetLevel;
            float maxMove = deltaTime / blendSeconds;
            float diff = targetLevel - current;
            if (Math.Abs(diff) <= maxMove) return targetLevel;
            return current + Math.Sign(diff) * maxMove;
        }

        /// <summary>Burn damage (0-1) for a blended fire level: 0 at level 0, 1 at <see cref="MaxLevel"/>.</summary>
        public static float CharFromLevel(float level)
        {
            if (level <= 0f) return 0f;
            if (level >= MaxLevel) return 1f;
            return level / MaxLevel;
        }

        /// <summary>Linear interpolation across a per-level table (index 0..MaxLevel).</summary>
        public static float Sample(float[] perLevel, float level)
        {
            if (perLevel == null || perLevel.Length == 0) return 0f;
            if (level <= 0f) return perLevel[0];
            int last = Math.Min(perLevel.Length - 1, MaxLevel);
            if (level >= last) return perLevel[last];
            int lo = (int)level;
            float t = level - lo;
            return perLevel[lo] + (perLevel[lo + 1] - perLevel[lo]) * t;
        }
    }
}
