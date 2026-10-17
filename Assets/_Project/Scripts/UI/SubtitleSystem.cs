using System.Collections;
using TMPro;
using UnityEngine;

namespace ThreeMatches.UI
{
    /// <summary>Bottom-centre subtitles: serif text on a dark backing. A new line replaces the old one.</summary>
    public sealed class SubtitleSystem : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform backing;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Vector2 padding = new Vector2(36f, 18f);
        [SerializeField, Min(100f)] private float maxTextWidth = 1200f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.25f;
        [SerializeField, Min(0f)] private float minSeconds = 1.5f;
        [SerializeField, Min(0f)] private float secondsPerCharacter = 0.06f;

        private FadeGroup fade;
        private Coroutine hideRoutine;

        private void Awake()
        {
            fade = new FadeGroup(group);
            fade.SnapTo(0f);
        }

        /// <summary>Shows a line. Pass seconds &lt;= 0 to size the time from the text length.</summary>
        public void Show(string text, float seconds = 0f)
        {
            if (hideRoutine != null) StopCoroutine(hideRoutine);

            label.text = text;
            Vector2 size = label.GetPreferredValues(text, maxTextWidth, 0f);
            label.rectTransform.sizeDelta = size;
            backing.sizeDelta = size + padding * 2f;

            if (seconds <= 0f) seconds = Mathf.Max(minSeconds, text.Length * secondsPerCharacter);
            fade.To(1f, fadeDuration);
            hideRoutine = StartCoroutine(HideAfter(seconds));
        }

        public void Hide()
        {
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = null;
            fade.To(0f, fadeDuration);
        }

        private IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            hideRoutine = null;
            fade.To(0f, fadeDuration);
        }
    }
}
