using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Flow
{
    /// <summary>A placeholder figure on an ending panel: a coloured block with a small name label.</summary>
    internal sealed class Actor
    {
        private readonly RectTransform rect;
        private readonly Image image;
        private readonly TMP_Text label;
        private MotionHandle moveHandle, fadeHandle;
        private Vector2 position;

        public Actor(RectTransform parent, TMP_FontAsset font, string name, Color color, Vector2 size, Vector2 normalizedPosition, float labelSize)
        {
            image = UiKit.CreateImage(name, parent, color);
            rect = image.rectTransform;
            rect.sizeDelta = size;
            label = UiKit.CreateText("Label", rect, font, name, labelSize, new Color(1f, 1f, 1f, 0.7f));
            label.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            label.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            label.rectTransform.pivot = new Vector2(0.5f, 1f);
            label.rectTransform.sizeDelta = new Vector2(size.x + 160f, labelSize * 1.6f);
            SetPosition(normalizedPosition);
        }

        public Color Color
        {
            get => image.color;
            set => image.color = value;
        }

        public void SetPosition(Vector2 normalized)
        {
            position = normalized;
            rect.anchorMin = rect.anchorMax = normalized;
            rect.anchoredPosition = Vector2.zero;
        }

        public MotionHandle MoveTo(Vector2 target, float seconds, Ease ease = Ease.InOutSine)
        {
            if (moveHandle.IsActive()) moveHandle.Cancel();
            moveHandle = LMotion.Create(position, target, seconds)
                .WithEase(ease)
                .Bind(this, static (p, self) => self.SetPosition(p));
            return moveHandle;
        }

        public MotionHandle FadeTo(float alpha, float seconds)
        {
            if (fadeHandle.IsActive()) fadeHandle.Cancel();
            fadeHandle = LMotion.Create(image.color.a, alpha, seconds)
                .WithEase(Ease.InOutSine)
                .Bind(this, static (a, self) => self.SetAlpha(a));
            return fadeHandle;
        }

        public void SetAlpha(float a)
        {
            Color c = image.color;
            c.a = a;
            image.color = c;
            label.alpha = a * 0.7f;
        }

        public void Cancel()
        {
            if (moveHandle.IsActive()) moveHandle.Cancel();
            if (fadeHandle.IsActive()) fadeHandle.Cancel();
        }
    }

    /// <summary>One strip's ending overlay: a backdrop that can fade in over the live world, placeholder actors and a caption.</summary>
    internal sealed class StripPanel
    {
        private readonly RectTransform root;
        private readonly RectTransform stage;
        private readonly Image backdrop;
        private readonly TMP_Text caption;
        private readonly TMP_FontAsset font;
        private readonly System.Collections.Generic.List<Actor> actors = new System.Collections.Generic.List<Actor>(8);
        private MotionHandle backdropHandle, captionHandle;

        public StripPanel(Transform parent, TMP_FontAsset font, string name, float captionSize)
        {
            this.font = font;
            root = UiKit.CreateRect(name, parent);
            root.gameObject.AddComponent<RectMask2D>();
            backdrop = UiKit.CreateImage("Backdrop", root, new Color(0f, 0f, 0f, 0f));
            UiKit.Stretch(backdrop.rectTransform);
            stage = UiKit.CreateRect("Stage", root);
            UiKit.Stretch(stage);
            caption = UiKit.CreateText("Caption", root, font, "", captionSize, Color.white);
            caption.rectTransform.anchorMin = new Vector2(0.05f, 0.04f);
            caption.rectTransform.anchorMax = new Vector2(0.95f, 0.04f);
            caption.rectTransform.pivot = new Vector2(0.5f, 0f);
            caption.rectTransform.sizeDelta = new Vector2(0f, captionSize * 1.8f);
            caption.alpha = 0f;
            root.gameObject.SetActive(false);
        }

        public void Follow(Rect viewport)
        {
            root.anchorMin = viewport.min;
            root.anchorMax = viewport.max;
            root.offsetMin = root.offsetMax = Vector2.zero;
        }

        public void SetActive(bool active) => root.gameObject.SetActive(active);

        public Actor AddActor(string name, Color color, Vector2 size, Vector2 normalizedPosition, float labelSize = 26f)
        {
            var actor = new Actor(stage, font, name, color, size, normalizedPosition, labelSize);
            actors.Add(actor);
            return actor;
        }

        public MotionHandle FadeBackdrop(Color color, float seconds)
        {
            if (backdropHandle.IsActive()) backdropHandle.Cancel();
            Color from = backdrop.color;
            backdropHandle = LMotion.Create(0f, 1f, seconds)
                .WithEase(Ease.InOutSine)
                .Bind(this, (t, self) => self.backdrop.color = Color.Lerp(from, color, t));
            return backdropHandle;
        }

        public void SetCaption(string text, float fadeSeconds = 0.5f)
        {
            if (captionHandle.IsActive()) captionHandle.Cancel();
            caption.text = text;
            captionHandle = LMotion.Create(caption.alpha, string.IsNullOrEmpty(text) ? 0f : 1f, fadeSeconds)
                .WithEase(Ease.InOutSine)
                .Bind(caption, static (a, label) => label.alpha = a);
        }

        /// <summary>Removes actors and resets backdrop and caption, ready for the next ending.</summary>
        public void Clear()
        {
            if (backdropHandle.IsActive()) backdropHandle.Cancel();
            if (captionHandle.IsActive()) captionHandle.Cancel();
            for (int i = 0; i < actors.Count; i++) actors[i].Cancel();
            for (int i = stage.childCount - 1; i >= 0; i--) Object.Destroy(stage.GetChild(i).gameObject);
            actors.Clear();
            backdrop.color = new Color(0f, 0f, 0f, 0f);
            caption.alpha = 0f;
            caption.text = "";
            root.gameObject.SetActive(false);
        }
    }
}
