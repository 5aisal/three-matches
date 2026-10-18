using ThreeMatches.Core;
using ThreeMatches.Fire;
using UnityEngine;

namespace ThreeMatches.Lighting
{
    /// <summary>
    /// Past strip: warm amber room light at t=0 that gives way to orange/red as the fire grows
    /// (the flame point lights in each FireRoom take over the lighting).
    /// </summary>
    public sealed class PastAtmosphere : MonoBehaviour
    {
        [SerializeField] private StripAtmosphere strip;
        [SerializeField] private PastFireController fire;

        [Header("Amber room light (t = 0)")]
        [SerializeField] private Color amberAmbient = new Color(0.16f, 0.09f, 0.04f, 1f);
        [SerializeField] private Color amberKey = new Color(1f, 0.68f, 0.35f, 1f);
        [SerializeField, Min(0f)] private float amberKeyIntensity = 0.9f;

        [Header("Fire has taken over")]
        [SerializeField] private Color fireAmbient = new Color(0.12f, 0.03f, 0.015f, 1f);
        [SerializeField] private Color fireKey = new Color(1f, 0.32f, 0.1f, 1f);
        [SerializeField, Min(0f)] private float fireKeyIntensity = 0.12f;

        private float lastTakeover = -1f;

        private void LateUpdate()
        {
            FireRoom[] rooms = fire.Rooms;
            float highest = 0f;
            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i].BlendedLevel > highest) highest = rooms[i].BlendedLevel;
            }

            float takeover = AtmosphereRules.FireTakeover(highest);
            if (Mathf.Approximately(takeover, lastTakeover)) return;
            lastTakeover = takeover;

            strip.Ambient = Color.Lerp(amberAmbient, fireAmbient, takeover);
            Light key = strip.KeyLight;
            key.color = Color.Lerp(amberKey, fireKey, takeover);
            key.intensity = Mathf.Lerp(amberKeyIntensity, fireKeyIntensity, takeover);
        }
    }
}
