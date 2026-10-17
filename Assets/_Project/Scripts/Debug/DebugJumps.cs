using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Player;
using ThreeMatches.Strips;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeMatches.DebugTools
{
    /// <summary>
    /// GAME_SPEC §22 jump keys F2, F3, F4, F6. Editor and development builds only.
    /// Each jump replaces the rescue state, then moves the stage and the match to match.
    /// (Namespace is DebugTools, not Debug, so UnityEngine.Debug keeps working inside ThreeMatches.*.)
    /// </summary>
    public sealed class DebugJumps : MonoBehaviour
    {
        [SerializeField] private RescueStateService rescue;
        [SerializeField] private MatchController matches;
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private GameFlagsService flags;

        private void Awake()
        {
            // Debug.isDebugBuild is true in the editor and in development builds.
            if (!Debug.isDebugBuild) enabled = false;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || layout.IsBusy) return;

            if (kb.f2Key.wasPressedThisFrame) JumpToMatch1();
            else if (kb.f3Key.wasPressedThisFrame) JumpToWindow2();
            else if (kb.f4Key.wasPressedThisFrame) JumpToFinalChoice();
            else if (kb.f6Key.wasPressedThisFrame) Sandbox();
        }

        /// <summary>F2: Stage 2, CLOSET burned, match 1 lit (window open).</summary>
        private void JumpToMatch1()
        {
            Apply(new[] { MemoryId.Closet }, steps: 0, matchesUsed: 1, stage: 2);
            matches.DebugOpenWindow(1, false);
        }

        /// <summary>F3: Stage 3, CLOSET+SONG burned, FIND done, match 2 lit (start of window 2).</summary>
        private void JumpToWindow2()
        {
            Apply(new[] { MemoryId.Closet, MemoryId.Song }, steps: Bit(RescueStep.Find), matchesUsed: 2, stage: 3);
            matches.DebugOpenWindow(2, false);
        }

        /// <summary>F4: Stage 3, CLOSET+SONG burned, FIND+CALM done, no match lit, one match left for the final choice.</summary>
        private void JumpToFinalChoice()
        {
            Apply(new[] { MemoryId.Closet, MemoryId.Song }, steps: Bit(RescueStep.Find) | Bit(RescueStep.Calm), matchesUsed: 2, stage: 3);
            matches.DebugClearSession();
        }

        /// <summary>F6: all four memories burned, an endless test match, Stage 3, t = 0.</summary>
        private void Sandbox()
        {
            Apply(new[] { MemoryId.Closet, MemoryId.Song, MemoryId.Window, MemoryId.Candle }, steps: 0, matchesUsed: 0, stage: 3);
            matches.DebugOpenWindow(0, true);
        }

        private void Apply(MemoryId[] burned, int steps, int matchesUsed, int stage)
        {
            flags.Flags.Set(GameFlags.HasMatches);

            var stepFlags = new bool[MemoryRules.StepCount];
            for (int i = 0; i < stepFlags.Length; i++) stepFlags[i] = (steps & (1 << i)) != 0;
            rescue.State.Restore(new RescueSnapshot(burned, stepFlags, 0f, matchesUsed, NoorState.Hidden));

            layout.SetStage(stage);
        }

        private static int Bit(RescueStep step) => 1 << (int)step;
    }
}
