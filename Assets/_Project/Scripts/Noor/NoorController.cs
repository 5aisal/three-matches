using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Shadow;
using ThreeMatches.UI;
using UnityEngine;

namespace ThreeMatches.Noor
{
    /// <summary>
    /// Drives Noor on the upper floor of House_1996 from the shadow's position and humming (GAME_SPEC §12).
    /// The rules live in <see cref="NoorModel"/>; this component feeds it, shows the placeholder sprite,
    /// plays the subtitles and keeps <see cref="RescueState"/> up to date.
    /// </summary>
    public sealed class NoorController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShadowController shadow;
        [SerializeField] private ShadowInteractor interactor;
        [SerializeField] private ShadowHumming humming;
        [SerializeField] private MatchController matches;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private Transform closet;
        [SerializeField] private Transform window;
        [SerializeField] private SpriteRenderer visual;

        [Header("Floor line")]
        [Tooltip("World y of her feet (the upper floor).")]
        [SerializeField] private float floorY = 3.1f;
        [Tooltip("World z she stands at, just in front of the back wall.")]
        [SerializeField] private float depth = 3.5f;
        [SerializeField] private float minX = -8.8f;
        [SerializeField] private float maxX = 8.8f;
        [Tooltip("How far from the closet she stands after stepping out.")]
        [SerializeField] private float stepOutOffset = 1f;

        [Header("Behaviour")]
        [SerializeField, Min(0f)] private float panicRadius = 3f;
        [SerializeField, Min(0f)] private float followRadius = 4f;
        [SerializeField, Min(0f)] private float followSpeed = 1.5f;
        [Tooltip("How close she walks to the shadow.")]
        [SerializeField, Min(0f)] private float followGap = 0.6f;
        [SerializeField, Min(0f)] private float humStopSeconds = 1.5f;
        [SerializeField, Min(0f)] private float panicRunDistance = 1.5f;
        [SerializeField, Min(0.1f)] private float panicRunSpeed = 3f;
        [SerializeField, Min(0f)] private float panicFreezeSeconds = 3f;
        [Tooltip("How close to the window she must get to climb onto the sill.")]
        [SerializeField, Min(0f)] private float windowReach = 1f;

        [Header("Subtitles")]
        [SerializeField] private string panicLine = "Go away! Go away!";
        [SerializeField] private string followLine = "...Papa? Is that you?";
        [SerializeField, Min(0.5f)] private float lineSeconds = 2.5f;

        private NoorModel model;
        private float lastX;
        private NoorState written;

        public NoorState State => model.State;

        private void Awake()
        {
            var tuning = new NoorTuning
            {
                PanicRadius = panicRadius,
                FollowRadius = followRadius,
                FollowSpeed = followSpeed,
                FollowGap = followGap,
                HumStopSeconds = humStopSeconds,
                PanicRunDistance = panicRunDistance,
                PanicRunSpeed = panicRunSpeed,
                PanicFreezeSeconds = panicFreezeSeconds,
                WindowReach = windowReach
            };
            model = new NoorModel(tuning, minX, maxX, window.position.x);
            lastX = StepOutX;
            Apply();
        }

        private void OnEnable()
        {
            shadow.ActiveChanged += OnShadowActiveChanged;
            interactor.StepCompleted += OnStepCompleted;
        }

        private void OnDisable()
        {
            shadow.ActiveChanged -= OnShadowActiveChanged;
            interactor.StepCompleted -= OnStepCompleted;
        }

        private float StepOutX => closet.position.x + stepOutOffset;

        private void Update()
        {
            if (!shadow.IsActive) return;

            RescueState state = matches.State;
            if (state.NoorState != written) LoadFrom(state);

            bool windowOpen = state.IsStepCompleted(RescueStep.Exit);
            NoorEvents events = model.Step(Time.deltaTime, shadow.transform.position.x, humming.IsHumming, windowOpen);
            HandleEvents(events, state);
            Publish(state);
            Apply();
        }

        private void OnShadowActiveChanged(bool open)
        {
            if (open) LoadFrom(matches.State);
        }

        /// <summary>Loads Noor from the stored rescue state (a window opened, or the state was replaced).</summary>
        private void LoadFrom(RescueState state)
        {
            NoorState stored = state.NoorState;
            if (stored == NoorState.Hidden && state.IsStepCompleted(RescueStep.Find)) stored = NoorState.Out;

            float x = stored == NoorState.AtWindow ? window.position.x : lastX;
            model.Restore(stored, x);
            Publish(state, true);
            Apply();
        }

        private void OnStepCompleted(RescueStep step)
        {
            if (step != RescueStep.Find) return;

            model.Reveal(StepOutX);
            Publish(matches.State);
            Apply();
        }

        private void HandleEvents(NoorEvents events, RescueState state)
        {
            if ((events & NoorEvents.Panicked) != 0) subtitles.Show(panicLine, lineSeconds);

            if ((events & NoorEvents.StartedFollowing) != 0)
            {
                if ((events & NoorEvents.FirstFollow) != 0) subtitles.Show(followLine, lineSeconds);
                state.CompleteStep(RescueStep.Calm);
            }
        }

        private void Publish(RescueState state, bool force = false)
        {
            if (!force && model.State == written) return;
            written = model.State;
            state.SetNoorState(written);
        }

        private void Apply()
        {
            if (model.State != NoorState.Hidden) lastX = model.X;
            visual.enabled = model.State != NoorState.Hidden;
            transform.position = new Vector3(model.X, floorY, depth);
            visual.flipX = !model.FacingRight;
        }
    }
}
