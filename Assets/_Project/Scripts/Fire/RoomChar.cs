using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// Burn damage for one room of the past house (GAME_SPEC §19): a 0-1 "char" value that darkens and
    /// dulls every mesh in the room toward a charred tint. It follows the room's fire level by default,
    /// so scorching blends in with the fire; <see cref="SetChar"/> drives it by hand when no fire is wired.
    /// Uses property blocks, so no material is duplicated.
    /// </summary>
    public sealed class RoomChar : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        [SerializeField] private FireRoom fireRoom;
        [SerializeField, Range(0f, 1f)] private float charAmount;
        [Tooltip("Meshes to scorch. Empty = every MeshRenderer under this room, collected once at start.")]
        [SerializeField] private MeshRenderer[] targets;

        [Header("Look")]
        [SerializeField] private Color charTint = new Color(0.07f, 0.055f, 0.045f, 1f);
        [SerializeField, Range(0f, 1f)] private float smoothnessLoss = 0.8f;

        private MaterialPropertyBlock block;
        private Color[][] baseColors;
        private float[][] baseSmoothness;
        private float applied = -1f;

        public float Char => charAmount;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            if (targets == null || targets.Length == 0) targets = GetComponentsInChildren<MeshRenderer>(true);

            baseColors = new Color[targets.Length][];
            baseSmoothness = new float[targets.Length][];
            for (int i = 0; i < targets.Length; i++)
            {
                Material[] mats = targets[i].sharedMaterials;
                baseColors[i] = new Color[mats.Length];
                baseSmoothness[i] = new float[mats.Length];
                for (int m = 0; m < mats.Length; m++)
                {
                    Material mat = mats[m];
                    baseColors[i][m] = mat != null && mat.HasProperty(BaseColorId) ? mat.GetColor(BaseColorId) : Color.white;
                    baseSmoothness[i][m] = mat != null && mat.HasProperty(SmoothnessId) ? mat.GetFloat(SmoothnessId) : 0.5f;
                }
            }

            Apply(charAmount);
        }

        private void Update()
        {
            if (fireRoom == null) return;
            Apply(FireBlend.CharFromLevel(fireRoom.BlendedLevel));
        }

        /// <summary>Sets the char amount (0-1) directly. Ignored while a fire room keeps overriding it.</summary>
        public void SetChar(float value) => Apply(value);

        private void Apply(float value)
        {
            value = Mathf.Clamp01(value);
            charAmount = value;
            if (Mathf.Approximately(value, applied)) return;
            applied = value;

            for (int i = 0; i < targets.Length; i++)
            {
                MeshRenderer r = targets[i];
                if (r == null) continue;
                for (int m = 0; m < baseColors[i].Length; m++)
                {
                    Color baseColor = baseColors[i][m];
                    Color scorched = Color.Lerp(baseColor, new Color(charTint.r, charTint.g, charTint.b, baseColor.a), value);
                    block.Clear();
                    block.SetColor(BaseColorId, scorched);
                    block.SetFloat(SmoothnessId, baseSmoothness[i][m] * (1f - smoothnessLoss * value));
                    r.SetPropertyBlock(block, m);
                }
            }
        }
    }
}
