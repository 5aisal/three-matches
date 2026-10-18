using System;
using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// Extra particle layers of a room fire (tongues, licks, embers, ground-fire size). FireRoom owns the blended fire
    /// level (it eases over 1.5 s); this follows it and samples each layer's rate and size tables (GAME_SPEC §24).
    /// Layers are children of the room's fire/smoke systems, so FireRoom's pause also freezes them.
    /// </summary>
    public sealed class FireLayers : MonoBehaviour
    {
        [Serializable]
        private sealed class Layer
        {
            public ParticleSystem system;
            [Tooltip("Emission rate per fire level 0-3. Leave empty to keep the system's own rate.")]
            public float[] ratePerLevel;
            [Tooltip("Size multiplier per fire level 0-3. Leave empty to keep the authored size.")]
            public float[] sizePerLevel;
            [Tooltip("Width of the box emitter (local x) per fire level 0-3. Leave empty to keep the authored width.")]
            public float[] widthPerLevel;

            private float baseX, baseY;

            public void CacheSize()
            {
                var main = system.main;
                baseX = main.startSizeXMultiplier;
                baseY = main.startSizeYMultiplier;
            }

            public void Apply(float level)
            {
                if (ratePerLevel != null && ratePerLevel.Length > 0)
                {
                    var emission = system.emission;
                    emission.rateOverTime = FireBlend.Sample(ratePerLevel, level);
                }

                if (widthPerLevel != null && widthPerLevel.Length > 0)
                {
                    var shape = system.shape;
                    Vector3 scale = shape.scale;
                    scale.x = Mathf.Max(0.05f, FireBlend.Sample(widthPerLevel, level));
                    shape.scale = scale;
                }

                if (sizePerLevel != null && sizePerLevel.Length > 0)
                {
                    float s = FireBlend.Sample(sizePerLevel, level);
                    var main = system.main;
                    main.startSizeXMultiplier = baseX * s;
                    main.startSizeYMultiplier = baseY * s;
                }
            }
        }

        [SerializeField] private FireRoom room;
        [SerializeField] private Layer[] layers;

        private float lastLevel = -1f;

        private void Awake()
        {
            for (int i = 0; i < layers.Length; i++) layers[i].CacheSize();
        }

        private void Update()
        {
            float level = room.BlendedLevel;
            if (Mathf.Approximately(level, lastLevel)) return;
            lastLevel = level;
            for (int i = 0; i < layers.Length; i++) layers[i].Apply(level);
        }
    }
}
