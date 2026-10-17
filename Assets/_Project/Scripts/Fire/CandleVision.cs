using LitMotion;
using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// The 3 s "how it started" vision (GAME_SPEC §8, CANDLE): a curtain drifts onto the candle, then the
    /// candle flame flares. Placeholder visuals: a cloth quad, a small flame and a light.
    /// </summary>
    public sealed class CandleVision : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private Transform ghostCurtain;
        [Tooltip("Where the curtain starts (the real curtains).")]
        [SerializeField] private Transform curtainStart;
        [Tooltip("Where the curtain ends (on the candle).")]
        [SerializeField] private Transform candle;
        [SerializeField] private ParticleSystem candleFlame;
        [SerializeField] private Light flameLight;

        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float duration = 3f;
        [SerializeField, Range(0.1f, 0.9f)] private float driftShare = 0.65f;

        [Header("Look")]
        [SerializeField, Min(0f)] private float swayAmount = 0.12f;
        [SerializeField, Min(0f)] private float swayCycles = 2f;
        [SerializeField, Min(0f)] private float swayTilt = 12f;
        [SerializeField, Min(0f)] private float flameRate = 40f;
        [SerializeField, Min(0f)] private float flameLightIntensity = 3f;
        [SerializeField, Min(0f)] private float curtainFlameOffset = 0.15f;

        private MotionHandle handle;
        private Vector3 curtainFrom;
        private Vector3 curtainTo;
        private Vector3 curtainBaseScale;
        private ParticleSystem.EmissionModule flameEmission;

        public bool IsPlaying => handle.IsActive();

        private void Awake()
        {
            flameEmission = candleFlame.emission;
            curtainBaseScale = ghostCurtain.localScale;
            HideAll();
        }

        private void OnDestroy()
        {
            if (handle.IsActive()) handle.Cancel();
        }

        public void Play()
        {
            if (handle.IsActive()) handle.Cancel();

            curtainFrom = curtainStart.position;
            curtainTo = candle.position + new Vector3(0f, curtainFlameOffset, -0.1f);
            ghostCurtain.gameObject.SetActive(true);
            flameLight.enabled = true;
            flameLight.intensity = 0f;
            candleFlame.Clear();
            candleFlame.Play();
            flameEmission.rateOverTime = 0f;
            Apply(0f);

            handle = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.Linear)
                .WithOnComplete(HideAll)
                .Bind(this, static (t, self) => self.Apply(t));
        }

        private void Apply(float t)
        {
            float drift = Mathf.Clamp01(t / driftShare);
            float eased = EaseValue(drift);
            float sway = Mathf.Sin(drift * Mathf.PI * 2f * swayCycles) * swayAmount * (1f - eased);

            ghostCurtain.position = Vector3.Lerp(curtainFrom, curtainTo, eased) + new Vector3(0f, sway, 0f);
            ghostCurtain.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(drift * Mathf.PI * 2f * swayCycles) * swayTilt * (1f - eased));
            // The cloth shrinks as it settles over the flame.
            ghostCurtain.localScale = curtainBaseScale * Mathf.Lerp(1f, 0.35f, eased);

            float ignite = Mathf.Clamp01((t - driftShare) / (1f - driftShare));
            flameEmission.rateOverTime = flameRate * ignite;
            flameLight.intensity = flameLightIntensity * ignite;
        }

        private float EaseValue(float x) => x * x * (3f - 2f * x);

        private void HideAll()
        {
            ghostCurtain.gameObject.SetActive(false);
            flameLight.enabled = false;
            candleFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
