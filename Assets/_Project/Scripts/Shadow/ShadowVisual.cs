using ThreeMatches.Fire;
using UnityEngine;

namespace ThreeMatches.Shadow
{
    /// <summary>
    /// The look of Shadow-Rafi (GAME_SPEC §11, §19): the animated Rafi rig drawn as a pure black silhouette,
    /// flattened against the wall, shown only while the shadow is in play. Scale and alpha flicker with the
    /// nearest burning room's light. The silhouette shader draws each pixel once, so overlapping limbs do not
    /// double-darken.
    /// </summary>
    public sealed class ShadowVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int FlattenId = Shader.PropertyToID("_Flatten");
        private static readonly int PlaneZId = Shader.PropertyToID("_FlattenPlaneZ");

        [SerializeField] private PastFireController fire;
        [SerializeField] private ShadowController shadow;
        [Tooltip("Takes the flicker scale (uniform) and marks the wall plane the silhouette is flattened onto.")]
        [SerializeField] private Transform body;
        [SerializeField] private Renderer[] renderers;

        [Header("Look")]
        [Tooltip("Scale on the wall's normal axis, applied by the silhouette shader (a transform scale would break the Humanoid rig).")]
        [SerializeField, Range(0.001f, 1f)] private float wallScale = 0.02f;
        [SerializeField] private Color tint = Color.black;
        [SerializeField, Range(0f, 1f)] private float baseAlpha = 0.85f;

        [Header("Flicker")]
        [Tooltip("Light intensity that gives the full flicker.")]
        [SerializeField, Min(0.1f)] private float fullIntensity = 5f;
        [Tooltip("Distance beyond which a light no longer disturbs the shadow.")]
        [SerializeField, Min(0.1f)] private float lightRange = 7f;
        [SerializeField, Min(0f)] private float scaleAmount = 0.08f;
        [SerializeField, Range(0f, 1f)] private float alphaDip = 0.35f;
        [SerializeField, Min(0.1f)] private float flickerSpeed = 9f;

        private MaterialPropertyBlock block;
        private float clock;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            SetVisible(false);
            ApplyLook(0f, 0f);
        }

        private void OnEnable() => shadow.ActiveChanged += SetVisible;
        private void OnDisable() => shadow.ActiveChanged -= SetVisible;

        private void Update()
        {
            if (!shadow.IsActive) return;

            clock += Time.deltaTime;
            float strength = NearestFireStrength();
            float noise = Mathf.PerlinNoise(clock * flickerSpeed, 0.37f);
            ApplyLook(strength, noise);
        }

        private void SetVisible(bool value)
        {
            for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = value;
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
            block.SetColor(BaseColorId, new Color(tint.r, tint.g, tint.b, baseAlpha * dip));
            block.SetFloat(FlattenId, wallScale);
            block.SetFloat(PlaneZId, body.position.z);
            for (int i = 0; i < renderers.Length; i++) renderers[i].SetPropertyBlock(block);
        }
    }
}
