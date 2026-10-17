using LitMotion;
using ThreeMatches.Core;
using ThreeMatches.Player;
using ThreeMatches.Strips;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatches.Memories
{
    /// <summary>
    /// Match icons (bottom-right of the Present strip) and the flame timer (bottom of the Present strip).
    /// A used match burns down 0.5 s, then turns ash-grey (GAME_SPEC §24).
    /// </summary>
    public sealed class MatchHudUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MatchController matches;
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private GameFlagsService flags;
        [SerializeField] private RectTransform iconsRoot;
        [SerializeField] private Image[] matchIcons;
        [SerializeField] private RectTransform flameRoot;
        [SerializeField] private Image flame;

        [Header("Placement (pixels from the Present strip edges)")]
        [SerializeField] private Vector2 iconsOffset = new Vector2(-28f, 24f);
        [SerializeField] private Vector2 flameOffset = new Vector2(0f, 24f);

        [Header("Look")]
        [SerializeField] private Color matchReadyColor = new Color(0.93f, 0.78f, 0.45f);
        [SerializeField] private Color matchUsedColor = new Color(0.32f, 0.31f, 0.3f);
        [SerializeField] private Color flameColor = new Color(1f, 0.62f, 0.18f);
        [SerializeField, Range(0.01f, 1f)] private float usedMatchHeight = 0.55f;
        [SerializeField, Min(0.01f)] private float burnDownSeconds = 0.5f;
        [SerializeField, Range(0.01f, 1f)] private float flameMinScale = 0.12f;

        [Header("Flicker (last seconds)")]
        [SerializeField, Min(0.1f)] private float flickerSpeed = 22f;
        [SerializeField, Range(0f, 1f)] private float flickerDepth = 0.35f;

        private sealed class IconState
        {
            public Image Image;
            public float Burn;
            public MotionHandle Handle;
        }

        private IconState[] icons;
        private int shownUsed;

        private void Awake()
        {
            icons = new IconState[matchIcons.Length];
            for (int i = 0; i < matchIcons.Length; i++)
            {
                icons[i] = new IconState { Image = matchIcons[i] };
                ApplyIcon(icons[i], 0f);
            }
            flame.enabled = false;
        }

        private void OnEnable()
        {
            matches.StateChanged += Refresh;
            layout.LayoutUpdated += PlaceOnPresentStrip;
            PlaceOnPresentStrip();
            Refresh();
        }

        private void OnDisable()
        {
            matches.StateChanged -= Refresh;
            layout.LayoutUpdated -= PlaceOnPresentStrip;
        }

        private void OnDestroy()
        {
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].Handle.IsActive()) icons[i].Handle.Cancel();
            }
        }

        private void Update()
        {
            MatchSession session = matches.Session;
            bool lit = session.IsActive;
            if (flame.enabled != lit) flame.enabled = lit;
            if (!lit) return;

            float scale = Mathf.Lerp(flameMinScale, 1f, session.Fraction);
            float alpha = 1f;
            if (session.IsFlickering)
            {
                float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.37f);
                scale *= 1f - flickerDepth * noise;
                alpha = 1f - flickerDepth * noise;
            }

            flameRoot.localScale = new Vector3(scale, scale, 1f);
            Color c = flameColor;
            c.a = alpha;
            flame.color = c;
        }

        private void Refresh()
        {
            bool hasMatches = flags.Flags.Has(GameFlags.HasMatches);
            iconsRoot.gameObject.SetActive(hasMatches);

            int used = matches.State.MatchesUsed;
            if (used == shownUsed + 1) AnimateUsed(shownUsed);
            else SnapIcons(used);
            shownUsed = used;
        }

        private void AnimateUsed(int index)
        {
            if (index < 0 || index >= icons.Length) return;
            IconState icon = icons[index];
            if (icon.Handle.IsActive()) icon.Handle.Cancel();
            icon.Handle = LMotion.Create(icon.Burn, 1f, burnDownSeconds)
                .WithEase(Ease.InCubic)
                .Bind(this, icon, static (t, self, state) => self.ApplyIcon(state, t));
        }

        private void SnapIcons(int used)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].Handle.IsActive()) icons[i].Handle.Cancel();
                ApplyIcon(icons[i], i < used ? 1f : 0f);
            }
        }

        private void ApplyIcon(IconState icon, float burnT)
        {
            icon.Burn = burnT;
            icon.Image.color = Color.Lerp(matchReadyColor, matchUsedColor, burnT);
            icon.Image.rectTransform.localScale = new Vector3(1f, Mathf.Lerp(1f, usedMatchHeight, burnT), 1f);
        }

        private void PlaceOnPresentStrip()
        {
            Rect r = layout.GetRect(StripId.Present);

            iconsRoot.anchorMin = iconsRoot.anchorMax = new Vector2(r.xMax, r.yMin);
            iconsRoot.pivot = new Vector2(1f, 0f);
            iconsRoot.anchoredPosition = iconsOffset;

            flameRoot.anchorMin = flameRoot.anchorMax = new Vector2(r.center.x, r.yMin);
            flameRoot.pivot = new Vector2(0.5f, 0f);
            flameRoot.anchoredPosition = flameOffset;
        }
    }
}
