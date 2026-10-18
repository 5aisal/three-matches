using System;
using LitMotion;
using ThreeMatches.Core;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.Memories
{
    /// <summary>
    /// One framed photo on the Memory Wall: placeholder photo quad, burn animation, selection pop.
    /// The photo is a sibling of the frame (not a child) because the frame quad is non-uniformly scaled.
    /// </summary>
    public sealed class MemoryFrame : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BurnId = Shader.PropertyToID("_Burn");

        [SerializeField] private MemoryId memory;
        [SerializeField, TextArea] private string caption = "";
        [SerializeField] private string burnedCaption = "...";
        [SerializeField] private Interactable interactable;
        [SerializeField] private Renderer photoRenderer;
        [SerializeField] private Color photoColor = Color.white;
        [SerializeField] private Vector3 hitSize = new Vector3(0.7f, 0.9f, 0.4f);

        [Header("Burn (placeholder)")]
        [SerializeField, Min(0.1f)] private float pulseScale = 0.12f;
        [SerializeField, Min(1)] private int pulseCount = 3;
        [SerializeField] private Ease burnEase = Ease.InOutSine;

        [Header("Selection / refuse")]
        [SerializeField, Min(1f)] private float selectedScale = 1.1f;
        [SerializeField, Min(0.01f)] private float selectDuration = 0.12f;
        [SerializeField, Min(0f)] private float refuseShake = 0.035f;
        [SerializeField, Min(0.01f)] private float refuseDuration = 0.22f;

        private MaterialPropertyBlock block;
        private Transform photoTransform;
        private Vector3 photoBaseScale;
        private Vector3 photoBasePosition;
        private Vector3 frameBasePosition;
        private float pulse = 1f;
        private float selectT;
        private MotionHandle burnHandle, selectHandle, refuseHandle;
        private Bounds hitBounds;

        public MemoryId Memory => memory;
        public Interactable Interactable => interactable;
        public bool IsBurned { get; private set; }
        public string OriginalCaption => caption;
        public string CurrentCaption => IsBurned ? burnedCaption : caption;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            photoTransform = photoRenderer.transform;
            photoBaseScale = photoTransform.localScale;
            photoBasePosition = photoTransform.localPosition;
            frameBasePosition = transform.localPosition;
            hitBounds = new Bounds(transform.position, hitSize);
            ApplyTint(0f);
        }

        private void OnDestroy()
        {
            CancelMotions();
        }

        /// <summary>Distance along the ray to the frame, or a negative number if missed.</summary>
        public float Raycast(Ray ray) => hitBounds.IntersectRay(ray, out float distance) ? distance : -1f;

        /// <summary>Snaps to the burned/unburned look (also used after debug jumps).</summary>
        public void SetBurned(bool burned)
        {
            CancelMotions();
            IsBurned = burned;
            pulse = 1f;
            selectT = 0f;
            ApplyTint(0f);
            ApplyTransforms(0f);
            photoRenderer.enabled = !burned;
        }

        public void SetSelected(bool selected)
        {
            if (IsBurned && selected) return;
            if (selectHandle.IsActive()) selectHandle.Cancel();
            selectHandle = LMotion.Create(selectT, selected ? 1f : 0f, selectDuration)
                .WithEase(Ease.OutCubic)
                .Bind(this, static (t, self) =>
                {
                    self.selectT = t;
                    self.ApplyScale();
                });
        }

        /// <summary>Tint to black with a few scale pulses; the frame is empty afterwards.</summary>
        public void PlayBurn(float duration, Action finished)
        {
            CancelMotions();
            burnHandle = LMotion.Create(0f, 1f, duration)
                .WithEase(burnEase)
                .WithOnComplete(() =>
                {
                    IsBurned = true;
                    photoRenderer.enabled = false;
                    finished?.Invoke();
                })
                .Bind(this, static (t, self) =>
                {
                    self.ApplyTint(t);
                    self.pulse = 1f + self.pulseScale * Mathf.Sin(t * Mathf.PI * self.pulseCount) * (1f - t);
                    self.ApplyScale();
                });
        }

        /// <summary>Small sideways shake to say "no" (picking a burned frame).</summary>
        public void PlayRefuse()
        {
            if (refuseHandle.IsActive()) refuseHandle.Cancel();
            refuseHandle = LMotion.Create(0f, 1f, refuseDuration)
                .WithEase(Ease.Linear)
                .WithOnComplete(() => ApplyTransforms(0f))
                .Bind(this, static (t, self) => self.ApplyTransforms(Mathf.Sin(t * Mathf.PI * 4f) * (1f - t)));
        }

        private void ApplyTint(float burnT)
        {
            photoRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, photoColor);
            block.SetFloat(BurnId, burnT);
            photoRenderer.SetPropertyBlock(block);
        }

        private void ApplyScale()
        {
            photoTransform.localScale = photoBaseScale * (pulse * Mathf.Lerp(1f, selectedScale, selectT));
        }

        private void ApplyTransforms(float shake)
        {
            Vector3 offset = new Vector3(shake * refuseShake, 0f, 0f);
            transform.localPosition = frameBasePosition + offset;
            photoTransform.localPosition = photoBasePosition + offset;
            ApplyScale();
        }

        private void CancelMotions()
        {
            if (burnHandle.IsActive()) burnHandle.Cancel();
            if (selectHandle.IsActive()) selectHandle.Cancel();
            if (refuseHandle.IsActive()) refuseHandle.Cancel();
        }
    }
}
