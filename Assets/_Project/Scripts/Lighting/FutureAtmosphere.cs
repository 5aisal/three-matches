using LitMotion;
using ThreeMatches.Core;
using ThreeMatches.Memories;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThreeMatches.Lighting
{
    /// <summary>Future strip: soft grey morning light that warms with each completed rescue step.</summary>
    public sealed class FutureAtmosphere : MonoBehaviour
    {
        [SerializeField] private RescueStateService rescue;
        [SerializeField] private StripAtmosphere strip;
        [SerializeField] private Volume volume;

        [Header("Grey morning (0 steps)")]
        [SerializeField] private Color greyAmbient = new Color(0.17f, 0.18f, 0.2f, 1f);
        [SerializeField] private Color greyKey = new Color(0.72f, 0.77f, 0.84f, 1f);
        [SerializeField, Min(0f)] private float greyKeyIntensity = 0.8f;
        [SerializeField] private Color greyFilter = new Color(0.9f, 0.94f, 1f, 1f);

        [Header("Warm (all steps done)")]
        [SerializeField] private Color warmAmbient = new Color(0.27f, 0.2f, 0.14f, 1f);
        [SerializeField] private Color warmKey = new Color(1f, 0.82f, 0.58f, 1f);
        [SerializeField, Min(0f)] private float warmKeyIntensity = 1.3f;
        [SerializeField] private Color warmFilter = new Color(1f, 0.93f, 0.82f, 1f);

        [SerializeField, Min(0.05f)] private float warmSeconds = 1f;

        private RescueState state;
        private ColorAdjustments adjustments;
        private MotionHandle handle;
        private float warmth;

        private void Awake()
        {
            volume.profile.TryGet(out adjustments);
        }

        private void OnEnable()
        {
            state = rescue.State;
            state.StepCompleted += OnStepCompleted;
            state.Restored += SnapToState;
            SnapToState();
        }

        private void OnDisable()
        {
            state.StepCompleted -= OnStepCompleted;
            state.Restored -= SnapToState;
            if (handle.IsActive()) handle.Cancel();
        }

        private float Target() => AtmosphereRules.FutureWarmth(state.CompletedStepCount, MemoryRules.StepCount);

        private void OnStepCompleted(RescueStep step)
        {
            if (handle.IsActive()) handle.Cancel();
            handle = LMotion.Create(warmth, Target(), warmSeconds)
                .WithEase(Ease.InOutSine)
                .Bind(this, static (v, self) => self.Apply(v));
        }

        private void SnapToState()
        {
            if (handle.IsActive()) handle.Cancel();
            Apply(Target());
        }

        private void Apply(float w)
        {
            warmth = w;
            strip.Ambient = Color.Lerp(greyAmbient, warmAmbient, w);
            Light key = strip.KeyLight;
            key.color = Color.Lerp(greyKey, warmKey, w);
            key.intensity = Mathf.Lerp(greyKeyIntensity, warmKeyIntensity, w);
            if (adjustments != null) adjustments.colorFilter.value = Color.Lerp(greyFilter, warmFilter, w);
        }
    }
}
