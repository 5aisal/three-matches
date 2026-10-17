using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Strips;
using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// Applies the §10 fire schedule to the rooms of the past house. While a window is open the fire
    /// runs and the Past strip looks active; when the match is out everything freezes and the strip
    /// takes the inactive look. Plays the candle vision the first time a window opens with CANDLE burned.
    /// </summary>
    public sealed class PastFireController : MonoBehaviour
    {
        [SerializeField] private MatchController matches;
        [SerializeField] private StripPastLook pastLook;
        [SerializeField] private CandleVision vision;
        [SerializeField] private FireRoom[] rooms;

        private bool windowOpen;
        private bool visionPlayed;
        private bool candleBurned;

        public FireRoom[] Rooms => rooms;
        public bool CandleBurned => candleBurned;
        public bool IsWindowOpen => windowOpen;

        private void Start()
        {
            pastLook.SetInactive(true, true);
            SetFrozen(true);
            ApplyLevels(true);
        }

        private void Update()
        {
            RescueState state = matches.State;
            candleBurned = state.IsBurned(MemoryId.Candle);

            bool open = matches.Session.IsWindowOpen;
            if (open != windowOpen)
            {
                windowOpen = open;
                SetFrozen(!open);
                pastLook.SetInactive(!open);
                if (open && candleBurned && !visionPlayed)
                {
                    visionPlayed = true;
                    vision.Play();
                }
            }

            ApplyLevels(false);
        }

        /// <summary>Debug: lets the vision play again the next time a window opens.</summary>
        public void ResetVision() => visionPlayed = false;

        private void ApplyLevels(bool snap)
        {
            float t = matches.State.PastTime;
            for (int i = 0; i < rooms.Length; i++)
            {
                int level = FireTimeline.GetLevel(rooms[i].Room, t, candleBurned);
                if (snap) rooms[i].SnapToLevel(level);
                else rooms[i].SetTargetLevel(level);
            }
        }

        private void SetFrozen(bool frozen)
        {
            for (int i = 0; i < rooms.Length; i++) rooms[i].SetFrozen(frozen);
        }
    }
}
