using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeMatches.Strips
{
    /// <summary>TEMPORARY: 1/2/3 = stage, Tab = cycle active strip. Editor and development builds only.</summary>
    public sealed class StripDebugKeys : MonoBehaviour
    {
        [SerializeField] private StripLayoutController layout;

        private void Awake()
        {
            // Debug.isDebugBuild is true in the editor and in development builds.
            if (!Debug.isDebugBuild) enabled = false;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) layout.SetStage(1);
            else if (kb.digit2Key.wasPressedThisFrame) layout.SetStage(2);
            else if (kb.digit3Key.wasPressedThisFrame) layout.SetStage(3);
            else if (kb.tabKey.wasPressedThisFrame) layout.CycleActiveStrip();
        }
    }
}
