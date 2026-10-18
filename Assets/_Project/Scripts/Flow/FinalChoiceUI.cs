using System;
using TMPro;
using ThreeMatches.Core;
using ThreeMatches.Player;
using ThreeMatches.Strips;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Flow
{
    /// <summary>
    /// Final choice (GAME_SPEC §15): every strip glows, hovering one shows its text, click chooses.
    /// Runs on unscaled time because the game is slowed to 0.2 while it is open.
    /// </summary>
    public sealed class FinalChoiceUI : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float inputDelaySeconds = 0.6f;
        [SerializeField, Min(0.1f)] private float pulseSeconds = 1.6f;
        [SerializeField, Range(0f, 1f)] private float glowMin = 0.08f;
        [SerializeField, Range(0f, 1f)] private float glowMax = 0.2f;
        [SerializeField, Range(0f, 1f)] private float glowHover = 0.38f;
        [SerializeField, Min(0.1f)] private float smoothing = 12f;
        [SerializeField] private float textSize = 52f;
        [SerializeField, Min(0f)] private float hintAfterSeconds = 4f;
        [SerializeField] private string hintText = "Choose.";
        [SerializeField] private Color unavailableColor = new Color(0.55f, 0.55f, 0.55f);

        private static readonly EndingId[] EndingByStrip =
        {
            EndingId.WalkIntoFire, EndingId.SaveHerForgetHer, EndingId.KeepHer
        };

        private readonly Color[] glowColors =
        {
            new Color(1f, 0.6f, 0.2f), new Color(0.5f, 0.7f, 1f), new Color(0.92f, 0.9f, 0.82f)
        };

        private readonly Image[] glows = new Image[StripLayoutCalculator.StripCount];
        private readonly TMP_Text[] texts = new TMP_Text[StripLayoutCalculator.StripCount];
        private readonly float[] hoverAmount = new float[StripLayoutCalculator.StripCount];
        private GameInput input;
        private StripLayoutController layout;
        private RescueState state;
        private RectTransform root;
        private TMP_Text hint;
        private Action<EndingId> chosen;
        private float shownAt;
        private float lastHoverAt;
        private bool active;

        public bool IsOpen => active;

        internal void Build(TMP_FontAsset font, Canvas topCanvas, GameInput gameInput, StripLayoutController stripLayout)
        {
            input = gameInput;
            layout = stripLayout;
            root = UiKit.CreateRect("FinalChoice", topCanvas.transform);
            UiKit.Stretch(root);

            for (int i = 0; i < glows.Length; i++)
            {
                glows[i] = UiKit.CreateImage("Glow_" + (StripId)i, root, glowColors[i]);
                texts[i] = UiKit.CreateText("Text_" + (StripId)i, root, font, "", textSize, Color.white);
                texts[i].alpha = 0f;
            }

            hint = UiKit.CreateText("Hint", root, font, hintText, 40f, new Color(1f, 1f, 1f, 0f));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            hint.rectTransform.sizeDelta = new Vector2(600f, 60f);

            root.gameObject.SetActive(false);
        }

        public void Show(RescueState rescueState, Action<EndingId> onChosen)
        {
            state = rescueState;
            chosen = onChosen;
            shownAt = lastHoverAt = Time.unscaledTime;
            active = true;
            root.gameObject.SetActive(true);
            for (int i = 0; i < hoverAmount.Length; i++)
            {
                hoverAmount[i] = 0f;
                texts[i].text = FinalChoiceRules.HoverText(state, EndingByStrip[i]);
                texts[i].color = EndingRules.IsAvailable(state, EndingByStrip[i]) ? Color.white : unavailableColor;
                texts[i].alpha = 0f;
            }
            hint.alpha = 0f;
            Layout();
        }

        public void Hide()
        {
            active = false;
            root.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!active) return;

            float dt = Time.unscaledDeltaTime;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / pulseSeconds));
            int hovered = HoveredStrip();
            if (hovered >= 0) lastHoverAt = Time.unscaledTime;

            for (int i = 0; i < glows.Length; i++)
            {
                bool available = EndingRules.IsAvailable(state, EndingByStrip[i]);
                hoverAmount[i] = Mathf.MoveTowards(hoverAmount[i], i == hovered ? 1f : 0f, smoothing * dt);

                float baseGlow = Mathf.Lerp(glowMin, glowMax, pulse);
                float glow = Mathf.Lerp(baseGlow, glowHover, hoverAmount[i]);
                Color c = available ? glowColors[i] : unavailableColor;
                c.a = available ? glow : glow * 0.5f;
                glows[i].color = c;
                texts[i].alpha = hoverAmount[i];
            }

            bool showHint = hovered < 0 && Time.unscaledTime - lastHoverAt > hintAfterSeconds;
            hint.alpha = Mathf.MoveTowards(hint.alpha, showHint ? 0.7f : 0f, 2f * dt);

            Layout();

            if (hovered < 0 || Time.unscaledTime - shownAt < inputDelaySeconds) return;
            EndingId ending = EndingByStrip[hovered];
            if (input.Click.WasPressedThisFrame() && EndingRules.IsAvailable(state, ending))
            {
                active = false;
                chosen?.Invoke(ending);
            }
        }

        private int HoveredStrip()
        {
            Vector2 pointer = input.Point.ReadValue<Vector2>();
            var normalized = new Vector2(pointer.x / Screen.width, pointer.y / Screen.height);
            for (int i = 0; i < glows.Length; i++)
            {
                if (layout.GetRect((StripId)i).Contains(normalized)) return i;
            }
            return -1;
        }

        private void Layout()
        {
            for (int i = 0; i < glows.Length; i++)
            {
                Rect r = layout.GetRect((StripId)i);
                RectTransform g = glows[i].rectTransform;
                g.anchorMin = r.min;
                g.anchorMax = r.max;
                g.offsetMin = g.offsetMax = Vector2.zero;

                RectTransform t = texts[i].rectTransform;
                t.anchorMin = new Vector2(0.1f, r.center.y);
                t.anchorMax = new Vector2(0.9f, r.center.y);
                t.offsetMin = new Vector2(0f, -60f);
                t.offsetMax = new Vector2(0f, 60f);
            }
        }
    }
}
