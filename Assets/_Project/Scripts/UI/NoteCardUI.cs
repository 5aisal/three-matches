using System.Collections;
using TMPro;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.UI
{
    /// <summary>Centre-screen paper note in a child's handwriting (the matchbox note).</summary>
    public sealed class NoteCardUI : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text noteLabel;
        [SerializeField, Min(0f)] private float fadeDuration = 0.35f;
        [Tooltip("Ignore the dismiss key this long after opening, so the opening press doesn't close it.")]
        [SerializeField, Min(0f)] private float dismissDelay = 0.4f;

        private FadeGroup fade;

        private void Awake()
        {
            fade = new FadeGroup(group);
            fade.SnapTo(0f);
        }

        /// <summary>Shows the note and waits until the player dismisses it or the time runs out.</summary>
        public IEnumerator ShowRoutine(string text, float maxSeconds)
        {
            noteLabel.text = text;
            fade.To(1f, fadeDuration);

            float elapsed = 0f;
            while (elapsed < maxSeconds)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= dismissDelay && input.Interact.WasPressedThisFrame()) break;
                yield return null;
            }

            fade.To(0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
    }
}
