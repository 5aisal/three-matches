using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// The past stairs are gone (GAME_SPEC §10): the upper steps are removed and the lower ones are
    /// broken and sunk. Applied once at start; the steps are ordered bottom to top.
    /// </summary>
    public sealed class StairsCollapse : MonoBehaviour
    {
        [SerializeField] private Transform[] steps;
        [Tooltip("Steps from this index upward are gone.")]
        [SerializeField, Min(0)] private int goneFromIndex = 4;
        [SerializeField, Min(0f)] private float sinkDepth = 0.18f;
        [SerializeField, Min(0f)] private float maxTilt = 14f;
        [SerializeField, Range(0.1f, 1f)] private float brokenHeightScale = 0.7f;

        private void Awake()
        {
            for (int i = 0; i < steps.Length; i++)
            {
                Transform step = steps[i];
                if (i >= goneFromIndex)
                {
                    step.gameObject.SetActive(false);
                    continue;
                }

                // Alternate the tilt direction so the broken steps look uneven.
                float sign = (i % 2 == 0) ? 1f : -1f;
                float tilt = maxTilt * sign * (0.5f + 0.5f * i / Mathf.Max(1, goneFromIndex - 1));
                Vector3 scale = step.localScale;
                float oldHeight = scale.y;
                scale.y *= brokenHeightScale;
                step.localScale = scale;

                Vector3 pos = step.localPosition;
                pos.y -= (oldHeight - scale.y) * 0.5f + sinkDepth * i / Mathf.Max(1, goneFromIndex);
                step.localPosition = pos;
                step.localRotation = Quaternion.Euler(0f, 0f, tilt);
            }
        }
    }
}
