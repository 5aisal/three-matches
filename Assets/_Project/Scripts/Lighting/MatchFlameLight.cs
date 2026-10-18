using ThreeMatches.Memories;
using UnityEngine;

namespace ThreeMatches.Lighting
{
    /// <summary>The warm point light of the match in Rafi's hand: on while a match burns, flickering, fading at the end.</summary>
    public sealed class MatchFlameLight : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private Light flame;

        [SerializeField, Min(0f)] private float intensity = 3.2f;
        [SerializeField, Min(0.1f)] private float flickerSpeed = 9f;
        [SerializeField, Range(0f, 1f)] private float flickerDepth = 0.25f;
        [Tooltip("Fraction of the window left where the flame starts to die down.")]
        [SerializeField, Range(0f, 1f)] private float dyingFraction = 0.2f;

        private bool finalLit;
        private float clock;

        private void Awake()
        {
            flame.enabled = false;
        }

        private void OnEnable()
        {
            match.OnMatchLit += OnMatchLit;
            match.OnFinalMatchLit += OnFinalLit;
        }

        private void OnDisable()
        {
            match.OnMatchLit -= OnMatchLit;
            match.OnFinalMatchLit -= OnFinalLit;
        }

        private void OnMatchLit(int number)
        {
            if (number < ThreeMatches.Core.MatchSession.FinalMatchNumber) finalLit = false;
        }

        private void OnFinalLit()
        {
            finalLit = true;
        }

        private void Update()
        {
            var session = match.Session;
            bool lit = session.IsActive || finalLit;
            if (flame.enabled != lit) flame.enabled = lit;
            if (!lit) return;

            clock += Time.deltaTime;
            float noise = Mathf.PerlinNoise(clock * flickerSpeed, 0.37f);
            float fade = finalLit ? 1f : Mathf.Clamp01(session.Fraction / Mathf.Max(0.01f, dyingFraction));
            flame.intensity = intensity * fade * (1f - flickerDepth + flickerDepth * 2f * noise);
        }
    }
}
