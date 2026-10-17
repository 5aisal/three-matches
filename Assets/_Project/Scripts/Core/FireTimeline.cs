namespace ThreeMatches.Core
{
    /// <summary>Fire level (0-3) per room over past time (GAME_SPEC §10).</summary>
    public static class FireTimeline
    {
        /// <summary>All schedule times are multiplied by this when Candle is burned.</summary>
        public const float CandleSlowdown = 1.3f;

        public const float BedroomAblaze = 20f;
        public const float HallwayLevel1 = 20f;
        public const float HallwayLevel2 = 45f;
        public const float HallwayLevel3 = 65f;
        public const float LandingLevel1 = 65f;
        public const float UpperFloorEngulfed = 80f;

        public static int GetLevel(RoomId room, float pastTime, bool candleBurned)
        {
            float t = candleBurned ? pastTime / CandleSlowdown : pastTime;
            switch (room)
            {
                case RoomId.Bedroom:
                    return t >= BedroomAblaze ? 3 : 1;
                case RoomId.Hallway:
                    if (t >= HallwayLevel3) return 3;
                    if (t >= HallwayLevel2) return 2;
                    return t >= HallwayLevel1 ? 1 : 0;
                default:
                    if (t >= UpperFloorEngulfed) return 2;
                    return t >= LandingLevel1 ? 1 : 0;
            }
        }
    }
}
