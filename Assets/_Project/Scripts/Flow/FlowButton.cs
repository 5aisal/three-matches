using System;
using TMPro;
using ThreeMatches.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Flow
{
    /// <summary>A mouse button for the flow screens. Polls the pointer itself, so no EventSystem is needed.</summary>
    public sealed class FlowButton : MonoBehaviour
    {
        private GameInput input;
        private RectTransform rect;
        private Image background;
        private TMP_Text label;
        private Color normal, hover;
        private bool interactable = true;

        public event Action Clicked;

        internal static FlowButton Create(Transform parent, GameInput input, TMP_FontAsset font, string text,
            Vector2 size, Color normal, Color hover)
        {
            Image bg = UiKit.CreateImage("Button_" + text, parent, normal);
            var button = bg.gameObject.AddComponent<FlowButton>();
            button.input = input;
            button.rect = bg.rectTransform;
            button.rect.sizeDelta = size;
            button.background = bg;
            button.normal = normal;
            button.hover = hover;
            button.label = UiKit.CreateText("Label", bg.transform, font, text, 38f, Color.white);
            UiKit.Stretch(button.label.rectTransform);
            return button;
        }

        public RectTransform Rect => rect;

        public void SetInteractable(bool value) => interactable = value;

        private void Update()
        {
            if (!interactable) return;

            Vector2 pointer = input.Point.ReadValue<Vector2>();
            bool over = RectTransformUtility.RectangleContainsScreenPoint(rect, pointer, null);
            background.color = over ? hover : normal;
            if (over && input.Click.WasPressedThisFrame()) Clicked?.Invoke();
        }
    }
}
