using System;
using LitMotion;
using UnityEngine;

namespace ThreeMatches.Strips
{
    /// <summary>
    /// Owns the stage (1-3) and the active strip, and drives the three camera viewport rects.
    /// Layout rules live in <see cref="StripLayoutCalculator"/>.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class StripLayoutController : MonoBehaviour
    {
        [Header("Cameras (Past, Present, Future)")]
        [SerializeField] private Camera pastCamera;
        [SerializeField] private Camera presentCamera;
        [SerializeField] private Camera futureCamera;

        [Header("Layout")]
        [SerializeField, Range(1, 3)] private int startStage = 1;
        [SerializeField, Range(0.5f, 0.9f)] private float twoStripActiveFraction = 0.62f;
        [SerializeField, Range(0.34f, 0.8f)] private float threeStripActiveFraction = 0.5f;
        [SerializeField, Min(0.01f)] private float resizeDuration = 0.35f;
        [SerializeField] private Ease resizeEase = Ease.InOutCubic;
        [SerializeField, Min(0f)] private float hiddenCameraMaxHeight = 0.001f;

        [Header("Optional")]
        [SerializeField] private StripCrackTransition crack;

        private readonly Rect[] from = new Rect[StripLayoutCalculator.StripCount];
        private readonly Rect[] to = new Rect[StripLayoutCalculator.StripCount];
        private readonly Rect[] current = new Rect[StripLayoutCalculator.StripCount];
        private Camera[] cameras;
        private Transform[] cameraTransforms;
        private MotionHandle resizeHandle;
        private bool crackPlaying;

        public int Stage { get; private set; }
        public StripId ActiveStrip { get; private set; } = StripId.Present;
        public bool IsBusy => crackPlaying;

        /// <summary>Raised whenever any strip rect changed (every frame while resizing).</summary>
        public event Action LayoutUpdated;

        /// <summary>Raised when the crack transition has finished; the resize to the new layout starts right after.</summary>
        public event Action OnCrackFinished;

        public Rect GetRect(StripId strip) => current[(int)strip];

        public Camera GetCamera(StripId strip) => cameras[(int)strip];

        private void Awake()
        {
            cameras = new[] { pastCamera, presentCamera, futureCamera };
            cameraTransforms = new[] { pastCamera.transform, presentCamera.transform, futureCamera.transform };
            Stage = StripLayoutCalculator.ClampStage(startStage);
            ActiveStrip = StripLayoutCalculator.SanitizeActive(Stage, ActiveStrip);
            StripLayoutCalculator.Compute(Stage, ActiveStrip, twoStripActiveFraction, threeStripActiveFraction, to);
            Array.Copy(to, current, current.Length);
            ApplyRects();
        }

        private void OnDestroy()
        {
            if (resizeHandle.IsActive()) resizeHandle.Cancel();
        }

        /// <summary>Changes the stage. Going up plays the crack first; going down (debug) just resizes.</summary>
        public void SetStage(int stage)
        {
            stage = StripLayoutCalculator.ClampStage(stage);
            if (crackPlaying || stage == Stage) return;

            int previousStage = Stage;
            Stage = stage;
            ActiveStrip = StripLayoutCalculator.SanitizeActive(Stage, ActiveStrip);
            StripLayoutCalculator.Compute(Stage, ActiveStrip, twoStripActiveFraction, threeStripActiveFraction, to);

            if (crack != null && StripLayoutCalculator.TryGetNewStrip(previousStage, Stage, out var newStrip))
            {
                // The new split line sits on the new strip's edge that touches the strips above/below it.
                Rect r = to[(int)newStrip];
                float splitY = newStrip == StripId.Past ? r.yMin : r.yMax;
                crackPlaying = true;
                crack.Play(splitY, cameraTransforms, HandleCrackFinished);
            }
            else
            {
                StartResize();
            }
        }

        public void SetActiveStrip(StripId strip)
        {
            if (crackPlaying) return;
            strip = StripLayoutCalculator.SanitizeActive(Stage, strip);
            if (strip == ActiveStrip) return;

            ActiveStrip = strip;
            StripLayoutCalculator.Compute(Stage, ActiveStrip, twoStripActiveFraction, threeStripActiveFraction, to);
            StartResize();
        }

        /// <summary>Final choice (GAME_SPEC §15): the three strips resize to equal thirds.</summary>
        public void ShowEqualThirds()
        {
            if (crackPlaying) return;
            StripLayoutCalculator.Compute(3, ActiveStrip, 1f / 3f, 1f / 3f, to);
            StartResize();
        }

        /// <summary>Resizes back to the normal layout of the current stage.</summary>
        public void RestoreLayout()
        {
            if (crackPlaying) return;
            StripLayoutCalculator.Compute(Stage, ActiveStrip, twoStripActiveFraction, threeStripActiveFraction, to);
            StartResize();
        }

        public void CycleActiveStrip() => SetActiveStrip(StripLayoutCalculator.NextActive(Stage, ActiveStrip));

        private void HandleCrackFinished()
        {
            crackPlaying = false;
            OnCrackFinished?.Invoke();
            StartResize();
        }

        private void StartResize()
        {
            if (resizeHandle.IsActive()) resizeHandle.Cancel();
            Array.Copy(current, from, current.Length);
            resizeHandle = LMotion.Create(0f, 1f, resizeDuration)
                .WithEase(resizeEase)
                .Bind(this, static (t, self) => self.ApplyLerp(t));
        }

        private void ApplyLerp(float t)
        {
            for (int i = 0; i < current.Length; i++)
            {
                Rect a = from[i], b = to[i];
                current[i] = new Rect(
                    Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t),
                    Mathf.Lerp(a.width, b.width, t), Mathf.Lerp(a.height, b.height, t));
            }
            ApplyRects();
        }

        private void ApplyRects()
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                Rect r = current[i];
                bool visible = r.height > hiddenCameraMaxHeight;
                // Zero-height viewports are invalid for cameras, so hidden strips are switched off.
                if (visible) cameras[i].rect = r;
                if (cameras[i].enabled != visible) cameras[i].enabled = visible;
            }
            LayoutUpdated?.Invoke();
        }
    }
}
