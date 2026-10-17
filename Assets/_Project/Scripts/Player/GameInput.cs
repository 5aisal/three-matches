using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeMatches.Player
{
    /// <summary>Owns the input actions asset and exposes each action by name (GAME_SPEC §6).</summary>
    public sealed class GameInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;

        private InputActionMap playerMap;
        private InputActionMap uiMap;

        public InputAction Move { get; private set; }
        public InputAction Vertical { get; private set; }
        public InputAction Interact { get; private set; }
        public InputAction Strike { get; private set; }
        public InputAction Hum { get; private set; }
        public InputAction Pause { get; private set; }
        public InputAction Point { get; private set; }
        public InputAction Click { get; private set; }

        private void Awake()
        {
            playerMap = actions.FindActionMap("Player", true);
            uiMap = actions.FindActionMap("UI", true);

            Move = playerMap.FindAction("Move", true);
            Vertical = playerMap.FindAction("Vertical", true);
            Interact = playerMap.FindAction("Interact", true);
            Strike = playerMap.FindAction("Strike", true);
            Hum = playerMap.FindAction("Hum", true);
            Pause = playerMap.FindAction("Pause", true);
            Point = uiMap.FindAction("Point", true);
            Click = uiMap.FindAction("Click", true);
        }

        private void OnEnable()
        {
            playerMap.Enable();
            uiMap.Enable();
        }

        private void OnDisable()
        {
            playerMap.Disable();
            uiMap.Disable();
        }

        /// <summary>Key label for an action, e.g. "E" for Interact.</summary>
        public string KeyLabel(InputAction action) => action.GetBindingDisplayString();
    }
}
