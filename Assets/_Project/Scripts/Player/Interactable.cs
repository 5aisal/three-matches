using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatches.Player
{
    /// <summary>
    /// Something the player can stand next to and use. Reach is measured side-on (x and height),
    /// so props at different depths still count. Prompt-only entries just show a hint (stairs).
    /// </summary>
    public sealed class Interactable : MonoBehaviour
    {
        private static readonly List<Interactable> Registry = new List<Interactable>();

        [SerializeField] private string verb = "Use";
        [Tooltip("Key shown in the prompt. Empty = the Interact binding.")]
        [SerializeField] private string keyLabelOverride = "";
        [SerializeField] private bool promptOnly;
        [SerializeField] private bool startAvailable = true;
        [SerializeField, Min(0f)] private float reachX = 1.2f;
        [SerializeField, Min(0f)] private float reachY = 1.6f;
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 0.6f, 0f);

        private bool available;

        public static IReadOnlyList<Interactable> All => Registry;

        public event Action<Interactable> Interacted;

        public string Verb => verb;
        public string KeyLabelOverride => keyLabelOverride;
        public bool PromptOnly => promptOnly;
        public bool Available => available;
        public Vector3 PromptPosition => transform.position + promptOffset;

        private void Awake() => available = startAvailable;
        private void OnEnable() => Registry.Add(this);
        private void OnDisable() => Registry.Remove(this);

        public void SetAvailable(bool value) => available = value;

        public bool InReach(float x, float y)
        {
            Vector3 p = transform.position;
            return Mathf.Abs(p.x - x) <= reachX && Mathf.Abs(p.y - y) <= reachY;
        }

        public float DistanceX(float x) => Mathf.Abs(transform.position.x - x);

        public void Interact()
        {
            if (available && !promptOnly) Interacted?.Invoke(this);
        }
    }
}
