using System;
using System.Collections;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Strips
{
    /// <summary>
    /// Placeholder "crack" (GAME_SPEC §4): white flash, a line spreading from the new split line,
    /// small camera shake. Real VFX and the bass hit come later.
    /// </summary>
    public sealed class StripCrackTransition : MonoBehaviour
    {
        [Header("Overlay (Screen Space - Overlay canvas)")]
        [SerializeField] private Image flashImage;
        [SerializeField] private RectTransform crackLine;

        [Header("Timing")]
        [SerializeField, Min(1)] private int flashFrames = 2;
        [SerializeField, Min(0.01f)] private float crackDuration = 0.4f;
        [SerializeField, Min(0.01f)] private float crackFadeDuration = 0.2f;

        [Header("Look")]
        [SerializeField, Min(1f)] private float lineThickness = 6f;
        [SerializeField] private Ease crackEase = Ease.OutCubic;

        [Header("Shake")]
        [SerializeField, Min(0f)] private float shakeAmplitude = 0.12f;
        [SerializeField, Min(0.01f)] private float shakeDuration = 0.4f;
        [SerializeField, Min(0.1f)] private float shakeFrequency = 35f;

        private Image crackLineImage;
        private Transform[] shakeTargets;
        private readonly Vector3[] shakeBase = new Vector3[StripLayoutCalculator.StripCount];
        private Action onFinished;
        private MotionHandle crackHandle, fadeHandle, shakeHandle;
        private Coroutine routine;
        private float splitY;

        public bool IsPlaying => routine != null;

        private void Awake()
        {
            crackLineImage = crackLine.GetComponent<Image>();
            flashImage.enabled = false;
            crackLineImage.enabled = false;
        }

        private void OnDestroy()
        {
            CancelHandles();
        }

        public void Play(float newSplitY, Transform[] cameraTransforms, Action finished)
        {
            if (routine != null) return;
            splitY = newSplitY;
            shakeTargets = cameraTransforms;
            onFinished = finished;
            routine = StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            flashImage.enabled = true;
            for (int i = 0; i < flashFrames; i++) yield return null;
            flashImage.enabled = false;

            crackLine.anchorMin = new Vector2(0f, splitY);
            crackLine.anchorMax = new Vector2(1f, splitY);
            crackLine.pivot = new Vector2(0.5f, 0.5f);
            crackLine.anchoredPosition = Vector2.zero;
            crackLine.sizeDelta = new Vector2(0f, lineThickness);
            crackLine.localScale = new Vector3(0f, 1f, 1f);
            SetLineAlpha(1f);
            crackLineImage.enabled = true;

            for (int i = 0; i < shakeTargets.Length; i++) shakeBase[i] = shakeTargets[i].localPosition;
            shakeHandle = LMotion.Create(0f, 1f, shakeDuration)
                .WithEase(Ease.Linear)
                .WithOnComplete(RestoreShakeTargets)
                .Bind(this, static (t, self) => self.ApplyShake(t));

            crackHandle = LMotion.Create(0f, 1f, crackDuration)
                .WithEase(crackEase)
                .Bind(this, static (t, self) => self.crackLine.localScale = new Vector3(t, 1f, 1f));
            yield return crackHandle.ToYieldInstruction();

            fadeHandle = LMotion.Create(1f, 0f, crackFadeDuration)
                .Bind(this, static (a, self) => self.SetLineAlpha(a));
            yield return fadeHandle.ToYieldInstruction();

            crackLineImage.enabled = false;
            if (shakeHandle.IsActive()) yield return shakeHandle.ToYieldInstruction();

            routine = null;
            onFinished?.Invoke();
        }

        private void ApplyShake(float t)
        {
            float falloff = (1f - t) * (1f - t);
            float time = t * shakeDuration * shakeFrequency;
            for (int i = 0; i < shakeTargets.Length; i++)
            {
                // A different noise lane per camera so the strips don't move in lockstep.
                float nx = Mathf.PerlinNoise(time, i * 17.3f) - 0.5f;
                float ny = Mathf.PerlinNoise(i * 31.7f, time) - 0.5f;
                shakeTargets[i].localPosition = shakeBase[i] + new Vector3(nx, ny, 0f) * (2f * shakeAmplitude * falloff);
            }
        }

        private void RestoreShakeTargets()
        {
            for (int i = 0; i < shakeTargets.Length; i++) shakeTargets[i].localPosition = shakeBase[i];
        }

        private void SetLineAlpha(float a)
        {
            Color c = crackLineImage.color;
            c.a = a;
            crackLineImage.color = c;
        }

        private void CancelHandles()
        {
            if (crackHandle.IsActive()) crackHandle.Cancel();
            if (fadeHandle.IsActive()) fadeHandle.Cancel();
            if (shakeHandle.IsActive()) shakeHandle.Cancel();
        }
    }
}
