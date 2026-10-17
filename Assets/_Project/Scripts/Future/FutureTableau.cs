using System;
using LitMotion;
using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Strips;
using UnityEngine;

namespace ThreeMatches.Future
{
    /// <summary>
    /// House_2036 live preview (GAME_SPEC §14). Each rescue step fades one placeholder in (1 s) with a soft chime
    /// and a glow on the Future strip. Steps done while the strip is not yet shown (before Stage 3, or restored by
    /// a debug jump) appear instantly and silently. Each step animates at most once per completion.
    /// </summary>
    public sealed class FutureTableau : MonoBehaviour
    {
        private const string ColorProperty = "_BaseColor";

        [Serializable]
        private sealed class StepVisual
        {
            public RescueStep step;
            public Renderer[] renderers;
            public Color tint = Color.white;

            private MaterialPropertyBlock block;
            private MotionHandle handle;
            private float alpha;

            public bool IsShown { get; private set; }

            public void Cancel()
            {
                if (handle.IsActive()) handle.Cancel();
            }

            public void SetShown(bool shown, float visibleAlpha, float seconds, bool animate)
            {
                if (animate && IsShown == shown) return;
                IsShown = shown;
                Cancel();
                float target = shown ? visibleAlpha : 0f;
                if (!animate)
                {
                    Apply(target);
                    return;
                }

                handle = LMotion.Create(alpha, target, seconds)
                    .WithEase(Ease.InOutSine)
                    .Bind(this, static (v, self) => self.Apply(v));
            }

            private void Apply(float a)
            {
                alpha = a;
                block ??= new MaterialPropertyBlock();
                block.SetColor(ColorProperty, new Color(tint.r, tint.g, tint.b, a));
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = a > 0.001f;
                    renderers[i].SetPropertyBlock(block);
                }
            }
        }

        [SerializeField] private RescueStateService rescue;
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private FutureStripGlow glow;
        [SerializeField] private AudioSource source;
        [SerializeField] private StepVisual[] visuals;

        [Header("Tuning")]
        [SerializeField, Min(0.05f)] private float fadeSeconds = 1f;
        [SerializeField, Range(0f, 1f)] private float visibleAlpha = 0.55f;
        [SerializeField, Min(3)] private int firstVisibleStage = 3;
        [SerializeField, Range(0f, 1f)] private float chimeVolume = 0.25f;
        [SerializeField, Min(100f)] private float chimeHz = 880f;
        [SerializeField, Min(0.1f)] private float chimeSeconds = 1.4f;

        private RescueState state;

        private void Awake()
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.clip = BuildChime();
        }

        private void OnEnable()
        {
            state = rescue.State;
            state.StepCompleted += OnStepCompleted;
            state.Restored += SyncInstant;
            layout.OnCrackFinished += SyncInstant;
            SyncInstant();
        }

        private void OnDisable()
        {
            state.StepCompleted -= OnStepCompleted;
            state.Restored -= SyncInstant;
            layout.OnCrackFinished -= SyncInstant;
            for (int i = 0; i < visuals.Length; i++) visuals[i].Cancel();
        }

        private void OnStepCompleted(RescueStep step)
        {
            for (int i = 0; i < visuals.Length; i++)
            {
                StepVisual v = visuals[i];
                if (v.step != step || v.IsShown) continue;

                bool visibleNow = layout.Stage >= firstVisibleStage;
                v.SetShown(true, visibleAlpha, fadeSeconds, animate: visibleNow);
                if (!visibleNow) continue;

                source.PlayOneShot(source.clip, chimeVolume);
                glow.Pulse();
            }
        }

        /// <summary>Puts every visual in line with the rescue state without animation or sound.</summary>
        private void SyncInstant()
        {
            for (int i = 0; i < visuals.Length; i++)
            {
                StepVisual v = visuals[i];
                v.SetShown(state.IsStepCompleted(v.step), visibleAlpha, fadeSeconds, animate: false);
            }
        }

        private AudioClip BuildChime()
        {
            int rate = AudioSettings.outputSampleRate;
            var data = new float[Mathf.CeilToInt(rate * chimeSeconds)];
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / rate;
                float decay = Mathf.Exp(-t * 3f);
                float tone = Mathf.Sin(2f * Mathf.PI * chimeHz * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * chimeHz * 2f * t);
                data[i] = tone * decay * Mathf.Min(1f, t * 100f) * 0.5f;
            }

            AudioClip clip = AudioClip.Create("FutureChime", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
