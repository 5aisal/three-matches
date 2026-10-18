using System.Collections.Generic;

namespace ThreeMatches.Core
{
    /// <summary>Cost, availability and outcomes of the final choice (GAME_SPEC §15, §16).</summary>
    public static class FinalChoiceRules
    {
        public const string PastHover = "Go yourself. You won't come back.";
        public const string FutureHover = "Blow it out. Keep what's left.";
        public const string PresentUnavailable = "Not enough left to give.";

        /// <summary>Hover text of the Present strip: the real cost N, or the greyed-out text.</summary>
        public static string PresentHover(RescueState state)
        {
            if (!IsPresentAvailable(state)) return PresentUnavailable;
            int n = Cost(state);
            if (n == 0) return "Save her. There is nothing left to burn.";
            return n == 1 ? "Burn 1 more memory to save her." : "Burn " + n + " more memories to save her.";
        }

        public static string HoverText(RescueState state, EndingId ending)
        {
            switch (ending)
            {
                case EndingId.WalkIntoFire: return PastHover;
                case EndingId.SaveHerForgetHer: return PresentHover(state);
                default: return FutureHover;
            }
        }

        public static List<RescueStep> MissingSteps(RescueState state)
        {
            var missing = new List<RescueStep>(MemoryRules.StepCount);
            for (int i = 0; i < MemoryRules.StepCount; i++)
            {
                var step = (RescueStep)i;
                if (!state.IsStepCompleted(step)) missing.Add(step);
            }
            return missing;
        }

        public static int Cost(RescueState state) => MemoryRules.StepCount - state.CompletedStepCount;

        public static bool IsPresentAvailable(RescueState state) => Cost(state) <= state.UnburnedCount;

        /// <summary>Empty if the Present option is unavailable. Missing-step memories first, then others in enum order.</summary>
        public static List<MemoryId> MemoriesBurnedByEndingB(RescueState state)
        {
            var result = new List<MemoryId>();
            if (!IsPresentAvailable(state)) return result;

            int cost = Cost(state);
            foreach (var step in MissingSteps(state))
            {
                var memory = MemoryRules.MemoryFor(step);
                if (result.Count < cost && state.CanBurn(memory)) result.Add(memory);
            }
            for (int i = 0; i < MemoryRules.MemoryCount && result.Count < cost; i++)
            {
                var memory = (MemoryId)i;
                if (state.CanBurn(memory) && !result.Contains(memory)) result.Add(memory);
            }
            return result;
        }

        public static List<MemoryId> KeptAfter(RescueState state, EndingId ending)
        {
            var kept = new List<MemoryId>(MemoryRules.MemoryCount);
            var burnedByB = ending == EndingId.SaveHerForgetHer ? MemoriesBurnedByEndingB(state) : null;
            for (int i = 0; i < MemoryRules.MemoryCount; i++)
            {
                var memory = (MemoryId)i;
                if (!state.CanBurn(memory)) continue;
                if (burnedByB != null && burnedByB.Contains(memory)) continue;
                kept.Add(memory);
            }
            return kept;
        }
    }
}
