namespace ThreeMatches.Core
{
    public enum MatchPhase { Idle, Choosing, Burning, Cracking, Window }

    /// <summary>
    /// Rules for striking a match and the 40 s rescue window (GAME_SPEC §9). Plain C# so it can be tested.
    /// Flow: Idle -> Strike -> Choosing -> BeginBurn -> Burning -> FinishBurn -> Cracking -> BeginWindow -> Window -> out -> Idle.
    /// Match 3 never leaves Idle: it only reports that the final match was lit.
    /// </summary>
    public sealed class MatchSession
    {
        public const int TotalMatches = 3;
        public const int FinalMatchNumber = 3;

        readonly float _windowSeconds;
        readonly float _flickerSeconds;

        public MatchSession(float windowSeconds, float flickerSeconds)
        {
            _windowSeconds = windowSeconds;
            _flickerSeconds = flickerSeconds;
        }

        public MatchPhase Phase { get; private set; }
        public float Remaining { get; private set; }
        public bool Sandbox { get; private set; }

        /// <summary>The match number (1-3) of the current session, 0 when none.</summary>
        public int MatchNumber { get; private set; }

        public float WindowSeconds => _windowSeconds;
        public bool IsActive => Phase != MatchPhase.Idle;
        public bool IsWindowOpen => Phase == MatchPhase.Window;
        public bool IsFlickering => Phase == MatchPhase.Window && !Sandbox && Remaining <= _flickerSeconds;

        /// <summary>1 while the flame is full (before the window opens) down to 0 when it is out.</summary>
        public float Fraction
        {
            get
            {
                if (Phase == MatchPhase.Window) return Sandbox || _windowSeconds <= 0f ? 1f : Remaining / _windowSeconds;
                return IsActive ? 1f : 0f;
            }
        }

        public static int MatchesLeft(RescueState state) => TotalMatches - state.MatchesUsed;

        public bool CanStrike(RescueState state, bool hasMatches) =>
            hasMatches && Phase == MatchPhase.Idle && state.MatchesUsed < TotalMatches;

        /// <summary>Uses up a match. Returns its number (1-3), or 0 if striking is not allowed.</summary>
        public int Strike(RescueState state, bool hasMatches)
        {
            if (!CanStrike(state, hasMatches)) return 0;
            state.StartMatch();
            MatchNumber = state.MatchesUsed;
            if (MatchNumber < FinalMatchNumber) Phase = MatchPhase.Choosing;
            return MatchNumber;
        }

        /// <summary>Records the chosen memory as burned. Refuses burned memories and the wrong phase.</summary>
        public bool BeginBurn(RescueState state, MemoryId memory)
        {
            if (Phase != MatchPhase.Choosing || !state.Burn(memory)) return false;
            Phase = MatchPhase.Burning;
            return true;
        }

        /// <summary>Burn animation done. Returns the stage to crack to (2 after match 1, 3 after match 2).</summary>
        public int FinishBurn()
        {
            if (Phase != MatchPhase.Burning) return 0;
            Phase = MatchPhase.Cracking;
            return MatchNumber + 1;
        }

        public void BeginWindow()
        {
            if (Phase != MatchPhase.Cracking) return;
            Phase = MatchPhase.Window;
            Remaining = _windowSeconds;
        }

        /// <summary>Counts the flame down. Returns true on the frame the match goes out.</summary>
        public bool Tick(float deltaTime)
        {
            if (Phase != MatchPhase.Window || Sandbox) return false;
            Remaining -= deltaTime;
            if (Remaining > 0f) return false;
            Remaining = 0f;
            Phase = MatchPhase.Idle;
            return true;
        }

        /// <summary>Debug jump: puts the session straight into an open window (or the sandbox match).</summary>
        public void ForceWindow(int matchNumber, bool sandbox)
        {
            MatchNumber = matchNumber;
            Sandbox = sandbox;
            Phase = MatchPhase.Window;
            Remaining = _windowSeconds;
        }

        /// <summary>Debug jump: drops any running session.</summary>
        public void Reset()
        {
            Phase = MatchPhase.Idle;
            Remaining = 0f;
            MatchNumber = 0;
            Sandbox = false;
        }
    }
}
