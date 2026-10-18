using UnityEngine;
using UnityEngine.Rendering;

namespace ThreeMatches.Lighting
{
    /// <summary>
    /// Per-timeline ambient light and key (moon / room) light. The three houses share one scene, so the
    /// key light is only enabled and the ambient colour only applied while this strip's camera renders.
    /// </summary>
    public sealed class StripAtmosphere : MonoBehaviour
    {
        [SerializeField] private Camera stripCamera;
        [SerializeField] private Light keyLight;
        [SerializeField] private Color ambient = new Color(0.02f, 0.03f, 0.06f, 1f);

        public Color Ambient
        {
            get => ambient;
            set => ambient = value;
        }

        public Light KeyLight => keyLight;

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endContextRendering += OnEndContext;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endContextRendering -= OnEndContext;
            keyLight.enabled = false;
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            bool mine = cam == stripCamera;
            keyLight.enabled = mine;
            if (!mine) return;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
            var probe = new SphericalHarmonicsL2();
            probe.AddAmbientLight(ambient);
            RenderSettings.ambientProbe = probe;
        }

        private void OnEndContext(ScriptableRenderContext context, System.Collections.Generic.List<Camera> cameras)
        {
            keyLight.enabled = false;
        }
    }
}
