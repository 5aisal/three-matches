using TMPro;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.UI
{
    /// <summary>
    /// World-space prompt (key icon + verb) floating above the focused Interactable. Lives in the
    /// present house, so only the present camera draws it.
    /// </summary>
    public sealed class InteractPromptUI : MonoBehaviour
    {
        [SerializeField] private InteractionDetector detector;
        [SerializeField] private GameInput input;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text keyLabel;
        [SerializeField] private TMP_Text verbLabel;
        [Tooltip("World z the prompt floats at (toward the camera so props never hide it).")]
        [SerializeField] private float worldZ = -0.6f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.15f;

        private FadeGroup fade;

        private void Awake()
        {
            fade = new FadeGroup(group);
            fade.SnapTo(0f);
        }

        private void OnEnable() => detector.FocusChanged += OnFocusChanged;
        private void OnDisable() => detector.FocusChanged -= OnFocusChanged;

        private void OnFocusChanged(Interactable focus)
        {
            if (focus == null)
            {
                fade.To(0f, fadeDuration);
                return;
            }

            keyLabel.text = string.IsNullOrEmpty(focus.KeyLabelOverride)
                ? input.KeyLabel(input.Interact)
                : focus.KeyLabelOverride;
            verbLabel.text = focus.Verb;
            Vector3 p = focus.PromptPosition;
            transform.position = new Vector3(p.x, p.y, worldZ);
            fade.To(1f, fadeDuration);
        }
    }
}
