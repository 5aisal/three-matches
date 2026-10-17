using LitMotion;
using ThreeMatches.Strips;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Future
{
    /// <summary>Full-strip glow overlay that follows the Future strip's rect and pulses on <see cref="Pulse"/>.</summary>
    public sealed class FutureStripGlow : MonoBehaviour
    {
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private Image glow;
        [SerializeField] private Color color = new Color(1f, 0.82f, 0.5f, 1f);
        [SerializeField, Range(0f, 1f)] private float peakAlpha = 0.35f;
        [SerializeField, Min(0.05f)] private float pulseSeconds = 1f;
        [SerializeField, Min(0.001f)] private float hiddenMaxHeight = 0.001f;

        private RectTransform glowRect;
        private MotionHandle pulseHandle;

        private void Awake()
        {
            glowRect = glow.rectTransform;
            glow.raycastTarget = false;
            SetAlpha(0f);
        }

        private void OnEnable()
        {
            layout.LayoutUpdated += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            layout.LayoutUpdated -= Refresh;
            if (pulseHandle.IsActive()) pulseHandle.Cancel();
            SetAlpha(0f);
        }

        /// <summary>Fades the glow up and back down over <c>pulseSeconds</c> (peak at the half-way point).</summary>
        public void Pulse()
        {
            if (pulseHandle.IsActive()) pulseHandle.Cancel();
            pulseHandle = LMotion.Create(0f, 1f, pulseSeconds)
                .WithEase(Ease.Linear)
                .Bind(this, static (t, self) => self.SetAlpha(self.peakAlpha * (1f - Mathf.Abs(2f * t - 1f))));
        }

        private void SetAlpha(float a)
        {
            glow.color = new Color(color.r, color.g, color.b, a);
        }

        private void Refresh()
        {
            Rect r = layout.GetRect(StripId.Future);
            bool visible = r.height > hiddenMaxHeight;
            if (glow.enabled != visible) glow.enabled = visible;
            glowRect.anchorMin = new Vector2(r.xMin, r.yMin);
            glowRect.anchorMax = new Vector2(r.xMax, r.yMax);
            glowRect.offsetMin = glowRect.offsetMax = Vector2.zero;
        }
    }
}
