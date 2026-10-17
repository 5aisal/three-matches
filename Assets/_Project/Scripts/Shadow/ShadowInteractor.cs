using System;
using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.Shadow
{
    /// <summary>
    /// Picks the nearest available <see cref="ShadowInteractable"/> in reach, runs the press / hold on
    /// the Interact key and completes the rescue step through <see cref="RescueState.CompleteStep"/>.
    /// </summary>
    public sealed class ShadowInteractor : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private MatchController matches;
        [SerializeField] private ShadowController shadow;

        private HoldTimer hold;
        private ShadowInteractable focus;

        public ShadowInteractable Focus => focus;
        public float HoldProgress => hold != null ? hold.Progress : 0f;

        public event Action<ShadowInteractable> FocusChanged;
        public event Action<RescueStep> StepCompleted;

        private void Update()
        {
            if (!shadow.IsActive)
            {
                SetFocus(null);
                return;
            }

            SetFocus(FindFocus());
            if (focus == null) return;

            if (focus.HoldSeconds <= 0f)
            {
                if (input.Interact.WasPressedThisFrame()) Complete();
                return;
            }

            if (hold.Tick(input.Interact.IsPressed(), Time.deltaTime)) Complete();
        }

        private ShadowInteractable FindFocus()
        {
            RescueState state = matches.State;
            Vector3 p = shadow.BodyCenter;
            ShadowInteractable best = null;
            float bestDist = float.MaxValue;
            var all = ShadowInteractable.All;
            for (int i = 0; i < all.Count; i++)
            {
                ShadowInteractable c = all[i];
                if (!c.IsAvailable(state) || !c.InReach(p)) continue;
                float d = c.DistanceX(p.x);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = c;
                }
            }
            return best;
        }

        private void SetFocus(ShadowInteractable next)
        {
            if (next == focus) return;
            focus = next;
            hold = next != null && next.HoldSeconds > 0f ? new HoldTimer(next.HoldSeconds) : null;
            FocusChanged?.Invoke(next);
        }

        private void Complete()
        {
            RescueStep step = focus.Step;
            if (matches.State.CompleteStep(step)) StepCompleted?.Invoke(step);
            SetFocus(null);
        }
    }
}
