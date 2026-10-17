using System;
using UnityEngine;

namespace ThreeMatches.Player
{
    /// <summary>Picks the nearest <see cref="Interactable"/> in reach and fires it on the Interact key.</summary>
    public sealed class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private RafiController rafi;

        public Interactable Focus { get; private set; }

        public event Action<Interactable> FocusChanged;

        private void Update()
        {
            SetFocus(FindFocus());

            if (Focus != null && !Focus.PromptOnly && input.Interact.WasPressedThisFrame())
            {
                Focus.Interact();
            }
        }

        private Interactable FindFocus()
        {
            if (rafi.Mode == ThreeMatches.Core.WalkerMode.Stairs || rafi.InputLocked) return null;

            Vector3 p = transform.position;
            Interactable best = null;
            float bestDist = float.MaxValue;
            var all = Interactable.All;
            for (int i = 0; i < all.Count; i++)
            {
                Interactable candidate = all[i];
                if (!candidate.Available || !candidate.InReach(p.x, p.y)) continue;
                float d = candidate.DistanceX(p.x);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }
            }
            return best;
        }

        private void SetFocus(Interactable next)
        {
            if (next == Focus) return;
            Focus = next;
            FocusChanged?.Invoke(next);
        }
    }
}
