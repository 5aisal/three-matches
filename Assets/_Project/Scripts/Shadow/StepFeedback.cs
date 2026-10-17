using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Shadow
{
    /// <summary>
    /// Placeholder feedback when a rescue step is completed: a soft procedural chime and a short slow-motion
    /// beat. Time scale is global, so the whole game slows for the beat (it is only 0.2 s and the shadow's
    /// window is the only thing running).
    /// </summary>
    public sealed class StepFeedback : MonoBehaviour
    {
        [SerializeField] private ShadowInteractor interactor;
        [SerializeField] private AudioSource source;
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
        [SerializeField, Min(0.05f)] private float slowMoSeconds = 0.2f;
        [SerializeField, Range(0.05f, 1f)] private float slowMoScale = 0.25f;
        [SerializeField, Min(100f)] private float chimeHz = 660f;
        [SerializeField, Min(0.1f)] private float chimeSeconds = 1.2f;

        private float slowMoLeft;

        private void Awake()
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.clip = BuildChime();
        }

        private void OnEnable() => interactor.StepCompleted += OnStepCompleted;

        private void OnDisable()
        {
            interactor.StepCompleted -= OnStepCompleted;
            EndSlowMo();
        }

        private void Update()
        {
            if (slowMoLeft <= 0f) return;
            slowMoLeft -= Time.unscaledDeltaTime;
            if (slowMoLeft <= 0f) EndSlowMo();
        }

        private void OnStepCompleted(RescueStep step)
        {
            source.PlayOneShot(source.clip, volume);
            slowMoLeft = slowMoSeconds;
            Time.timeScale = slowMoScale;
        }

        private void EndSlowMo()
        {
            slowMoLeft = 0f;
            Time.timeScale = 1f;
        }

        private AudioClip BuildChime()
        {
            int rate = AudioSettings.outputSampleRate;
            var data = new float[Mathf.CeilToInt(rate * chimeSeconds)];
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / rate;
                float decay = Mathf.Exp(-t * 3.5f);
                float tone = Mathf.Sin(2f * Mathf.PI * chimeHz * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * chimeHz * 1.5f * t);
                data[i] = tone * decay * Mathf.Min(1f, t * 200f) * 0.5f;
            }

            AudioClip clip = AudioClip.Create("StepChime", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
