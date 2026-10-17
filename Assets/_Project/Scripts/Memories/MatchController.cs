using System;
using ThreeMatches.Core;
using ThreeMatches.Player;
using ThreeMatches.Strips;
using UnityEngine;

namespace ThreeMatches.Memories
{
    /// <summary>
    /// Strike (F) -> choose a photo -> burn -> crack -> 40 s window (GAME_SPEC §9).
    /// The rules live in <see cref="MatchSession"/>; this component wires them to input, the wall,
    /// the strip layout and Rafi.
    /// </summary>
    public sealed class MatchController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameInput input;
        [SerializeField] private GameFlagsService flags;
        [SerializeField] private RescueStateService rescue;
        [SerializeField] private MemoryWall wall;
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private RafiController rafi;

        [Header("Timing")]
        [SerializeField, Min(1f)] private float windowSeconds = 40f;
        [SerializeField, Min(0f)] private float flickerSeconds = 8f;

        private MatchSession session;
        private bool waitingForCrack;

        /// <summary>Raised when a match is struck; the argument is its number (1-3).</summary>
        public event Action<int> OnMatchLit;

        /// <summary>Raised when a rescue window's match goes out; the argument is its number (1-2).</summary>
        public event Action<int> OnMatchOut;

        /// <summary>Raised when the third match is struck. It never opens a window.</summary>
        public event Action OnFinalMatchLit;

        /// <summary>Raised whenever the session, the matches used or the burned memories changed.</summary>
        public event Action StateChanged;

        public MatchSession Session => session;
        public RescueState State => rescue.State;

        private void Awake()
        {
            session = new MatchSession(windowSeconds, flickerSeconds);
        }

        private void OnEnable()
        {
            wall.Picked += OnPicked;
            flags.Flags.FlagSet += OnFlagSet;
        }

        private void OnDisable()
        {
            wall.Picked -= OnPicked;
            flags.Flags.FlagSet -= OnFlagSet;
            StopWaitingForCrack();
        }

        private void Start()
        {
            RaiseStateChanged();
        }

        private void Update()
        {
            if (session.IsWindowOpen && session.Tick(Time.deltaTime))
            {
                int number = session.MatchNumber;
                rafi.InputLocked = false;
                OnMatchOut?.Invoke(number);
                RaiseStateChanged();
                return;
            }

            if (input.Strike.WasPressedThisFrame()) TryStrike();
        }

        private bool CanStrikeNow()
        {
            return !rafi.InputLocked
                && rafi.Mode != WalkerMode.Stairs
                && wall.InReach(rafi.transform.position)
                && session.CanStrike(rescue.State, flags.Flags.Has(GameFlags.HasMatches));
        }

        private void TryStrike()
        {
            if (!CanStrikeNow()) return;

            int number = session.Strike(rescue.State, true);
            if (number == 0) return;

            OnMatchLit?.Invoke(number);
            if (number == MatchSession.FinalMatchNumber)
            {
                OnFinalMatchLit?.Invoke();
                RaiseStateChanged();
                return;
            }

            rafi.InputLocked = true;
            RaiseStateChanged();
            wall.BeginChoosing();
        }

        private void OnPicked(MemoryId memory)
        {
            if (!session.BeginBurn(rescue.State, memory))
            {
                wall.Refuse(memory);
                return;
            }

            RaiseStateChanged();
            wall.PlayBurn(memory, OnBurnFinished);
        }

        private void OnBurnFinished()
        {
            int stage = session.FinishBurn();
            if (stage == 0) return;

            if (layout.Stage >= stage)
            {
                OpenWindow();
                return;
            }

            waitingForCrack = true;
            layout.OnCrackFinished += OnCrackFinished;
            layout.SetStage(stage);
        }

        private void OnCrackFinished()
        {
            StopWaitingForCrack();
            OpenWindow();
        }

        private void OpenWindow()
        {
            session.BeginWindow();
            RaiseStateChanged();
        }

        private void StopWaitingForCrack()
        {
            if (!waitingForCrack) return;
            waitingForCrack = false;
            layout.OnCrackFinished -= OnCrackFinished;
        }

        private void OnFlagSet(string flag)
        {
            if (flag == GameFlags.HasMatches) RaiseStateChanged();
        }

        /// <summary>Debug: drops any running match and pick, leaving the rescue state as it is.</summary>
        public void DebugClearSession()
        {
            StopWaitingForCrack();
            wall.CancelAll();
            session.Reset();
            rafi.InputLocked = false;
            wall.SyncFrames();
            RaiseStateChanged();
        }

        /// <summary>Debug: opens a rescue window right now (matchNumber 0 + sandbox = endless test match).</summary>
        public void DebugOpenWindow(int matchNumber, bool sandbox)
        {
            DebugClearSession();
            session.ForceWindow(matchNumber, sandbox);
            rafi.InputLocked = true;
            RaiseStateChanged();
        }

        private void RaiseStateChanged()
        {
            wall.Refresh(session.CanStrike(rescue.State, flags.Flags.Has(GameFlags.HasMatches)));
            StateChanged?.Invoke();
        }
    }
}
