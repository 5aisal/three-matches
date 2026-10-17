using LitMotion;
using TMPro;
using ThreeMatches.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Shadow
{
    /// <summary>World-space prompt (key + verb + hold ring) above the shadow's focused prop in the past house.</summary>
    public sealed class ShadowPromptUI : MonoBehaviour
    {
        [SerializeField] private ShadowInteractor interactor;
        [SerializeField] private GameInput input;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text keyLabel;
        [SerializeField] private TMP_Text verbLabel;
        [SerializeField] private Image ring;
        [Tooltip("World z the prompt floats at (toward the camera so props never hide it).")]
        [SerializeField] private float worldZ = 3.2f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.15f;

        private MotionHandle fadeHandle;
        private bool showRing;

        private void Awake()
        {
            group.alpha = 0f;
            ring.gameObject.SetActive(false);
        }

        private void OnEnable() => interactor.FocusChanged += OnFocusChanged;
        private void OnDisable()
        {
            interactor.FocusChanged -= OnFocusChanged;
            if (fadeHandle.IsActive()) fadeHandle.Cancel();
        }

        private void Update()
        {
            if (showRing) ring.fillAmount = interactor.HoldProgress;
        }

        private void OnFocusChanged(ShadowInteractable focus)
        {
            if (focus == null)
            {
                showRing = false;
                ring.gameObject.SetActive(false);
                FadeTo(0f);
                return;
            }

            keyLabel.text = input.KeyLabel(input.Interact);
            verbLabel.text = focus.Verb;
            Vector3 p = focus.PromptPosition;
            transform.position = new Vector3(p.x, p.y, worldZ);
            showRing = focus.HoldSeconds > 0f;
            ring.fillAmount = 0f;
            ring.gameObject.SetActive(showRing);
            FadeTo(1f);
        }

        private void FadeTo(float alpha)
        {
            if (fadeHandle.IsActive()) fadeHandle.Cancel();
            fadeHandle = LMotion.Create(group.alpha, alpha, fadeDuration)
                .WithEase(Ease.InOutSine)
                .Bind(group, static (a, g) => g.alpha = a);
        }
    }
}
