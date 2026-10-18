using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Flow
{
    /// <summary>Builds the flow screens' uGUI in code, so the scene needs no hand-made canvases.</summary>
    internal static class UiKit
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        public static Canvas CreateCanvas(string name, Transform parent, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        public static Image CreateImage(string name, Transform parent, Color color)
        {
            RectTransform rt = CreateRect(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, string text, float size,
            Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rt = CreateRect(name, parent);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }
    }
}
