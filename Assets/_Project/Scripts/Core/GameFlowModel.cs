using System;

namespace ThreeMatches.Core
{
    public enum GamePhase { Title, Act1, Match1, Between, Match2, Final, FinalChoice, Ending, EndCard, Credits }

    /// <summary>
    /// Where the player is between title and credits (GAME_SPEC §17). Plain C#; the director feeds it events.
    /// Transitions that do not fit the current phase are refused (return false).
    /// </summary>
    public sealed class GameFlowModel
    {
        public GamePhase Phase { get; private set; } = GamePhase.Title;
        public EndingId? Ending { get; private set; }

        public event Action<GamePhase> PhaseChanged;

        public bool DismissTitle() => Move(GamePhase.Title, GamePhase.Act1);

        /// <summary>Match 1 may be struck in Act 1, match 2 in Between. Match 3 goes through <see cref="FinalMatchLit"/>.</summary>
        public bool MatchLit(int number)
        {
            if (number == 1) return Move(GamePhase.Act1, GamePhase.Match1);
            if (number == 2) return Move(GamePhase.Between, GamePhase.Match2);
            return false;
        }

        public bool MatchOut(int number)
        {
            if (number == 1) return Move(GamePhase.Match1, GamePhase.Between);
            if (number == 2) return Move(GamePhase.Match2, GamePhase.Final);
            return false;
        }

        public bool FinalMatchLit() => Move(GamePhase.Final, GamePhase.FinalChoice);

        public bool ChooseEnding(EndingId ending)
        {
            if (!Move(GamePhase.FinalChoice, GamePhase.Ending)) return false;
            Ending = ending;
            return true;
        }

        public bool EndingFinished() => Move(GamePhase.Ending, GamePhase.EndCard);

        public bool ShowCredits() => Move(GamePhase.EndCard, GamePhase.Credits);

        public bool BackToEndCard() => Move(GamePhase.Credits, GamePhase.EndCard);

        /// <summary>"Choose again": back to the moment before the final match.</summary>
        public bool ChooseAgain()
        {
            if (Phase != GamePhase.EndCard) return false;
            Ending = null;
            Set(GamePhase.Final);
            return true;
        }

        /// <summary>Debug jumps replace the whole rescue state; put the phase where that state belongs.</summary>
        public void SyncToState(int matchesUsed, bool windowOpen)
        {
            if (Phase == GamePhase.Title) return;
            Ending = null;
            Set(PhaseFor(matchesUsed, windowOpen));
        }

        public static GamePhase PhaseFor(int matchesUsed, bool windowOpen)
        {
            if (matchesUsed <= 0) return GamePhase.Act1;
            if (matchesUsed == 1) return windowOpen ? GamePhase.Match1 : GamePhase.Between;
            if (matchesUsed == 2) return windowOpen ? GamePhase.Match2 : GamePhase.Final;
            return GamePhase.FinalChoice;
        }

        private bool Move(GamePhase from, GamePhase to)
        {
            if (Phase != from) return false;
            Set(to);
            return true;
        }

        private void Set(GamePhase phase)
        {
            if (Phase == phase) return;
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }
    }
}
