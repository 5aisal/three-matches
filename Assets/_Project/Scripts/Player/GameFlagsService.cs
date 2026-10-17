using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Player
{
    /// <summary>Scene-side holder for the story flags so other components can reference them.</summary>
    public sealed class GameFlagsService : MonoBehaviour
    {
        public GameFlags Flags { get; } = new GameFlags();
    }
}
