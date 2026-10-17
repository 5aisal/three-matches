using LitMotion;
using UnityEngine;

namespace ThreeMatches.UI
{
    /// <summary>Fades a CanvasGroup with LitMotion; shared by every panel in this assembly.</summary>
    internal struct FadeGroup
    {
        private readonly CanvasGroup group;
        private MotionHandle handle;

        public FadeGroup(CanvasGroup group)
        {
            this.group = group;
            handle = default;
        }

        public void SnapTo(float alpha)
        {
            if (handle.IsActive()) handle.Cancel();
            group.alpha = alpha;
        }

        public void To(float alpha, float duration)
        {
            if (handle.IsActive()) handle.Cancel();
            CanvasGroup g = group;
            handle = LMotion.Create(g.alpha, alpha, duration)
                .WithEase(Ease.InOutSine)
                .Bind(g, static (a, target) => target.alpha = a);
        }
    }
}
