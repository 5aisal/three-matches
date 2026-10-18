using System;
using System.Collections;
using LitMotion;
using TMPro;
using ThreeMatches.Core;
using ThreeMatches.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Flow
{
    /// <summary>Ending title card with "Choose again" / "Credits" buttons, and the scrolling credits screen (GAME_SPEC §16, §18).</summary>
    public sealed class EndCardUI : MonoBehaviour
    {
        [Header("Title card")]
        [SerializeField, Min(0.05f)] private float cardFadeSeconds = 1.2f;
        [SerializeField, Min(0f)] private float buttonsDelaySeconds = 2.5f;
        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.6f;
        [SerializeField] private float titleSize = 84f;
        [SerializeField] private float sublineSize = 44f;
        [SerializeField] private Vector2 buttonSize = new Vector2(420f, 90f);
        [SerializeField] private Color buttonColor = new Color(0.12f, 0.1f, 0.09f, 0.9f);
        [SerializeField] private Color buttonHoverColor = new Color(0.4f, 0.28f, 0.14f, 0.95f);

        [Header("Credits")]
        [SerializeField, Min(1f)] private float creditsScrollSpeed = 70f;
        [SerializeField] private float creditsTextSize = 34f;
        [SerializeField] private float creditsWidth = 1300f;

        private CanvasGroup cardGroup, creditsGroup;
        private TMP_Text title, subline, creditsText;
        private FlowButton chooseAgain, credits, back;
        private RectTransform creditsRect;
        private MotionHandle cardHandle, scrollHandle;
        private Coroutine routine;
        private string creditsRichText = "";

        public event Action ChooseAgainClicked;
        public event Action CreditsClicked;
        public event Action BackClicked;

        internal void Build(TMP_FontAsset font, Canvas topCanvas, GameInput input, string creditsMarkdown)
        {
            creditsRichText = CreditsFormatter.ToRichText(creditsMarkdown);

            RectTransform card = UiKit.CreateRect("EndCard", topCanvas.transform);
            UiKit.Stretch(card);
            cardGroup = card.gameObject.AddComponent<CanvasGroup>();

            Image dim = UiKit.CreateImage("Dim", card, new Color(0f, 0f, 0f, dimAlpha));
            UiKit.Stretch(dim.rectTransform);

            title = UiKit.CreateText("Title", card, font, "", titleSize, new Color(0.96f, 0.85f, 0.65f));
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
            title.rectTransform.sizeDelta = new Vector2(1600f, 260f);

            subline = UiKit.CreateText("Subline", card, font, "", sublineSize, new Color(1f, 1f, 1f, 0.85f));
            subline.rectTransform.anchorMin = subline.rectTransform.anchorMax = new Vector2(0.5f, 0.45f);
            subline.rectTransform.sizeDelta = new Vector2(1500f, 140f);

            chooseAgain = FlowButton.Create(card, input, font, "Choose again", buttonSize, buttonColor, buttonHoverColor);
            Place(chooseAgain.Rect, new Vector2(0.4f, 0.25f));
            credits = FlowButton.Create(card, input, font, "Credits", buttonSize, buttonColor, buttonHoverColor);
            Place(credits.Rect, new Vector2(0.6f, 0.25f));
            chooseAgain.Clicked += () => ChooseAgainClicked?.Invoke();
            credits.Clicked += () => CreditsClicked?.Invoke();

            RectTransform creditsRoot = UiKit.CreateRect("Credits", topCanvas.transform);
            UiKit.Stretch(creditsRoot);
            creditsGroup = creditsRoot.gameObject.AddComponent<CanvasGroup>();
            Image black = UiKit.CreateImage("Black", creditsRoot, Color.black);
            UiKit.Stretch(black.rectTransform);

            creditsText = UiKit.CreateText("Text", creditsRoot, font, creditsRichText, creditsTextSize, new Color(0.92f, 0.9f, 0.85f),
                TextAlignmentOptions.Top);
            creditsText.richText = true;
            creditsRect = creditsText.rectTransform;

            back = FlowButton.Create(creditsRoot, input, font, "Back", new Vector2(260f, 80f), buttonColor, buttonHoverColor);
            back.Rect.anchorMin = back.Rect.anchorMax = new Vector2(0.92f, 0.08f);
            back.Rect.anchoredPosition = Vector2.zero;
            back.Clicked += () => BackClicked?.Invoke();

            card.gameObject.SetActive(false);
            creditsRoot.gameObject.SetActive(false);
        }

        private static void Place(RectTransform rt, Vector2 anchor)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;
        }

        /// <summary>Fades the card in, then the buttons after a short pause.</summary>
        public void ShowTitleCard(string titleText, string sublineText)
        {
            title.text = titleText;
            subline.text = sublineText;
            SetButtonsVisible(false);
            cardGroup.gameObject.SetActive(true);
            FadeCard(1f);
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShowButtonsLater());
        }

        public void HideTitleCard()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            if (cardHandle.IsActive()) cardHandle.Cancel();
            cardGroup.alpha = 0f;
            cardGroup.gameObject.SetActive(false);
        }

        /// <summary>Hides the card but keeps it ready (the credits cover it).</summary>
        public void SetCardVisible(bool visible)
        {
            cardGroup.gameObject.SetActive(visible);
            if (visible) cardGroup.alpha = 1f;
        }

        public void ShowCredits()
        {
            creditsGroup.gameObject.SetActive(true);
            creditsGroup.alpha = 1f;
            creditsRect.anchorMin = new Vector2(0.5f, 0f);
            creditsRect.anchorMax = new Vector2(0.5f, 0f);
            creditsRect.pivot = new Vector2(0.5f, 1f);

            float height = creditsText.GetPreferredValues(creditsRichText, creditsWidth, 0f).y;
            creditsRect.sizeDelta = new Vector2(creditsWidth, height);

            float screenHeight = UiKit.ReferenceResolution.y;
            float startY = 0f;
            float endY = height + screenHeight;
            if (scrollHandle.IsActive()) scrollHandle.Cancel();
            scrollHandle = LMotion.Create(startY, endY, endY / creditsScrollSpeed)
                .WithEase(Ease.Linear)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(creditsRect, static (y, rt) => rt.anchoredPosition = new Vector2(0f, y));
        }

        public void HideCredits()
        {
            if (scrollHandle.IsActive()) scrollHandle.Cancel();
            creditsGroup.gameObject.SetActive(false);
        }

        private IEnumerator ShowButtonsLater()
        {
            yield return new WaitForSecondsRealtime(buttonsDelaySeconds);
            SetButtonsVisible(true);
            routine = null;
        }

        private void SetButtonsVisible(bool visible)
        {
            chooseAgain.gameObject.SetActive(visible);
            credits.gameObject.SetActive(visible);
        }

        private void FadeCard(float alpha)
        {
            if (cardHandle.IsActive()) cardHandle.Cancel();
            cardGroup.alpha = 0f;
            cardHandle = LMotion.Create(0f, alpha, cardFadeSeconds)
                .WithEase(Ease.InOutSine)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(cardGroup, static (a, g) => g.alpha = a);
        }

        private void OnDestroy()
        {
            if (cardHandle.IsActive()) cardHandle.Cancel();
            if (scrollHandle.IsActive()) scrollHandle.Cancel();
        }
    }
}
