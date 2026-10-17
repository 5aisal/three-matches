using ThreeMatches.Memories;
using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// Advances <c>RescueState.PastTime</c> only while a rescue window is open, i.e. from the moment the
    /// crack finishes and the flame starts counting until the match goes out (GAME_SPEC §9, §10).
    /// Striking, choosing and burning a photo do not move the past.
    /// </summary>
    public sealed class PastClock : MonoBehaviour
    {
        [SerializeField] private MatchController matches;

        public bool IsRunning => matches.Session.IsWindowOpen;

        private void Update()
        {
            if (IsRunning) matches.State.AdvancePastTime(Time.deltaTime);
        }
    }
}
