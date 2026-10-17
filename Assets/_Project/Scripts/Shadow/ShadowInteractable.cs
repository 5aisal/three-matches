using System.Collections.Generic;
using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Shadow
{
    /// <summary>
    /// A prop the shadow can use (closet door, stuck window). It offers a prompt only while its memory
    /// is burned and its step is not done yet; otherwise the shadow passes over it (GAME_SPEC §11).
    /// </summary>
    public sealed class ShadowInteractable : MonoBehaviour
    {
        private static readonly List<ShadowInteractable> Registry = new List<ShadowInteractable>();

        [SerializeField] private RescueStep step;
        [SerializeField] private string verb = "Open";
        [Tooltip("0 = a single press of the Interact key; above 0 = hold it for this long.")]
        [SerializeField, Min(0f)] private float holdSeconds;
        [SerializeField, Min(0f)] private float reachX = 1f;
        [SerializeField, Min(0f)] private float reachY = 1.4f;
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.4f, 0f);

        public static IReadOnlyList<ShadowInteractable> All => Registry;

        public RescueStep Step => step;
        public string Verb => verb;
        public float HoldSeconds => holdSeconds;
        public Vector3 PromptPosition => transform.position + promptOffset;

        private void OnEnable() => Registry.Add(this);
        private void OnDisable() => Registry.Remove(this);

        public bool IsAvailable(RescueState state)
        {
            return state.IsBurned(MemoryRules.MemoryFor(step)) && !state.IsStepCompleted(step);
        }

        public bool InReach(Vector3 p)
        {
            Vector3 c = transform.position;
            return Mathf.Abs(c.x - p.x) <= reachX && Mathf.Abs(c.y - p.y) <= reachY;
        }

        public float DistanceX(float x) => Mathf.Abs(transform.position.x - x);
    }
}
