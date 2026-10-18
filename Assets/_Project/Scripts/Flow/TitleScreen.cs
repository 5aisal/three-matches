using System;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeMatches.Flow
{
    /// <summary>"THREE MATCHES" + "Press any key" (GAME_SPEC §17, §18).</summary>
    public sealed class TitleScreen : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float inputDelaySeconds = 0.4f;
        [SerializeField, Min(0.05f)] private float fadeOutSeconds = 0.8f;
        [SerializeField, Min(0.1f)] private float promptPulseSeconds = 1.2f;
        [SerializeField, Range(0f, 1f)] private float promptMinAlpha = 0.25f;
        [SerializeField] private float titleSize = 150f;
        [SerializeField] private float promptSize = 44f;
        [SerializeField] private Color titleColor = new Color(0.95f, 0.78f, 0.5f);

        private CanvasGroup group;
        private TMP_Text prompt;
        private MotionHandle pulseHandle, fadeHandle;
        private Action dismissed;
        private float shownAt;
        private bool waiting;

        internal void Build(TMP_FontAsset font, Canvas topCanvas)
        {
            RectTransform root = UiKit.CreateRect("Title", topCanvas.transform);
            UiKit.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            Image bg = UiKit.CreateImage("Black", root, Color.black);
            UiKit.Stretch(bg.rectTransform);

            TMP_Text title = UiKit.CreateText("Name", root, font, "THREE MATCHES", titleSize, titleColor);
            title.characterSpacing = 18f;
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.5f, 0.58f);
            title.rectTransform.sizeDelta = new Vector2(1700f, 220f);

            prompt = UiKit.CreateText("Prompt", root, font, "Press any key", promptSize, Color.white);
            prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
            prompt.rectTransform.sizeDelta = new Vector2(800f, 80f);

            root.gameObject.SetActive(false);
        }

        public void Show(Action onDismissed)
        {
            dismissed = onDismissed;
            group.gameObject.SetActive(true);
            group.alpha = 1f;
            shownAt = Time.unscaledTime;
            waiting = true;
            pulseHandle = LMotion.Create(1f, promptMinAlpha, promptPulseSeconds)
                .WithEase(Ease.InOutSine)
                .WithLoops(-1, LoopType.Yoyo)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(prompt, static (a, label) => label.alpha = a);
        }

        private void OnDestroy()
        {
            if (pulseHandle.IsActive()) pulseHandle.Cancel();
            if (fadeHandle.IsActive()) fadeHandle.Cancel();
        }

        private void Update()
        {
            if (!waiting || Time.unscaledTime - shownAt < inputDelaySeconds) return;
            if (!AnyInputThisFrame()) return;

            waiting = false;
            if (pulseHandle.IsActive()) pulseHandle.Cancel();
            dismissed?.Invoke();
            fadeHandle = LMotion.Create(1f, 0f, fadeOutSeconds)
                .WithEase(Ease.InOutSine)
                .WithOnComplete(() => group.gameObject.SetActive(false))
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(group, static (a, g) => g.alpha = a);
        }

        private static bool AnyInputThisFrame()
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            return (kb != null && kb.anyKey.wasPressedThisFrame)
                || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        }
    }
}
