using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThreeMatches.Strips
{
    /// <summary>
    /// "Match is out" look for the Past strip: a local URP Volume (desaturate + darken) around
    /// Cam_Past whose weight is faded. The Volume's profile holds the actual grade values.
    /// </summary>
    public sealed class StripPastLook : MonoBehaviour
    {
        [SerializeField] private Volume pastVolume;
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;
        [SerializeField] private Ease fadeEase = Ease.InOutCubic;

        private MotionHandle handle;

        public bool IsInactive { get; private set; }

        private void Awake()
        {
            pastVolume.weight = 0f;
        }

        private void OnDestroy()
        {
            if (handle.IsActive()) handle.Cancel();
        }

        public void SetInactive(bool inactive, bool instant = false)
        {
            IsInactive = inactive;
            if (handle.IsActive()) handle.Cancel();

            float target = inactive ? 1f : 0f;
            if (instant || fadeDuration <= 0f)
            {
                pastVolume.weight = target;
                return;
            }

            handle = LMotion.Create(pastVolume.weight, target, fadeDuration)
                .WithEase(fadeEase)
                .Bind(pastVolume, static (w, v) => v.weight = w);
        }
    }
}
