using TMPro;
using UnityEngine;

namespace ThreeMatches.Strips
{
    /// <summary>Keeps one year label pinned to the top-left corner of each strip.</summary>
    public sealed class StripYearLabels : MonoBehaviour
    {
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private TMP_Text pastLabel;
        [SerializeField] private TMP_Text presentLabel;
        [SerializeField] private TMP_Text futureLabel;
        [SerializeField] private Vector2 cornerOffset = new Vector2(24f, -16f);
        [SerializeField, Range(0f, 1f)] private float labelOpacity = 0.6f;
        [SerializeField, Min(0.001f)] private float hiddenMaxHeight = 0.001f;

        private TMP_Text[] labels;
        private RectTransform[] labelRects;

        private void Awake()
        {
            labels = new[] { pastLabel, presentLabel, futureLabel };
            labelRects = new RectTransform[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                labelRects[i] = labels[i].rectTransform;
                labels[i].alpha = labelOpacity;
            }
        }

        private void OnEnable()
        {
            layout.LayoutUpdated += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            layout.LayoutUpdated -= Refresh;
        }

        private void Refresh()
        {
            for (int i = 0; i < labels.Length; i++)
            {
                Rect r = layout.GetRect((StripId)i);
                bool visible = r.height > hiddenMaxHeight;
                if (labels[i].enabled != visible) labels[i].enabled = visible;

                // Top-left of the strip, in normalized canvas space (the canvas is full screen).
                labelRects[i].anchorMin = labelRects[i].anchorMax = new Vector2(r.xMin, r.yMax);
                labelRects[i].pivot = new Vector2(0f, 1f);
                labelRects[i].anchoredPosition = cornerOffset;
            }
        }
    }
}
