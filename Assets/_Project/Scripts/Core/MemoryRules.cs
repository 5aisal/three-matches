namespace ThreeMatches.Core
{
    /// <summary>Memory to rescue step mapping (GAME_SPEC §8).</summary>
    public static class MemoryRules
    {
        public const int MemoryCount = 4;
        public const int StepCount = 3;

        /// <summary>Returns the step a memory unlocks, or null for Candle.</summary>
        public static RescueStep? StepFor(MemoryId memory)
        {
            switch (memory)
            {
                case MemoryId.Closet: return RescueStep.Find;
                case MemoryId.Song: return RescueStep.Calm;
                case MemoryId.Window: return RescueStep.Exit;
                default: return null;
            }
        }

        public static MemoryId MemoryFor(RescueStep step)
        {
            switch (step)
            {
                case RescueStep.Find: return MemoryId.Closet;
                case RescueStep.Calm: return MemoryId.Song;
                default: return MemoryId.Window;
            }
        }
    }
}
