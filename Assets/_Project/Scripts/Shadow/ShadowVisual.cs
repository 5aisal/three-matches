using ThreeMatches.Fire;
using UnityEngine;

namespace ThreeMatches.Shadow
{
    /// <summary>
    /// Placeholder look of the shadow: a black Rafi silhouette with a softer, larger halo copy behind it.
    /// Scale and alpha flicker with the nearest burning room's light (GAME_SPEC §11).
    /// </summary>
    public sealed class ShadowVisual : MonoBehaviour
    {
        [SerializeField] private PastFireController fire;
        [SerializeField] private ShadowController shadow;
        [SerializeField] private Transform body;
        [SerializeField] private SpriteRenderer main;
        [SerializeField] private SpriteRenderer halo;

        [Header("Look")]
        [SerializeField] private Color tint = Color.black;
        [SerializeField, Range(0f, 1f)] private float baseAlpha = 0.92f;
        [SerializeField, Range(0f, 1f)] private float haloAlpha = 0.35f;
        [SerializeField, Min(1f)] private float haloScale = 1.12f;

        [Header("Flicker")]
        [Tooltip("Light intensity that gives the full flicker.")]
        [SerializeField, Min(0.1f)] private float fullIntensity = 5f;
        [Tooltip("Distance beyond which a light no longer disturbs the shadow.")]
        [SerializeField, Min(0.1f)] private float lightRange = 7f;
        [SerializeField, Min(0f)] private float scaleAmount = 0.08f;
        [SerializeField, Range(0f, 1f)] private float alphaDip = 0.35f;
        [SerializeField, Min(0.1f)] private float flickerSpeed = 9f;

        private float clock;

        private void Awake()
        {
            halo.transform.localScale = Vector3.one * haloScale;
            ApplyLook(0f, 0f);
        }

        private void Update()
        {
            if (!shadow.IsActive) return;

            clock += Time.deltaTime;
            float strength = NearestFireStrength();
            float noise = Mathf.PerlinNoise(clock * flickerSpeed, 0.37f);
            ApplyLook(strength, noise);
        }

        private float NearestFireStrength()
        {
            Vector3 p = shadow.BodyCenter;
            FireRoom[] rooms = fire.Rooms;
            float best = 0f;
            float bestDist = float.MaxValue;
            for (int i = 0; i < rooms.Length; i++)
            {
                Light l = rooms[i].FireLight;
                if (l == null || !l.enabled) continue;
                float d = Mathf.Abs(l.transform.position.x - p.x);
                if (d >= bestDist) continue;
                bestDist = d;
                best = Mathf.Clamp01(l.intensity / fullIntensity) * Mathf.Clamp01(1f - d / lightRange);
            }
            return best;
        }

        private void ApplyLook(float strength, float noise)
        {
            float wobble = (noise - 0.5f) * 2f;
            body.localScale = Vector3.one * (1f + wobble * scaleAmount * strength);

            float dip = 1f - alphaDip * strength * noise;
            SetAlpha(main, baseAlpha * dip);
            SetAlpha(halo, haloAlpha * dip);
        }

        private void SetAlpha(SpriteRenderer r, float alpha)
        {
            r.color = new Color(tint.r, tint.g, tint.b, alpha);
        }
    }
}
