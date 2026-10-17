using System.Collections.Generic;

namespace ThreeMatches.Core
{
    /// <summary>Immutable copy of a RescueState, used by "Choose again" (GAME_SPEC §16).</summary>
    public sealed class RescueSnapshot
    {
        public readonly MemoryId[] BurnedMemories;
        public readonly bool[] CompletedSteps;
        public readonly float PastTime;
        public readonly int MatchesUsed;
        public readonly NoorState NoorState;

        public RescueSnapshot(MemoryId[] burned, bool[] steps, float pastTime, int matchesUsed, NoorState noorState)
        {
            BurnedMemories = burned;
            CompletedSteps = steps;
            PastTime = pastTime;
            MatchesUsed = matchesUsed;
            NoorState = noorState;
        }
    }

    /// <summary>Rescue progress that persists between windows (GAME_SPEC §13).</summary>
    public sealed class RescueState
    {
        readonly List<MemoryId> _burned = new List<MemoryId>(MemoryRules.MemoryCount);
        readonly bool[] _steps = new bool[MemoryRules.StepCount];

        /// <summary>Raised once when a step becomes completed through <see cref="CompleteStep"/>.</summary>
        public event System.Action<RescueStep> StepCompleted;

        /// <summary>Raised after <see cref="Restore"/> replaced the whole state (debug jumps, "Choose again").</summary>
        public event System.Action Restored;

        public IReadOnlyList<MemoryId> BurnedMemories => _burned;
        public float PastTime { get; private set; }
        public int MatchesUsed { get; private set; }
        public NoorState NoorState { get; private set; } = NoorState.Hidden;

        public bool IsBurned(MemoryId id) => _burned.Contains(id);
        public bool IsStepCompleted(RescueStep step) => _steps[(int)step];

        public int CompletedStepCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _steps.Length; i++) if (_steps[i]) n++;
                return n;
            }
        }

        public int UnburnedCount => MemoryRules.MemoryCount - _burned.Count;

        public bool CanBurn(MemoryId id) => !IsBurned(id);

        public bool Burn(MemoryId id)
        {
            if (!CanBurn(id)) return false;
            _burned.Add(id);
            return true;
        }

        /// <summary>Completes a step only if its memory is burned and it is not already done.</summary>
        public bool CompleteStep(RescueStep step)
        {
            if (_steps[(int)step] || !IsBurned(MemoryRules.MemoryFor(step))) return false;
            _steps[(int)step] = true;
            StepCompleted?.Invoke(step);
            return true;
        }

        public void StartMatch() => MatchesUsed++;

        public void AdvancePastTime(float seconds)
        {
            if (seconds > 0f) PastTime += seconds;
        }

        public void SetNoorState(NoorState state) => NoorState = state;

        public RescueSnapshot TakeSnapshot() =>
            new RescueSnapshot(_burned.ToArray(), (bool[])_steps.Clone(), PastTime, MatchesUsed, NoorState);

        public void Restore(RescueSnapshot s)
        {
            _burned.Clear();
            _burned.AddRange(s.BurnedMemories);
            System.Array.Copy(s.CompletedSteps, _steps, _steps.Length);
            PastTime = s.PastTime;
            MatchesUsed = s.MatchesUsed;
            NoorState = s.NoorState;
            Restored?.Invoke();
        }
    }
}
