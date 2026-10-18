namespace ThreeMatches.Core
{
    /// <summary>Plain maths behind the per-timeline lighting (GAME_SPEC §19). Testable without Unity objects.</summary>
    public static class AtmosphereRules
    {
        /// <summary>Future warmth 0-1: grey morning at 0 steps done, fully warmed at all steps done.</summary>
        public static float FutureWarmth(int stepsDone, int totalSteps)
        {
            if (totalSteps <= 0 || stepsDone <= 0) return 0f;
            if (stepsDone >= totalSteps) return 1f;
            return (float)stepsDone / totalSteps;
        }

        /// <summary>
        /// How far the fire has taken over the Past room light (0 = warm amber room, 1 = fire only),
        /// from the highest blended fire level of any room.
        /// </summary>
        public static float FireTakeover(float highestBlendedLevel) => FireBlend.CharFromLevel(highestBlendedLevel);
    }
}
