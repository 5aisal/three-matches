using System.Collections;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.UI
{
    /// <summary>The §6 controls card: shown for a few seconds at start, and toggled by Esc.</summary>
    public sealed class ControlsCardUI : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private CanvasGroup group;
        [SerializeField, Min(0f)] private float startDuration = 6f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.3f;

        private FadeGroup fade;
        private Coroutine autoHide;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            fade = new FadeGroup(group);
            fade.SnapTo(0f);
        }

        /// <summary>Shows the card for a few seconds. Called by the game flow once the title is dismissed.</summary>
        public void ShowIntro()
        {
            if (autoHide != null) StopCoroutine(autoHide);
            Open();
            autoHide = StartCoroutine(CloseAfter(startDuration));
        }

        private void Update()
        {
            if (!input.Pause.WasPressedThisFrame()) return;
            if (IsOpen) Close();
            else Open();
        }

        private void Open()
        {
            IsOpen = true;
            fade.To(1f, fadeDuration);
        }

        private void Close()
        {
            if (autoHide != null) StopCoroutine(autoHide);
            autoHide = null;
            IsOpen = false;
            fade.To(0f, fadeDuration);
        }

        private IEnumerator CloseAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            autoHide = null;
            Close();
        }
    }
}
