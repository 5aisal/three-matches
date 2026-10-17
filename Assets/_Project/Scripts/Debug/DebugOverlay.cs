using System.Text;
using ThreeMatches.Core;
using ThreeMatches.Fire;
using ThreeMatches.Memories;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeMatches.DebugTools
{
    /// <summary>GAME_SPEC §22 F5: toggles a text overlay with past time, fire levels, steps, burned memories and Noor. Editor and development builds only.</summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private MatchController matches;
        [SerializeField] private PastFireController fire;
        [SerializeField, Min(0.02f)] private float refreshSeconds = 0.1f;
        [SerializeField, Min(8)] private int fontSize = 14;
        [SerializeField] private Vector2 margin = new Vector2(12f, 28f);

        private readonly StringBuilder builder = new StringBuilder(512);
        private string text = "";
        private float nextRefresh;
        private bool visible;
        private GUIStyle style;

        private void Awake()
        {
            // Debug.isDebugBuild is true in the editor and in development builds.
            if (!Debug.isDebugBuild) enabled = false;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.f5Key.wasPressedThisFrame) visible = !visible;
            if (!visible || Time.unscaledTime < nextRefresh) return;

            nextRefresh = Time.unscaledTime + refreshSeconds;
            Rebuild();
        }

        private void Rebuild()
        {
            RescueState s = matches.State;
            MatchSession m = matches.Session;
            builder.Clear();

            builder.Append("t = ").Append(s.PastTime.ToString("F1")).Append(" s");
            if (fire.CandleBurned) builder.Append("  (x1.3 slower: CANDLE)");
            builder.Append('\n');
            builder.Append("match ").Append(m.MatchNumber).Append(" : ").Append(m.Phase);
            if (m.IsWindowOpen) builder.Append("  left ").Append(m.Remaining.ToString("F1")).Append(" s");
            builder.Append("  | past ").Append(fire.IsWindowOpen ? "RUNNING" : "FROZEN").Append('\n');

            FireRoom[] rooms = fire.Rooms;
            for (int i = 0; i < rooms.Length; i++)
            {
                builder.Append(rooms[i].Room).Append(" L").Append(rooms[i].TargetLevel)
                    .Append(" (").Append(rooms[i].BlendedLevel.ToString("F2")).Append(")  ");
            }
            builder.Append('\n');

            builder.Append("steps: ");
            for (int i = 0; i < MemoryRules.StepCount; i++)
            {
                RescueStep step = (RescueStep)i;
                builder.Append(step).Append(s.IsStepCompleted(step) ? "+ " : "- ");
            }
            builder.Append("| burned: ");
            for (int i = 0; i < s.BurnedMemories.Count; i++) builder.Append(s.BurnedMemories[i]).Append(' ');
            builder.Append("| noor: ").Append(s.NoorState);
            text = builder.ToString();
        }

        private void OnGUI()
        {
            if (!visible) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = fontSize,
                    richText = false
                };
            }

            Vector2 size = style.CalcSize(new GUIContent(text));
            float height = size.y + 8f;
            GUI.Box(new Rect(margin.x, Screen.height - height - margin.y, size.x + 12f, height), text, style);
        }
    }
}
