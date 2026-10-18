using System.Collections.Generic;

namespace ThreeMatches.Core
{
    /// <summary>What one ending shows, including its GAME_SPEC §16 variants. Built by <see cref="EndingRules"/>.</summary>
    public sealed class EndingPlan
    {
        public EndingId Ending;
        public string Title;

        /// <summary>Song was burned before the final choice (A: Noor says "You hummed my song."; B: Noor hums as she leaves).</summary>
        public bool SongBurnedBefore;

        /// <summary>A only: true = carried out through the window, false = through the flames.</summary>
        public bool ExitDone;

        /// <summary>B only: memories burned by the choice, in burn order.</summary>
        public List<MemoryId> BurnedNow = new List<MemoryId>();

        /// <summary>Memories still kept once the ending is over (B: after its burns).</summary>
        public List<MemoryId> Kept = new List<MemoryId>();

        /// <summary>B variant "He kept one thing": the first kept memory, or null.</summary>
        public MemoryId? KeptMemory => Kept.Count > 0 ? Kept[0] : (MemoryId?)null;
    }

    /// <summary>Ending titles, availability and variants (GAME_SPEC §15, §16).</summary>
    public static class EndingRules
    {
        public const string TitleA = "She grew up. He didn't grow old.";
        public const string TitleB = "She lived. He forgot.";
        public const string TitleC = "He couldn't save her. He never forgot her.";

        public const string NoorSongLineA = "You hummed my song.";

        public static string TitleFor(EndingId ending)
        {
            switch (ending)
            {
                case EndingId.WalkIntoFire: return TitleA;
                case EndingId.SaveHerForgetHer: return TitleB;
                default: return TitleC;
            }
        }

        /// <summary>A and C are always available; B only when <see cref="FinalChoiceRules.IsPresentAvailable"/>.</summary>
        public static bool IsAvailable(RescueState state, EndingId ending) =>
            ending != EndingId.SaveHerForgetHer || FinalChoiceRules.IsPresentAvailable(state);

        /// <summary>Returns null when the ending is not available.</summary>
        public static EndingPlan Build(RescueState state, EndingId ending)
        {
            if (!IsAvailable(state, ending)) return null;

            var plan = new EndingPlan
            {
                Ending = ending,
                Title = TitleFor(ending),
                SongBurnedBefore = state.IsBurned(MemoryId.Song),
                ExitDone = state.IsStepCompleted(RescueStep.Exit),
                Kept = FinalChoiceRules.KeptAfter(state, ending)
            };
            if (ending == EndingId.SaveHerForgetHer) plan.BurnedNow = FinalChoiceRules.MemoriesBurnedByEndingB(state);
            return plan;
        }
    }
}
