using System.Collections.Generic;
using TMPro;
using ThreeMatches.Flow;
using ThreeMatches.Strips;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeMatches.DebugTools
{
    /// <summary>
    /// GAME_SPEC §22 F1 (restart Act 1) and F9 (trailer mode: hide all UI). Editor and development builds only.
    /// Trailer mode keeps the ending visuals and the crack effect; it hides everything a player would read as UI.
    /// </summary>
    public sealed class FlowDebugKeys : MonoBehaviour
    {
        [SerializeField] private GameFlowDirector flow;
        [SerializeField] private DebugOverlay overlay;

        private readonly List<Canvas> hiddenCanvases = new List<Canvas>(8);
        private readonly List<TMP_Text> culledTexts = new List<TMP_Text>(8);
        private bool trailerMode;

        private void Awake()
        {
            // Debug.isDebugBuild is true in the editor and in development builds.
            if (!Debug.isDebugBuild) enabled = false;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f1Key.wasPressedThisFrame) flow.RestartAct1();
            else if (kb.f9Key.wasPressedThisFrame) SetTrailerMode(!trailerMode);
        }

        private void SetTrailerMode(bool on)
        {
            trailerMode = on;
            if (overlay != null) overlay.enabled = !on;

            if (!on)
            {
                for (int i = 0; i < hiddenCanvases.Count; i++) if (hiddenCanvases[i] != null) hiddenCanvases[i].enabled = true;
                for (int i = 0; i < culledTexts.Count; i++) if (culledTexts[i] != null) culledTexts[i].canvasRenderer.cull = false;
                hiddenCanvases.Clear();
                culledTexts.Clear();
                return;
            }

            Canvas[] canvases = FindObjectsByType<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (!canvas.enabled || canvas.GetComponent<TrailerKeep>() != null) continue;

                if (canvas.GetComponent<StripCrackTransition>() != null)
                {
                    // The crack canvas also holds the year labels: hide the text, keep the crack.
                    TMP_Text[] labels = canvas.GetComponentsInChildren<TMP_Text>();
                    for (int t = 0; t < labels.Length; t++)
                    {
                        labels[t].canvasRenderer.cull = true;
                        culledTexts.Add(labels[t]);
                    }
                    continue;
                }

                canvas.enabled = false;
                hiddenCanvases.Add(canvas);
            }
        }
    }
}
